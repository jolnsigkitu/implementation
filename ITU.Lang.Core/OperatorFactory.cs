using System;
using System.Collections.Generic;

using ITU.Lang.Core.Types;
using IType = ITU.Lang.Core.Types.IType;

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
            private OperatorDictionary<IType, IType> dict = new OperatorDictionary<IType, IType>();

            public void Bind(string op, IType returnType, IType inputType)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    innerDict = new Dictionary<IType, IType>();
                    dict.Add(op, innerDict);
                }
                innerDict.TryGetValue(returnType, out var value);
                if (value != null)
                {
                    throw new TranspilationException($"Operator '{op}' for type '{returnType.AsNativeName()}' already exists");
                }
                innerDict[returnType] = inputType;
            }

            public IType Get(string op, IType inputType, TokenLocation loc = null)
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
            private OperatorDictionary<(IType, IType), IType> dict = new OperatorDictionary<(IType, IType), IType>();

            public void Bind(string op, (IType, IType) inputTypes, IType returnType)
            {
                dict.TryGetValue(op, out var innerDict);
                if (innerDict == null)
                {
                    innerDict = new Dictionary<(IType, IType), IType>();
                    dict.Add(op, innerDict);
                }
                innerDict.TryGetValue(inputTypes, out var value);
                if (value != null)
                {
                    throw new TranspilationException($"Operator '({inputTypes.Item1.AsNativeName()}) {op} ({inputTypes.Item2.AsNativeName()})' already exists");
                }
                innerDict[inputTypes] = returnType;
            }

            public IType Get(string op, IType inputType1, IType inputType2, TokenLocation loc = null)
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
