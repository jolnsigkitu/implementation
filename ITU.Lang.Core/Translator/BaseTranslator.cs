using System.Text;
using System.Collections.Generic;
using System.Linq;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;


using static ITU.Lang.Core.Grammar.LangParser;
using ITU.Lang.Core.Types;
using ITU.Lang.Core.Grammar;
using ITU.Lang.StandardLib;

namespace ITU.Lang.Core.Translator
{
    public partial class Translator : LangBaseVisitor<Node>
    {
        private Scope<Node> scopes = new Scope<Node>();
        private Scope<Type> typeScopes = new Scope<Type>();
        private IList<ClassType> classes = new List<ClassType>();

        private ITokenStream tokenStream;

        public Translator(ITokenStream tokenStream)
        {
            this.tokenStream = tokenStream;

            MakeGlobalScope();
        }

        public override Node VisitProg([NotNull] ProgContext context)
        {
            var res = VisitStatements(context.statements());

            var classes = string.Join("\n\n", this.classes.Select(x =>
            {
                var translatedMembers = x.Members.Select(member =>
                {
                    var (name, (typ, node)) = member;

                    if (typ is FunctionType f)
                    {
                        var isConstructor = name == "constructor";
                        var actualName = isConstructor ? x.Name : name;
                        var returnType = isConstructor ? "" : f.ReturnType.AsTranslatedName();
                        var paramList = f.ParameterTypes.Zip(f.ParameterNames, (t, n) => $"{t.AsTranslatedName()} {n}");
                        var end = f?.IsLambda == true ? ";" : "";
                        return $"public {returnType} {actualName}({string.Join(",", paramList)}){node.TranslatedValue}{end}";
                    }

                    return $"public {typ.AsTranslatedName()} {name}{(!string.IsNullOrEmpty(node?.TranslatedValue) ? $"={node.TranslatedValue}" : "")};";
                });

                return $"class {x.Name} {{\n{string.Join("\n", translatedMembers)}\n}}";
            }));

            return new Node()
            {
                TranslatedValue = "using System;\nusing ITU.Lang.StandardLib;\n" + res.TranslatedValue + "\n" + classes,
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

        private System.IDisposable UseScope()
        {
            return new DisposableScopes(scopes.UseScope(), typeScopes.UseScope());
        }

        private class DisposableScopes : System.IDisposable
        {
            System.IDisposable scope1;
            System.IDisposable scope2;

            public DisposableScopes(System.IDisposable sc1, System.IDisposable sc2)
            {
                scope1 = sc1;
                scope2 = sc2;
            }

            public void Dispose()
            {
                scope1.Dispose();
                scope2.Dispose();
            }
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
                        new IntType(),
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
                        new IntType(),
                    }
                },
                IsConst = true,
            });

            #region signals

            var PushSignal = new ObjectType()
            {
                Members = new Dictionary<string, Type>(),
                Name = "PushSignal<int>",
            };

            PushSignal.Members.Add("Map", new FunctionType()
            {
                ReturnType = PushSignal,
                ParameterNames = new List<string>() { "mapper" },
                ParameterTypes = new List<Type>()
                {
                    new FunctionType()
                    {
                        ParameterNames = new List<string>() { "item" },
                        ParameterTypes = new List<Type>() { new IntType() },
                        ReturnType = new IntType(),
                    }
                },
            });
            PushSignal.Members.Add("Reduce", new FunctionType()
            {
                ReturnType = PushSignal,
                ParameterNames = new List<string>() { "reducer" },
                ParameterTypes = new List<Type>()
                {
                    new FunctionType()
                    {
                        ParameterNames = new List<string>() { "acc", "item" },
                        ParameterTypes = new List<Type>() { new IntType(), new IntType() },
                        ReturnType = new IntType(),
                    }
                },
            });
            PushSignal.Members.Add("Filter", new FunctionType()
            {
                ReturnType = PushSignal,
                ParameterNames = new List<string>() { "filter" },
                ParameterTypes = new List<Type>()
                {
                    new FunctionType()
                    {
                        ParameterNames = new List<string>() { "item" },
                        ParameterTypes = new List<Type>() { new IntType() },
                        ReturnType = new BooleanType(),
                    }
                },
            });
            /* PushSignal.Members.Add("ForEach", new GenericFunctionType() */
            PushSignal.Members.Add("ForEach", new FunctionType()
            {
                // Generics = new List<string>() { "TInput" },
                ParameterNames = new List<string>() { "func" },
                ParameterTypes = new List<Type>()
                {
                    new FunctionType()
                    {
                        // ParameterNames = new List<string>() { "item" },
                        ParameterTypes = new List<Type>() { new IntType() },
                        /* ParameterTypes = new List<Type>() { new GenericType("TInput") } */
                    }
                },
            });

            scopes.Bind("Signal", new Node()
            {
                Type = new ObjectType()
                {
                    Members = new Dictionary<string, Type>()
                    {
                        {"Timer", new FunctionType()
                            {
                                ParameterNames = new List<string>() { "millisecondInterval" },
                                ParameterTypes = new List<Type>() { new IntType() },
                                ReturnType = PushSignal,
                            }
                        },
                    }
                },
            });
            #endregion
        }
        #endregion
    }
}
