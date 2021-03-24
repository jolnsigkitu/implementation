using System.Text;
using System.Collections.Generic;
using System.Reflection;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;


using static ITU.Lang.Core.Grammar.LangParser;
using ITU.Lang.Core.Types;
using ITU.Lang.Core.Grammar;
using ITU.Lang.StandardLib;
using System.Linq;

namespace ITU.Lang.Core.Translator
{
    public partial class Translator : LangBaseVisitor<Node>
    {
        private Scope<Node> scopes = new Scope<Node>();
        private Scope<Type> typeScopes = new Scope<Type>();

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
                TranslatedValue = "using System; using ITU.Lang.StandardLib;" + res.TranslatedValue,
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
        private Node VisitIfExists(IParseTree ctx)
        {
            return ctx != null ? Visit(ctx) : null;
        }

        #region Helpers
        private TokenLocation GetTokenLocation(ISyntaxTree node)
        {
            var interval = node.SourceInterval;
            var start = tokenStream.Get(interval.a);
            var end = tokenStream.Get(interval.b);
            return new TokenLocation(start, end);
        }

        private void PushScope()
        {
            scopes.Push();
            typeScopes.Push();
        }

        private void PopScope()
        {
            scopes.Pop();
            typeScopes.Pop();
        }

        private void MakeGlobalScope()
        {
            PushScope();

            typeScopes.Bind("int", new IntType());
            typeScopes.Bind("boolean", new BooleanType());
            typeScopes.Bind("string", new StringType());
            typeScopes.Bind("char", new CharType());

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

            /* var stdLib = typeof(PushSignal<int>).Assembly.DefinedTypes.Select(x => (x.Name, x.GetMethods()));


            typeof(PushSignal<int>).Assembly.DefinedTypes.Select((ti) => {
                var name = ti.Name;
                var constructorArgs = ti.GetConstructor();
            }); */
        }
        #endregion
    }
}
