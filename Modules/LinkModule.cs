using System;

namespace Cootstrap.Modules
{
    public class LinkModule : ShellModule
    {
        private const string LinkCommand = "ln";
        private const string LinkArgument = "-s";

        private bool createSymLink = true;
        private string source;
        private string target;

        public LinkModule(string source, string target, bool createSymLink = true) 
            : base(LinkCommand, LinkArgument)
        {
            this.createSymLink = createSymLink;
            this.source = source;
            this.target = target;
        }
    }
}