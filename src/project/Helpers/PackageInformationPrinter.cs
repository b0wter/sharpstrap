using System;
using System.Collections.Generic;
using SharpStrap.Modules;

namespace SharpStrap.Helpers
{
    public interface IPackageInformationPrinter
    {
        void PrintPackageSummary(IEnumerable<Package> previouslyRunPackages, IEnumerable<Package> packagesToRun);
    }

    public class PackageInformationPrinter : IPackageInformationPrinter
    {
        private IIODefinition ioDefinition;
        
        public PackageInformationPrinter(IIODefinition ioDefinition)
        {
            this.ioDefinition = ioDefinition;
        }

        public void PrintPackageSummary(IEnumerable<Package> previouslyRunPackages, IEnumerable<Package> packagesToRun)
        {
            this.ioDefinition.TextWriter.WriteLine($"The following packages have been finished previously and will not be run:");
            foreach(var package in previouslyRunPackages)
                this.ioDefinition.TextWriter.WriteLine(package.Name);

            this.ioDefinition.TextWriter.WriteLine($"The following packages will be run:");
            foreach(var package in packagesToRun)
                this.ioDefinition.TextWriter.WriteLine(package.Name);
        }
    }
}