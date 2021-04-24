using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class SpecificClassType : ClassType
    {
        public IDictionary<string, Type> SpecificTypes { get; init; }

        public IList<string> GenericIdentifiers { get; set; }

        public SpecificClassType() { }

        public SpecificClassType(GenericClassType ct, IDictionary<string, Type> specificTypes)
        {
            Name = ct.Name;
            SpecificTypes = specificTypes;
            GenericIdentifiers = ct.GenericIdentifiers;
        }

        public override string AsTranslatedName()
        {
            var setGenerics = GenericIdentifiers.Select(id => SpecificTypes[id].AsTranslatedName());
            return $"{Name}<{string.Join(", ", setGenerics)}>";
        }

        public override string AsNativeName() => AsTranslatedName();

        public Type SpecifyMember(Type inputType)
        {
            if (inputType is GenericTypeIdentifier gti)
                return SpecificTypes[gti.Identifier];

            if (inputType is IGenericType<Type>)
                throw new TranspilationException("We cannot handle inner generics");

            if (inputType is FunctionType ft)
                return new FunctionType()
                {
                    ReturnType = SpecifyMember(ft.ReturnType),
                    IsLambda = ft.IsLambda,
                    ParameterNames = ft.ParameterNames,
                    ParameterTypes = ft.ParameterTypes.Select(t => SpecifyMember(t)).ToList(),
                };

            if (inputType is SpecificClassType sct)
                return new SpecificClassType()
                {
                    Name = sct.Name,
                    GenericIdentifiers = sct.GenericIdentifiers,
                    SpecificTypes = this.SpecificTypes,
                };

            // inputType should not be a composite type or generic identifier
            return inputType;
        }

        public override string ToString()
        {
            var giString = string.Join("\", \"", GenericIdentifiers);
            var stString = string.Join(", ", SpecificTypes.Select(member => $"{{ \"{member.Key}\", {member.Value} }}"));
            return $"new SpecificClassType() {{ Name = \"{Name}\", GenericIdentifiers = new List<string>() {{ \"{giString}\" }}, SpecificTypes = new Dictionary<string, Type>() {{  }} }}";
        }
    }
}
