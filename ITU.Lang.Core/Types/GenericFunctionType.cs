using System.Collections.Generic;
using System.Linq;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Core.Types
{
    public class GenericFunctionType : FunctionType, IGenericType<FunctionType>
    {
        public IList<string> GenericIdentifiers { get; set; }
        public GenericFunctionType() { }

        public GenericFunctionType(FunctionType func, IList<string> genericIdentifiers)
        {
            IsLambda = func.IsLambda;
            ReturnType = func.ReturnType;
            ParameterNames = func.ParameterNames;
            ParameterTypes = func.ParameterTypes;
            GenericIdentifiers = genericIdentifiers;
        }

        public IDictionary<string, Type> Resolve(IEnumerable<Type> parameterExpressions)
        {
            var specificTypes = new Dictionary<string, Type>();

            foreach (var (paramType, exprType) in ParameterTypes.Zip(parameterExpressions))
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
            }

            return specificTypes;
        }

        public FunctionType Specify(IDictionary<string, Type> specificTypes)
        {
            System.Console.WriteLine(specificTypes != null ? string.Join(", ", specificTypes) : "null");
            System.Console.WriteLine(specificTypes.Count);
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
                    System.Console.WriteLine(string.Join(",", specificTypes));
                    throw new TranspilationException($"Cannot specify generic type '{identifier.Identifier}' in {AsNativeName()}");
                }
                return specType;
            }

            return type;
        }

        public override string ToString() => $"new GenericFunctionType(){{ IsLambda = {(IsLambda ? "true" : "false")}, ReturnType = {ReturnType}, ParameterNames = new List<string>() {{ \"{string.Join("\", \"", ParameterNames ?? new string[0])}\" }}, ParameterTypes = new List<Type>() {{ {string.Join(", ", ParameterTypes ?? new Type[0])} }}, GenericIdentifiers = new List<string>() {{ \"{string.Join("\", \"", GenericIdentifiers ?? new string[0])}\" }} }}";
    }
}
