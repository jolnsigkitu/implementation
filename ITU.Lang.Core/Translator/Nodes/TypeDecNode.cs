using ITU.Lang.Core.Translator.TypeNodes;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class TypeDecNode : Node
    {
        private string Name;
        private TypeNode TypeNode;
        private ClassNode ClassNode;

        public TypeDecNode(string name, TypeNode typeNode, ClassNode classNode, TokenLocation location) : base(location)
        {
            Name = name;
            TypeNode = typeNode;
            ClassNode = classNode;
        }

        public override void Validate(Environment env)
        {
            if (TypeNode != null)
            {
                var type = TypeNode.EvalType(env);

                env.Scopes.Types.Bind(Name, new TypeBinding()
                {
                    Type = type,
                });
            }
            else
            {
                env.Classes.Add(ClassNode);
                ClassNode.Validate(env);
            }
        }

        public override string ToString() => "";
    }
}
