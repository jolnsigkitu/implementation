using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class FunctionType : Type
    {
        public Type ReturnType { get; set; } = new VoidType();

        public IList<Type> ParameterTypes = new List<Type>();

        public string AsTranslatedName()
        {
            var paramTypes = ParameterTypes.Select((t) => t.AsTranslatedName());
            var paramStr = string.Join(",", paramTypes);

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

        public bool Equals(Type other)
        {
            if (other is FunctionType f)
                return ReturnType.Equals(f.ReturnType) && ParameterTypes.Equals(f.ParameterTypes);
            return false;
        }
    }
}
