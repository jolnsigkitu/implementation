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

            var tree = parser.prog();

            try
            {
                var visitor = new CSharpASTTranslator();
                return visitor.VisitProg(tree);
            }
            catch (TranspilationException ex)
            {
                Console.Error.WriteLine("An error occoured during transpilation: \n\t" + ex.Message);
                Environment.Exit(1);
                return "";
            }
        }
    }
}
