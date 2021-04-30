using ITU.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class ClassNode : Node
    {
        public string ClassName;
        public IClassType Type;
        IList<ClassMemberNode> Members;
        GenericHandleNode Handle;
        public ClassNode(IList<ClassMemberNode> members, GenericHandleNode handle, TokenLocation location) : base(location)
        {
            Members = members;
            Type = new ClassType();
            Handle = handle;
        }

        public override void Validate(Environment env)
        {
            var classMembers = Type.Members;
            if (Handle != null)
            {
                var wrapper = new GenericClassWrapper(Type, Handle.Names.ToList());
                Type = wrapper;
            }

            Type.Name = ClassName;

            env.Scopes.Types.Bind(ClassName, Type);

            using var _ = env.Scopes.Use();

            if (Handle != null)
                Handle.Bind(env);

            env.Scopes.Values.Bind("this", new VariableBinding()
            {
                Type = Type,
            });

            foreach (var member in Members)
            {
                member.Validate(env);
                classMembers.Add(member.Name, member.Binding.Type);
                env.Scopes.Values.Bind(member.Name, member.Binding);
            }
        }

        public override string ToString()
        {
            var memberStrs = Members.Select(m => m.ToString(ClassName));
            return $"class {ClassName} {Handle} {{\n{string.Join("\n", memberStrs)}\n}}";
        }
    }
}
