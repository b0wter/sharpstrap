using System;
using System.IO;
using System.Text;

namespace SharpStrap.Helpers
{
    public abstract class ColoredTextWriter : TextWriter
    {
        public abstract void SetForegroundColor(ConsoleColor color);
        public abstract void SetBackgroundColor(ConsoleColor color);
        public abstract void ResetColors();
    }

    public class ConsoleWriter : ColoredTextWriter
    {
        public override void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public override void WriteLine()
        {
            Console.WriteLine();
        }

        public override Encoding Encoding => Console.OutputEncoding;

        public override void ResetColors()
        {
            Console.Out.Flush();
            Console.ResetColor();
        }

        public override void SetBackgroundColor(ConsoleColor color)
        {
            Console.Out.Flush();
            Console.BackgroundColor = color;
        }

        public override void SetForegroundColor(ConsoleColor color)
        {
            Console.Out.Flush();
            Console.ForegroundColor = color;
        }

        public override void Flush()
        {
            Console.Out.Flush();
        }
    }
}