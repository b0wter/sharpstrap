using System;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Cootstrap.Helpers
{
    internal sealed class YamlModuleConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type.IsSubclassOf(typeof(Modules.BaseModule));
        }

        public object ReadYaml(IParser parser, Type type)
        {
            BaseModule result;

            if (parser.Current.GetType() != _mappingStartType) // You could also use parser.Accept<MappingStart>()
            {
                throw new InvalidDataException("Invalid YAML content.");
            }

            parser.MoveNext();
        }

        public void WriteYaml(IEmitter emitter, object val, Type type)
        {
            throw new NotImplementedException();
        }
    }
}