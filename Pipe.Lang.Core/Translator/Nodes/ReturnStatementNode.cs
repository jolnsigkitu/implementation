using Pipe.Lang.Core.Translator.Nodes.Expressions;
using Pipe.Lang.Core.Types;
using static Pipe.Lang.Core.Grammar.LangParser;

namespace Pipe.Lang.Core.Translator.Nodes
{
    public class ReturnStatementNode : StatementNode
    {
        private ExprNode Expr;
        public IType ReturnType;
        public ReturnStatementNode(ExprNode expr, TokenLocation location) : base(location)
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
