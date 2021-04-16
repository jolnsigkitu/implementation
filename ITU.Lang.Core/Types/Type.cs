using System;

namespace ITU.Lang.Core.Types
{
    public interface Type : IEquatable<Type>
    {
        string AsTranslatedName();
        string AsNativeName();
        void Validate(Scope<Type> scope);
    }
}
