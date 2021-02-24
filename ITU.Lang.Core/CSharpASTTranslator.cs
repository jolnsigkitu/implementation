using System;
using Antlr4.Runtime.Misc;

namespace ITU.Lang.Core
{
    public class CSharpASTTranslator : LangBaseListener
    {
        public override void EnterStatement([NotNull] LangParser.StatementContext ctx)
        {
            Console.WriteLine(ctx.GetText());
        }

    }
}
