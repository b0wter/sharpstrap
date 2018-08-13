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
    }

    public class PackageInstallModule : ShellModule
    {
        private const string PackageManagerCommand = "dnf";
        private const string PackageManagerArgument = "install -y";

        public IEnumerable<string> PackageNames { get; set; }

        public PackageInstallModule(params string[] packageNames) 
            : base(PackageManagerCommand, PackageManagerArgument)
        {
            this.RequiresElevation = true;
            this.Arguments += " " + string.Join(" ", packageNames);
            this.PackageNames = packageNames;
        }
    }

    public class PackageRemovalModule : ShellModule
    {
        private const string PackageManagerCommand = "dnf";
        private const string PackageManagerArgument = "remove -y";

        public PackageRemovalModule(params string[] packageNames) 
            : base(PackageManagerCommand, PackageManagerArgument)
        {
            this.RequiresElevation = true;
            this.Arguments += " " + string.Join(" ", packageNames);
        }
    }

    public class PackageImportModule : ShellModule
    {
        private const string PackageManagerCommand = "dnf";
        private const string PackageManagerArgument = "config-manager --add-repo";

        public PackageImportModule(string url)
            : base(PackageManagerCommand, PackageManagerArgument)
        {
            this.Arguments += " " + url;
        }
    }

    public class KeyImportModule : ShellModule
    {
        private const string PackageManagerCommand = "rpm";
        private const string PackageManagerArgument = "--import";

        public KeyImportModule(string url) : base(PackageManagerCommand, PackageManagerArgument)
        {
            this.Arguments += " " + url;
        }
    }
}