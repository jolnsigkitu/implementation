using Antlr4.Runtime;
using ITU.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.NewTranslator.Nodes
{
    public abstract class StatementNode : Node
    {
        protected StatementNode(ParserRuleContext ctx) : base(ctx) { }
    }
}
