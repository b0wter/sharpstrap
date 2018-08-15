using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cootstrap.Helpers;

namespace Cootstrap.Modules
{
    public class Package
    {
        /// <summary>
        /// Name of this package. Needs to be unique.
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
        public IEnumerable<BaseModule> Modules { get; set; } = new List<BaseModule>();

        /// <summary>
        /// Gets/sets wether this package depends on any other package.
        /// </summary>
        public IEnumerable<string> Requires { get; set; } = new List<string>();

        /// <summary>
        /// Stores variables within the scope of this package.
        /// Variables can only be strings as they are read from the command line as strings.
        /// </summary>
        /// <typeparam name="string">Name of the variable.</typeparam>
        /// <typeparam name="string">Content of the variable.</typeparam>
        public IDictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        public async Task Run(ColoredTextWriter output)
        {
            output.WriteLine($"Starting work on '{Name}'");

            foreach(var module in Modules)
            {
                output.WriteLine($"Starting {module.GetType().Name}");
                var result = await module.Run(this.Variables, output);

                if(result.State != ModuleResultStates.Success)
                    throw new ShellCommandException();
                else
                {
                    foreach(var pair in result.Variables)
                        this.Variables.Add(pair.Key, pair.Value);
                }
            }

            output.SetForegroundColor(ConsoleColor.Green);
            output.WriteLine("Finished successfully.");
            output.ResetColors();
        }
    }
}