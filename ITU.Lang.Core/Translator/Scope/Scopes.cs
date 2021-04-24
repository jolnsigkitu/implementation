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
            var PushSignal = new TypeBinding()
            {
                Type = new GenericClassType()
                {
                    Name = "PushSignal",
                    GenericIdentifiers = new List<string>() { "TInput" },
                },
                Members = new Dictionary<string, VariableBinding>(),
            };

            PushSignal.Members.Add("Map", new VariableBinding()
            {
                Name = "Map",
                Type = new GenericFunctionType()
                {
                    GenericIdentifiers = new List<string>() { "TResult" },
                    IsLambda = false,
                    ReturnType = PushSignal.Type,
                    ParameterNames = new List<string>() {
                        "mapper"
                    },
                    ParameterTypes = new List<Type>() {
                        new FunctionType()
                        {
                            IsLambda = false,
                            ReturnType = new GenericTypeIdentifier("TResult"),
                            ParameterNames = new List<string>() {"value"},
                            ParameterTypes = new List<Type>() {new GenericTypeIdentifier("TInput")},
                        }
                    }
                }
            });

            Types.Bind("PushSignal", PushSignal);

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
                    ReturnType = PushSignal.Type,
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
