using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ninject;
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
            var kernel = InitKernel();
            var bootstrapDeserializer = kernel.Get<BootstrapDeserializer>();
            var moduleFinder = kernel.Get<IModuleFinder>();
            var cliValidator = kernel.Get<CLIValidator>();

            if(cliValidator.Validate(args) == false)
                return;

            var overrideUserDecision = args.First() == "-y";
            var configFilename = args.Last();
            
            if(bootstrapDeserializer.Validate(configFilename) == false)
                return;

            await Run(configFilename, overrideUserDecision, bootstrapDeserializer);
        }

        private static async Task Run(string configFilename, bool overrideUserDecision, BootstrapDeserializer bootstrapDeserializer)
        {
            using(var reader = File.OpenText(configFilename))
            {
                var deserializer = bootstrapDeserializer.CreateDefaultDeserializer();

                try{
                    var bootstrap = deserializer.Deserialize<Bootstrap>(reader);
                    var ioDefinition = new ConsoleIODefinition();
                    await bootstrap.Run(
                        new PackageInformationPrinter(ioDefinition),
                        new FileBootstrapStatusLogger(), 
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

        private static IKernel InitKernel()
        {
            var kernel = new StandardKernel(new Helpers.SharpStrapModule());
            return kernel;
        }
    }
}
