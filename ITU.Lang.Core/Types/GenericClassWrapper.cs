using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class GenericClassWrapper : GenericWrapper, IClassType
    {
        public new IClassType Child { get; set; }
        public string Name { get => Child.Name; set => Child.Name = value; }
        public IDictionary<string, IType> Members
        {
            get => this.Child.Members;
            set => this.Child.Members = value;
        }
        public GenericClassWrapper(IClassType child, IEnumerable<string> bindings) : base(child, bindings)
        {
            Child = child;
        }

        public GenericClassWrapper(IClassType child, IDictionary<string, IType> bindings, IList<string> handle) : base(child, bindings, handle)
        {
            Child = child;
        }

        public override string AsNativeName() => Child.AsNativeName();

        public override string AsTranslatedName()
        {
            var handleStr = string.Join(", ", Handle.Select(h => Bindings[h].AsTranslatedName()));
            return $"{Child.AsTranslatedName()}<{handleStr}>";
        }

        public override IClassType ResolveByIdentifier(IDictionary<string, IType> resolutions)
        {
            var newBindings = Bindings.ToDictionary(binding => binding.Key, binding =>
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

            return new GenericClassWrapper(Child, newBindings, Handle);
        }

        public override string ToString() => $"(ClassWrapper - Name: {Name}, Child: {Child})";

        public bool TryGetMember(string key, out IType member)
        {
            if (!Members.TryGetValue(key, out member))
            {
                return false;
            }

            if (member is GenericTypeIdentifier id && Bindings.TryGetValue(id.Identifier, out var result))
            {
                member = result;
            }

            // member = member switch
            // {
            //     GenericClassWrapper wrapper => wrapper,
            //     GenericTypeIdentifier id => id,
            //     _ => member,
            // };

            return true;
        }
    }
}
