using System.Diagnostics;
using System.Threading.Tasks;

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
        public abstract Task Run();
    }
}
