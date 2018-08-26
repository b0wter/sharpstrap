using System;

namespace Cootstrap.Modules
{
    /// <summary>
    /// Downloads remote files and saves them locally.
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
        /// <value></value>
        public string Target { get; set; }

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
            if(string.IsNullOrWhiteSpace(this.Target))
                return $"{Url}";
            else
                return $"{TargetFileNameArgument} {Target} {Url}";
        }
    }
}