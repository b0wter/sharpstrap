using System;

namespace Cootstrap.Modules
{
    public class LinkModule : ShellModule
    {
        private const string LinkCommand = "ln";
        private const string UseSymbolicLinkArgument = "-s";

        public bool UseSymbolicLink { get; set; } = true;
        public string Source { get; set; }
        public string Target { get; set; }

        public LinkModule()
        {
            //
        }

        public LinkModule(string source, string target, bool useSymbolicLink = true) 
        {
            this.UseSymbolicLink = useSymbolicLink;
            this.Source = source;
            this.Target = target;
        }

        protected override void PrepareForExecution()
        {
            if(string.IsNullOrWhiteSpace(Source))
                throw new InvalidOperationException("Cannot use LinkModule without a Source.");
            if(System.IO.File.Exists(Source) == false)
                throw new InvalidOperationException("Cannot create link for a source that doesn't exist.");
            // Target can be null as the source name will be used instead.
        }

        private string CreateArgument()
        {
            if(this.UseSymbolicLink)
                return $"{UseSymbolicLinkArgument} {Source} {Target}";
            else
                return $"{Source} {Target}";
        }
    }
}