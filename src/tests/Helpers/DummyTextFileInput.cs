using System;
using SharpStrap.Helpers;

namespace Tests.Helpers
{
    public class DummyTextFileInput : ITextFileInput
    {
        public string[] Content { get; private set; }

        public DummyTextFileInput(string [] content)
        {
            this.Content = content;
        }

        public string[] ReadAllLines(string filename)
        {
            return this.Content;
        }

        public string ReadAllText(string filename)
        {
            return String.Join(Environment.NewLine, this.Content);
        }
    }
}