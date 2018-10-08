using System;
using System.Collections.Generic;
using System.Linq;
using SharpStrap.Modules;

namespace SharpStrap.Helpers
{
    public interface IPackageInformationPrinter
    {
        /// <summary>
        /// Prints a short summary of packages that have been run previously and will be run now.
        /// </summary>
        /// <param name="previouslyRunPackages"></param>
        /// <param name="packagesToRun"></param>
        void PrintPackageSummary(IEnumerable<Package> previouslyRunPackages, IEnumerable<Package> packagesToRun);
        
        /// <summary>
        /// Prints the name, description and module count for each package.
        /// </summary>
        /// <param name="packages"></param>
        void PrintDetailedPackageSummary(IEnumerable<Package> packages);

        /// <summary>
        /// Prints a result list (name & state).
        /// </summary>
        /// <param name="previouslyRun"></param>
        /// <param name="solved"></param>
        /// <param name="unsolved"></param>
        void PrintResults(IEnumerable<Package> previouslyRun, IEnumerable<Package> solved,
            IEnumerable<Package> unsolved);
    }

    public class PackageInformationPrinter : IPackageInformationPrinter
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
        
        private IIODefinition ioDefinition;
        
        public PackageInformationPrinter(IIODefinition ioDefinition)
        {
            this.ioDefinition = ioDefinition;
        }

        /// <summary>
        /// Prints a human readable summary of all packages and their details.
        /// </summary>
        /// <param name="packages"></param>
        public void PrintDetailedPackageSummary(IEnumerable<Package> packages)
        {
            int noOfPackages = packages.Count();
            int noOfModules = packages.Sum(p => p.Modules.Count());
            this.ioDefinition.TextWriter.WriteLine($"This bootstrap configuration contains {noOfPackages} Packages with a total of {noOfModules} operations.");
            this.ioDefinition.TextWriter.WriteLine();
            
            this.ioDefinition.TextWriter.WriteLine($"{"NAME".PadRight(PackageNameWidth)} {"OPS".PadRight(PackageModuleCountWidth)} {"CRITICAL".PadRight(PackageIsCriticalWidth)} DESCRIPTION");
            this.ioDefinition.TextWriter.WriteLine(new String('=', this.ioDefinition.ColumnWidth));

            int remainingWidth = this.ioDefinition.ColumnWidth - PackageNameWidth - PackageModuleCountWidth - PackageIsCriticalWidth - 3;

            foreach(var package in packages)
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
        }
        
        /// <summary>
        /// Prints a short summary of packages that have been run previously and will be run now.
        /// </summary>
        /// <param name="previouslyRunPackages"></param>
        /// <param name="packagesToRun"></param>
        public void PrintPackageSummary(IEnumerable<Package> previouslyRunPackages, IEnumerable<Package> packagesToRun)
        {
            this.ioDefinition.TextWriter.WriteLine($"The following packages have been finished previously and will not be run:");
            foreach(var package in previouslyRunPackages)
                this.ioDefinition.TextWriter.WriteLine(package.Name);

            this.ioDefinition.TextWriter.WriteLine($"The following packages will be run:");
            foreach(var package in packagesToRun)
                this.ioDefinition.TextWriter.WriteLine(package.Name);
        }
        
        /// <summary>
        /// Prints a short summary of each package and its result (success, failure, previously run).
        /// </summary>
        /// <param name="previouslyRun"></param>
        /// <param name="solved"></param>
        /// <param name="unsolved"></param>
        public void PrintResults(IEnumerable<Package> previouslyRun, IEnumerable<Package> solved, IEnumerable<Package> unsolved)
        {
            this.ioDefinition.TextWriter.WriteLine();
            PrintResultSummary(previouslyRun, solved, unsolved);
            PrintResultHeader();
            PrintResultsFor(previouslyRun.Select(l => l.Name), "PREV");
            PrintResultsFor(solved.Select(l => l.Name)       , "SUCCESS", ConsoleColor.Green);
            PrintResultsFor(unsolved.Select(l => l.Name)     , "FAILED", ConsoleColor.Red);
            this.ioDefinition.TextWriter.ResetColors();
        }

        private void PrintResultSummary(IEnumerable<Package> previouslyRun, IEnumerable<Package> solved, IEnumerable<Package> unsolved)
        {
            this.ioDefinition.TextWriter.WriteLine($"{unsolved.Count()} packages have not been run due to errors or unmet requirements.");
            this.ioDefinition.TextWriter.WriteLine($"{solved.Count()} packages have been run successfully.");
            this.ioDefinition.TextWriter.WriteLine($"{previouslyRun.Count()} packages have been run previously and will not be run again.");
        }

        private void PrintResultHeader()
        {
            this.ioDefinition.TextWriter.WriteLine();
            this.ioDefinition.TextWriter.WriteLine($"{"NAME".PadRight(PackageNameWidth)} {"RESULT".PadRight(PackageModuleCountWidth)}");
            this.ioDefinition.TextWriter.WriteLine(new String('=', this.ioDefinition.ColumnWidth));
        }

        private void PrintResultsFor(IEnumerable<string> packageNames, string status, ConsoleColor resultColor = ConsoleColor.White)
        {
            const int columnWidth = 7;
            string paddedStatus = status.PadRight(columnWidth).ToUpper();
            if(paddedStatus.Length > columnWidth)
                paddedStatus = paddedStatus.Substring(0, columnWidth - 3) + "...";

            foreach(var name in packageNames)
            {
                var packageName = string.IsNullOrWhiteSpace(name) ? "no name?" : name;
                var paddedPackageName = packageName.PadRight(PackageNameWidth);
                this.ioDefinition.TextWriter.WriteLine($"{paddedPackageName} {paddedStatus}");
            }
        }
    }
}