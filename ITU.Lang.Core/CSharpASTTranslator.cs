using System.Text;
using System.Linq;
using Antlr4.Runtime.Misc;
using static LangParser;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ITU.Lang.Core.Types;
using System.Collections.Generic;

namespace ITU.Lang.Core
{
    public class CSharpASTTranslator : LangBaseVisitor<CSharpASTNode>
    {
        private Scope<CSharpASTNode> scopes = new Scope<CSharpASTNode>();

        private ITokenStream tokenStream;

        public CSharpASTTranslator(ITokenStream tokenStream)
        {
            this.tokenStream = tokenStream;

            MakeGlobalScope();
        }

        public override CSharpASTNode VisitProg([NotNull] ProgContext context)
        {
            var res = VisitStatements(context.statements());

            return new CSharpASTNode()
            {
                TranslatedValue = "using System;" + res.TranslatedValue,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitChildren(Antlr4.Runtime.Tree.IRuleNode node)
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

            return new CSharpASTNode()
            {
                TranslatedValue = buf.ToString(),
                Location = GetTokenLocation(node),
            };
        }

        #region Statements
        public override CSharpASTNode VisitSemiStatement([NotNull] SemiStatementContext context)
        {
            var children = VisitChildren(context);

            return new CSharpASTNode()
            {
                TranslatedValue = children.TranslatedValue + ";",
                Location = GetTokenLocation(context),
                Type = children.Type,
            };
        }

        public override CSharpASTNode VisitBlock([NotNull] BlockContext context)
        {
            Type blockType = null;
            var hasReturnStatement = false;
            var buf = new StringBuilder();

            scopes.Push();

            foreach (var statement in context.statements()?.statement() ?? new StatementContext[0])
            {
                if (hasReturnStatement) throw new TranspilationException("Statements after return will be ignored");

                var returnStatement = statement.returnStatement();

                CSharpASTNode res;
                if (returnStatement != null)
                {
                    res = VisitReturnStatement(returnStatement);
                    blockType = res.Type;
                    hasReturnStatement = true;
                }
                else
                {
                    res = Visit(statement);
                }

                buf.Append(res.TranslatedValue);
            }

            scopes.Pop();

            return new CSharpASTNode()
            {
                TranslatedValue = "{" + buf.ToString() + "}",
                Location = GetTokenLocation(context),
                Type = blockType,
            };
        }

        public override CSharpASTNode VisitIfStatement([NotNull] IfStatementContext context)
        {
            var expr = this.VisitExpr(context.expr());
            expr.AssertType(new BooleanType());

            var block = this.VisitBlock(context.block()).TranslatedValue;
            var elseIf = string.Join("", context.elseIfStatement().Select(x => VisitElseIfStatement(x).TranslatedValue));
            var elseRes = context.elseStatement() != null ? this.VisitElseStatement(context.elseStatement()).TranslatedValue : "";

            return new CSharpASTNode()
            {
                TranslatedValue = "if(" + expr.TranslatedValue + ")" + block + elseIf + elseRes,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitElseIfStatement([NotNull] ElseIfStatementContext context)
        {
            var expr = this.VisitExpr(context.expr());

            expr.AssertType(new BooleanType());

            var block = this.VisitBlock(context.block());

            return new CSharpASTNode()
            {
                TranslatedValue = "else if(" + expr.TranslatedValue + ")" + block.TranslatedValue,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitElseStatement([NotNull] ElseStatementContext context)
        {
            return new CSharpASTNode()
            {
                TranslatedValue = "else" + base.VisitElseStatement(context).TranslatedValue,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitVardec([NotNull] VardecContext context)
        {
            var typedName = context.typedName();
            var name = typedName.Name().GetText();
            var typeAnnotationName = typedName.typeAnnotation()?.Name()?.GetText();
            Type typeAnnotation = null;

            var expr = VisitExpr(context.expr());

            if (expr.Type.Equals(new VoidType()))
            {
                throw new TranspilationException("Cannot assign variables to values of type 'void'!", GetTokenLocation(context));
            }

            if (typeAnnotation != null)
            {
                typeAnnotation = scopes.GetBinding(typeAnnotationName).Type;
                expr.AssertType(typeAnnotation);
            }

            var binding = new CSharpASTNode()
            {
                TranslatedValue = name,
                Type = typeAnnotation ?? expr.Type,
                IsConst = context.Const() != null,
            };

            scopes.Bind(name, binding);
            var constPrefix = binding.IsConst ? "const " : "";
            return new CSharpASTNode()
            {
                TranslatedValue = $"{constPrefix}{binding.Type.AsTranslatedName()} {name} = {expr.TranslatedValue}",
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitAssign([NotNull] AssignContext context)
        {
            var name = context.Name().GetText();
            if (!scopes.HasBinding(name))
            {
                throw new TranspilationException($"Variable {name} was not declared!", GetTokenLocation(context.Name()));
            }

            var expr = VisitExpr(context.expr());
            var cur = scopes.GetBinding(name);

            if (cur.IsConst)
            {
                throw new TranspilationException($"Cannot update const variable '{name}'!", GetTokenLocation(context.Name()));
            }

            if (!cur.IsType(expr))
            {
                var msg = $"Cannot assign value of type '{expr.Type.AsNativeName()}' to variable '{name}' of type '{cur.Type.AsNativeName()}'";
                throw new TranspilationException(msg, GetTokenLocation(context));
            }

            scopes.Rebind(name, expr);

            return new CSharpASTNode()
            {
                TranslatedValue = $"{name} = {expr.TranslatedValue}",
                Location = GetTokenLocation(context),
            };
        }

        #endregion

        #region Expressions
        public override CSharpASTNode VisitExpr([NotNull] ExprContext context)
        {
            var leftParen = context.LeftParen()?.GetText() ?? "";
            var rightParen = context.RightParen()?.GetText() ?? "";

            var buf = new StringBuilder();
            Type lastSeenType = null;
            var hasVisitedFirstChild = false;

            foreach (var child in context.children)
            {
                if (child == null) continue;

                var res = Visit(child);

                if (res == null) continue;

                if (!hasVisitedFirstChild)
                {
                    hasVisitedFirstChild = true;
                    lastSeenType = res.Type;
                }
                else if (!lastSeenType.Equals(res.Type))
                {
                    var msg = $"Type mismatch: Expected expression '{res.TranslatedValue}' of type '{res.Type.AsNativeName()}' to be of type '{lastSeenType?.AsNativeName()}'";
                    throw new TranspilationException(msg, GetTokenLocation(child));
                }

                buf.Append(res.TranslatedValue);
            }

            if (lastSeenType == null)
            {
                throw new TranspilationException("Could not derive type of expression", GetTokenLocation(context));
            }

            return new CSharpASTNode()
            {
                TranslatedValue = leftParen + buf.ToString() + rightParen,
                Type = lastSeenType,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitOperator([NotNull] OperatorContext context)
        {
            return new CSharpASTNode()
            {
                TranslatedValue = context.GetText(),
                Type = new IntType(),
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitAccess([NotNull] AccessContext context)
        {
            var name = context.GetText();
            var res = scopes.GetBinding(name);
            if (!scopes.HasBinding(name))
            {
                throw new TranspilationException("Variable '" + name + "' was not declared before accessing!");
            }

            return res;
        }

        public override CSharpASTNode VisitBool([NotNull] BoolContext context)
        {
            return new CSharpASTNode()
            {
                TranslatedValue = (context.False() ?? context.True()).GetText(),
                Type = new BooleanType(),
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitInteger([NotNull] IntegerContext context)
        {
            return new CSharpASTNode()
            {
                TranslatedValue = context.Int().GetText(),
                Type = new IntType(),
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitStringLiteral([NotNull] StringLiteralContext context)
        {
            var content = context.StringLiteral().GetText();

            return new CSharpASTNode()
            {
                TranslatedValue = content,
                Type = new StringType(),
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitCharLiteral([NotNull] CharLiteralContext context)
        {
            var content = context.CharLiteral().GetText();

            return new CSharpASTNode()
            {
                TranslatedValue = content,
                Type = new CharType(),
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitFunction([NotNull] FunctionContext context)
        {
            var args = context.functionArguments();

            var paramNames = args.Name().Select(p => p.GetText());
            var paramTypes = args.typeAnnotation().Select(x =>
            {
                var name = x.Name().GetText();
                if (scopes.HasBinding(name))
                {
                    return scopes.GetBinding(name);
                }

                if (name == "void")
                {
                    throw new TranspilationException("Cannot use void as a parameter type", GetTokenLocation(x));
                }
                throw new TranspilationException("Type '" + name + "' was not declared before used in function argument!", GetTokenLocation(context));
            }).ToList();

            var body = Visit(((IParseTree)context.expr()) ?? context.block());

            var returnTypeName = context?.typeAnnotation()?.Name()?.GetText();

            var returnType = body.Type;

            if (returnTypeName != null)
            {
                returnType = returnTypeName == "void" ? new VoidType() : scopes.GetBinding(returnTypeName).Type;
            }

            if (body.Type != null)
            {
                body.AssertType(returnType);
            }

            var functionType = new FunctionType()
            {
                ReturnType = returnType,
                ParameterTypes = paramTypes.Select(p => p.Type).ToList(),
            };

            return new CSharpASTNode()
            {
                TranslatedValue = $"({string.Join(",", paramNames)}) => {body.TranslatedValue}",
                Location = GetTokenLocation(context),
                Type = functionType,
            };
        }

        public override CSharpASTNode VisitInvokeFunction([NotNull] InvokeFunctionContext context)
        {
            var name = context.Name().GetText();

            var function = scopes.GetBinding(name);

            if (!(function?.Type is FunctionType))
            {
                throw new TranspilationException($"Cannot call non-invokable '{name}'", GetTokenLocation(context));
            }

            var exprs = context.expr().Select(expr => VisitExpr(expr));

            var exprTypes = exprs.Select(expr => expr.Type).ToList();
            var funcType = (FunctionType)function.Type;
            var paramTypes = funcType.ParameterTypes;

            if (!exprTypes.Equals(paramTypes))
            {
                var exprCount = exprTypes.Count;
                var paramCount = paramTypes.Count;

                if (exprCount != paramCount)
                {
                    throw new TranspilationException($"Function '{name}' takes {paramCount} parameters, got {exprCount}", GetTokenLocation(context));
                }

                for (int i = 0; i > exprCount; i++)
                {
                    var expr = exprTypes[i];
                    var param = paramTypes[i];
                    if (!expr.Equals(param))
                    {
                        throw new TranspilationException($"Function '{name}' could not be invoked: parameter {i} must be of type '{param}', but was '{expr}'", GetTokenLocation(context));
                    }
                }
            }

            var exprText = string.Join(",", exprs.Select(expr => expr.TranslatedValue));

            // Construct type of what the call would be, so that we can compare it to the variable in scope of Name
            return new CSharpASTNode()
            {
                TranslatedValue = $"{function.TranslatedValue}({exprText})",
                Type = funcType.ReturnType,
                Location = GetTokenLocation(context),
            };
        }

        public override CSharpASTNode VisitTerm([NotNull] TermContext context)
        {
            return Visit(context.GetRuleContext<ParserRuleContext>(0));
        }

        public override CSharpASTNode VisitLiteral([NotNull] LiteralContext context)
        {
            return Visit(context.GetRuleContext<ParserRuleContext>(0));
        }

        public override CSharpASTNode VisitReturnStatement([NotNull] ReturnStatementContext context)
        {
            var expr = Visit(context.expr());

            return new CSharpASTNode()
            {
                TranslatedValue = $"return {expr.TranslatedValue};",
                Location = GetTokenLocation(context),
                Type = expr.Type,
            };
        }
        #endregion

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

            scopes.Bind("int", new CSharpASTNode()
            {
                TranslatedValue = "int",
                Type = new IntType(),
                IsConst = true,
            });
            scopes.Bind("boolean", new CSharpASTNode()
            {
                TranslatedValue = "bool",
                Type = new BooleanType(),
                IsConst = true,
            });
            scopes.Bind("println", new CSharpASTNode()
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
            scopes.Bind("print", new CSharpASTNode()
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
