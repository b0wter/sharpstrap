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
            PrepareForExecution();

            string elevationPrefix = this.RequiresElevation ? ElevationPrefix : string.Empty;

            var startInfo = new ProcessStartInfo
            {
                FileName = ReplaceVariablesInString(ShellCommand, variables),
                Arguments = ReplaceVariablesInString($"-c \"{elevationPrefix} {Command} {Arguments}\"", variables),
                WorkingDirectory = this.WorkingDirectory,
                RedirectStandardOutput = this.RedirectStandardOutput
            };

            PreExecution(variables, output);
            var result = await RunProcessAsTask(startInfo);
            PostExecution(variables, output);

            return new ModuleResult(
                (result == 0 ? ModuleResultStates.Success : ModuleResultStates.Error),
                this.Output,
                $"{startInfo.FileName} {startInfo.Arguments}",
                ReturnVariables()
            );
        }

        protected virtual void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            // can be overriden in other modules in case they need to do some setup work
        }

        protected virtual void PostExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            // can be overriden in other modules in case they need to clean some work up
        }

        /// <summary>
        /// Gives the module the chance to properly initialize itself before execution.
        /// </summary>
        protected abstract void PrepareForExecution();

        protected virtual string ReplaceVariablesInString(string s, IDictionary<string, string> variables)
        {
            foreach(var pair in variables)
            {
                s = s.Replace($"${pair.Key}", pair.Value);
            }
            return s;
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