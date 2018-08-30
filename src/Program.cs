﻿using System;
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

            if((args.Length != 1 && args.Length != 2) || File.Exists(args.Last()) == false)
            {
                Console.WriteLine("This tool requires at least parameter (config file). You may add the '-y' option before the filename to autorun the bootstrap.");
                return;
            }

            bool overrideUserDecision = args.First() == "-y";

            using(var reader = File.OpenText(args.Last()))
            {
                var deserializer = CreateDefaultDeserializer();

                try{
                    var bootstrap = deserializer.Deserialize<Bootstrap>(reader);
                    await bootstrap.Run(Console.In, new ConsoleWriter(), Console.BufferWidth, overrideUserDecision);
                }
                catch(IOException ex)
                {
                    Console.WriteLine("Encountered an exception while trying to parse the yaml file:");
                    Console.WriteLine(ex.Message);
                }
            }
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