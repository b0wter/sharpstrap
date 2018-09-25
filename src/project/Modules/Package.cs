using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpStrap.Helpers;

namespace SharpStrap.Modules
{
    public class Package
    {
        /// <summary>
        /// Name of this package. Needs to be unique.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of this package (optional).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/sets if this package is mission ciritical.
        /// Bootstrapping will stop if this fails.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Gets/sets wether this package will be run even it has been previously finished.
        /// </summary>
        public bool IgnoreAlreadySolved { get; set; }

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

        public async Task Run(ColoredTextWriter output, IDictionary<string, string> variables)
        {
            foreach(var variable in variables)
                this.Variables.Add(variable.Key, variable.Value);

            output.SetForegroundColor(ConsoleColor.Yellow);
            output.WriteLine($"Starting work on '{Name}'");
            output.ResetColors();

            foreach(var module in Modules)
            {
                var result = await module.Run(this.Variables, output);

                if(result.State != ModuleResultStates.Success)
                {
                    output.SetForegroundColor(ConsoleColor.DarkYellow);
                    output.WriteLine("Command run:");
                    output.WriteLine(result.CommandRun);
                    output.WriteLine("Output:");
                    output.WriteLine(string.Join(Environment.NewLine, result.Output));
                    output.ResetColors();

                    if(module.AllowError)
                    {
                        output.SetForegroundColor(ConsoleColor.Yellow);
                        output.WriteLine($"{module.GetType().Name} failed! Since it is marked with 'AllowError' the rest of the package will be run.");
                        output.ResetColors();
                    }
                    else
                    {
                        output.SetForegroundColor(ConsoleColor.Red);
                        output.WriteLine($"{module.GetType().Name} failed for package {this.Name}! This package is marked to fail on any module error. Stopping run of this package.");
                        output.ResetColors();
                        throw new ShellCommandException(result.Output);
                    }
                }
                else
                {
                    foreach(var pair in result.Variables)
                    {
                        if(this.Variables.ContainsKey(pair.Key))
                        {
                            output.WriteLine($"Replacing value '{this.Variables[pair.Key]}' of '${pair.Key}' with '{pair.Value}'.");
                            this.Variables[pair.Key] = pair.Value;
                        }
                        else
                        {
                            output.WriteLine($"Adding '{pair.Key}' to variable store.");
                            this.Variables.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }

            output.SetForegroundColor(ConsoleColor.Green);
            output.WriteLine($"Finished '{this.Name}' successfully.");
            output.ResetColors();
        }

        public bool CanRun(IEnumerable<string> finishedPackages)
        {
            return this.Requires.Except(finishedPackages).Any() == false;
        }
    }
}