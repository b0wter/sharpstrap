using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Reflection.Metadata;
using SharpStrap.Helpers;
using System.Runtime.InteropServices;
using YamlDotNet.Serialization;

namespace SharpStrap.Modules
{
    /// <summary>
    /// Holds all information for a complete bootstrap process.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")] // public needed bc of yaml deserialization
    public class Bootstrap
    {
        /// <summary>
        /// Input device, usually the console.
        /// </summary>
        private IIODefinition ioDefinition;
        /// <summary>
        /// List of packages that will be run. Needed for deserialization but emptied afterwards.
        /// </summary>
        [YamlMember(Alias = "Packages", ApplyNamingConventions = false)]
        public List<Package> RawPackages { get; set; } = new List<Package>();
        /// <summary>
        /// Filename for the logfile containing the successful packages. No file will be written if it's empty.
        /// </summary>
        public string LogFilename { get; set; } = "bootstrap.log";
        /// <summary>
        /// Global variables are injected into every package that is executed.
        /// </summary>
        public IDictionary<string, string> GlobalVariables { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// List of packages that will be run at the end of the bootstrap process. Regular packages may not depend on these.
        /// </summary>
        [YamlMember(Alias = "CleanupPackages", ApplyNamingConventions = false)]
        public List<Package> RawCleanupPackages { get; set; } = new List<Package>();

        /// <summary>
        /// Used to read text files from the local filesystem.
        /// </summary>
        private ITextFileInput textFileInput;
        /// <summary>
        /// Used to write text files to the local filesystem.
        /// </summary>
        private ITextFileOutput textFileOutput;
        /// <summary>
        /// Contains all packages and their current states (run successfully, failed, ...).
        /// </summary>
        private PackageStorage packages;
        /// <summary>
        /// Contains all packages and their current states for the cleanup operation.
        /// </summary>
        private PackageStorage cleanupPackages;
        /// <summary>
        /// Reads and writes the status of the bootstrap operation into a specific storage.
        /// </summary>
        private IBootstrapStatusLogger statusLogger;
        /// <summary>
        /// Used to write various information to the ui.
        /// </summary>
        private IPackageInformationPrinter packageInformationPrinter;
        
        /// <summary>
        /// Initializes and runs the bootstrap process.
        /// </summary>
        /// <param name="input">Device that provides user input.</param>
        /// <param name="output">Device with display capabilities.</param>
        /// <param name="columnCount">Number of columns the output devices can render.</param>
        /// <param name="overrideUserDecision">Override the user interaction asking for confirmation.</param>
        /// <returns></returns>
        public async Task<bool> Run(IPackageInformationPrinter packageInformationPrinter, IBootstrapStatusLogger statusLogger, IIODefinition ioDefinition, ITextFileInput textFileInput, ITextFileOutput textFileOutput, bool overrideUserDecision = false)
        {
            this.textFileInput = textFileInput;
            this.textFileOutput = textFileOutput;
            this.ioDefinition = ioDefinition;
            this.statusLogger = statusLogger;
            this.packageInformationPrinter = packageInformationPrinter;
              
            InitPackageStorages();
            AddDefaultVariables();
            
            var dryRunSuccess = DryRunPackages();
            if (dryRunSuccess == false)
                return false;

            if(InitPackageOperation(overrideUserDecision))
            {
                await RunAllPackages(this.packages);
                WriteLog();
                await RunAllPackages(this.cleanupPackages);
            }
            else
            {
                return false;
            }

            // Print the user feedback.
            this.packageInformationPrinter.PrintResults(this.packages.PreviouslyRun, this.packages.Solved, this.packages.Unsolved);

            return true;
        }

        private void InitPackageStorages()
        {
            var logEntries = statusLogger.LoadOldLog(this.LogFilename);
            this.packages = new PackageStorage(logEntries, this.RawPackages);
            this.RawPackages.Clear();
            this.packages.ValidatePackages();
            this.cleanupPackages = new PackageStorage(null, RawCleanupPackages);
        }

        private void AddDefaultVariables()
        {
            this.GlobalVariables.Add("username", Environment.UserName);

            var envHome = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "HOMEPATH" : "HOME";
            var home = Environment.GetEnvironmentVariable(envHome);

            this.GlobalVariables.Add("homedir", home);
            this.GlobalVariables.Add("~", home);
        }

        private bool DryRunPackages()
        {
            var (dryRunSuccess, dryRunError) = this.packages.DryRunDependencies();
            if (dryRunSuccess == false)
            {
                this.ioDefinition.TextWriter.WriteLine("Execution stopped because the dry run was not successful.");
                this.ioDefinition.TextWriter.WriteLine(dryRunError);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Creates an <see cref="BootstrapResults"/> instance based on the contents of <see cref="this.solvedPackages"/> and <see cref="this.unsolvedPackages"/>.
        /// </summary>
        /// <returns></returns>
        private BootstrapResults CreateBootstrapResultsFromWorkedPackages()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Displays a summary of the deserialized packages. Includes all packages, even the previously completed ones.
        /// </summary>
        /// <param name="overrideUserDecision"></param>
        /// <returns></returns>
        private bool InitPackageOperation(bool overrideUserDecision)
        {
            this.packageInformationPrinter.PrintDetailedPackageSummary(packages.All);

            if(overrideUserDecision)
            {
                return true;
            }
            else
            {
                this.ioDefinition.TextWriter.WriteLine();
                this.ioDefinition.TextWriter.WriteLine("Do you want to continue? (y/N)");
                char readChar = (char)0;
                while(char.IsLetter(readChar) == false && char.IsNumber(readChar) == false)
                    readChar = (char)this.ioDefinition.TextReader.Read();

                this.ioDefinition.TextWriter.WriteLine();
                return readChar == 'y' || readChar == 'Y';
            }
        }

        private async Task RunAllPackages(PackageStorage packageStorage)
        {
            Package package = null;
            while ((package = packageStorage.GetNextPackage()) != null)
            {
                try
                {
                    await package.Run(this.ioDefinition.TextWriter, this.GlobalVariables);
                    packageStorage.MarkPackageSolved(package);
                }
                catch (ShellCommandException ex)
                {
                    this.ioDefinition.TextWriter.WriteLine($"Encountered an {ex.GetType().Name} with: {ex.Message}");
                    packageStorage.MarkPackageFailed(package);
                    
                    if(package.IsCritical)
                    {
                        this.ioDefinition.TextWriter.WriteLine("Bootstrapping won't continue as this is a critical package.");
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the requirements for the given package have been installed.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool ValidateRequirementsMet(Package p, IEnumerable<Package> solvedPackages)
        {
            return p.Requires.Except(solvedPackages.Where(d => d.Name != null).Select(d => d.Name)).Any() == false;
        }

        private void WriteLog()
        {
            try
            {
                this.statusLogger.SaveNewLog(this.LogFilename, this.packages.GetLogResult());
            }
            catch (UnauthorizedAccessException ex)
            {
                this.ioDefinition.TextWriter.SetBackgroundColor(ConsoleColor.Red);
                this.ioDefinition.TextWriter.WriteLine("Could not write a log file because write access was denied.");
            }
        }
    }
}