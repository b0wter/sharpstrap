using System;
using System.Collections.Generic;
using System.Linq;
using Cootstrap.Helpers;

namespace Cootstrap.Modules
{
    public class ShellEvaluateModule : ShellModule
    {
        protected override bool RedirectStandardOutput => true;
        protected override bool SkipVariableReplacement => false;

        /// <summary>
        /// Name of the variable the contents will be stored in.
        /// </summary>
        /// <value></value>
        public string VariableName { get; set; }
        /// <summary>
        /// Gets/sets wether only the last line of the output is used as return value.
        /// </summary>
        /// <value></value>
        public bool LastLineOnly { get; set; } = true;
        /// <summary>
        /// Gets/sets wether empty lines at the end of the output will be trimmed.
        /// </summary>
        /// <value></value>
        public bool TrimEmpty { get; set; } = true;

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            if(string.IsNullOrWhiteSpace(this.VariableName))
                throw new InvalidOperationException("Cannot run ReadVariableModule without a variable name.");
        }

        protected override IDictionary<string, string> ReturnVariables()
        {
            var dict = new Dictionary<string, string>(1);

            if(this.Output == null || this.Output.Count == 0)
                throw new ShellCommandException(new List<string>(1), $"{nameof(ShellEvaluateModule)} has no output.");

            while(string.IsNullOrWhiteSpace(this.Output.Last()) && this.TrimEmpty)
                this.Output = this.Output.Reverse().Skip(1).Reverse().ToList();

            if(LastLineOnly)
            {
                dict.Add(this.VariableName, this.Output.Reverse().Skip(TrimEmpty ? 0 : 1).First());
            }
            else
            {
                var squashedOutput = string.Join(Environment.NewLine, this.Output);
                dict.Add(this.VariableName, squashedOutput);
            }

            return dict;
        }
    }
}