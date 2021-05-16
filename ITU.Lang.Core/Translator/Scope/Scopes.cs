using System.Collections.Generic;
using ITU.Lang.Core.Translator.Nodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.Translator
{
    public class Scopes
    {
        public Scope<VariableBinding> Values { get; } = new Scope<VariableBinding>();
        public Scope<IType> Types { get; } = new Scope<IType>();

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

        public override string ToString() => $"{{ Values: {Values}, Types: {Types} }}";

        private void MakeGlobalScope()
        {
            Push();

            Types.Bind("int", new IntType());
            Types.Bind("double", new DoubleType());
            Types.Bind("boolean", new BooleanType());
            Types.Bind("string", new StringType());
            Types.Bind("char", new CharType());
            Types.Bind("any", new AnyType());

            Values.Bind("println", new VariableBinding()
            {
                Name = "Console.WriteLine",
                Type = new FunctionType()
                {
                    ParameterTypes = new List<IType>()
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
                    ParameterTypes = new List<IType>()
                    {
                        new AnyType(),
                    }
                },
                IsConst = true,
            });

            #region Signals
            var members = new Dictionary<string, IType>();

            var ChainablePushSignal = new GenericClassWrapper(
                new ClassType()
                {
                    Name = "ChainablePushSignal",
                    Members = members,
                },
                new Dictionary<string, IType>() {
                    { "In", new GenericTypeIdentifier("In") },
                    { "Out", new GenericTypeIdentifier("Out") }
                },
                new List<string>() { "In", "Out" }
            );

            members.Add("Map", new GenericFunctionWrapper(
                new FunctionType()
                {
                    IsLambda = true,
                    ReturnType = ChainablePushSignal,
                    ParameterNames = new List<string>() { "mapper" },
                    ParameterTypes = new List<IType>() {
                        new FunctionType()
                        {
                            IsLambda = false,
                            ReturnType = new GenericTypeIdentifier("U"),
                            ParameterNames = new List<string>() { "" },
                            ParameterTypes = new List<IType>() { new GenericTypeIdentifier("Out") }
                        }
                    }
                },
                new Dictionary<string, IType>() { { "U", new GenericTypeIdentifier("U") } },
                new List<string>() { "U" }
            ));

            members.Add("Reduce", new GenericFunctionWrapper(
                new FunctionType()
                {
                    IsLambda = true,
                    ReturnType = ChainablePushSignal,
                    ParameterNames = new List<string>() { "reducer", "defaultResult" },
                    ParameterTypes = new List<IType>() {
                        new FunctionType(){
                            IsLambda = false,
                            ReturnType = new GenericTypeIdentifier("U"),
                            ParameterNames = new List<string>() { "" },
                            ParameterTypes = new List<IType>() { new GenericTypeIdentifier("U"), new GenericTypeIdentifier("Out") }
                        },
                        new GenericTypeIdentifier("U")
                    }
                },
                new Dictionary<string, IType>() {
                    { "U", new GenericTypeIdentifier("U") }
                },
                new List<string>() { "U" }
            ));
            members.Add("Filter", new FunctionType()
            {
                IsLambda = true,
                ReturnType = ChainablePushSignal,
                ParameterNames = new List<string>() { "filter" },
                ParameterTypes = new List<IType>() {
                        new FunctionType(){
                            IsLambda = false,
                            ReturnType = new BooleanType(),
                            ParameterNames = new List<string>() { "" },
                            ParameterTypes = new List<IType>() { new GenericTypeIdentifier("Out") }
                        }
                    }
            });

            members.Add("ForEach", new FunctionType()
            {
                IsLambda = true,
                ReturnType = ChainablePushSignal,
                ParameterNames = new List<string>() { "function" },
                ParameterTypes = new List<IType>() { new FunctionType(){
                    IsLambda = false,
                    ReturnType = new VoidType(),
                    ParameterNames = new List<string>() { "" },
                    ParameterTypes = new List<IType>() { new GenericTypeIdentifier("Out") }
                }}
            });

            Types.Bind("ChainablePushSignal", ChainablePushSignal);

            var SignalGlobal = new ClassType()
            {
                Name = "SignalGlobal",
                Members = new Dictionary<string, IType>(),
            };

            SignalGlobal.Members.Add("Timer", new FunctionType()
            {
                ParameterNames = new List<string>() { "millisecondInterval" },
                ParameterTypes = new List<IType>() { new IntType() },
                ReturnType = ChainablePushSignal.ResolveByIdentifier(new Dictionary<string, IType>() {
                    { "In", new IntType() },
                    { "Out", new IntType() }
                }),
            });


            Types.Bind("SignalGlobal", SignalGlobal);

            Values.Bind("Signal", new VariableBinding()
            {
                Name = "Signal",
                Type = SignalGlobal
            });

            // var ChainablePushSignal = new TypeBinding()
            // {
            //     Type = new GenericClassType() { Name = "ChainablePushSignal", GenericIdentifiers = new List<string>() { "In", "Out" } },
            //     Members = new Dictionary<string, VariableBinding>() {
            //         { "Map", new VariableBinding() {
            //             Name = "Map",
            //             Type = new GenericFunctionType() {
            //                 IsLambda = true,
            //                 ReturnType = new SpecificClassType() {
            //                     Name = "ChainablePushSignal",
            //                     GenericIdentifiers = new List<string>() { "In", "Out" },
            //                     SpecificTypes = new Dictionary<string, IType>() { }
            //                 },
            //                 ParameterNames = new List<string>() { "mapper" },
            //                 ParameterTypes = new List<IType>() {
            //                     new GenericFunctionType() {
            //                         IsLambda = false,
            //                         ReturnType = new GenericTypeIdentifier("U"),
            //                         ParameterNames = new List<string>() { "" },
            //                         ParameterTypes = new List<IType>() { new GenericTypeIdentifier("Out") }
            //                     }
            //                 },
            //                 GenericIdentifiers = new List<string>() { "U" }
            //             },
            //             IsConst = false
            //         }},
            //         { "Reduce", new VariableBinding() {
            //             Name = "Reduce",
            //             Type = new GenericFunctionType() {
            //                 IsLambda = true,
            //                 ReturnType = new SpecificClassType() {
            //                     Name = "ChainablePushSignal",
            //                     GenericIdentifiers = new List<string>() { "In", "Out" },
            //                     SpecificTypes = new Dictionary<string, IType>() { }
            //                 },
            //                 ParameterNames = new List<string>() { "reducer", "defaultResult" },
            //                 ParameterTypes = new List<IType>() {
            //                     new GenericFunctionType() {
            //                         IsLambda = false,
            //                         ReturnType = new GenericTypeIdentifier("U"),
            //                         ParameterNames = new List<string>() { "" },
            //                         ParameterTypes = new List<IType>() { new GenericTypeIdentifier("U"), new GenericTypeIdentifier("Out") }
            //                     },
            //                     new GenericTypeIdentifier("U")
            //                 },
            //                 GenericIdentifiers = new List<string>() { "U" }
            //             },
            //             IsConst = false
            //         }},
            //         { "Filter", new VariableBinding() {
            //             Name = "Filter",
            //             Type = new FunctionType() {
            //                 IsLambda = true,
            //                 ReturnType = new SpecificClassType() {
            //                     Name = "ChainablePushSignal",
            //                     GenericIdentifiers = new List<string>() { "In", "Out" },
            //                     SpecificTypes = new Dictionary<string, IType>() { }
            //                 },
            //                 ParameterNames = new List<string>() { "filter" },
            //                 ParameterTypes = new List<IType>() {
            //                     new FunctionType() {
            //                         IsLambda = false,
            //                         ReturnType = new BooleanType(),
            //                         ParameterNames = new List<string>() { "" },
            //                         ParameterTypes = new List<IType>() { new GenericTypeIdentifier("Out") }
            //                     }
            //                 }
            //             },
            //             IsConst = false
            //         }},
            //         { "ForEach", new VariableBinding() {
            //             Name = "ForEach",
            //             Type = new FunctionType() {
            //                 IsLambda = true,
            //                 ReturnType = new SpecificClassType() {
            //                     Name = "ChainablePushSignal",
            //                     GenericIdentifiers = new List<string>() { "In", "Out" },
            //                     SpecificTypes = new Dictionary<string, IType>() { }
            //                 },
            //                 ParameterNames = new List<string>() { "function" },
            //                 ParameterTypes = new List<IType>() {
            //                     new FunctionType() {
            //                         IsLambda = false,
            //                         ReturnType = new VoidType(),
            //                         ParameterNames = new List<string>() { "" },
            //                         ParameterTypes = new List<IType>() { new GenericTypeIdentifier("Out") }
            //                     }
            //                 }
            //             },
            //             IsConst = false
            //         }}
            //     }
            // };

            // Types.Bind("ChainablePushSignal", ChainablePushSignal);

            // var SignalGlobal = new ClassType()
            // {
            //     Name = "SignalGlobal",
            //     Members = new Dictionary<string, VariableBinding>(),
            // };

            // SignalGlobal.Members.Add("Timer", new VariableBinding()
            // {
            //     Name = "Timer",
            //     Type = new FunctionType()
            //     {
            //         ParameterNames = new List<string>() { "millisecondInterval" },
            //         ParameterTypes = new List<IType>() { new IntType() },
            //         ReturnType = ((GenericClassType)ChainablePushSignal.Type).Specify(new Dictionary<string, IType>() { { "In", new IntType() }, { "Out", new IntType() } }),
            //     }
            // });


            // Types.Bind("SignalGlobal", SignalGlobal);

            // Values.Bind("Signal", new VariableBinding()
            // {
            //     Name = "Signal",
            //     Type = SignalGlobal.Type
            // });

            #endregion
        }
    }
}
