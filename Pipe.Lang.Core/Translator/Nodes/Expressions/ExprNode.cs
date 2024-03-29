using Antlr4.Runtime;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.Nodes.Expressions
{
    public abstract class ExprNode : Node
    {
        public IType Type { get; set; }
        protected ExprNode(IType type, TokenLocation location) : base(location)
        {
            Type = type;
        }

        protected ExprNode(TokenLocation location) : base(location) { }

        public bool IsType(IType v)
        {
            return Type.Equals(v);
        }

        public bool IsType(ExprNode n)
        {
            return IsType(n.Type);
        }

        public void AssertType(IType v)
        {
            if (IsType(v)) return;

            throw new TranspilationException($"Expected type '{(v.AsNativeName())}', got '{Type.AsNativeName()}'", Location);
        }

        public override void Validate(Environment env)
        {
            Type = ValidateExpr(env);
        }

        protected abstract IType ValidateExpr(Environment env);
    }
}
