using System;
using SharpStrap.Helpers;

namespace Tests.Helpers
{
    public class DummyTextFileInput : ITextFileInput
    {
        private string[] content;

        public DummyTextFileInput(string [] content)
        {
            this.content = content;
        }

        public string[] ReadAllLines(string filename)
        {
            return content;
        }

        public string ReadAllText(string filename)
        {
            return String.Join(Environment.NewLine, content);
        }
    }
}