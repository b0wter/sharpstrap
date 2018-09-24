using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DocsGenerator
{
    public class ReflectionHelper
    {
        internal IList<Type> GetBaseClasses<T>(string oldestAncestor, bool addInitialClass, IList<Type> accumulator = null)
        {
            return GetBaseClasses(typeof(T), oldestAncestor, addInitialClass, accumulator);
        }
        
        internal IList<Type> GetBaseClasses(Type type, string oldestAncestor, bool addInitialClass, IList<Type> accumulator = null)
        {
            if(accumulator == null)
                accumulator = new List<Type>();
            
            if(addInitialClass)
                accumulator.Add(type);

            if (type.BaseType.Name == oldestAncestor)
            {
                // no need to traverse deeper
                accumulator.Add(type.BaseType);
                return accumulator;
            }
            else if(type.BaseType != null)
            {
                var baseClass = type.BaseType;
                accumulator.Add(baseClass);
                return GetBaseClasses (baseClass, oldestAncestor, false, accumulator);
            }
            else
            {
                throw new ArgumentException($"There is no base class named '{oldestAncestor}'.");   
            }
        }

        internal IEnumerable<string> GetPropertiesForClass<T>()
        {
            return GetPropertiesForClass(typeof(T));
        }
        
        internal IEnumerable<string> GetPropertiesForClass(Type type)
        {
            var unfilteredProperties = type.GetProperties(BindingFlags.Public);
            var properties = unfilteredProperties.Where(p => p.GetSetMethod() != null && p.CanWrite == true);
            var propertyNames = properties.Select(p => p.Name);
            return propertyNames;
        }

        internal IEnumerable<Type> GetClassesFromNamespaceMatching(string filename, string nameSpace, string classNameEndsWith)
        {
            var types = Assembly.LoadFrom(filename).GetTypes();
            var filtered = types.Where(t =>
                            t.IsClass == true &&
                            t.Name.EndsWith(classNameEndsWith) &&
                            t.Namespace != null &&
                            t.Namespace.EndsWith(nameSpace)
                         );

            return filtered;
        }
    }
}