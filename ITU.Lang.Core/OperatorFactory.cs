using System;
using System.Collections.Generic;

using ITU.Lang.Core.NewTranslator.Nodes.Expressions;
using ITU.Lang.Core.NewTranslator.Nodes;
using ITU.Lang.Core.Types;
using Type = ITU.Lang.Core.Types.Type;

namespace ITU.Lang.Core.Operators
{
    public class OperatorFactory
    {
        public UnaryOperatorCollection UnaryPrefix { get; } = new UnaryOperatorCollection();
        public UnaryOperatorCollection UnaryPostfix { get; } = new UnaryOperatorCollection();
        public BinaryOperatorCollection Binary { get; } = new BinaryOperatorCollection();

        public class OperatorDictionary<TKey, TVal> : Dictionary<string, Dictionary<TKey, TVal>> { }
        public class UnaryOperatorCollection
        {
            private OperatorDictionary<Type, Func<ExprNode, ExprNode>> dict = new OperatorDictionary<Type, Func<ExprNode, ExprNode>>();

            public void Bind(string op, Type typ, Func<ExprNode, ExprNode> func)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    innerDict = new Dictionary<Type, Func<ExprNode, ExprNode>>();
                    dict.Add(op, innerDict);
                }
                innerDict.TryGetValue(typ, out var value);
                if (value != null)
                {
                    throw new TranspilationException($"Operator '{op}' for type '{typ.AsNativeName()}' already exists");
                }
                innerDict[typ] = func;
            }

            public ExprNode Get(string op, ExprNode node)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    throw new TranspilationException(
                        $"Attempting to use undeclared unary operator '{op}'"
                    );
                }

                innerDict.TryGetValue(node.Type, out var func);

                if (func == null)
                {
                    throw new TranspilationException(
                        $"Operator '{op}' is not applicable to expression of type '{node.Type.AsNativeName()}'"
                    );
                }

                return func(node);
            }
        }

        public class BinaryOperatorCollection
        {
            private OperatorDictionary<(Type, Type), Func<ExprNode, ExprNode, BinaryOperatorNode>> dict = new OperatorDictionary<(Type, Type), Func<ExprNode, ExprNode, BinaryOperatorNode>>();

            public void Bind(string op, (Type, Type) types, Func<ExprNode, ExprNode, BinaryOperatorNode> func)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    innerDict = new Dictionary<(Type, Type), Func<ExprNode, ExprNode, BinaryOperatorNode>>();
                    dict.Add(op, innerDict);
                }
                innerDict.TryGetValue(types, out var value);
                if (value != null)
                {
                    throw new TranspilationException($"Operator '({types.Item1.AsNativeName()}) {op} ({types.Item2.AsNativeName()})' already exists");
                }
                innerDict[types] = func;
            }

            public ExprNode Get(string op, ExprNode node1, ExprNode node2)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    throw new TranspilationException($"Attempting to use undeclared binary operator '{op}'");
                }

                System.Console.WriteLine(node1 + ", " + node2);
                innerDict.TryGetValue((node1.Type, node2.Type), out var func);

                if (func == null)
                {
                    throw new TranspilationException(
                        $"Operator '{op}' is not applicable to expression of type '({node1.Type.AsNativeName()}, {node2.Type.AsNativeName()})'"
                    );
                }

                return func(node1, node2);
            }
        }
    }

    public class Operators
    {
        public static OperatorFactory InitializeOperators(OperatorFactory factory)
        {
            var boolean = new BooleanType();
            var integer = new IntType();
            var str = new StringType();

            #region Unary boolean
            factory.UnaryPrefix.Bind("!", boolean,
                (n1) => new UnaryPreOperatorNode("!", n1, str, n1.Context)
            );
            #endregion

            #region Unary algebraic
            factory.UnaryPrefix.Bind("++", integer,
                (n1) => new UnaryPreOperatorNode("++", n1, integer, n1.Context)
            );
            factory.UnaryPrefix.Bind("--", integer,
                (n1) => new UnaryPreOperatorNode("--", n1, integer, n1.Context)
            );
            factory.UnaryPrefix.Bind("+", integer,
                (n1) => new UnaryPreOperatorNode("+", n1, integer, n1.Context)
            );
            factory.UnaryPrefix.Bind("-", integer,
                (n1) => new UnaryPreOperatorNode("-", n1, integer, n1.Context)
            );

            factory.UnaryPostfix.Bind("++", integer,
                (n1) => new UnaryPostOperatorNode("++", n1, integer, n1.Context)
            );
            factory.UnaryPostfix.Bind("--", integer,
                (n1) => new UnaryPostOperatorNode("--", n1, integer, n1.Context)
            );
            #endregion

            #region Binary algebraic
            factory.Binary.Bind("+", (str, str),
                (n1, n2) => new BinaryOperatorNode("+", n1, n2, str, n1.Context)
            );
            factory.Binary.Bind("+", (integer, integer),
                (n1, n2) => new BinaryOperatorNode("+", n1, n2, integer, n1.Context)
            );
            factory.Binary.Bind("-", (integer, integer),
                (n1, n2) => new BinaryOperatorNode("-", n1, n2, integer, n1.Context)
            );
            factory.Binary.Bind("*", (integer, integer),
                (n1, n2) => new BinaryOperatorNode("*", n1, n2, integer, n1.Context)
            );
            factory.Binary.Bind("/", (integer, integer),
                (n1, n2) => new BinaryOperatorNode("/", n1, n2, integer, n1.Context)
            );
            factory.Binary.Bind("%", (integer, integer),
                (n1, n2) => new BinaryOperatorNode("%", n1, n2, integer, n1.Context)
            );
            #endregion

            #region Binary Boolean
            factory.Binary.Bind("==", (boolean, boolean),
                (n1, n2) => new BinaryOperatorNode("==", n1, n2, boolean, n1.Context)
            );
            factory.Binary.Bind("&&", (boolean, boolean),
                (n1, n2) => new BinaryOperatorNode("&&", n1, n2, boolean, n1.Context)
            );
            factory.Binary.Bind("||", (boolean, boolean),
                (n1, n2) => new BinaryOperatorNode("||", n1, n2, boolean, n1.Context)
            );
            factory.Binary.Bind("^", (boolean, boolean),
                (n1, n2) => new BinaryOperatorNode("^", n1, n2, boolean, n1.Context)
            );

            factory.Binary.Bind("==", (integer, integer),
                (n1, n2) => new BinaryOperatorNode("==", n1, n2, boolean, n1.Context)
            );
            factory.Binary.Bind("<", (integer, integer),
                (n1, n2) => new BinaryOperatorNode("<", n1, n2, boolean, n1.Context)
            );
            factory.Binary.Bind(">", (integer, integer),
                (n1, n2) => new BinaryOperatorNode(">", n1, n2, boolean, n1.Context)
            );
            factory.Binary.Bind("<=", (integer, integer),
                (n1, n2) => new BinaryOperatorNode("<=", n1, n2, boolean, n1.Context)
            );
            factory.Binary.Bind(">=", (integer, integer),
                (n1, n2) => new BinaryOperatorNode(">=", n1, n2, boolean, n1.Context)
            );
            #endregion

            return factory;
        }
    }
}
