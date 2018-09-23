using System;
using System.Text;
using SharpStrap.Helpers;

namespace Tests.Helpers
{
    public class LogFileOutputWriter : ColoredTextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void ResetColors()
        {
            //
        }

        public override void SetBackgroundColor(ConsoleColor color)
        {
            //
        }

        public override void SetForegroundColor(ConsoleColor color)
        {
            //
        }
    }
}