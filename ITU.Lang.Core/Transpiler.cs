using Antlr4.Runtime;
using ITU.Lang.Core.Grammar;
using ITU.Lang.Core.NewTranslator;

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

            var visitor = new NewTranslator.Translator(tokens);

            var prog = visitor.VisitProg(tree);

            prog.Validate(new Environment());
            return prog.ToString();

            // try
            // {
            // var visitor = new Translator.Translator(tokens);
            // return visitor.VisitProg(tree).TranslatedValue;
            // }
            // catch (TranspilationException ex)
            // {
            //     throw ex;
            //     // Console.Error.WriteLine(ex.Message);
            //     // Environment.Exit(1);
            //     // return "";
            // }
        }
    }
}
