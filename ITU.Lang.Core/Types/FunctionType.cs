using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class FunctionType : Type
    {
        Type ReturnType { get; set; }
        // de skal have navne!
        IList<Type> ParameterTypes = new List<Type>();

        public string AsTranslatedName()
        {
            var paramTypes = ParameterTypes.Select((t) => t.AsTranslatedName());
            var paramStr = string.Join(",", paramTypes);

            return ReturnType == null ? $"Action<{paramStr}>" : $"Func<{paramStr},{ReturnType.AsTranslatedName()}>";
        }

        public bool Equals(Type other)
        {
            if (other is FunctionType f)
                return ReturnType.Equals(f.ReturnType) && ParameterTypes.Equals(f.ParameterTypes);
            return false;
        }
    }
}
