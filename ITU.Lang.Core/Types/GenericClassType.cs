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

        public override string AsTranslatedName() => AsNativeName();
        public override string AsNativeName() => $"{Name}<{string.Join(", ", GenericIdentifiers)}>";

        public override string ToString() => $"new GenericClassType() {{ Name = \"{Name}\", GenericIdentifiers = new List<string>() {{ \"{string.Join("\", \"", GenericIdentifiers)}\" }} }}";
    }
}
