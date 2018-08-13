using System;

namespace Cootstrap.Modules
{
    public class FolderCreationModule : ShellModule
    {
        private const string FolderCommand = "mkdir";
        private const string FolderArgument = "-p";

        public FolderCreationModule(params string[] folderNames)
            : base(FolderCommand, FolderArgument)
        {
            if(folderNames.Length == 0)
                throw new InvalidOperationException("At least one folder name must be supplied to create this module.");
            this.Arguments += " " + string.Join(" ", folderNames);
        }
    }

    public class FolderRemovalModule : ShellModule
    {
        private const string FolderCommand = "rmdir";
        private const string FolderArgument = "-p";

        private bool removeParents = false;

        public FolderRemovalModule(params string[] folderNames)
            : base(FolderCommand, FolderArgument)
        {
            if(folderNames.Length == 0)
                throw new InvalidOperationException("At least one folder name must be supplied to create this module.");
            this.Arguments += " " + string.Join(" ", folderNames);
        }

        public FolderRemovalModule(bool removeParents, params string[] folderNames)
            : this(folderNames)
        {
            this.removeParents = removeParents;
        }
    }
}