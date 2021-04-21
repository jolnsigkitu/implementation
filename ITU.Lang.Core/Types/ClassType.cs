using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator.Nodes;

namespace ITU.Lang.Core.Types
{
    public class ClassType : Type
    {
        public Dictionary<string, (Type, Node)> Members = new Dictionary<string, (Type, Node)>();

        public string Name { get; set; }

        public string AsNativeName() => Name;
        public string AsTranslatedName() => Name;

        public bool Equals(Type other)
        {
            if (other is AnyType) return true;
            if (other is ClassType t)
                return Name.Equals(t.Name) && Members.Equals(t.Members);
            return false;
        }

        // Hash codes are needed for successful lookup in dictionaries
        public override int GetHashCode()
        {
            const int seed = 97;
            const int modifier = 19;

            // We go unchecked in order to let the integer automatically overflow for better hashing chaos
            unchecked
            {
                return Members.Aggregate(
                    (seed + Name.GetHashCode()) * modifier,
                    (cur, item) => ((cur * modifier) + item.Key.GetHashCode()) * modifier + item.Value.GetHashCode()
                );
            }
        }

        public ObjectType ToObjectType()
        {
            var objMembers = this.Members
                .Select((entry) =>
                {
                    var typ = entry.Value.Item1;
                    if (typ is ClassType t)
                        typ = t.ToObjectType();
                    return (entry.Key, typ);
                })
                .ToDictionary(x => x.Item1, x => x.Item2);
            return new ObjectType()
            {
                Name = this.Name,
                Members = objMembers,
            };
        }

        public string ToFullClass()
        {
            var translatedMembers = this.Members.Select(member =>
            {
                var (name, (typ, node)) = member;

                if (typ is FunctionType f)
                {
                    var isConstructor = name == "constructor";
                    var actualName = isConstructor ? this.Name : name;
                    var returnType = isConstructor ? "" : f.ReturnType.AsTranslatedName();
                    var paramList = f.ParameterTypes.Zip(f.ParameterNames, (t, n) => $"{t.AsTranslatedName()} {n}");
                    var end = f?.IsLambda == true ? ";" : "";
                    return $"public {returnType} {actualName}({string.Join(",", paramList)}){node.TranslatedValue}{end}";
                }

                return $"public {typ.AsTranslatedName()} {name}{(!string.IsNullOrEmpty(node?.TranslatedValue) ? $"={node.TranslatedValue}" : "")};";
            });

            return $"class {this.Name} {{\n{string.Join("\n", translatedMembers)}\n}}";
        }

        public void Validate(Scope<Type> scope) { }
    }
}
