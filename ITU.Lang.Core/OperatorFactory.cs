using System;
using System.Collections.Generic;
using ITU.Lang.Core.Translator;
using Type = ITU.Lang.Core.Types.Type;

namespace ITU.Lang.Core
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
                var innerDict = dict[op];
                if (innerDict == null)
                {
                    innerDict = new Dictionary<Type, Func<Node, Node>>();
                }
                if (innerDict[typ] != null)
                {
                    // TODO: Better error message
                    throw new TranspilationException("Cannot override operator");
                }
                innerDict[typ] = func;
            }

            public Node Get(string op, Node node)
            {
                var innerDict = dict[op];
                if (innerDict == null)
                {
                    // TODO: Better error message
                    throw new TranspilationException("Attempting to use undeclared operator");
                }

                var func = innerDict[node.Type];

                if (func == null)
                {
                    throw new TranspilationException("Operator '' is not applicable to expression of type ''");
                }

                return func(node);
            }
        }

        public class BinaryOperatorCollection
        {
            private OperatorDictionary<(Type, Type), Func<Node, Node, Node>> dict = new OperatorDictionary<(Type, Type), Func<Node, Node, Node>>();

            public void Bind(string op, (Type, Type) typs, Func<Node, Node, Node> func)
            {
                var innerDict = dict[op];
                if (innerDict == null)
                {
                    innerDict = new Dictionary<(Type, Type), Func<Node, Node, Node>>();
                }
                if (innerDict[typs] != null)
                {
                    // TODO: Better error message
                    throw new TranspilationException("Cannot override operator");
                }
                innerDict[typs] = func;
            }

            public Node Get(string op, Node node1, Node node2)
            {
                var innerDict = dict[op];
                if (innerDict == null)
                {
                    // TODO: Better error message
                    throw new TranspilationException("Attempting to use undeclared operator");
                }

                var func = innerDict[(node1.Type, node2.Type)];

                if (func == null)
                {
                    throw new TranspilationException("Operator '' is not applicable to expression of type ''");
                }

                return func(node1, node2);
            }
        }
    }
}
