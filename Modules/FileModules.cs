using System;
using System.Collections.Generic;

namespace Cootstrap.Modules
{
    public abstract class FileModule : ShellModule
    {
        protected abstract string FileOperation { get; }

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

        protected override void PrepareForExecution()
        {
            var argument = $"{FileOperation} {GetSpecialArguments()} {string.Join(" ", this.Filenames)}";
        }

        protected abstract string GetSpecialArguments();
    }

    public class FileRemovalModule : FileModule
    {
        private const string ForceArgument = "-f";
        private const string RecursiveArgument = "-r";

        protected override string FileOperation => "rm";
        
        public bool Force{ get; set; } 
        public bool Recursive { get; set; }

        protected override string GetSpecialArguments()
        {
            var force = Force ? ForceArgument : "";
            var recursive = Recursive ? RecursiveArgument : "";

            return $"{force} {recursive}";
        }
    }
}