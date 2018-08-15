using System;
using System.Collections.Generic;

namespace Cootstrap.Helpers
{
    public class ShellCommandException : Exception
    {
        public IEnumerable<string> Output { get; private set; }

        public ShellCommandException(IEnumerable<string> output)
        {
            this.Output = output;
        }

        public ShellCommandException(IEnumerable<string> output, string message)
            : base(message)
        {
            this.Output = output;
        }
    }
}