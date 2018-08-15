using System;
using System.Collections.Generic;
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
                SetCommandAndArguments(ReadCommand, $"{VariableShellPrefix}{VariableName}");
            else
                SetCommandAndArguments(PromptCommand, $"{Prompt} {CommandDelimter} {ReadCommand} {VariableShellPrefix}{VariableName}");
        }

        /*
        public override Task<ModuleResult> Run(IDictionary<string, string> variables, ColoredTextWriter output)
        {   
            if(string.IsNullOrWhiteSpace(Prompt) == false)
                output.WriteLine(Prompt);

            var result = Console.ReadLine();

            return new ModuleResult(ModuleResultStates.Success, new List<string>(), new Dictionary<string, string>() { { this.VariableName,  result } } );
        }
        */
    }
}