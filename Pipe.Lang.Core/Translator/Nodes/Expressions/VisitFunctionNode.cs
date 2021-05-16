using Antlr4.Runtime;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator.Nodes.Expressions
{
    public class VisitFunctionNode : ExprNode
    {
        public VisitFunctionNode(TokenLocation location) : base(location)
        {
        }

        protected override IType ValidateExpr(Environment env)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString() => throw new System.NotImplementedException();
    }
}
