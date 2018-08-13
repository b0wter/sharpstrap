using System;
using System.Collections.Generic;

namespace Cootstrap.Modules
{
    public class Package
    {
        /// <summary>
        /// Name of this package.
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// Description of this package (optional).
        /// </summary>
        /// <value></value>
        public string Description { get; set; }

        /// <summary>
        /// Gets/sets if this package is mission ciritical.
        /// Bootstrapping will stop if this failes.
        /// </summary>
        /// <value></value>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Actual working modules of this package.
        /// </summary>
        public IEnumerable<BaseModule> Modules { get; set; }
    }
}