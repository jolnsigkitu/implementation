using Antlr4.Runtime;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.Nodes.Expressions
{
    public class UnaryOperatorNode : ExprNode
    {
        public string Operator { get; private set; }
        protected readonly ExprNode Expr;
        public bool IsPrefix;
        public UnaryOperatorNode(string op, ExprNode expr, bool isPrefix, TokenLocation location) : base(location)
        {
            Expr = expr;
            Operator = op;
            IsPrefix = isPrefix;
        }
        protected override IType ValidateExpr(Environment env)
        {
            Expr.Validate(env);

            var collection = IsPrefix ? env.Operators.UnaryPrefix : env.Operators.UnaryPostfix;

            return collection.Get(Operator, Expr.Type, Location);
        }

        public override string ToString()
        {
            return IsPrefix ? $"{Operator}{Expr}" : $"{Expr}{Operator}";
        }
    }
}
