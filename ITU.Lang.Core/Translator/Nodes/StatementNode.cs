using Antlr4.Runtime;
using ITU.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;
using static ITU.Lang.Core.Grammar.LangParser;

namespace ITU.Lang.Core.Translator.Nodes
{
    public abstract class StatementNode : Node
    {
        protected StatementNode(TokenLocation loc) : base(loc) { }
    }
}
