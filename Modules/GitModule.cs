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

        public string Url { get; set; }

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
            return SubCommand + " " + this.Url;
        }
    }
}