using System;
using System.IO;

namespace SharpStrap.Helpers
{
    /// <summary>
    /// Interface for reading text files.
    /// </summary>
    public interface ITextFileInput
    {
        string ReadAllText(string filename);
        string[] ReadAllLines(string filename);
    }

    /// <summary>
    /// Provides the (text) contents of file using the native framework methods.
    /// </summary>
    public class FrameworkTextFileInput : ITextFileInput
    {
        public string[] ReadAllLines(string filename)
        {
            return File.ReadAllLines(filename);
        }

        public string ReadAllText(string filename)
        {
            return File.ReadAllText(filename);
        }
    }
}