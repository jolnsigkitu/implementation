using Pipe.Lang.Core.Translator;

namespace Pipe.Lang.Core.Types
{
    public class BooleanType : IType
    {
        public string AsNativeName() => "boolean";

        public string AsTranslatedName() => "bool";

        public bool Equals(IType other) => other is BooleanType || other is AnyType;

        // We just choose a unique prime for our base types, probably shouldn't bring us many problems
        public override int GetHashCode() => 11;

        public override string ToString() => "new BooleanType()";
    }
}
