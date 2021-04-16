namespace ITU.Lang.Core.Types
{
    public class BooleanType : Type
    {
        public string AsNativeName() => "boolean";

        public string AsTranslatedName() => "bool";

        public bool Equals(Type other) => other is BooleanType || other is AnyType;

        // We just choose a unique prime for our base types, probably shouldn't bring us many problems
        public override int GetHashCode() => 11;

        public void Validate(Scope<Type> scope) { }
    }
}
