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

            try
            {
                prog.Validate(new Environment());
            }
            catch (TranspilationException ex)
            {
                System.Console.Error.WriteLine(ex.Message);
                var lines = ex.StackTrace.Split('\n');
                System.Console.Error.WriteLine(string.Join("\n", lines[0..5]));
                if (lines.Length - 5 > 0) System.Console.Error.WriteLine($"   ... {lines.Length - 5} more lines");
                System.Environment.Exit(1);
                return "";
            }

            return prog.ToString();
        }
    }
}
