using System;

namespace Cootstrap.Helpers
{
    public class ShellCommandException : Exception
    {
        public ShellCommandException()
        {
            //
        }

        public ShellCommandException(string message)
            : base(message)
        {
            //
        }
    }
}