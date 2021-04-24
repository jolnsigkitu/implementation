using System.Collections.Generic;
using ITU.Lang.Core.Translator.Nodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    public class Scopes
    {
        public Scope<VariableBinding> Values { get; } = new Scope<VariableBinding>();
        public Scope<TypeBinding> Types { get; } = new Scope<TypeBinding>();

        public Scopes()
        {
            MakeGlobalScope();
        }

        public void Push()
        {
            Values.Push();
            Types.Push();
        }

        public void Pop()
        {
            Values.Pop();
            Types.Pop();
        }

        public System.IDisposable Use()
        {
            return new DisposableScopes(Values.UseScope(), Types.UseScope());
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
            Push();

            Types.Bind("int", new TypeBinding(new IntType()));
            Types.Bind("boolean", new TypeBinding(new BooleanType()));
            Types.Bind("string", new TypeBinding(new StringType()));
            Types.Bind("char", new TypeBinding(new CharType()));
            Types.Bind("any", new TypeBinding(new AnyType()));

            Values.Bind("println", new VariableBinding()
            {
                Name = "Console.WriteLine",
                Type = new FunctionType()
                {
                    ParameterTypes = new List<Type>()
                    {
                        new AnyType(),
                    }
                },
                IsConst = true,
            });
            Values.Bind("print", new VariableBinding()
            {
                Name = "Console.Write",
                Type = new FunctionType()
                {
                    ParameterTypes = new List<Type>()
                    {
                        new AnyType(),
                    }
                },
                IsConst = true,
            });

            #region Signals
            var ChainablePushSignal = new TypeBinding()
            {
                Type = new GenericClassType() { Name = "ChainablePushSignal", GenericIdentifiers = new List<string>() { "In", "Out" } },
                Members = new Dictionary<string, VariableBinding>() {
                    { "Map", new VariableBinding() {
                        Name = "Map",
                        Type = new GenericFunctionType() {
                            IsLambda = true,
                            ReturnType = new SpecificClassType() {
                                Name = "ChainablePushSignal",
                                GenericIdentifiers = new List<string>() { "In", "Out" },
                                SpecificTypes = new Dictionary<string, Type>() { }
                            },
                            ParameterNames = new List<string>() { "mapper" },
                            ParameterTypes = new List<Type>() {
                                new GenericFunctionType() {
                                    IsLambda = false,
                                    ReturnType = new GenericTypeIdentifier("U"),
                                    ParameterNames = new List<string>() { "" },
                                    ParameterTypes = new List<Type>() { new GenericTypeIdentifier("Out") }
                                }
                            },
                            GenericIdentifiers = new List<string>() { "U" }
                        },
                        IsConst = false
                    }},
                    { "Reduce", new VariableBinding() {
                        Name = "Reduce",
                        Type = new GenericFunctionType() {
                            IsLambda = true,
                            ReturnType = new SpecificClassType() {
                                Name = "ChainablePushSignal",
                                GenericIdentifiers = new List<string>() { "In", "Out" },
                                SpecificTypes = new Dictionary<string, Type>() { }
                            },
                            ParameterNames = new List<string>() { "reducer", "defaultResult" },
                            ParameterTypes = new List<Type>() {
                                new GenericFunctionType() {
                                    IsLambda = false,
                                    ReturnType = new GenericTypeIdentifier("U"),
                                    ParameterNames = new List<string>() { "" },
                                    ParameterTypes = new List<Type>() { new GenericTypeIdentifier("U"), new GenericTypeIdentifier("Out") }
                                },
                                new GenericTypeIdentifier("U")
                            },
                            GenericIdentifiers = new List<string>() { "U" }
                        },
                        IsConst = false
                    }},
                    { "Filter", new VariableBinding() {
                        Name = "Filter",
                        Type = new FunctionType() {
                            IsLambda = true,
                            ReturnType = new SpecificClassType() {
                                Name = "ChainablePushSignal",
                                GenericIdentifiers = new List<string>() { "In", "Out" },
                                SpecificTypes = new Dictionary<string, Type>() { }
                            },
                            ParameterNames = new List<string>() { "filter" },
                            ParameterTypes = new List<Type>() {
                                new FunctionType() {
                                    IsLambda = false,
                                    ReturnType = new BooleanType(),
                                    ParameterNames = new List<string>() { "" },
                                    ParameterTypes = new List<Type>() { new GenericTypeIdentifier("Out") }
                                }
                            }
                        },
                        IsConst = false
                    }},
                    { "ForEach", new VariableBinding() {
                        Name = "ForEach",
                        Type = new FunctionType() {
                            IsLambda = true,
                            ReturnType = new SpecificClassType() {
                                Name = "ChainablePushSignal",
                                GenericIdentifiers = new List<string>() { "In", "Out" },
                                SpecificTypes = new Dictionary<string, Type>() { }
                            },
                            ParameterNames = new List<string>() { "function" },
                            ParameterTypes = new List<Type>() {
                                new FunctionType() {
                                    IsLambda = false,
                                    ReturnType = new VoidType(),
                                    ParameterNames = new List<string>() { "" },
                                    ParameterTypes = new List<Type>() { new GenericTypeIdentifier("Out") }
                                }
                            }
                        },
                        IsConst = false
                    }}
                }
            };

            Types.Bind("ChainablePushSignal", ChainablePushSignal);

            var SignalGlobal = new TypeBinding()
            {
                Type = new ClassType()
                {
                    Name = "SignalGlobal",
                },
                Members = new Dictionary<string, VariableBinding>(),
            };

            SignalGlobal.Members.Add("Timer", new VariableBinding()
            {
                Name = "Timer",
                Type = new FunctionType()
                {
                    ParameterNames = new List<string>() { "millisecondInterval" },
                    ParameterTypes = new List<Type>() { new IntType() },
                    ReturnType = ((GenericClassType)ChainablePushSignal.Type).Specify(new Dictionary<string, Type>() { { "In", new IntType() }, { "Out", new IntType() } }),
                }
            });


            Types.Bind("SignalGlobal", SignalGlobal);

            Values.Bind("Signal", new VariableBinding()
            {
                Name = "Signal",
                Type = SignalGlobal.Type,
                Members = SignalGlobal.Members,
            });

            #endregion
        }
    }
}
