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

        public override IClassType ResolveByIdentifier(IDictionary<string, IType> resolutions)
        {
            var newBindings = ResolveBindingsByIdentifier(resolutions);

            return new GenericClassWrapper(Child, newBindings, Handle);
        }

        public override IClassType ResolveByPosition(IEnumerable<IType> resolutions)
        {
            throw new System.NotImplementedException("TODO: ResolveByPosition in GenericClassWrapper");
        }

        public bool TryGetMember(string key, out IType member)
        {
            if (!Members.TryGetValue(key, out member))
            {
                return false;
            }

            member = TryResolveType(member);

            return true;
        }

        public override string AsNativeName() => Child.AsNativeName();

        public override string AsTranslatedName()
        {
            var handleStr = string.Join(", ", Handle.Select(h => Bindings[h].AsTranslatedName()));
            return $"{Child.AsTranslatedName()}<{handleStr}>";
        }

        public override string ToString() => $"(ClassWrapper - Name: {Name}, Handle: {Handle}, Child: {Child})";

        public override bool Equals(IType other)
        {
            return other is GenericClassWrapper wrapper &&
                   EqualityComparer<IDictionary<string, IType>>.Default.Equals(Bindings, wrapper.Bindings) &&
                   EqualityComparer<IType>.Default.Equals(Child, wrapper.Child) &&
                   EqualityComparer<IList<string>>.Default.Equals(Handle, wrapper.Handle) &&
                   EqualityComparer<IClassType>.Default.Equals(Child, wrapper.Child) &&
                   Name == wrapper.Name &&
                   EqualityComparer<IDictionary<string, IType>>.Default.Equals(Members, wrapper.Members);
        }

        public override int GetHashCode() => System.HashCode.Combine(Bindings, Child, Handle, Child, Name, Members);
    }
}
