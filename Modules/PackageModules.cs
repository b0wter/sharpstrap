using System;
using System.Collections.Generic;

namespace Cootstrap.Modules
{
    public class PackageUpdateModule : ShellModule
    {
        private const string PackageManagerCommand = "dnf";
        private const string PackageManagerArgument = "update -y";

        public PackageUpdateModule() 
            : base(PackageManagerCommand, PackageManagerArgument)
        {
            this.RequiresElevation = true;
        }

        protected override void PrepareForExecution()
        {
            SetCommandAndArguments(PackageManagerCommand, PackageManagerArgument);
        }
    }

    public class PackageInstallModule : ShellModule
    {
        private const string PackageManagerCommand = "dnf";
        private const string PackageManagerArgument = "install -y";

        public IEnumerable<string> PackageNames { get; set; }

        public PackageInstallModule()
            : base()
        {
            //
        }

        public PackageInstallModule(params string[] packageNames) 
            : base(PackageManagerCommand, PackageManagerArgument)
        {
            this.RequiresElevation = true;
            this.Arguments += " " + string.Join(" ", packageNames);
            this.PackageNames = packageNames;
        }

        protected override void PrepareForExecution()
        {
            this.SetCommandAndArguments(PackageManagerCommand, CreateArgument()); 
        }

        private string CreateArgument()
        {
            return $"{PackageManagerArgument} {(string.Join(" ", this.PackageNames))}";
        }
    }

    public class PackageRemovalModule : ShellModule
    {
        private const string PackageManagerCommand = "dnf";
        private const string PackageManagerArgument = "remove -y";

        public IEnumerable<string> PackageNames { get; set; }

        public PackageRemovalModule()
            : base()
        {
            //
        }
        
        public PackageRemovalModule(params string[] packageNames) 
            : base(PackageManagerCommand, PackageManagerArgument)
        {
            this.RequiresElevation = true;
            this.Arguments += " " + string.Join(" ", packageNames);
        }

        protected override void PrepareForExecution()
        {
            this.SetCommandAndArguments(PackageManagerCommand, CreateArgument()); 
        }

        private string CreateArgument()
        {
            return $"{PackageManagerArgument} {(string.Join(" ", this.PackageNames))}";
        }
    }

    public class PackageImportModule : ShellModule
    {
        private const string PackageManagerCommand = "dnf";
        private const string PackageManagerArgument = "config-manager --add-repo";

        public string Url { get; set; }

        public PackageImportModule()
            : base()
        {
            //
        }

        public PackageImportModule(string url)
            : base(PackageManagerCommand, PackageManagerArgument)
        {
            this.RequiresElevation = true;
            this.Arguments += " " + url;
        }

        protected override void PrepareForExecution()
        {
            SetCommandAndArguments(PackageManagerCommand, CreateArgument());
        }

        private string CreateArgument()
        {
            return $"{this.Arguments} {this.Url}";
        }
    }

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
        {
            this.Url = url;
        }

        protected override void PrepareForExecution()
        {
            if(string.IsNullOrWhiteSpace(this.Url))
                throw new InvalidOperationException("Cannot import an empty url.");

            SetCommandAndArguments(PackageManagerCommand, PackageManagerArgument + " " + this.Url);
        }
    }
}