using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class UnaryOperatorNode : ExprNode
    {
        public string Operator { get; private set; }
        protected readonly ExprNode Expr;
        public bool IsPrefix;
        public UnaryOperatorNode(string op, ExprNode expr, bool isPrefix, ParserRuleContext context) : base(context)
        {
            Expr = expr;
            Operator = op;
            IsPrefix = isPrefix;
        }
        public override Type ValidateExpr(Environment env)
        {
            Expr.ValidateExpr(env);

            var collection = IsPrefix ? env.Operators.UnaryPrefix : env.Operators.UnaryPostfix;

            return collection.Get(Operator, Expr.Type);
        }

        public override string ToString()
        {
            throw new System.NotImplementedException();
        }
    }
}
