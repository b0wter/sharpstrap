using System;
using System.IO;
using System.Linq;
using SharpStrap.Modules;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SharpStrap.Helpers
{
    internal class BootstrapDeserializer
    {
        private readonly IModuleFinder moduleFinder;

        public BootstrapDeserializer(IModuleFinder moduleFinder)
        {
            this.moduleFinder = moduleFinder;
        }

        public bool Validate(string filename)
        {
            if(System.IO.File.Exists(filename) == false)
            {
                Console.WriteLine($"The given file does not exist.");
                return false;
            }

            var deserializer = CreateDefaultDeserializer();
            try
            {
                using(var reader = File.OpenText(filename))
                {
                    var bootstrap = deserializer.Deserialize<Bootstrap>(reader);
                    Console.WriteLine("File parsed succesfully.");
                }
            }
            catch(YamlException ex)
            {
                Console.WriteLine($"There is at least one error in the file:");
                Console.WriteLine($"{ex.Message}");
                if(ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine("Config file:");
                Console.WriteLine(" ...  |");


                const int marginLines = 3;
                var startLine = Math.Max(0, ex.Start.Line - marginLines);
                var numberOfRelevantLines = ex.End.Line - ex.Start.Line + 2 * marginLines + 1;
                var relevantConfigFileContent = File.ReadAllLines(filename).Skip(startLine).Take(numberOfRelevantLines).ToArray();

                for(int i = 0; i < numberOfRelevantLines; ++i)
                {
                    var paddedLineNumber = (i + startLine).ToString().PadLeft(4);
                    var currentLine = i + startLine;
                    if(currentLine >= ex.Start.Line && currentLine <= ex.End.Line)
                    {
                        Console.Out.Flush();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"{paddedLineNumber} >|{relevantConfigFileContent[i]}");
                    }
                    else
                    {
                        Console.Out.Flush();
                        Console.ResetColor();
                        Console.WriteLine($"{paddedLineNumber}  |{relevantConfigFileContent[i]}");
                    }
                }

                Console.WriteLine(" ...  |");
                return false;
            }

            return true;
        }

        internal Deserializer CreateDefaultDeserializer()
        {
            var mappings = this.moduleFinder.GetAllModulesForModulesNamespace();

            var builder = new DeserializerBuilder();
            foreach(var mapping in mappings)
            {
                System.Diagnostics.Debug.WriteLine($"Added custom tag for yaml mapping: {mapping.Name} - {mapping.Type.Name}");
                builder = builder.WithTagMapping(mapping.Name, mapping.Type);
            }
            
            return builder.Build();
        }
    }
}