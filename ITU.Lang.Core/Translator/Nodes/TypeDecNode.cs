using ITU.Lang.Core.Translator.TypeNodes;
using ITU.Lang.Core.Types;
using System.Linq;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class TypeDecNode : Node
    {
        private string Name;
        private TypeNode TypeNode;
        private ClassNode ClassNode;

        public bool IsExtern { get; }

        public TypeDecNode(string name, TypeNode typeNode, ClassNode classNode, bool isExtern, TokenLocation location) : base(location)
        {
            Name = name;
            TypeNode = typeNode;
            ClassNode = classNode;

            IsExtern = isExtern;
        }

        public override void Validate(Environment env)
        {
            if (TypeNode != null)
            {
                var type = TypeNode.EvalType(env);

                var binding = new TypeBinding()
                {
                    Type = type,
                };

                if (type is SpecificClassType gct)
                {
                    binding.Members = env.Scopes.Types.GetBinding(gct.Name).Members;
                }

                env.Scopes.Types.Bind(Name, binding);
            }
            else
            {
                if (!IsExtern)
                {
                    env.Classes.Add(ClassNode);
                }
                ClassNode.Validate(env);
            }
        }

        public override string ToString() => "";
    }
}
