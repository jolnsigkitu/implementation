using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class FunctionType : Type
    {
        public bool IsLambda { get; init; }

        public Type ReturnType { get; set; } = new VoidType();

        public IList<Type> ParameterTypes = new List<Type>();
        public IList<string> ParameterNames = new List<string>();

        public string AsTranslatedName()
        {
            var paramStr = GetTranslatedParameterList();

            if (ReturnType is VoidType)
            {
                return $"Action{(paramStr != "" ? $"<{paramStr}>" : "")}";
            }

            return $"Func<{(paramStr != "" ? paramStr + "," : "")}{ReturnType.AsTranslatedName()}>";
        }

        public string AsNativeName()
        {
            var paramTypes = ParameterTypes.Select((t) => t.AsNativeName());
            var paramStr = string.Join(",", paramTypes);

            return $"({paramStr}) => {ReturnType.AsNativeName()}";
        }

        public string GetTranslatedParameterList()
        {
            var paramTypes = ParameterTypes.Select((t) => t.AsTranslatedName());
            return string.Join(",", paramTypes);
        }

        public bool Equals(Type other)
        {
            if (other is FunctionType f)
                return ReturnType.Equals(f.ReturnType) && ParameterTypes.Equals(f.ParameterTypes);
            return false;
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
    }
}
