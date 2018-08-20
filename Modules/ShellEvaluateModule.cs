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

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            if(string.IsNullOrWhiteSpace(this.VariableName))
                throw new InvalidOperationException("Cannot run ReadVariableModule without a variable name.");
        }

        protected override IDictionary<string, string> ReturnVariables()
        {
            var dict = new Dictionary<string, string>(1);

            if(LastLineOnly)
            {
                var squashedOutput = string.Join(Environment.NewLine, this.Output);
                dict.Add(this.VariableName, squashedOutput);
            }
            else
            {
                dict.Add(this.VariableName, this.Output.Last());
            }

            return dict;
        }
    }
}