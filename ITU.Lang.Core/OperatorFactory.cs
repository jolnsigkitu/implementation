using System;
using System.Collections.Generic;

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
            private OperatorDictionary<Type, Type> dict = new OperatorDictionary<Type, Type>();

            public void Bind(string op, Type returnType, Type inputType)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    innerDict = new Dictionary<Type, Type>();
                    dict.Add(op, innerDict);
                }
                innerDict.TryGetValue(returnType, out var value);
                if (value != null)
                {
                    throw new TranspilationException($"Operator '{op}' for type '{returnType.AsNativeName()}' already exists");
                }
                innerDict[returnType] = inputType;
            }

            public Type Get(string op, Type inputType, TokenLocation loc = null)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    throw new TranspilationException(
                        $"Attempting to use undeclared unary operator '{op}'",
                        loc
                    );
                }

                innerDict.TryGetValue(inputType, out var returnType);

                if (returnType == null)
                {
                    throw new TranspilationException(
                        $"Operator '{op}' is not applicable to expression of type '{inputType.AsNativeName()}'",
                        loc
                    );
                }

                return returnType;
            }
        }

        public class BinaryOperatorCollection
        {
            private OperatorDictionary<(Type, Type), Type> dict = new OperatorDictionary<(Type, Type), Type>();

            public void Bind(string op, (Type, Type) inputTypes, Type returnType)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    innerDict = new Dictionary<(Type, Type), Type>();
                    dict.Add(op, innerDict);
                }
                innerDict.TryGetValue(inputTypes, out var value);
                if (value != null)
                {
                    throw new TranspilationException($"Operator '({inputTypes.Item1.AsNativeName()}) {op} ({inputTypes.Item2.AsNativeName()})' already exists");
                }
                innerDict[inputTypes] = returnType;
            }

            public Type Get(string op, Type inputType1, Type inputType2, TokenLocation loc = null)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    throw new TranspilationException($"Attempting to use undeclared binary operator '{op}'", loc);
                }

                innerDict.TryGetValue((inputType1, inputType2), out var returnType);

                if (returnType == null)
                {
                    throw new TranspilationException(
                        $"Operator '{op}' is not applicable to expression of type '({inputType1.AsNativeName()}, {inputType2.AsNativeName()})'",
                        loc
                    );
                }

                return returnType;
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
            factory.UnaryPrefix.Bind("!", boolean, boolean);
            #endregion

            #region Unary algebraic
            factory.UnaryPrefix.Bind("++", integer, integer);
            factory.UnaryPrefix.Bind("--", integer, integer);
            factory.UnaryPrefix.Bind("+", integer, integer);
            factory.UnaryPrefix.Bind("-", integer, integer);

            factory.UnaryPostfix.Bind("++", integer, integer);
            factory.UnaryPostfix.Bind("--", integer, integer);
            #endregion

            #region Binary algebraic
            factory.Binary.Bind("+", (str, str), str);
            factory.Binary.Bind("+", (integer, integer), integer);
            factory.Binary.Bind("-", (integer, integer), integer);
            factory.Binary.Bind("*", (integer, integer), integer);
            factory.Binary.Bind("/", (integer, integer), integer);
            factory.Binary.Bind("%", (integer, integer), integer);
            #endregion

            #region Binary Boolean
            factory.Binary.Bind("==", (boolean, boolean), boolean);
            factory.Binary.Bind("&&", (boolean, boolean), boolean);
            factory.Binary.Bind("||", (boolean, boolean), boolean);
            factory.Binary.Bind("^", (boolean, boolean), boolean);

            factory.Binary.Bind("==", (integer, integer), boolean);
            factory.Binary.Bind("<", (integer, integer), boolean);
            factory.Binary.Bind(">", (integer, integer), boolean);
            factory.Binary.Bind("<=", (integer, integer), boolean);
            factory.Binary.Bind(">=", (integer, integer), boolean);
            #endregion

            return factory;
        }
    }
}
