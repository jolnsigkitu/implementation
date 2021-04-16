using System.Collections.Generic;
using ITU.Lang.Core.NewTranslator.Nodes;
using ITU.Lang.Core.Types;

namespace ITU.Lang.Core.NewTranslator
{
    public class Scopes
    {
        public Scope<VariableBinding> Values { get; } = new Scope<VariableBinding>();
        public Scope<Type> Types { get; } = new Scope<Type>();

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

            Types.Bind("int", new IntType());
            Types.Bind("boolean", new BooleanType());
            Types.Bind("string", new StringType());
            Types.Bind("char", new CharType());
            Types.Bind("any", new AnyType());

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
        }
    }
}
