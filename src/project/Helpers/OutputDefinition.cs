using System;
using System.IO;

namespace SharpStrap.Helpers
{
    public interface IIODefinition
    {
        ColoredTextWriter TextWriter { get; }
        int ColumnWidth { get; }
        TextReader TextReader { get; }
    }

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