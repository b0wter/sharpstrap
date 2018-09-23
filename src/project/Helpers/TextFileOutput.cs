using System;
using System.Collections.Generic;

namespace SharpStrap.Helpers
{
    public interface ITextFileOutput
    {
        void WriteAllText(string path, string text);
        void WriteAllLines(string path, IEnumerable<string> lines);
    }

    public class FrameworkTextFileOutput : ITextFileOutput
    {
        public void WriteAllLines(string path, IEnumerable<string> lines)
        {
            System.IO.File.WriteAllLines(path, lines);
        }

        public void WriteAllText(string path, string text)
        {
            System.IO.File.WriteAllText(path, text);
        }
    }
}