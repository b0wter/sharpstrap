using System;

namespace SharpStrap.Modules
{
    /// <summary>
    /// Base module for git operations.
    /// </summary>
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
    /// Clones a git repository.
    /// </summary>
    public class GitCloneModule : GitModule
    {
        private const string SubCommand = "clone";

        /// <summary>
        /// Repository url.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Folder to clone into. Will be cloned to working directory using the repository name in case it's empty.
        /// </summary>
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