using System.Collections.Generic;
using Pipe.Lang.Core.Translator.Nodes;
using Pipe.Lang.Core.Types;

namespace Pipe.Lang.Core.Translator
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

            Types.Bind("Pair", new GenericClassWrapper(
                new ClassType()
                {
                    Name = "Pair",
                    Members = new Dictionary<string, IType>()
                    {
                        { "First", new GenericTypeIdentifier("TFirst") },
                        { "Second", new GenericTypeIdentifier("TSecond") },
                    },
                },
                new Dictionary<string, IType>() {
                    { "TFirst", new GenericTypeIdentifier("TFirst") },
                    { "TSecond", new GenericTypeIdentifier("TSecond") }
                },
                new List<string>() { "TFirst", "TSecond" }
            ));
        }
    }
}
