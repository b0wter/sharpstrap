using System;
using System.Collections.Generic;
using SharpStrap.Helpers;

namespace SharpStrap.Modules
{
    /// <summary>
    /// Base module to interact with the gnome config manager.
    /// </summary>
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

    /// <summary>
    /// Writes a dconf setting.
    /// </summary>
    public class GSettingsSetModule : GSettingsModule
    {
        /// <summary>
        /// Value to set. Will be written to the settings using double quotes.
        /// </summary>
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