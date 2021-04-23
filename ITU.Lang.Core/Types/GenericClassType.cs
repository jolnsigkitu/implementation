using ITU.Lang.Core.Translator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class GenericClassType : ClassType, IGenericType<ClassType>
    {
        public IList<string> GenericIdentifiers { get; set; }

        public GenericClassType() { }

        public GenericClassType(ClassType _class)
        {
            /* Name = _class.Name;
            Members = _class.Members; */
        }

        public ClassType Specify(IDictionary<string, Type> specificTypes)
        {
            return new ClassType()
            {
                Name = Name,
                // Members = Members.Select((KeyValuePair<string, (Type, Node)> member) =>
                // {
                //     var name = member.Key;
                //     var (type, node) = member.Value;
                //     if (type is GenericFunctionType generic)
                //         type = generic.Specify(specificTypes);
                //     else if (type is GenericClassType genericClass)
                //         type = genericClass.Specify(specificTypes);

                //     else if (type is GenericTypeIdentifier identifier)
                //     {
                //         if (!(specificTypes.TryGetValue(identifier.Identifier, out var specType)))
                //         {
                //             throw new TranspilationException($"Cannot find specification of generic type {identifier.Identifier}");
                //         }
                //         type = specType;
                //     }
                //     return (name, (type, node));
                // }).ToDictionary(member => member.Item1, member => member.Item2),
            };
        }

        public IDictionary<string, Type> Resolve(IList<Type> types)
        {
            return GenericIdentifiers.Zip(types, (i, t) => (i, t)).ToDictionary(member => member.Item1, member => member.Item2);
        }

        public override string ToString() => "aaaaaaaa";
        // $"GenericClassType: {{Name: {Name}, Members: [{string.Join(", ", Members.Select(m => m.ToString()))}]}}";
    }
}
