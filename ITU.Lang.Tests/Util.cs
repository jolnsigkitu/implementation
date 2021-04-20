using System.IO;

using Antlr4.Runtime;
using Xunit;

using ITU.Lang.Core;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.Translator;
using ITU.Lang.Core.Translator.Nodes;

namespace ITU.Lang.Tests
{
    public class Util
    {
        public static (LangParser, ITokenStream) GetTree(string code)
        {
            ICharStream stream = CharStreams.fromString(code);

            ITokenSource lexer = new LangLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            var parser = new LangParser(tokens);
            parser.BuildParseTree = true;
            return (parser, tokens);
        }

        public static ProgNode GetConstructedTree(string code)
        {
            var (parser, tokens) = Util.GetTree(code);

            var tree = parser.prog();
            var visitor = new Translator(tokens);

            return visitor.VisitProg(tree);
        }
    }
}
