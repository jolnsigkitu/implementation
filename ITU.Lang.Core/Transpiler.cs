using Antlr4.Runtime;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.Translator;

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

            var tree = parser.prog();

            var visitor = new Translator.Translator(tokens);

            var prog = visitor.VisitProg(tree);

            var env = new Environment();
            prog.Validate(env);

            System.Console.WriteLine(env.Scopes.Types);

            return prog.ToString();
        }
    }
}
