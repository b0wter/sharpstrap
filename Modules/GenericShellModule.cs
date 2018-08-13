using System;

namespace Cootstrap.Modules
{
    /// <summary>
    /// Fallback module for commands that are not yet implemented or too rare for wrrant an implementation.
    /// </summary>
    public class GenericShellModule : ShellModule
    {
        public GenericShellModule(string command, string argument) : base(command, argument)
        {
            //
        }
    }
}