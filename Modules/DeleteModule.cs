using System;
using System.Collections.Generic;
using System.Text;

namespace Cootstrap.Modules
{
    public class DeleteModule : ShellModule
    {
        private const string DeleteCommand = "rm";
        private const string ForceRemovalArgument = "-f";
        private const string RecursiveRemovalArgument = "-r";

        /// <summary>
        /// If the filename points to a folder it will be deleted recursively.
        /// </summary>
        public bool RemoveRecursive { get; set; } = false;
        /// <summary>
        /// Forces the remove of a write-protected file.
        /// </summary>
        public bool ForceRemoval { get; set; } = false;
        /// <summary>
        /// List of filenames to delete.
        /// </summary>
        public IEnumerable<string> Filenames { get; set; }

        public DeleteModule(params string[] filenames) 
        {
            this.Filenames = filenames;
        }

        public DeleteModule(bool force, bool recursive, params string[] filenames)
            : this(filenames)
        {
            this.RemoveRecursive = recursive;
            this.ForceRemoval = force;
        }

        protected override void PreExecution(IDictionary<string, string> variables, Helpers.ColoredTextWriter output)
        {
            SetCommandAndArguments(DeleteCommand, CreateArgument());
        }

        private string CreateArgument()
        {
            var builder = new StringBuilder();
            
            if(this.ForceRemoval)
                builder.Append(" " + ForceRemovalArgument);

            if(this.RemoveRecursive)
                builder.Append(" " + RecursiveRemovalArgument);

            builder.AppendJoin(" ", this.Filenames);

            return builder.ToString();
        }
    }
}