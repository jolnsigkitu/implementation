using ITU.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class ClassNode : Node
    {
        public ClassType Type;
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
            var members = new Dictionary<string, VariableBinding>();
            var binding = new TypeBinding()
            {
                Type = Type,
                Members = members
            };

            env.Scopes.Types.Bind(Type.Name, binding);

            using var _ = env.Scopes.Use();

            var thisBinding = new VariableBinding()
            {
                Type = Type,
                Members = members,
            };

            env.Scopes.Values.Bind("this", thisBinding);

            if (Handle != null)
            {
                Handle.Validate(env);
                var genericType = new GenericClassType(Type, Handle.Names.ToList());
                Type = genericType;
                binding.Type = genericType;
                thisBinding.Type = genericType;
            }

            foreach (var member in Members)
            {
                member.Validate(env);
                members.Add(member.Name, member.Binding);
                env.Scopes.Values.Bind(member.Name, member.Binding);
            }
        }

        public override string ToString()
        {
            var memberStrs = Members.Select(m => m.ToString(Type.Name));
            return $"class {Type.Name} {Handle} {{\n{string.Join("\n", memberStrs)}\n}}";
        }
    }
}
