using System;
using Antlr4.Runtime;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator.Nodes.Expressions
{
    public class VisitFunctionNode : ExprNode
    {
        public VisitFunctionNode(ParserRuleContext context) : base(new VoidType(), context)
        {
        }

        public override void Validate(Scopes scopes)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => throw new NotImplementedException();
    }
}
