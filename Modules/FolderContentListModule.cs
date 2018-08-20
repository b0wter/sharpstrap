using System;

namespace Cootstrap.Modules
{
    public class FolderContentListModule : ShellModule
    {
        private const string ListCommand = "ls -la";

        public string Path { get; set; }

        public FolderContentListModule()
        {
            //
        }

        public FolderContentListModule(string path)
        {
            this.Path = path;
        }

        protected override void PreExecution(System.Collections.Generic.IDictionary<string, string> variables, Helpers.ColoredTextWriter output)
        {
            SetCommandAndArguments(ListCommand, Path);
        }
    }
}