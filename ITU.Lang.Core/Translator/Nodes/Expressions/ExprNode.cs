using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator.Nodes.Expressions
{
    public abstract class ExprNode : Node
    {
        public Type Type { get; set; }
        protected ExprNode(Type type, TokenLocation location) : base(location)
        {
            Type = type;
        }

        protected ExprNode(TokenLocation location) : base(location) { }

        public bool IsType(Type v)
        {
            return Type.Equals(v);
        }

        public bool IsType(ExprNode n)
        {
            return IsType(n.Type);
        }

        public void AssertType(Type v)
        {
            if (IsType(v)) return;

            throw new TranspilationException($"Expected type '{(v.AsNativeName())}', got '{Type.AsNativeName()}'", Location);
        }

        public override void Validate(Environment env)
        {
            Type = ValidateExpr(env);
        }

        protected abstract Type ValidateExpr(Environment env);
    }
}
