using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cootstrap.Modules;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cootstrap
{
    class Program
    {
        static async Task Main(string[] args)
        {
            /* 
            var bootstrap = new Bootstrap
            {
                Packages = new List<Package>
                {
                    new Package
                    {
                        Name = "Basic package manager operations.",
                        Modules = new List<BaseModule>
                        {
                            new PackageRemovalModule("libreoffice", "evolution", "libreoffice*"),
                            new PackageUpdateModule(),
                            new PackageInstallModule("android-tools", "awesome")
                        }
                    }
                }
            };

            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(bootstrap);
            File.WriteAllText("generated.yaml", yaml);
            */
            if(args.Length != 1 || File.Exists(args[0]) == false)
            {
                Console.WriteLine("This tool requires exactly parameter.");
                return;
            }

            using(var reader = File.OpenText(args[0]))
            {
                var deserializer = new DeserializerBuilder()
                                        .WithTagMapping($"tag:yaml.org,2002:{nameof(PackageUpdateModule)}", typeof(PackageUpdateModule))
                                        .WithTagMapping($"tag:yaml.org,2002:{nameof(PackageInstallModule)}", typeof(PackageInstallModule))
                                        .WithTagMapping($"tag:yaml.org,2002:{nameof(PackageRemovalModule)}", typeof(PackageRemovalModule))
                                        .Build();
                var bootstrap = deserializer.Deserialize<Bootstrap>(reader);
            }
        }
    }
}
