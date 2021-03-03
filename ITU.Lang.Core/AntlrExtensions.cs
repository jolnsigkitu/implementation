using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ITU.Lang.Core
{
    static class AntlrExtensions
    {
        public static void TraverseRule(this ParserRuleContext ctx, IParseTreeListener listener)
        {
            ctx.EnterRule(listener);
            ctx.ExitRule(listener);
        }
    }
}
