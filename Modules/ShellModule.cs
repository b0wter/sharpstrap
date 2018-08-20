using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cootstrap.Helpers;

namespace Cootstrap.Modules
{
     /// <summary>
    /// Module that runs a shell command.
    /// </summary>
    public abstract class ShellModule : BaseModule
    {
        private const string ElevationPrefix = "sudo";
        private const string ShellCommand = "/usr/bin/bash";

        protected virtual bool RedirectStandardOutput => false;

        public string Command { get; set; }
        public string Arguments { get; set; }
        public bool RequiresElevation { get; set; }
        public IList<string> Output { get; set; } = new List<string>();
        public string WorkingDirectory { get; set; }

        public ShellModule()
        {
            // Empty constructor for deserialization.
        }

        public ShellModule(string command, string argument)
        {
            this.Command = command;
            this.Arguments = argument;
        }

        protected void SetCommandAndArguments(string command, string argument)
        {
            this.Command = command;
            this.Arguments = argument;
        }

        public async override Task<ModuleResult> Run(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            PreExecution(variables, output);

            string elevationPrefix = this.RequiresElevation ? ElevationPrefix : string.Empty;

            string workingDirectory = ReplaceVariablesInString(this.WorkingDirectory, variables);
            if(string.IsNullOrWhiteSpace(workingDirectory) == false && System.IO.Directory.Exists(workingDirectory) == false)
                throw new InvalidOperationException($"The given working directory '{this.WorkingDirectory}' does not exist.");

            var startInfo = new ProcessStartInfo
            {
                FileName = ReplaceVariablesInString(ShellCommand, variables),
                //Arguments = ReplaceVariablesInString($"-c \"{elevationPrefix} {Command} {Arguments}\"", variables),
                Arguments = ReplaceVariablesInString($"-c {CreateProcessCommand()}", variables),
                WorkingDirectory = ReplaceVariablesInString(this.WorkingDirectory, variables),
                RedirectStandardOutput = this.RedirectStandardOutput
            };

            var result = await RunProcessAsTask(startInfo);
            PostExecution(variables, output);

            return new ModuleResult(
                (result == 0 ? ModuleResultStates.Success : ModuleResultStates.Error),
                this.Output,
                $"{startInfo.FileName} {startInfo.Arguments}",
                ReturnVariables()
            );
        }

        protected abstract void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output);

        protected virtual void PostExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            // can be overriden in other modules in case they need to clean some work up
        }

        private string CreateProcessCommand()
        {
            return $"-c \"{ElevationPrefix} {Command} {Arguments}\"";
        }

        protected Task<int> RunProcessAsTask(ProcessStartInfo startInfo)
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

            process.OutputDataReceived += (sender, args) =>
            {
                this.Output.Add(args.Data);
            };

            process.Start();
            if(startInfo.RedirectStandardOutput)
                process.BeginOutputReadLine();
            return tcs.Task;
        }
    }   
}