using System;

namespace Cootstrap.Modules
{
    public abstract class GitModule : ShellModule
    {
        private const string GitCommand = "git";

        protected override void PreExecution(System.Collections.Generic.IDictionary<string, string> variables, Helpers.ColoredTextWriter output)
        {
            SetCommandAndArguments(GitCommand, CreateArgument());
        }

        protected abstract string CreateArgument();
    }

    /// <summary>
    /// Module to clone git repositories.
    /// </summary>
    public class GitCloneModule : GitModule
    {
        private const string SubCommand = "clone";

        /// <summary>
        /// Repository url.
        /// </summary>
        /// <value></value>
        public string Url { get; set; }
        /// <summary>
        /// Folder to clone into. Will be cloned to working directory in case it's empty.
        /// </summary>
        /// <value></value>
        public string Target { get; set; }

        public GitCloneModule()
        {
            //
        }

        public GitCloneModule(string url)
        {
            this.Url = url;
        }

        protected override string CreateArgument()
        {
            return SubCommand + " " + this.Url + " " + this.Target;
        }
    }
}