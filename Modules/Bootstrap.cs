using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Cootstrap.Helpers;

namespace Cootstrap.Modules
{
    public class Bootstrap
    {
        private const int PackageNameWidth = 40;
        private const int PackageModuleCountWidth = 3;
        private const int PackageIsCriticalWidth = 8;
        private List<Package> solvedPackages = new List<Package>();
        private List<Package> unsolvedPackages = new List<Package>();
        private TextReader input;
        private ColoredTextWriter output;
        private int columnCount;

        public List<Package> Packages { get; set; }
        public string LogFilename { get; set; } = "bootstrap.log";

        public async Task Run(TextReader input, ColoredTextWriter output, int columnCount, bool overrideUserDecision = false)
        {
            this.input = input;
            this.output = output;
            this.columnCount = columnCount;

            if(InitPackageOperation(overrideUserDecision))
            {
                LoadSolvedPackagesFromLog();
                await RunAllPackages();
                LogSolvedPackages();
            }
            else
            {
                // Package installation was cancelled.
                // Nothing to do here.
            }
        }

        private void LoadSolvedPackagesFromLog()
        {
            if(string.IsNullOrWhiteSpace(LogFilename) == false && System.IO.File.Exists(LogFilename))
            {
                var finishedPackages = System.IO.File.ReadAllLines(LogFilename).Where(l => string.IsNullOrWhiteSpace(l) == false);
                this.Packages.RemoveAll(p => finishedPackages.Contains(p.Name));

                output.WriteLine("The following packages have already been finished:");
                foreach(var package in finishedPackages)
                    output.WriteLine($" * {package}");
                output.WriteLine($"If you want to repeat these steps please delete the '{LogFilename}' file.");
            }
        }

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
                if(package.Description.Length > remainingWidth && remainingWidth - 3 > 0)
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

        private async Task RunAllPackages()
        {
            while(this.Packages.Count() != 0)
            {
                var solved = new List<Package>();
                var unsolved = new List<Package>();

                var solvablePackages = this.Packages
                                           .Where(p => ValidateRequirementsMet(p))
                                           .ToList();

                foreach(var package in solvablePackages)
                {
                    try{
                        await package.Run(output);

                        // Add the package to the solved dependencies so that the depending packages can be run.
                        //
                        output.WriteLine($"Installed '{package.Name}' succesfully.");
                        solved.Add(package);
                        this.Packages.Remove(package);
                    }
                    catch(Exception ex)
                    {
                        output.WriteLine($"Encountered an {ex.GetType().Name} with: {ex.Message}");
                        unsolved.Add(package);
                        this.Packages.Remove(package);
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

        private bool ValidateRequirementsMet(Package p)
        {
            return p.Requires.Except(this.solvedPackages.Select(d => d.Name)).Count() == 0;
        }
    }
}