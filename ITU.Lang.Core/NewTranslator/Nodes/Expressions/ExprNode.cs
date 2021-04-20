using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public abstract class ExprNode : Node
    {
        public Type Type { get; set; }
        protected ExprNode(Type type, ParserRuleContext context) : base(context)
        {
            Type = type;
        }

        protected ExprNode(ParserRuleContext context) : base(context) { }

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

            throw new TranspilationException($"Expected type '{(v.AsNativeName())}', got '{Type.AsNativeName()}'");
        }

        public override void Validate(Environment env)
        {
            Type = ValidateExpr(env);
        }

        public abstract Type ValidateExpr(Environment env);
    }
}
