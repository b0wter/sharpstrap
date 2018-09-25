using System;
using System.IO;

namespace SharpStrap.Helpers
{
    /// <summary>
    /// Defines a set of parameters required to write colored output on a device with a max column count.
    /// </summary>
    public interface IIODefinition
    {
        ColoredTextWriter TextWriter { get; }
        int ColumnWidth { get; }
        TextReader TextReader { get; }
    }

    /// <summary>
    /// Uses the framework's console functions to write colored text to the user's terminal.
    /// </summary>
    public class ConsoleIODefinition : IIODefinition
    {
        public ColoredTextWriter TextWriter { get; private set; }

        public int ColumnWidth => Console.BufferWidth;

        public TextReader TextReader => Console.In;

        public ConsoleIODefinition()
        {
            this.TextWriter = new ConsoleWriter();
        }
    }
}