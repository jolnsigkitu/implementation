using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public abstract class UnaryOperatorNode : ExprNode
    {
        public string Operator { get; private set; }
        protected readonly ExprNode Expr;
        public UnaryOperatorNode(string op, ExprNode expr, Type returnType, ParserRuleContext context) : base(returnType, context)
        {
            Expr = expr;
            Operator = op;
        }
        public override void Validate() => Expr.Validate();
    }
}
