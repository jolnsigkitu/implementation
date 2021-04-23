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

        public GenericClassType(ClassType _class, IList<string> genericIdentifiers)
        {
            Name = _class.Name;
            GenericIdentifiers = genericIdentifiers;
        }

        public ClassType Specify(IDictionary<string, Type> specificTypes)
        {
            return new SpecificClassType(this, specificTypes);
        }

        // public IDictionary<string, Type> Resolve(IList<Type> types)
        // {
        //     return GenericIdentifiers.Zip(types, (i, t) => (i, t)).ToDictionary(member => member.Item1, member => member.Item2);
        // }

        public override string AsTranslatedName() => ToString();
        public override string AsNativeName() => ToString();

        public override string ToString() => $"{Name}<{string.Join(", ", GenericIdentifiers)}>";
        // $"GenericClassType: {{Name: {Name}, Members: [{string.Join(", ", Members.Select(m => m.ToString()))}]}}";
    }
}
