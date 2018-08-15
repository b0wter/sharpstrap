using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cootstrap.Helpers;

namespace Cootstrap.Modules
{
    public class ReadVariableModule : ShellModule
    {
        private const string ReadCommand = "read";
        private const string PromptCommand = "echo";
        private const string VariableShellPrefix = "$";
        private const string CommandDelimter = ";";

        public string Prompt { get; set; }
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

        public ReadVariableModule(string variableName, string prompt)
            : this(variableName)
        {
            this.Prompt = prompt;
        }

        protected override void PrepareForExecution()
        {
            if(string.IsNullOrWhiteSpace(this.VariableName))
                throw new InvalidOperationException("Cannot run ReadVariableModule without a variable name.");

            if(string.IsNullOrWhiteSpace(this.Prompt))
                SetCommandAndArguments(ReadCommand, $"{VariableName} {CommandDelimter} {PromptCommand} {VariableShellPrefix}{VariableName}");
            else
                SetCommandAndArguments(PromptCommand, $"{Prompt} {CommandDelimter} {ReadCommand} {VariableName} {CommandDelimter} {PromptCommand} {VariableShellPrefix}{VariableName}");
        }

        protected override IDictionary<string, string> ReturnVariables()
        {
            return new Dictionary<string, string>() {
                {
                    this.VariableName,
                    this.Output.Last()
                }
            };
        }
    }
}