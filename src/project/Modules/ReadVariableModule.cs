using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpStrap.Helpers;

namespace SharpStrap.Modules
{
    /// <summary>
    /// Read a single variable from the command prompt (user input).
    /// </summary>
    public class ReadVariableModule : ShellModule
    {
        private const string ReadCommand = "read";
        private const string PromptCommand = "echo";
        private const string VariableShellPrefix = "$";
        private const string CommandDelimter = ";";

        protected override bool RedirectStandardOutput => true;
        protected override bool SkipVariableReplacement => true;

        /// <summary>
        /// Name of the new variable.
        /// </summary>
        public string VariableName { get; set; }

        public ReadVariableModule()
        {
            //
        }

        public ReadVariableModule(string variableName)
            : this()
        {
            this.VariableName = variableName;
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            if(string.IsNullOrWhiteSpace(this.VariableName))
                throw new InvalidOperationException("Cannot run ReadVariableModule without a variable name.");

            SetCommandAndArguments(ReadCommand, $"{VariableName} {CommandDelimter} {PromptCommand} {VariableShellPrefix}{VariableName}");
        }

        protected override IDictionary<string, string> ReturnVariables()
        {
            // Since read is ended by hitting return an additional empty line will be added at the end!
            return new Dictionary<string, string>() {
                {
                    this.VariableName,
                    this.Output.Reverse().Skip(1).First()
                }
            };
        }
    }
}