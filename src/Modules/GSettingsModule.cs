using System;
using System.Collections.Generic;
using SharpStrap.Helpers;

namespace SharpStrap.Modules
{
    public abstract class GSettingsModule : ShellModule
    {
        protected const string GSettingsCommand = "gsettings";

        /// <summary>
        /// Schema of the setting.
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// Key of the setting.
        /// </summary>
        public string Key { get; set; }
        public abstract string Action { get; }
    }

    public class GSettingsSetModule : GSettingsModule
    {
        /// <summary>
        /// Value to set. Will be written to the settings using double quotes.
        /// </summary>
        /// <value></value>
        public string Value { get; set; }
        public override string Action => "set";

        public GSettingsSetModule()
        {
            //
        }

        public GSettingsSetModule(string key, string value)
            : this()
        {
            //
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            SetCommandAndArguments(GSettingsCommand, $"{Action} {Key} \"{Value}\"");
        }
    }
}