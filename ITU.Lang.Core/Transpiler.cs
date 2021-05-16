using System.IO;
using Antlr4.Runtime;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core
{
    public class Transpiler
    {
        private string Stubs;

        public Transpiler()
        {
            Stubs = File.ReadAllText("../ITU.Lang.Core/stubs/signal.pipe").Replace('\n', ' ');
        }

        public string fromString(string input)
        {
            input = Stubs + input;

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
