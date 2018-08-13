using System.Diagnostics;
using System.Threading.Tasks;

namespace Cootstrap.Modules
{
     /// <summary>
    /// Module that runs a shell command.
    /// </summary>
    public abstract class ShellModule : BaseModule
    {
        private const string ElevationPrefix = "sudo";
        private const string ShellCommand = "/usr/bin/bash";

        public string Command { get; set; }
        public string Arguments { get; set; }
        public bool RequiresElevation { get; set; }

        public ShellModule(string command, string argument)
        {
            this.Command = command;
            this.Arguments = argument;
        }

        // TODO: return a meaningful Task with a sucess/error code.
        public async override Task Run()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ShellCommand,
                Arguments = $"-c \"{ElevationPrefix} {Command} {Arguments}\""
            };

            await RunProcessAsTask(startInfo);
        }

        private string CreateProcessCommand()
        {
            return $"-c \"{ElevationPrefix} {Command} {Arguments}\"";
        }

        protected Task RunProcessAsTask(ProcessStartInfo startInfo)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            return tcs.Task;
        }
    }   
}