using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ITU.Lang.Core
{
    public class Transpiler
    {
        public string fromString(string input)
        {
            ICharStream stream = CharStreams.fromString(input);

            ITokenSource lexer = new LangLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            var parser = new LangParser(tokens);
            parser.BuildParseTree = true;

            IParseTree tree = parser.prog();

            // ILangListener translator = new CSharpASTTranslator();

            // ParseTreeWalker.Default.Walk(translator, tree);

            return "wip";
        }
    }
}
// LexParse -[Lang.g4]-> AbsynTree -[CSharpASTTranslator]-> CSAbsynTree -[]-> C#
