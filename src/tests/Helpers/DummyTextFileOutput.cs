using System;
using System.Collections.Generic;
using SharpStrap.Helpers;

namespace Tests.Helpers
{
    public class DummyTextFileOutput : ITextFileOutput
    {
        public Dictionary<string, string> Contents { get; private set; } = new Dictionary<string, string>();

        public void WriteAllLines(string path, IEnumerable<string> lines)
        {
            if(Contents.ContainsKey(path))
                Contents.Remove(path);
            Contents.Add(path, string.Join(Environment.NewLine, lines));
        }

        public void WriteAllText(string path, string text)
        {
            if(Contents.ContainsKey(path))
                Contents.Remove(path);
            Contents.Add(path, text);
        }
    }
}