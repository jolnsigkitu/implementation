using ITU.Lang.Core.Translator.TypeNodes;

namespace ITU.Lang.Core.Translator.Nodes
{
    public class TypeDecNode : Node
    {
        private string Name;
        private TypeNode TypeNode;

        public TypeDecNode(string name, TypeNode typeNode, TokenLocation location) : base(location)
        {
            Name = name;
            TypeNode = typeNode;
        }

        public override void Validate(Environment env)
        {
            var type = TypeNode.EvalType(env);

            env.Scopes.Types.Bind(Name, type);
        }

        public override string ToString() => "";
    }
}