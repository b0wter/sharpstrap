using System;
using System.Collections.Generic;
using SharpStrap.Helpers;

namespace Tests.Helpers
{
    public class DummyTextFileOutput : ITextFileOutput
    {
        private Dictionary<string, string> contents = new Dictionary<string, string>();

        public void WriteAllLines(string path, IEnumerable<string> lines)
        {
            if(contents.ContainsKey(path))
                contents.Remove(path);
            contents.Add(path, string.Join(Environment.NewLine, lines));
        }

        public void WriteAllText(string path, string text)
        {
            if(contents.ContainsKey(path))
                contents.Remove(path);
            contents.Add(path, text);
        }
    }
}