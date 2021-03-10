using System.Text;
using System.Linq;
using Antlr4.Runtime.Misc;
using static LangParser;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ITU.Lang.Core.Types;
using System.Collections.Generic;

namespace ITU.Lang.Core.Translator
{
    public partial class Translator : LangBaseVisitor<Node>
    {
        private Scope<Node> scopes = new Scope<Node>();

        private ITokenStream tokenStream;

        public Translator(ITokenStream tokenStream)
        {
            this.tokenStream = tokenStream;

            MakeGlobalScope();
        }

        public override Node VisitProg([NotNull] ProgContext context)
        {
            var res = VisitStatements(context.statements());

            return new Node()
            {
                TranslatedValue = "using System;" + res.TranslatedValue,
                Location = GetTokenLocation(context),
            };
        }

        public override Node VisitChildren(Antlr4.Runtime.Tree.IRuleNode node)
        {
            var buf = new StringBuilder();
            for (var i = 0; i < node.ChildCount; i++)
            {
                var child = node.GetChild(i);
                if (child == null) continue;

                var res = Visit(child);
                if (res == null) continue;

                buf.Append(res.TranslatedValue);
            }

            return new Node()
            {
                TranslatedValue = buf.ToString(),
                Location = GetTokenLocation(node),
            };
        }

        #region Helpers
        private TokenLocation GetTokenLocation(ISyntaxTree node)
        {
            var interval = node.SourceInterval;
            var start = tokenStream.Get(interval.a);
            var end = tokenStream.Get(interval.b);
            return new TokenLocation(start, end);
        }

        private void MakeGlobalScope()
        {
            scopes.Push();

            scopes.Bind("int", new Node()
            {
                TranslatedValue = "int",
                Type = new IntType(),
                IsConst = true,
            });
            scopes.Bind("boolean", new Node()
            {
                TranslatedValue = "bool",
                Type = new BooleanType(),
                IsConst = true,
            });
            scopes.Bind("println", new Node()
            {
                TranslatedValue = "Console.WriteLine",
                Type = new FunctionType()
                {
                    ParameterTypes = new List<Type>()
                    {
                        new StringType(),
                    }
                },
                IsConst = true,
            });
            scopes.Bind("print", new Node()
            {
                TranslatedValue = "Console.Write",
                Type = new FunctionType()
                {
                    ParameterTypes = new List<Type>()
                    {
                        new StringType(),
                    }
                },
                IsConst = true,
            });
        }
        #endregion
    }
}
