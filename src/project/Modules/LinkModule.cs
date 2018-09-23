using System;

namespace SharpStrap.Modules
{
    /// <summary>
    /// Creates a filesystem link.
    /// </summary>
    public class LinkModule : ShellModule
    {
        private const string LinkCommand = "ln";
        private const string UseSymbolicLinkArgument = "-s";

        /// <summary>
        /// Create a symbolic link.
        /// </summary>
        public bool UseSymbolicLink { get; set; } = true;
        /// <summary>
        /// File to create a link to.
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Name of the link.
        /// </summary>
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

        protected override void PreExecution(System.Collections.Generic.IDictionary<string, string> variables, Helpers.ColoredTextWriter output)
        {
            if(string.IsNullOrWhiteSpace(Source))
                throw new InvalidOperationException("Cannot use LinkModule without a Source.");
            // Target may be null as the source name will be used instead.

            SetCommandAndArguments(LinkCommand, CreateArgument());
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