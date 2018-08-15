using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cootstrap.Helpers;

namespace Cootstrap.Modules
{
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

        /// <summary>
        /// Can be overriden to supply arguments back to the package. Default implementation yields an empty dictionary.
        /// </summary>
        protected virtual IDictionary<string, string> ReturnVariables()
        {
            return new Dictionary<string, string>();
        }
    }
}
