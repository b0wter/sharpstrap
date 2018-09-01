using System;

namespace SharpStrap.Modules
{
    /// <summary>
    /// Lists the contents of a folder.
    /// </summary>
    public class FolderContentListModule : ShellModule
    {
        private const string ListCommand = "ls -la";

        /// <summary>
        /// Path of the folder whose contents will be listed.
        /// </summary>
        /// <value></value>
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