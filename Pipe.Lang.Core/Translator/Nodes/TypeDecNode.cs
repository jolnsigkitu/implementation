using Pipe.Lang.Core.Translator.TypeNodes;
using Pipe.Lang.Core.Types;
using System.Linq;

namespace Pipe.Lang.Core.Translator.Nodes
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
            if (ClassNode != null)
            {
                if (!IsExtern)
                    env.Classes.Add(ClassNode);

                ClassNode.Validate(env);
                return;
            }

            env.Scopes.Types.Bind(Name, TypeNode.EvalType(env));
        }

        public override string ToString() => "";
    }
}
