using System.Collections.Generic;
using System.Linq;
using Pipe.Lang.Core.Translator;

namespace Pipe.Lang.Core.Types
{
    public interface IFunctionType : IType
    {
        bool IsLambda { get; set; }
        IType ReturnType { get; set; }
        IList<IType> ParameterTypes { get; set; }
        IEnumerable<string> ParameterNames { get; set; }

        // string GetTranslatedParameterList();
    }

    public class FunctionType : IFunctionType
    {
        public bool IsLambda { get; set; }

        public IType ReturnType { get; set; } = new VoidType();

        public IList<IType> ParameterTypes { get; set; } = new List<IType>();
        public IEnumerable<string> ParameterNames { get; set; } = new List<string>();

        public string AsNativeName()
        {
            var paramTypes = ParameterTypes.Select((t) => t.AsNativeName());
            var paramStr = string.Join(",", paramTypes);

            return $"({paramStr}) => {ReturnType.AsNativeName()}";
        }

        public string AsTranslatedName()
        {
            var paramStr = GetTranslatedParameterList();

            if (ReturnType is VoidType)
            {
                return $"Action{(paramStr != "" ? $"<{paramStr}>" : "")}";
            }

            return $"Func<{(paramStr != "" ? paramStr + "," : "")}{ReturnType.AsTranslatedName()}>";
        }

        public virtual string GetTranslatedParameterList()
        {
            var paramTypes = ParameterTypes.Select((t) => t.AsTranslatedName());
            return string.Join(",", paramTypes);
        }

        public bool Equals(IType other)
        {
            if (other is AnyType) return true;

            if (!(other is IFunctionType f))
            {
                return false;
            }

            var returnTypeMatches = ReturnType.Equals(f.ReturnType);

            var paramTypesMatches = Enumerable.SequenceEqual(ParameterTypes, f.ParameterTypes);

            return returnTypeMatches && paramTypesMatches;
        }

        // Hash codes are needed for successful lookup in dictionaries
        public override int GetHashCode()
        {
            const int seed = 103;
            const int modifier = 23;

            // We go unchecked in order to let the integer automatically overflow for better hashing chaos
            unchecked
            {
                return ParameterTypes.Aggregate(
                    (seed + ReturnType.GetHashCode()) * modifier,
                    (cur, item) => (cur * modifier) + item.GetHashCode()
                );
            }
        }

        // public override string ToString() => $"new FunctionType(){{\n\t IsLambda = {(IsLambda ? "true" : "false")},\n\t ReturnType = {ReturnType},\n\t ParameterNames = new List<string>() {{ \"{string.Join("\", \"", ParameterNames)}\" }},\n\t ParameterTypes = new List<Type>() {{ {string.Join(", ", ParameterTypes)} }}\n}}";
        public override string ToString()
        {
            var returnType = ReturnType is IClassType ? ReturnType.GetType().ToString() : ReturnType.ToString();
            return $"new FunctionType(){{\n\t IsLambda = {(IsLambda ? "true" : "false")},\n\t ReturnType = {returnType},\n\t ParameterNames = new List<string>() {{ \"{string.Join("\", \"", ParameterNames)}\" }},\n\t ParameterTypes = new List<Type>() {{ {string.Join(", ", ParameterTypes)} }}\n}}";
        }
    }
}
