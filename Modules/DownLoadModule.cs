using System;

namespace Cootstrap.Modules
{
    public abstract class DownloadModule : ShellModule
    {
        private const string DownloadCommand = "wget";
        private const string TargetFileNameArgument = "-O";


        public string Url { get; set; }
        public string TargetFilename { get; set; }

        public DownloadModule(string url, string targetFilename = null)
        {
            this.Url = url;
            this.TargetFilename = targetFilename;
        }

        protected override void PrepareForExecution()
        {
            SetCommandAndArguments(DownloadCommand, CreateArgument());
        }

        private string CreateArgument()
        {
            if(string.IsNullOrWhiteSpace(this.TargetFilename))
                return "";
            else
                return $"{TargetFileNameArgument} {TargetFilename}";
        }
    }
}