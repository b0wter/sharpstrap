using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly List<ClassPropertyComment> classCodeInfos = new List<ClassPropertyComment>();
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
                var analysis = ClassCodeInfo.FromSourceCodeFile(file);
                classCodeInfos.AddRange(analysis.GetPropertiesWithComments());
            }

            var types = GetModulesTypes();
            Console.WriteLine("Found the following module classes in the assembly:");
            foreach(var t in types)
                Console.WriteLine(t.Name);
            
            foreach(var type in types.Where(t => t.IsAbstract == false))
            {
                GetCommentsForClass(type);
            }
        }

        static void GetCommentsForClass(Type t)
        {
            var reflection = new ReflectionHelper();

            Console.WriteLine($"Starting work on '{t.Name}'.");

            var baseClasses = reflection.GetBaseClasses(t, baseModuleClassName, true);
            var properties = baseClasses.SelectMany(c => reflection.GetPropertiesForClass(c)).Distinct().Except(PropertyNameBlacklist);

            Console.WriteLine( "Properties for this class (and base classes):");
            foreach(var p in properties)
            {
                Console.WriteLine(p);
                var info = classCodeInfos.Find(x => x.ClassName == t.Name && x.PropertyName == p);
                if(string.IsNullOrWhiteSpace(info.Comment))
                    Console.WriteLine("<no documentation>");
                else
                    Console.WriteLine(info.Comment);
            }
        }

        /* 
        var reflection = new ReflectionHelper();
        var classes = reflection.GetBaseClasses<SharpStrap.Modules.DownloadModule>("BaseModule");
        
        return null;
        */
        /* 
        Console.WriteLine($"Parents are:");
        foreach (var c in classes)
        {
            Console.WriteLine(c.Name);
        }

        var properties = classes.SelectMany(c => reflection.GetPropertiesForClass(c)).Distinct();
        foreach(var p in properties)
        {
            Console.WriteLine(p);
        }
        */

    }
}