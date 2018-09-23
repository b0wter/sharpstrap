using System;

namespace SharpStrap.Modules
{
    /// <summary>
    /// Downloads remote files and saves it locally.
    /// </summary>
    public class DownloadModule : ShellModule
    {
        private const string DownloadCommand = "wget";
        private const string TargetFileNameArgument = "-O";

        /// <summary>
        /// Url to download the file from.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Filename for the download. If not set the remote filename will be used instead.
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// Custom user agent.
        /// </summary>
        public string UserAgent { get; set; }

        public DownloadModule()
        {
            //
        }

        public DownloadModule(string url, string targetFilename = null)
            : this()
        {
            this.Url = url;
            this.Target = targetFilename;
        }

        protected override void PreExecution(System.Collections.Generic.IDictionary<string, string> variables, Helpers.ColoredTextWriter output)
        {
            SetCommandAndArguments(DownloadCommand, CreateArgument());
        }

        private string CreateArgument()
        {
            string args = string.Empty;;
            if(string.IsNullOrWhiteSpace(this.UserAgent))
                args = $"--user-agent=\"{this.UserAgent}\"";

            if(string.IsNullOrWhiteSpace(this.Target))
                return $"{args} {Url}";
            else
                return $"{args} {TargetFileNameArgument} {Target} {Url}";
        }
    }
}