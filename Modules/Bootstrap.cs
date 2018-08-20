using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Cootstrap.Helpers;
using System.Runtime.InteropServices;

namespace Cootstrap.Modules
{
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
        /// List of packages that executred successfully.
        /// </summary>
        /// <typeparam name="Package"></typeparam>
        /// <returns></returns>
        private List<Package> solvedPackages = new List<Package>();
        /// <summary>
        /// List of packages that failed execution.
        /// </summary>
        /// <typeparam name="Package"></typeparam>
        /// <returns></returns>
        private List<Package> unsolvedPackages = new List<Package>();
        /// <summary>
        /// Input device, usually the console.
        /// </summary>
        private TextReader input;
        /// <summary>
        /// Output devices, usually the console.
        /// </summary>
        private ColoredTextWriter output;
        /// <summary>
        /// Number of columns of the current output device.
        /// </summary>
        private int columnCount;

        /// <summary>
        /// List of packages that will be run.
        /// </summary>
        /// <typeparam name="Package"></typeparam>
        /// <returns></returns>
        public List<Package> Packages { get; set; } = new List<Package>();
        /// <summary>
        /// Filename for the logfile. No file will be written if it's empty.
        /// </summary>
        /// <value></value>
        public string LogFilename { get; set; } = "bootstrap.log";
        /// <summary>
        /// Global variables are injected into every package that is executed.
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public IDictionary<string, string> GlobalVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// List of packages that will be run at the end of the bootstrap process. Regular packages may not depend on these.
        /// </summary>
        /// <typeparam name="Package"></typeparam>
        /// <returns></returns>
        public List<Package> CleanupPackages { get; set; } = new List<Package>();

        /// <summary>
        /// Initializes and runs the bootstrap process.
        /// </summary>
        /// <param name="input">Device that provides user input.</param>
        /// <param name="output">Device with display capabilities.</param>
        /// <param name="columnCount">Number of columns the output devices can render.</param>
        /// <param name="overrideUserDecision">Override the user interaction asking for confirmation.</param>
        /// <returns></returns>
        public async Task Run(TextReader input, ColoredTextWriter output, int columnCount, bool overrideUserDecision = false)
        {
            this.input = input;
            this.output = output;
            this.columnCount = columnCount;

            AddDefaultVariables();
            try {
                ValidatePackages();
                DryRunDependencies();
            } catch (ArgumentException ex) {
                output.WriteLine($"Execution stopped because: {ex.Message}");
                return;
            }

            if(InitPackageOperation(overrideUserDecision))
            {
                LoadSolvedPackagesFromLog();
                await RunAllPackages(this.Packages);
                LogSolvedPackages();
                await RunAllPackages(this.CleanupPackages);
            }
            else
            {
                // Package installation was cancelled.
                // Nothing to do here.
            }
        }

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
        /// Checks the existance of a previous log file and reads the previously installed packages from it.
        /// These packages will be moved to <see cref="solvedPackages"/> and not rerun.
        /// </summary>
        private void LoadSolvedPackagesFromLog()
        {
            if(string.IsNullOrWhiteSpace(LogFilename) == false && System.IO.File.Exists(LogFilename))
            {
                var finishedPackageNames = System.IO.File.ReadAllLines(LogFilename).Where(l => string.IsNullOrWhiteSpace(l) == false);
                var finishedPackages = finishedPackageNames.Select(name => this.Packages.Find(p => p.Name == name && p.IgnoreAlreadySolved == false)).Where(x => x != null).ToList();
                this.Packages.RemoveAll(p => finishedPackages.Contains(p) && p.IgnoreAlreadySolved == false);
                this.solvedPackages.AddRange(finishedPackages);

                output.WriteLine("The following packages have already been finished:");
                output.WriteLine();
                foreach(var package in finishedPackageNames)
                    output.WriteLine($" * {package}");
                output.WriteLine();
                output.WriteLine($"If you want to repeat these steps remove '{LogFilename}'.");
            }
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
            output.WriteLine($"This bootstrap configuration contains {noOfPackages} Packages with a total of {noOfModules} operations.");
            output.WriteLine();

            output.WriteLine($"{"NAME".PadRight(PackageNameWidth)} {"OPS".PadRight(PackageModuleCountWidth)} {"CRITICAL".PadRight(PackageIsCriticalWidth)} DESCRIPTION");
            output.WriteLine(new String('=', this.columnCount));

            int remainingWidth = this.columnCount - PackageNameWidth - PackageModuleCountWidth - PackageIsCriticalWidth - 3;

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

                output.WriteLine($"{paddedName} {paddedModuleCount} {paddedIsCritical} {paddedDescription}");
            }

            if(overrideUserDecision)
            {
                return true;
            }
            else
            {
                output.WriteLine();
                output.WriteLine("Do you want to continue? (y/N)");
                char readChar = (char)0;
                while(char.IsLetter(readChar) == false && char.IsNumber(readChar) == false)
                    readChar = (char)input.Read();

                output.WriteLine();
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
                    output.SetForegroundColor(ConsoleColor.Red);
                    output.WriteLine($"There are {packages.Count()} packages left to work on but their requirements have not been met.");
                    output.ResetColors();
                    return;
                }

                foreach(var package in solvablePackages)
                {
                    try{
                        await package.Run(output, this.GlobalVariables);

                        // Add the package to the solved dependencies so that the depending packages can be run.
                        //
                        solved.Add(package);
                        packages.Remove(package);
                    }
                    catch(ShellCommandException ex)
                    {
                        output.WriteLine($"Encountered an {ex.GetType().Name} with: {ex.Message}");
                        unsolved.Add(package);
                        packages.Remove(package);
                        if(package.IsCritical)
                        {
                            output.WriteLine("Bootstrapping won't continue as this is a critical package.");
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
        private void LogSolvedPackages()
        {
            if(string.IsNullOrWhiteSpace(this.LogFilename))
                return;

            try {
                System.IO.File.WriteAllLines(LogFilename, this.solvedPackages.Select(p => p.Name));
            }
            catch(UnauthorizedAccessException)
            {
                output.SetForegroundColor(ConsoleColor.Red);
                output.WriteLine($"Could not write log to '{LogFilename} because access was denied!");
                output.ResetColors();
            }
            catch(IOException ex)
            {
                output.SetForegroundColor(ConsoleColor.Red);
                output.WriteLine($"Could not write log to '{LogFilename} because a general IO exception was raised:");
                output.ResetColors();
                output.WriteLine(ex.Message);
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
    }
}