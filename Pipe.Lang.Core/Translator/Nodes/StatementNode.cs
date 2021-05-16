using Antlr4.Runtime;
using Pipe.Lang.Core.Types;
using System.Collections.Generic;
using System.Linq;
using static Pipe.Lang.Core.Grammar.LangParser;

namespace Pipe.Lang.Core.Translator.Nodes
{
    public abstract class StatementNode : Node
    {
        protected StatementNode(TokenLocation loc) : base(loc) { }
    }
}
