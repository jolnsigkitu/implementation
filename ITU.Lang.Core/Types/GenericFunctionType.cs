using System.Collections.Generic;
using System.Linq;

namespace ITU.Lang.Core.Types
{
    public class GenericFunctionType : FunctionType, IGenericType<FunctionType>
    {
        public GenericFunctionType() { }

        public GenericFunctionType(FunctionType func)
        {
            IsLambda = func.IsLambda;
            ReturnType = func.ReturnType;
            ParameterNames = func.ParameterNames;
            ParameterTypes = func.ParameterTypes;
        }

        public IList<string> GenericIdentifiers { get; set; }

        public IDictionary<string, Type> Resolve(IList<Type> expressions)
        {
            var specificTypes = new Dictionary<string, Type>();

            foreach (var (paramType, exprType) in ParameterTypes.Zip(expressions))
            {
                if (paramType is GenericTypeIdentifier id)
                {
                    if (!specificTypes.TryGetValue(id.Identifier, out var existingType))
                    {
                        specificTypes.Add(id.Identifier, exprType);
                    }
                    else if (!existingType.Equals(exprType))
                    {
                        throw new TranspilationException($"Tried to resolve generic '{id.Identifier}' to both types '{existingType}' and '{exprType}'");
                    }
                }

                else if (paramType is GenericFunctionType generic && exprType is FunctionType exprFun)
                {
                    foreach (var (key, val) in generic.Resolve(exprFun.ParameterTypes))
                    {
                        if (!specificTypes.TryGetValue(key, out var existingVal))
                        {
                            specificTypes.Add(key, val);
                        }
                        else if (!existingVal.Equals(val))
                        {
                            throw new TranspilationException($"Tried to resolve generic '{key}' to both types '{existingVal}' and '{val}'");
                        }
                    }
                }

                // const b = <T, U, V, ...>(a: int, b: (T) => (U) => (V) => ... => void) => ...
                // const c = (x: boolean) => void ...
                // b(17, c) ????
            }

            return specificTypes;
        }

        public FunctionType Specify(IDictionary<string, Type> specificTypes)
        {
            var returnType = SpecifyFuncType(ReturnType, specificTypes);

            var paramTypes = ParameterTypes.Select((typ) => SpecifyFuncType(typ, specificTypes)).ToList();

            return new FunctionType()
            {
                ReturnType = returnType,
                ParameterNames = ParameterNames,
                ParameterTypes = paramTypes,
                IsLambda = IsLambda
            };
        }

        private Type SpecifyFuncType(Type type, IDictionary<string, Type> specificTypes)
        {
            if (type is GenericFunctionType generic)
            {
                return generic.Specify(specificTypes);
            }

            if (type is GenericTypeIdentifier identifier)
            {
                if (!(specificTypes.TryGetValue(identifier.Identifier, out var specType)))
                {
                    throw new TranspilationException($"Cannot find specification of generic type {identifier.Identifier}");
                }
                return specType;
            }

            return type;
        }
    }
}
