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

        private (Type typ, IList<string> GenericIdentifiers) SpecifyMemberInner(Type inputType)
        {
            if (inputType is GenericTypeIdentifier gti)
            {
                if (SpecificTypes.TryGetValue(gti.Identifier, out var typ))
                    return (typ, null);
                return (gti, new List<string>() { gti.Identifier });
            }

            if (inputType is FunctionType ft)
            {
                var (returnType, returnGenerics) = SpecifyMemberInner(ft.ReturnType);
                var paramTypes = ft.ParameterTypes.Select(t => SpecifyMemberInner(t));
                var generics = new List<string>();

                if (returnGenerics != null) generics.AddRange(returnGenerics);

                generics.AddRange(
                    paramTypes
                        .Where(p => p.GenericIdentifiers != null)
                        .SelectMany(p => p.GenericIdentifiers)
                );

                var func = new FunctionType()
                {
                    ReturnType = returnType,
                    IsLambda = ft.IsLambda,
                    ParameterNames = ft.ParameterNames,
                    ParameterTypes = paramTypes.Select(p => p.typ).ToList(),
                };

                func = generics.Count == 0 ? func : new GenericFunctionType(func, generics);
                return (func, generics);
            }

            if (inputType is SpecificClassType sct)
                return (new SpecificClassType()
                {
                    Name = sct.Name,
                    GenericIdentifiers = sct.GenericIdentifiers,
                    SpecificTypes = this.SpecificTypes,
                }, null);

            // inputType should not be a composite type or generic identifier
            return (inputType, null);
        }

        public Type SpecifyMember(Type inputType) => SpecifyMemberInner(inputType).typ;

        public override string ToString()
        {
            var giString = string.Join("\", \"", GenericIdentifiers);
            var stString = string.Join(", ", SpecificTypes.Select(member => $"{{ \"{member.Key}\", {member.Value} }}"));
            return $"new SpecificClassType() {{ Name = \"{Name}\", GenericIdentifiers = new List<string>() {{ \"{giString}\" }}, SpecificTypes = new Dictionary<string, Type>() {{  }} }}";
        }
    }
}
