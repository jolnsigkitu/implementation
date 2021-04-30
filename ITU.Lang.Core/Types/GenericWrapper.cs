using System.Collections.Generic;
using System.Linq;
namespace ITU.Lang.Core.Types
{
    public abstract class GenericWrapper : IType
    {
        public IDictionary<string, IType> Bindings { get; set; }
        public IType Child { get; set; }
        public IList<string> Handle { get; set; }

        public GenericWrapper(IType child, IEnumerable<string> bindings)
        {
            Child = child;
            Bindings = bindings
                .ToDictionary(name => name, name => new GenericTypeIdentifier(name) as IType);
            Handle = bindings.ToList();
        }

        public GenericWrapper(IType child, IDictionary<string, IType> bindings, IList<string> handle)
        {
            Child = child;
            Bindings = bindings;
            Handle = handle;
        }

        public abstract IType ResolveByIdentifier(IDictionary<string, IType> resolutions);

        public abstract IType ResolveByPosition(IEnumerable<IType> resolutions);

        // public virtual IType ResolveByPosition(IEnumerable<IType> resolutions)
        // {
        //     if (resolutions.Count() != Handle.Count)
        //     {
        //         throw new TranspilationException($"Tried to resolve generic with {resolutions.Count()} identifiers, but was provided {Handle.Count}");
        //     }

        //     throw new System.NotImplementedException("TODO: Resolve by position on generic wrapper");
        // }

        protected IDictionary<string, IType> ResolveBindingsByIdentifier(IDictionary<string, IType> resolutions)
        {
            return Bindings.ToDictionary(binding => binding.Key, binding =>
            {
                if (!(binding.Value is GenericTypeIdentifier oldValue))
                {
                    return binding.Value;
                }

                // from old Value find new Key
                if (!resolutions.TryGetValue(oldValue.Identifier, out var val))
                {
                    throw new TranspilationException($"Cannot resolve generic '{oldValue.Identifier}'");
                }

                return val;
            });
        }

        protected IType TryResolveType(IType type)
        {
            if (type is GenericTypeIdentifier id && Bindings.TryGetValue(id.Identifier, out var result))
            {
                return result;
            }

            if (type is IFunctionType ft)
            {
                var func = new FunctionType()
                {
                    IsLambda = ft.IsLambda,
                    ReturnType = TryResolveType(ft.ReturnType),
                    ParameterNames = ft.ParameterNames,
                    ParameterTypes = ft.ParameterTypes.Select(TryResolveType).ToList(),
                };
                return func;
            }

            if (type is IClassType ct)
            {
                throw new System.NotImplementedException("TODO: Deal with classes in TryResolveType @ GenericWrapper");
            }

            return type;
        }

        public virtual string AsNativeName() => Child.AsNativeName();

        public virtual string AsTranslatedName()
        {
            var handleStr = string.Join(", ", Handle.Select(h => Bindings[h].AsTranslatedName()));
            return $"{Child.AsTranslatedName()}";
        }

        public override string ToString() => $"(Wrapper - Name: {AsTranslatedName()}, Child: {Child})";

        public abstract bool Equals(IType other);

        public abstract int GetHashCode();
    }
}
