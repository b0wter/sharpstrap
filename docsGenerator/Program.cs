using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpStrap.Modules;

namespace DocsGenerator
{
    //
    // This is whole tool is merely a proof of concept and requires some serious rework!
    // Do not use this in production!
    //
    class Program
    {
        private const string sharpStrapAssemblyName = "../src/bin/Debug/netcoreapp2.0/sharpstrap.dll";
        private const string modulesSourceCodeFolder = "../src/Modules";
        private const string modulesFileFilter = "*Module.cs";
        private const string ModuleNameSpaceEndsWith = "Modules";
        private const string ModuleClassNameEndsWith = "Module";
        private const string baseModuleClassName = "BaseModule";
        private static string templateFilename = string.Empty;
        private static string manualFilename = string.Empty;
        //private static readonly Dictionary<string, ClassCodeInfo> analysis = new Dictionary<string, ClassCodeInfo>();
        private static readonly List<ClassPropertyComment> classPropertyCodeInfos = new List<ClassPropertyComment>();
        private static readonly List<ClassComment> classCodeInfos = new List<ClassComment>();
        private static readonly List<string> PropertyNameBlacklist = new List<string>() { "Id", "Description", "AllowError", "Command", "Arguments", "Output", "WorkingDirectory", "RequiresElevation" };
        private static readonly List<string> ClassNameBlacklist = new List<string>() { "BaseModule", "ShellModule" };

        /// <summary>
        /// Returns all the files that contain module source code.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> GetModuleFilenames()
        {
            return System.IO.Directory.GetFiles(modulesSourceCodeFolder, modulesFileFilter);
        }

        private static IEnumerable<Type> GetModulesTypes()
        {
            var reflection = new ReflectionHelper();
            var types = reflection.GetClassesFromNamespaceMatching(sharpStrapAssemblyName, ModuleNameSpaceEndsWith, ModuleClassNameEndsWith);
            return types;
        }

        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("This tool requires exactly two parameters:");
                Console.WriteLine("1. filename of the markdown template");
                Console.WriteLine("2. filename of the output");
                return;
            }
            if(System.IO.File.Exists(args[0]) == false)            
            {
                Console.WriteLine($"The file '{args[0]}' does not exist.");
                return;
            }
            templateFilename = args[0];
            if(System.IO.File.Exists(args[1]))
            {
                Console.WriteLine("The destination file exists, overwrite? [y/N]");
                var key = Console.ReadKey().KeyChar;
                if(key != 'y' && key != 'Y')
                    return;
            }
            manualFilename = args[1];

            var moduleFiles = GetModuleFilenames();
            Console.WriteLine("Found the following code files to run through the code analysis:");
            foreach(var file in moduleFiles)
                Console.WriteLine(file);

            Console.WriteLine("Preparing code analysis for all source code files.");
            foreach(var file in moduleFiles)
            {
                var analysis = SourceCodeInfo.FromSourceCodeFile(file);
                if(analysis.HasErrors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Compilation of '{file}' resulted in the following errors:");
                    Console.WriteLine(string.Join(Environment.NewLine, analysis.ErrorMessages));
                }
                foreach(var c in analysis.ClassPropertyComments)
                    classPropertyCodeInfos.Add(c);

                foreach(var c in analysis.ClassComments)
                    classCodeInfos.Add(c);
            }

            var types = GetModulesTypes();
            Console.WriteLine("Found the following module classes in the assembly:");
            foreach(var t in types)
                Console.WriteLine(t.Name);
            
            foreach(var type in types.Where(t => t.IsAbstract == false))
            {
                GetCommentsForClassProperties(type);
            }

            var manual = FillMarkdownTemplate(System.IO.File.ReadAllLines(templateFilename));
            System.IO.File.WriteAllText(manualFilename, manual);
        }

        static IEnumerable<string> GetCommentsForClassProperties(Type t)
        {
            var reflection = new ReflectionHelper();

            Console.WriteLine($"Starting work on '{t.Name}'.");

            var baseClasses = reflection.GetBaseClasses(t, baseModuleClassName, true);
            var properties = baseClasses.SelectMany(c => reflection.GetPropertiesForClass(c)).Distinct().Except(PropertyNameBlacklist);

            int propertyDocumentationTagsFound = 0;
            foreach(var p in properties)
            {
                var info = classPropertyCodeInfos.Find(x => x.PropertyName == p);
                if(info == null)
                    Console.WriteLine($"Could not find info for '{t.Name}.{p}' in the class code infos.");
                else if(string.IsNullOrWhiteSpace(info.RawComment))
                    Console.WriteLine("<no documentation>");
                else
                    propertyDocumentationTagsFound++;
            }

            Console.WriteLine($"Class description:{Environment.NewLine}{classCodeInfos.Find(c => c.ClassName == t.Name).CleanedMergedDocumentationTags} ");
            Console.WriteLine($"Found {propertyDocumentationTagsFound} documentation tag{(propertyDocumentationTagsFound == 1 ? "" : "s")}.");

            return properties;
        }

        static string FillMarkdownTemplate(string template)
        {
            var lines = template.Split(Environment.NewLine);
            return FillMarkdownTemplate(lines);
        }

        static string FillMarkdownTemplate(string[] template)
        {
            const string templateStart = "{{";
            const string templateEnd = "}}";

            var templateConfig = new List<string>();
            int counter = 0;
            while(counter < template.Length && template[counter] != templateStart)
                counter++;
            int templateStartLine = counter;
            counter++;
            
            if(counter == template.Length)
            {
                Console.WriteLine($"Could not find template start '{templateStart}' in template file.");
                return string.Join(Environment.NewLine, template);
            }

            while(counter < template.Length && template[counter] != templateEnd)
            {
                templateConfig.Add(template[counter]);
                counter++;
            }
            int templateEndLine = counter;

            Console.WriteLine("=== Template Config Start ===");
            foreach(var line in templateConfig)
                Console.WriteLine(line);           
            Console.WriteLine("=== Template Config End ===");

            var config = ParseTemplateConfig(templateConfig);

            var filledTemplate = FillTemplate(config);

            var manual = template
                            .Take(templateStartLine)
                            .Append(filledTemplate)
                            .ToList();
            manual.AddRange(template.Skip(templateEndLine + 1).TakeWhile(_ => true));
            while(string.IsNullOrWhiteSpace(manual.Last()))
                manual.RemoveAt(manual.Count() - 1);

            var joinedManual = string.Join(Environment.NewLine, manual);
            Console.WriteLine(joinedManual);
            return joinedManual;
        }

        static TemplateConfig ParseTemplateConfig(IEnumerable<string> content)
        {
            var joined = string.Join(Environment.NewLine, content);
            var config = Newtonsoft.Json.JsonConvert.DeserializeObject<TemplateConfig>(joined);
            return config;
        }

        static string FillTemplate(TemplateConfig config)
        {
            var builder = new System.Text.StringBuilder();
            foreach(var cci in classCodeInfos.Where(x => ClassNameBlacklist.Contains(x.ClassName) == false && x.IsAbstract == false))
            {
                builder.AppendLine($"{config.ModulePrefix}{cci.ClassName}{config.ModuleSuffix}");
                foreach(var pair in cci.CleanedDocumentationTags)
                    builder.AppendLine($"{config.ModuleDescriptionPrefix}{pair.Value}{config.ModuleDescriptionSuffix}");

                //var type = Type.GetType($"{cci.Namespace}, {cci.ClassName}");
                var assembly = (typeof(SharpStrap.Modules.BaseModule)).Assembly;
                //var type = assembly.GetType("{cci.Namespace}, {cci.ClassName}");
                var type = assembly.GetTypes().First(t => t.Name == cci.ClassName);
                if(type == null)
                    Console.WriteLine($"Could not find type for: {cci.Namespace}, {cci.ClassName}");
                else
                    Console.WriteLine("Found type.");
                var properties = GetCommentsForClassProperties(type);
                var filteredProperties = properties.Select(y => classPropertyCodeInfos.Find(x => x.ClassName == cci.ClassName && x.PropertyName == y));

                var propertyAdded = false;
                foreach(var cpci in filteredProperties)
                {
                    builder.AppendLine($"{config.ModulePropertyPrefix}{cpci.PropertyName}{config.ModulePropertySuffix}");
                    if(cpci.CleanedDocumentationTags.ContainsKey("summary"))
                    {
                        //builder.AppendLine($"{config.ModulePropertyPrefix}{cpci.PropertyName}{config.ModulePropertySuffix}");
                        builder.AppendLine($"{config.ModulePropertyDescriptionPrefix}{cpci.CleanedDocumentationTags["summary"]}{config.ModulePropertyDescriptionSuffix}");
                    }
                    else
                        builder.AppendLine("No summary available for this property.");
                    builder.AppendLine();
                    propertyAdded = true;
                }
                if(propertyAdded == false)
                    builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}