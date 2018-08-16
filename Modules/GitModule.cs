using System;

namespace Cootstrap.Modules
{
    public abstract class GitModule : ShellModule
    {
        private const string GitCommand = "git";

        protected override void PrepareForExecution()
        {
            SetCommandAndArguments(GitCommand, CreateArgument());
        }

        protected abstract string CreateArgument();
    }

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