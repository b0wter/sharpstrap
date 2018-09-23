using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpStrap.Helpers;
using SharpStrap.Modules;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SharpStrap
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if(args.Length == 1 && args[0] == "modules")
            {
                PrintAllKnownModules();
                return;
            }

            if(args.Length == 2 && args[0] == "validate")
            {
                ValidateBootstrapConfig(args[1]);
                return;
            }

            if((args.Length != 1 && args.Length != 2) || File.Exists(args.Last()) == false)
            {
                Console.WriteLine("This tool requires at least parameter (config file). You may add the '-y' option before the filename to autorun the bootstrap.");
                return;
            }

            bool overrideUserDecision = args.First() == "-y";

            if(ValidateBootstrapConfig(args.Last()) == false)
                return;

            using(var reader = File.OpenText(args.Last()))
            {
                var deserializer = CreateDefaultDeserializer();

                try{
                    var bootstrap = deserializer.Deserialize<Bootstrap>(reader);
                    await bootstrap.Run(
                        new ConsoleIODefinition(), 
                        new FrameworkTextFileInput(),
                        new FrameworkTextFileOutput(),
                        overrideUserDecision);
                }
                catch(IOException ex)
                {
                    Console.WriteLine("Encountered an exception while trying to parse the yaml file:");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static bool ValidateBootstrapConfig(string filename)
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

        private static void PrintAllKnownModules()
        {
            Console.WriteLine("List of all available modules:");
            var moduleFinder = CreateDefaultModuleFinder();
            foreach(var module in moduleFinder.GetAllModulesForModulesNamespace())
                Console.WriteLine($"{module.Type.Name}");
        }

        private static ModuleFinder CreateDefaultModuleFinder()
        {
            var moduleFinder = new ModuleFinder
            {
                Prefix = "tag:yaml.org,2002:",
                Suffix = "",
                TrimStart = "",
                TrimEnd = "Module"
            };
            return moduleFinder;
        }

        private static Deserializer CreateDefaultDeserializer()
        {
            var moduleFinder = CreateDefaultModuleFinder();
            var mappings = moduleFinder.GetAllModulesForModulesNamespace();

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
