using System;

namespace Cootstrap.Modules
{
    public abstract class DownloadModule : ShellModule
    {
        private const string DownloadCommand = "wget";
        private const string DownloadArgument = "";
        private string url;

        public DownloadModule(string url, string targetFilename = null)
            : base(DownloadCommand, DownloadArgument)
        {
            if(string.IsNullOrWhiteSpace(targetFilename))
            {
                this.Arguments += $"{url}";
            } 
            else
            {
                this.Arguments += $"-O {targetFilename} {url}";
            }
        }

    }
}