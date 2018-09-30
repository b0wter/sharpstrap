using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
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
        /// Number of columns reserved for the names of the packages.
        /// </summary>
        private const int PackageNameWidth = 40;
        /// <summary>
        /// Number of columns reserved for the number of operations per package.
        /// </summary>
        private const int PackageModuleCountWidth = 3;
        /// <summary>
        /// Number of columns reserved for the IsCritical flag output.
        /// </summary>
        private const int PackageIsCriticalWidth = 8;
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
        public string LogFilename { get; set; } = "bootstrap.log.";
        /// <summary>
        /// Global variables are injected into every package that is executed.
        /// </summary>
        public IDictionary<string, string> GlobalVariables { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// List of packages that will be run at the end of the bootstrap process. Regular packages may not depend on these.
        /// </summary>
        public List<Package> CleanupPackages { get; set; } = new List<Package>();

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
        private readonly PackageStorage packages;
        
        /// <summary>
        /// Initializes and runs the bootstrap process.
        /// </summary>
        /// <param name="input">Device that provides user input.</param>
        /// <param name="output">Device with display capabilities.</param>
        /// <param name="columnCount">Number of columns the output devices can render.</param>
        /// <param name="overrideUserDecision">Override the user interaction asking for confirmation.</param>
        /// <returns></returns>
        public async Task<bool> Run(IIODefinition ioDefinition, ITextFileInput textFileInput, ITextFileOutput textFileOutput, bool overrideUserDecision = false)
        {
            await Task.Delay(1);
            return false;
            /*
            this.textFileInput = textFileInput;
            this.textFileOutput = textFileOutput;
            this.ioDefinition = ioDefinition;
            
            AddDefaultVariables();
            
            try {
                ValidatePackages();
                DryRunDependencies();
            } catch (ArgumentException ex) {
                this.ioDefinition.TextWriter.WriteLine($"Execution stopped because: {ex.Message}");
                return false;
            }

            if(InitPackageOperation(overrideUserDecision))
            {
                LoadSolvedPackagesFromLog();
                PrintPackageLogSummary();
                await RunAllPackages(this.Packages);
                LogPackagesToFile(this.solvedPackages, SuccessLogFilename);
                LogPackagesToFile(this.unsolvedPackages, ErrorLogFilename);
                await RunAllPackages(this.CleanupPackages);
            }
            else
            {
                // Package installation was cancelled.
                // Nothing to do here.
            }

            PrintResults();
            return true;
            */
        }

        /*
        private void AddDefaultVariables()
        {
            this.GlobalVariables.Add("username", Environment.UserName);

            var envHome = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "HOMEPATH" : "HOME";
            var home = Environment.GetEnvironmentVariable(envHome);

            this.GlobalVariables.Add("homedir", home);
            this.GlobalVariables.Add("~", home);
        }

        private void ValidatePackages()
        {
            AddMissingNamesForAllPackages();
            CheckExistanceOfRequirements();
        }

        private void AddMissingNamesForAllPackages()
        {
            var packagesWithoutName = this.Packages.Where(p => string.IsNullOrWhiteSpace(p.Name)).ToList();
            for(int i = 0; i < packagesWithoutName.Count; ++i)
                packagesWithoutName[i].Name = $"<Unnamed Package #{i}>";
        }

        private void DryRunDependencies()
        {
            var requirements = this.Packages.Select(p => (p.Name, p.Requires)).ToList();
            var solved = requirements.Where(r => r.Requires.Any() == false).ToList();

            while(requirements.Count() != 0)
            {
                var solvable = requirements.Where(p => p.Requires.Except(solved.Where(d => d.Name != null).Select(d => d.Name)).Count() == 0);
                if(solvable.Count() == 0)
                    throw new ArgumentException("The given package combination cannot be solved.");
                
                foreach(var s in solvable)
                    solved.Add(s);

                requirements.RemoveAll(r => solvable.Contains(r));
            }
        }

        private void CheckExistanceOfRequirements()
        {
            var requirements = this.Packages.SelectMany(p => p.Requires).Distinct();
            var names = this.Packages.Select(p => p.Name);

            var nonExistingNames = requirements.Where(r => names.Contains(r) == false);

            if(nonExistingNames.Count() != 0)
                throw new ArgumentException($"The following requirements are listed but do not exist: {string.Join(", ", nonExistingNames)}.");
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
        /// Checks the existance of a previous log file and reads the previously installed packages from it.
        /// These packages will be moved to <see cref="solvedPackages"/> and not rerun.
        /// </summary>
        private void LoadSolvedPackagesFromLog()
        {
            if(string.IsNullOrWhiteSpace(SuccessLogFilename) == false && System.IO.File.Exists(SuccessLogFilename))
            {
                var previouslyRunPackageNames = textFileInput.ReadAllLines(SuccessLogFilename).Where(l => string.IsNullOrWhiteSpace(l) == false);
                var previouslyRunPackages = previouslyRunPackageNames.Select(name => this.Packages.Find(p => p.Name == name && p.IgnoreAlreadySolved == false)).Where(x => x != null).ToList();
                this.Packages.RemoveAll(p => previouslyRunPackages.Contains(p) && p.IgnoreAlreadySolved == false);
                this.previouslyRunPackages.AddRange(previouslyRunPackages);

                ioDefinition.TextWriter.WriteLine("The following packages have already been finished:");
                ioDefinition.TextWriter.WriteLine();
                foreach(var package in previouslyRunPackageNames)
                    ioDefinition.TextWriter.WriteLine($" * {package}");
                ioDefinition.TextWriter.WriteLine();
                ioDefinition.TextWriter.WriteLine($"If you want to repeat these steps remove '{SuccessLogFilename}'.");
            }
        }

        private void PrintPackageLogSummary()
        {
            this.ioDefinition.TextWriter.WriteLine($"The following packages have been finished previously and will not be run:");
            foreach(var package in this.previouslyRunPackages)
                this.ioDefinition.TextWriter.WriteLine(package.Name);

            ioDefinition.TextWriter.WriteLine($"The following packages will be run:");
            foreach(var package in Packages)
                this.ioDefinition.TextWriter.WriteLine(package.Name);
        }

        /// <summary>
        /// Displays a summary of the deserialized packages. Includes all packages, even the previously completed ones.
        /// </summary>
        /// <param name="overrideUserDecision"></param>
        /// <returns></returns>
        private bool InitPackageOperation(bool overrideUserDecision)
        {
            int noOfPackages = Packages.Count();
            int noOfModules = Packages.Sum(p => p.Modules.Count());
            this.ioDefinition.TextWriter.WriteLine($"This bootstrap configuration contains {noOfPackages} Packages with a total of {noOfModules} operations.");
            this.ioDefinition.TextWriter.WriteLine();

            this.ioDefinition.TextWriter.WriteLine($"{"NAME".PadRight(PackageNameWidth)} {"OPS".PadRight(PackageModuleCountWidth)} {"CRITICAL".PadRight(PackageIsCriticalWidth)} DESCRIPTION");
            this.ioDefinition.TextWriter.WriteLine(new String('=', this.ioDefinition.ColumnWidth));

            int remainingWidth = this.ioDefinition.ColumnWidth - PackageNameWidth - PackageModuleCountWidth - PackageIsCriticalWidth - 3;

            foreach(var package in Packages)
            {
                string paddedName;
                if(package.Name.Length > PackageNameWidth)
                    paddedName = package.Name.Substring(0, PackageNameWidth -3 ) + "...";
                else
                    paddedName = package.Name.PadRight(PackageNameWidth);

                var paddedModuleCount = package.Modules.Count().ToString().PadLeft(PackageModuleCountWidth);
                var paddedIsCritical = package.IsCritical.ToString().PadRight(PackageIsCriticalWidth);

                string paddedDescription;
                if(package.Description != null && package.Description.Length > remainingWidth && remainingWidth - 3 > 0)
                    paddedDescription = package.Description.Substring(0, remainingWidth - 3) + "...";
                else
                    paddedDescription = package.Description;

                this.ioDefinition.TextWriter.WriteLine($"{paddedName} {paddedModuleCount} {paddedIsCritical} {paddedDescription}");
            }

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

        /// <summary>
        /// Runs each package separately, taking their prerequisites into account.
        /// </summary>
        /// <returns></returns>
        private async Task RunAllPackages(List<Package> packages)
        {
            while(packages.Count() != 0)
            {
                var solved = new List<Package>();
                var unsolved = new List<Package>();

                var solvablePackages = packages
                                           .Where(p => ValidateRequirementsMet(p, this.solvedPackages))
                                           .ToList();

                if (solvablePackages.Count == 0)
                {
                    this.ioDefinition.TextWriter.SetForegroundColor(ConsoleColor.Red);
                    this.ioDefinition.TextWriter.WriteLine($"There are {packages.Count()} packages left to work on but their requirements have not been met.");
                    this.ioDefinition.TextWriter.ResetColors();
                    return;
                }

                foreach(var package in solvablePackages)
                {
                    try{
                        await package.Run(this.ioDefinition.TextWriter, this.GlobalVariables);

                        // Add the package to the solved dependencies so that the depending packages can be run.
                        //
                        solved.Add(package);
                        packages.Remove(package);
                    }
                    catch(ShellCommandException ex)
                    {
                        this.ioDefinition.TextWriter.WriteLine($"Encountered an {ex.GetType().Name} with: {ex.Message}");
                        unsolved.Add(package);
                        packages.Remove(package);
                        if(package.IsCritical)
                        {
                            this.ioDefinition.TextWriter.WriteLine("Bootstrapping won't continue as this is a critical package.");
                            return;
                        }
                    }
                }

                this.solvedPackages.AddRange(solved);
                this.unsolvedPackages.AddRange(unsolved);
            }
        }


        /// <summary>
        /// Writes the contents of <see cref="solvedPackages"/> to the log file.
        /// </summary>
        private void LogPackagesToFile(IEnumerable<Package> packages, string filename)
        {
            if(string.IsNullOrWhiteSpace(filename))
                return;

            try {
                textFileOutput.WriteAllLines(filename, packages.Select(p => p.Name));
            }
            catch(UnauthorizedAccessException)
            {
                this.ioDefinition.TextWriter.SetForegroundColor(ConsoleColor.Red);
                this.ioDefinition.TextWriter.WriteLine($"Could not write log to '{SuccessLogFilename} because access was denied!");
                this.ioDefinition.TextWriter.ResetColors();
            }
            catch(IOException ex)
            {
                this.ioDefinition.TextWriter.SetForegroundColor(ConsoleColor.Red);
                this.ioDefinition.TextWriter.WriteLine($"Could not write log to '{SuccessLogFilename} because a general IO exception was raised:");
                this.ioDefinition.TextWriter.ResetColors();
                this.ioDefinition.TextWriter.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Checks if the requirements for the given package have been installed.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool ValidateRequirementsMet(Package p, IEnumerable<Package> solvedPackages)
        {
            return p.Requires.Except(solvedPackages.Where(d => d.Name != null).Select(d => d.Name)).Count() == 0;
        }

        private void PrintResults()
        {
            this.ioDefinition.TextWriter.WriteLine();
            PrintResultSummary();
            PrintResultHeader();
            PrintResultsFor(this.previouslyRunPackages, "PREV");
            PrintResultsFor(this.solvedPackages, "SUCCESS", ConsoleColor.Green);
            PrintResultsFor(this.unsolvedPackages, "FAILED", ConsoleColor.Red);
            this.ioDefinition.TextWriter.ResetColors();
        }

        private void PrintResultSummary()
        {
            this.ioDefinition.TextWriter.WriteLine($"{this.unsolvedPackages.Count} packages have not been run due to errors or unmet requirements.");
            this.ioDefinition.TextWriter.WriteLine($"{this.solvedPackages.Count} packages have been run successfully.");
            this.ioDefinition.TextWriter.WriteLine($"{this.previouslyRunPackages.Count} packages have been run previously and will not be run again.");
        }

        private void PrintResultHeader()
        {
            this.ioDefinition.TextWriter.WriteLine();
            this.ioDefinition.TextWriter.WriteLine($"{"NAME".PadRight(PackageNameWidth)} {"RESULT".PadRight(PackageModuleCountWidth)}");
            this.ioDefinition.TextWriter.WriteLine(new String('=', this.ioDefinition.ColumnWidth));
        }

        private void PrintResultsFor(IEnumerable<Package> packages, string status, ConsoleColor resultColor = ConsoleColor.White)
        {
            const int columnWidth = 7;
            string paddedStatus = status.PadRight(columnWidth).ToUpper();
            if(paddedStatus.Length > columnWidth)
                paddedStatus = paddedStatus.Substring(0, columnWidth - 3) + "...";


            foreach(var p in packages)
            {
                var packageName = string.IsNullOrWhiteSpace(p?.Name) ? "no name?" : p.Name;
                var paddedPackageName = packageName.PadRight(PackageNameWidth);
                this.ioDefinition.TextWriter.WriteLine($"{paddedPackageName} {paddedStatus}");
            }
        }
        */
    }
}