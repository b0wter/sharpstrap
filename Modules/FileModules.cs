using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cootstrap.Helpers;

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
    }

    /*
        Using framework function comes at the price that they cannot be elevated individually.
        So we use shell functions instead.

    public class FileRemovalModule : BaseModule
    {
        public IEnumerable<string> Filenames { get; set; }

        public override Task<ModuleResult> Run(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            return Task<ModuleResult>.Run(() => {

                var filenames = Filenames.Select(f => ReplaceVariablesInString(f, variables));

                foreach(var file in filenames)
                    System.IO.File.Delete(file);

                return new ModuleResult(ModuleResultStates.Success, new List<string>(), string.Empty);
            });
        }
    }
    */

    public class FileRemovalModule : FileModule
    {
        private const string ForceArgument = "-f";
        private const string RecursiveArgument = "-r";

        protected override string FileOperation => "rm";
        
        public bool Force{ get; set; } 
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

    public class FileCopyModule : FileModule
    {
        private const string ForceArgument = "-f";
        private const string RecursiveArgument = "-r";

        protected override string FileOperation => "cp";

        public bool Force{ get; set; } 
        public bool Recursive { get; set; }
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
}