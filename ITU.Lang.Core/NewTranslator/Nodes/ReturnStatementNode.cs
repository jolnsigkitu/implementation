using ITU.Lang.Core.NewTranslator.Nodes.Expressions;
using ITU.Lang.Core.Types;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.NewTranslator.Nodes
{
    public class ReturnStatementNode : StatementNode
    {
        private ExprNode Expr;
        public Type ReturnType;
        public ReturnStatementNode(ExprNode expr, ReturnStatementContext context) : base(context)
        {
            Expr = expr;
        }

        public override void Validate(Environment env)
        {
            Expr.Validate(env);
            ReturnType = Expr.Type;
        }

        public override string ToString() => $"return {Expr};";
    }
}
