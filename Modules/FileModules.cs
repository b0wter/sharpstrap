using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpStrap.Helpers;

namespace SharpStrap.Modules
{
    public abstract class FileModule : ShellModule
    {
        // TODO: Move the redundant properties into this base class.

        protected abstract string FileOperation { get; }

        /// <summary>
        /// List of files to work on.
        /// </summary>
        public IEnumerable<string> Filenames { get; set; }

        public FileModule()
        {
            //
        }

        public FileModule(IEnumerable<string> filenames)
            : this()
        {
            this.Filenames = filenames;
        }
    }

    /// <summary>
    /// Deletes files or folders.
    /// </summary>
    public class FileRemovalModule : FileModule
    {
        private const string ForceArgument = "-f";
        private const string RecursiveArgument = "-r";

        protected override string FileOperation => "rm";
        
        /// <summary>
        /// Forces the removal of write-protected files.
        /// </summary>
        /// <value></value>
        public bool Force{ get; set; } 
        /// <summary>
        /// If the filename points to a folder it will be deleted recursively.
        /// </summary>
        public bool Recursive { get; set; }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            var argument = GetSpecialArguments() + " " + string.Join(" ", this.Filenames);
            SetCommandAndArguments(FileOperation, argument);
        }

        protected string GetSpecialArguments()
        {
            var force = Force ? ForceArgument : "";
            var recursive = Recursive ? RecursiveArgument : "";

            return $"{force} {recursive}";
        }

    }

    /// <summary>
    /// Copies files or folders.
    /// </summary>
    public class FileCopyModule : FileModule
    {
        private const string ForceArgument = "-f";
        private const string RecursiveArgument = "-r";

        protected override string FileOperation => "cp";

        /// <summary>
        /// Forces the file to copied even if the destination cannot be opened.
        /// </summary>
        public bool Force{ get; set; } 
        /// <summary>
        /// If the filename points to a folder it will be copied recursively.
        /// </summary>
        public bool Recursive { get; set; }
        /// <summary>
        /// Target filename/folder.
        /// </summary>
        public string Target { get; set; }

        public FileCopyModule()
        {
            //
        }

        public FileCopyModule(string target, IEnumerable<string> filenames)
            : base(filenames)
        {
            this.Target = target;
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            var argument = GetSpecialArguments() + " " + string.Join(" ", this.Filenames) + " " + Target;
            SetCommandAndArguments(this.FileOperation, argument);
        }

        protected string GetSpecialArguments()
        {
            var force = Force ? ForceArgument : "";
            var recursive = Recursive ? RecursiveArgument : "";

            return $"{force} {recursive}";
        }
    }

    public class FileMoveModule : FileModule
    {
        private const string ForceArgument = "-f";
        private const string RecursiveArgument = "-r";

        protected override string FileOperation => "mv";

        /// <summary>
        /// Forces the file to copied even if the destination cannot be opened.
        /// </summary>
        public bool Force{ get; set; } 
        /// <summary>
        /// If the filename points to a folder it will be copied recursively.
        /// </summary>
        public bool Recursive { get; set; }
        /// <summary>
        /// Target filename/folder.
        /// </summary>
        public string Target { get; set; }

        public FileMoveModule()
        {
            //
        }

        public FileMoveModule(string target, IEnumerable<string> filenames)
            : base(filenames)
        {
            this.Target = target;
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            var argument = GetSpecialArguments() + " " + string.Join(" ", this.Filenames) + " " + Target;
            SetCommandAndArguments(this.FileOperation, argument);
        }

        protected string GetSpecialArguments()
        {
            var force = Force ? ForceArgument : "";
            var recursive = Recursive ? RecursiveArgument : "";

            return $"{force} {recursive}";
        }
    }
}