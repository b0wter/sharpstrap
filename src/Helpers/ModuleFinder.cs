using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpStrap.Helpers
{
    internal class ModuleFinder
    {
        public string TrimEnd { get; set; }
        public string TrimStart { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }

        public IEnumerable<(string Name, Type Type)> GetAllModulesForModulesNamespace()
        {
            var types = GetAllTypes("Cootstrap.Modules");

            var tuples = new List<(string Name, Type Type)>(types.Count());
            foreach(var t in types)
            {
                var name = t.Name;
                if(name.EndsWith(this.TrimEnd))
                    name = name.Substring(0, name.Length - this.TrimEnd.Length);
                if(name.StartsWith(this.TrimStart))
                    name = name.Substring(TrimStart.Length, name.Length - this.TrimStart.Length);

                name = $"{Prefix}{name}{Suffix}";
                tuples.Add((name, t));
            }

            return tuples;
        }
        private IEnumerable<Type> GetAllTypes(string name)
        {
            return AppDomain
                   .CurrentDomain
                   .GetAssemblies()
                   .SelectMany(a => a.GetTypes())
                   .Where(t => 
                            t.IsClass == true && 
                            t.IsAbstract == false &&
                            t.Namespace != null &&
                            t.Name.EndsWith(TrimEnd) &&
                            t.Namespace.EndsWith(name)
                          );
        }
    }
}