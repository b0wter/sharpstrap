using System;
using System.Collections.Generic;

namespace SharpStrap.Modules
{
    public enum ModuleResultStates
    {
        Success = 0,
        Error = 1
    }

    public class ModuleResult
    {
        public ModuleResultStates State { get; private set; }
        public IEnumerable<string> Output { get; private set; } = new List<string>();
        public IDictionary<string, string> Variables { get; private set; } = new Dictionary<string, string>();
        public string CommandRun { get; private set; }

        public ModuleResult(ModuleResultStates state, IEnumerable<string> output, string commandRun)
        {
            this.Output = output;
            this.State = state;
            this.CommandRun = commandRun;
        }

        public ModuleResult(ModuleResultStates state, IEnumerable<string> output, string commandRun, IDictionary<string, string> variables)
            : this(state, output, commandRun)
        {
            this.Variables = variables;
        }
    }

}