using System;

namespace SharpStrap.Modules
{
    /// <summary>
    /// Fallback module for commands that are not yet implemented or too rare for wrrant an implementation.
    /// Set the command and arguments using the <see cref="ShellModule.Command"/> and <see cref="ShellModule.Argument"/> property.
    /// </summary>
    public class GenericShellModule : ShellModule
    {
        public GenericShellModule()
        {
            //
        }

        public GenericShellModule(string command, string argument) 
            : base(command, argument)
        {
            //
        }

        protected override void PreExecution(System.Collections.Generic.IDictionary<string, string> variables, Helpers.ColoredTextWriter output)
        {
            if(string.IsNullOrWhiteSpace(this.Command))
                throw new InvalidOperationException("Cannot run a shell command with a command.");
        }
    }
}