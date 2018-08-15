using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cootstrap.Helpers;

namespace Cootstrap.Modules
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

    public abstract class BaseModule
    {
        /// <summary>
        /// Unique Id of this module.
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// Description of this module.
        /// </summary>
        /// <value></value>
        public string Description { get; set; }
        /// <summary>
        /// Performs the action this module is intended to do. Requires previous setup.
        /// </summary>
        public abstract Task<ModuleResult> Run(IDictionary<string, string> variables, ColoredTextWriter output);
        /// <summary>
        /// Gets/sets if this module is allowed to fail.
        /// If it is true the package will continue to be executed.
        /// </summary>
        public bool AllowError { get; set; }
    }
}
