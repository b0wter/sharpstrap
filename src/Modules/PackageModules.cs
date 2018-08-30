using System;
using System.Collections.Generic;
using SharpStrap.Helpers;

namespace SharpStrap.Modules
{
    public abstract class PackageBaseModule : ShellModule
    {
        protected const string PackageManagerCommand = "dnf";
    }

    public abstract class PackageWithPackageNameBaseModule : PackageBaseModule
    {
        /// <summary>
        /// List of package names to work with.
        /// </summary>
        /// <value></value>
        public IList<string> PackageNames { get; set; } = new List<string>();
        /// <summary>
        /// File to load the PackageNames from.
        /// </summary>
        /// <value></value>
        public string SourceFile { get; set; }

        protected void AddPackagesFromFile(IDictionary<string, string> variables)
        {
            if(SourceFile.IsNullOrWhiteSpace())
                return;

            var filename = ReplaceVariablesInString(this.SourceFile, variables);

            if(PackageNames != null)
                PackageNames = new List<string>();

            if(System.IO.File.Exists(filename) == false)
                throw new ArgumentException($"The source file '{filename}' for a '{this.GetType().Name}' does not exist.");

            var names = System.IO.File.ReadAllLines(filename);
            foreach(var name in names)
                PackageNames.Add(name);
        }
    }

    /// <summary>
    /// Updates currently installed packages.
    /// </summary>
    public class PackageUpdateModule : PackageBaseModule
    {
        private const string PackageManagerArgument = "update -y";

        public PackageUpdateModule() 
        {
            this.RequiresElevation = true;
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            SetCommandAndArguments(PackageManagerCommand, PackageManagerArgument);
        }
    }

    /// <summary>
    /// Installs a set of packages.
    /// </summary>
    public class PackageInstallModule : PackageWithPackageNameBaseModule
    {
        private const string PackageManagerArgument = "install -y";


        public PackageInstallModule()
            : base()
        {
            //
        }

        public PackageInstallModule(params string[] packageNames) 
            : this()
        {
            this.RequiresElevation = true;
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            AddPackagesFromFile(variables);            
            this.SetCommandAndArguments(PackageManagerCommand, CreateArgument()); 
        }

        private string CreateArgument()
        {
            return $"{PackageManagerArgument} {(string.Join(" ", this.PackageNames))}";
        }
    }

    /// <summary>
    /// Removes a set of packages.
    /// </summary>
    public class PackageRemovalModule : PackageWithPackageNameBaseModule
    {
        private const string PackageManagerArgument = "remove -y";

        public PackageRemovalModule()
            : base()
        {
            //
        }
        
        public PackageRemovalModule(params string[] packageNames) 
            : this()
        {
            this.RequiresElevation = true;
            this.Arguments += " " + string.Join(" ", packageNames);
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            AddPackagesFromFile(variables);            
            this.SetCommandAndArguments(PackageManagerCommand, CreateArgument()); 
        }

        private string CreateArgument()
        {
            return $"{PackageManagerArgument} {(string.Join(" ", this.PackageNames))}";
        }
    }

    /// <summary>
    /// Imports a repository using the package manager (dnf).
    /// </summary>
    public class PackageImportModule : PackageBaseModule
    {
        private const string PackageManagerArgument = "config-manager --add-repo";

        public string Url { get; set; }

        public PackageImportModule()
            : base()
        {
            //
        }

        public PackageImportModule(string url)
            : this()
        {
            this.RequiresElevation = true;
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            SetCommandAndArguments(PackageManagerCommand, CreateArgument());
        }

        private string CreateArgument()
        {
            return $"{PackageManagerArgument} {this.Arguments} {this.Url}";
        }
    }

    /// <summary>
    /// Import keys into the local storage.
    /// </summary>
    public class KeyImportModule : ShellModule
    {
        private const string PackageManagerCommand = "rpm";
        private const string PackageManagerArgument = "--import";

        public string Url { get; set; }

        public KeyImportModule()
        {
            //
        }

        public KeyImportModule(string url) 
            : this()
        {
            this.Url = url;
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            if(string.IsNullOrWhiteSpace(this.Url))
                throw new InvalidOperationException("Cannot import an empty url.");

            SetCommandAndArguments(PackageManagerCommand, PackageManagerArgument + " " + this.Url);
        }
    }

    /// <summary>
    /// Import a repository using rpm.
    /// </summary>
    public class RepositoryImportModule : ShellModule
    {
        private const string PackageManagerCommand = "rpm";
        private const string PackageManagerArgument = "-Uvh";

        public string Url { get; set; }

        public RepositoryImportModule()
        {
            //
        }

        public RepositoryImportModule(string url)
            : this()
        {
            this.Url = url;
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            if(string.IsNullOrWhiteSpace(this.Url))
                throw new InvalidOperationException("Cannot import an empty url.");

            SetCommandAndArguments(PackageManagerCommand, PackageManagerArgument + " " + this.Url);
        }
    }
}
