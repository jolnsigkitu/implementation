using System.Collections.Generic;
using System.Linq;
namespace ITU.Lang.Core.Types
{
    public class GenericWrapper : IType
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

        public virtual IType ResolveByIdentifier(IDictionary<string, IType> resolutions)
        {
            throw new System.NotImplementedException("TODO: Resolve by id on generic wrapper");
        }
        public virtual IType ResolveByPosition(IEnumerable<IType> resolutions)
        {
            if (resolutions.Count() != Handle.Count)
            {
                throw new TranspilationException($"Tried to resolve generic with {resolutions.Count()} identifiers, but was provided {Handle.Count}");
            }

            throw new System.NotImplementedException("TODO: Resolve by position on generic wrapper");
        }

        public virtual string AsNativeName() => Child.AsNativeName();

        public virtual string AsTranslatedName() => Child.AsTranslatedName();

        public bool Equals(IType other)
        {
            throw new System.NotImplementedException();
        }
    }
}
