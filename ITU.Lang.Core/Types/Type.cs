using System;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public interface Type : IEquatable<Type>
    {
        string AsTranslatedName();
        string AsNativeName();
        void Validate(Scope<TypeBinding> scope);
    }
}
