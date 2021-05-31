using System.IO;
using Antlr4.Runtime;
using Pipe.Lang.Core.Grammar;
using Pipe.Lang.Core.Translator;

namespace Pipe.Lang.Core
{
    public class Transpiler
    {
        public string fromString(string input)
        {
            input = new Stubs() + input;

            ICharStream stream = CharStreams.fromString(input);

            ITokenSource lexer = new LangLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            var parser = new LangParser(tokens);
            parser.BuildParseTree = true;

            var tree = parser.prog();

            var visitor = new Translator.Translator(tokens);

            var prog = visitor.VisitProg(tree);

            var env = new Environment();
            prog.Validate(env);

            return prog.ToString();
        }
    }
}
