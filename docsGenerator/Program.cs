using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpStrap.Modules;

namespace DocsGenerator
{
    class Program
    {
        private const string sharpStrapAssemblyName = "../src/bin/Debug/netcoreapp2.0/sharpstrap.dll";
        private const string modulesSourceCodeFolder = "../src/Modules";
        private const string modulesFileFilter = "*Module.cs";
        private const string ModuleNameSpaceEndsWith = "Modules";
        private const string ModuleClassNameEndsWith = "Module";
        private const string baseModuleClassName = "BaseModule";
        //private static readonly Dictionary<string, ClassCodeInfo> analysis = new Dictionary<string, ClassCodeInfo>();
        private static readonly List<ClassPropertyComment> classPropertyCodeInfos = new List<ClassPropertyComment>();
        private static readonly List<ClassComment> classCodeInfos = new List<ClassComment>();
        private static readonly List<string> PropertyNameBlacklist = new List<string>() { "Id", "Description", "AllowError", "Command", "Arguments", "Output", "WorkingDirectory", "RequiresElevation" };

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
        }

        static void GetCommentsForClassProperties(Type t)
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
            Console.WriteLine($"Found {propertyDocumentationTagsFound} documentation tag{(propertyDocumentationTagsFound == 1 ? "" : "s")}.");
        }
    }
}