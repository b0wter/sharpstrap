using System;

namespace Cootstrap.Modules
{
    public class PromptModule : ShellModule
    {
        private const string PromptCommand = "echo";

        public string Text { get; set; }
        public string Color { get; set; } = "White";

        protected override void PrepareForExecution()
        {
            SetCommandAndArguments(PromptCommand, Text);
        }

        protected override void PreExecution(System.Collections.Generic.IDictionary<string, string> variables, Helpers.ColoredTextWriter output)
        {
            var color = ColorNameToConsoleColor(this.Color);
            output.SetForegroundColor(color);
        }

        private ConsoleColor ColorNameToConsoleColor(string colorName)
        {
            switch(colorName.ToLower())
            {
                case "white":
                    return ConsoleColor.White;
                case "black":
                    return ConsoleColor.Black;
                case "blue":
                    return ConsoleColor.Blue;
                case "cyan":
                    return ConsoleColor.Cyan;
                case "gray":
                    return ConsoleColor.Gray;
                case "green":
                    return ConsoleColor.Green;
                case "magenta":
                    return ConsoleColor.Magenta;
                case "red":
                    return ConsoleColor.Red;
                case "yellow":
                    return ConsoleColor.Yellow;
                default:
                    throw new ArgumentException($"The color '{this.Color}' is unknown.");
            }
        }

        protected override void PostExecution(System.Collections.Generic.IDictionary<string, string> variables, Helpers.ColoredTextWriter output)
        {
            output.ResetColors();
        }
    }
}