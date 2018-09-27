using System;
using System.IO;
using System.Linq;

namespace SharpStrap.Helpers
{
    internal class CLIValidator
    {
        private IModuleFinder moduleFinder;
        private BootstrapDeserializer boostrapDeserializer;

        public CLIValidator(IModuleFinder moduleFinder, BootstrapDeserializer bootstrapDeserializer)
        {
            this.moduleFinder = moduleFinder;
            this.boostrapDeserializer = bootstrapDeserializer;
        }
        
        internal bool Validate(string[] args)
        {
            if(args.Length == 1 && args[0] == "modules")
            {
                PrintAllKnownModules();
                return false;
            }

            if(args.Length == 2 && args[0] == "validate")
            {
                boostrapDeserializer.Validate(args[1]);
                return false;
            }

            if((args.Length != 1 && args.Length != 2) || File.Exists(args.Last()) == false)
            {
                Console.WriteLine("This tool requires at least parameter (config file). You may add the '-y' option before the filename to autorun the bootstrap.");
                return false;
            }

            return true;
        }
        
        private void PrintAllKnownModules()
        {
            Console.WriteLine("List of all available modules:");
            foreach(var module in moduleFinder.GetAllModulesForModulesNamespace())
                Console.WriteLine($"{module.Type.Name}");
        }
    }
}