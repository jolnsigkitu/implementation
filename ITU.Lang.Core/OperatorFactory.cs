using System;
using System.Collections.Generic;
using ITU.Lang.Core.Translator;
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
            private OperatorDictionary<Type, Func<Node, Node>> dict = new OperatorDictionary<Type, Func<Node, Node>>();

            public void Bind(string op, Type typ, Func<Node, Node> func)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    innerDict = new Dictionary<Type, Func<Node, Node>>();
                    dict.Add(op, innerDict);
                }
                innerDict.TryGetValue(typ, out var value);
                if (value != null)
                {
                    throw new TranspilationException($"Operator '{op}' for type '{typ.AsNativeName()}' already exists");
                }
                innerDict[typ] = func;
            }

            public Node Get(string op, Node node)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    throw new TranspilationException(
                        $"Attempting to use undeclared unary operator '{op}'",
                        node.Location
                    );
                }

                innerDict.TryGetValue(node.Type, out var func);

                if (func == null)
                {
                    throw new TranspilationException(
                        $"Operator '{op}' is not applicable to expression of type '{node.Type.AsNativeName()}'",
                        node.Location
                    );
                }

                return func(node);
            }
        }

        public class BinaryOperatorCollection
        {
            private OperatorDictionary<(Type, Type), Func<Node, Node, Node>> dict = new OperatorDictionary<(Type, Type), Func<Node, Node, Node>>();

            public void Bind(string op, (Type, Type) types, Func<Node, Node, Node> func)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    innerDict = new Dictionary<(Type, Type), Func<Node, Node, Node>>();
                    dict.Add(op, innerDict);
                }
                innerDict.TryGetValue(types, out var value);
                if (value != null)
                {
                    throw new TranspilationException($"Operator '({types.Item1.AsNativeName()}) {op} ({types.Item2.AsNativeName()})' already exists");
                }
                innerDict[types] = func;
            }

            public Node Get(string op, Node node1, Node node2)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    throw new TranspilationException($"Attempting to use undeclared binary operator '{op}'", node1.Location);
                }

                innerDict.TryGetValue((node1.Type, node2.Type), out var func);

                if (func == null)
                {
                    throw new TranspilationException(
                        $"Operator '{op}' is not applicable to expression of type '({node1.Type.AsNativeName()}, {node2.Type.AsNativeName()})'",
                        node1.Location
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

            factory.UnaryPrefix.Bind("!", boolean, (node) => new Node() {
                Type = boolean,
                TranslatedValue = $"!{node.TranslatedValue}",
                Location = node.Location,
            });
            factory.UnaryPrefix.Bind("++", integer, (node) => new Node() {
                Type = integer,
                TranslatedValue = $"++{node.TranslatedValue}",
                Location = node.Location,
            });
            factory.UnaryPrefix.Bind("--", integer, (node) => new Node() {
                Type = integer,
                TranslatedValue = $"--{node.TranslatedValue}",
                Location = node.Location,
            });
            factory.UnaryPrefix.Bind("+", integer, (node) => new Node() {
                Type = integer,
                TranslatedValue = $"+{node.TranslatedValue}",
                Location = node.Location,
            });
            factory.UnaryPrefix.Bind("-", integer, (node) => new Node() {
                Type = integer,
                TranslatedValue = $"-{node.TranslatedValue}",
                Location = node.Location,
            });

            factory.UnaryPostfix.Bind("++", integer, (node) => new Node() {
                Type = integer,
                TranslatedValue = $"{node.TranslatedValue}++",
                Location = node.Location,
            });
            factory.UnaryPostfix.Bind("--", integer, (node) => new Node() {
                Type = integer,
                TranslatedValue = $"{node.TranslatedValue}--",
                Location = node.Location,
            });

            factory.Binary.Bind("+", (str, str), (n1, n2) => new Node() {
                Type = str,
                TranslatedValue = $"{n1.TranslatedValue}+{n2.TranslatedValue}",
                Location = n1.Location,
            });
            factory.Binary.Bind("+", (integer, integer), (n1, n2) => new Node() {
                Type = integer,
                TranslatedValue = $"{n1.TranslatedValue}+{n2.TranslatedValue}",
                Location = n1.Location,
            });
            factory.Binary.Bind("-", (integer, integer), (n1, n2) => new Node() {
                Type = integer,
                TranslatedValue = $"{n1.TranslatedValue}-{n2.TranslatedValue}",
                Location = n1.Location,
            });
            factory.Binary.Bind("*", (integer, integer), (n1, n2) => new Node() {
                Type = integer,
                TranslatedValue = $"{n1.TranslatedValue}*{n2.TranslatedValue}",
                Location = n1.Location,
            });
            factory.Binary.Bind("/", (integer, integer), (n1, n2) => new Node() {
                Type = integer,
                TranslatedValue = $"{n1.TranslatedValue}/{n2.TranslatedValue}",
                Location = n1.Location,
            });
            factory.Binary.Bind("**", (integer, integer), (n1, n2) => new Node() {
                Type = integer,
                TranslatedValue = $"{n1.TranslatedValue}**{n2.TranslatedValue}",
                Location = n1.Location,
            });
            factory.Binary.Bind("&&", (boolean, boolean), (n1, n2) => new Node() {
                Type = boolean,
                TranslatedValue = $"{n1.TranslatedValue}&&{n2.TranslatedValue}",
                Location = n1.Location,
            });
            factory.Binary.Bind("||", (boolean, boolean), (n1, n2) => new Node() {
                Type = boolean,
                TranslatedValue = $"{n1.TranslatedValue}||{n2.TranslatedValue}",
                Location = n1.Location,
            });
            factory.Binary.Bind("^", (boolean, boolean), (n1, n2) => new Node() {
                Type = boolean,
                TranslatedValue = $"{n1.TranslatedValue}^{n2.TranslatedValue}",
                Location = n1.Location,
            });

            return factory;
        }
    }
}
