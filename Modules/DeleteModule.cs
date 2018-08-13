using System;

namespace Cootstrap.Modules
{
    public class DeleteModule : ShellModule
    {
        private const string DeleteCommand = "rm";
        private const string DeleteArgument = "";

        private bool removeRecursive = false;
        private bool forceRemoval = false;

        public DeleteModule(params string[] filenames) 
            : base(DeleteCommand, DeleteArgument)
        {
            this.Arguments = string.Join(" ", filenames);
        }

        public DeleteModule(bool force, bool recursive, params string[] filenames)
            : this(filenames)
        {
            this.removeRecursive = recursive;
            this.forceRemoval = force;
        }
    }
}