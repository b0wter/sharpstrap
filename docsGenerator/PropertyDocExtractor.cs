using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DocsGenerator
{
    internal class PropertyDocExtractor
    {
        internal IList<ClassPropertyComment> GetPropertiesForClass<T>()
        {
            // GetSetMethod() is null for properties without public setters.
            var propertyInfos = typeof(T).GetProperties().Where(p => p.GetSetMethod() != null);
            var properties = new List<ClassPropertyComment>(propertyInfos.Count());
            
            foreach(var info in propertyInfos)
            {
                var xml = DocsByReflection.XMLFromMember(info);
                var comment = xml["summary"].InnerText.Trim();

                var cpc = new ClassPropertyComment
                {
                    ClassName = typeof(T).Name,
                    PropertyName = info.Name,
                    Comment = comment
                };
                properties.Add(cpc);
            }

            return properties;
        }
    }
}