using System;

namespace Cootstrap.Modules
{
    public class DownloadModule : ShellModule
    {
        private const string DownloadCommand = "wget";
        private const string TargetFileNameArgument = "-O";


        public string Url { get; set; }
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

        protected override void PrepareForExecution()
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