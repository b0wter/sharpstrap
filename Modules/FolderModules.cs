using System;
using System.Collections.Generic;
using System.Linq;

namespace Cootstrap.Modules
{
    public abstract class FolderModule : ShellModule
    {
        protected const string WorkOnParentsArguments = "-p";

        protected abstract string FolderCommand { get; }
        public IEnumerable<string> Foldernames { get; set; }
        public bool WorkOnParents { get; set; }

        public FolderModule()
        {
            //
        }

        public FolderModule(params string[] folderNames)
        {
            this.Foldernames = folderNames;
        }

        public FolderModule(bool workOnParents, params string[] folderNames)
            : this(folderNames)
        {
            this.WorkOnParents = workOnParents;
        }

        protected override void PreExecution(IDictionary<string, string> variables, Helpers.ColoredTextWriter output)
        {
            ThrowIfNoFolderSet();
            SetCommandAndArguments(FolderCommand, CreateArgument());
        }

        protected void ThrowIfNoFolderSet()
        {
            if(this.Foldernames.Count() == 0)
                throw new InvalidOperationException("At least one folder name must be supplied to create this module.");
        }

        protected string CreateArgument()
        {
            if(this.WorkOnParents)
                return $"{WorkOnParentsArguments} {string.Join(" ", this.Foldernames)}";
            else
                return $"{string.Join(" ", this.Foldernames)}";
        }
    }

    public class FolderCreationModule : FolderModule
    {
        protected override string FolderCommand => "mkdir";
    }

    public class FolderRemovalModule : FolderModule
    {
        protected override string FolderCommand => "rmdir";
    }
}