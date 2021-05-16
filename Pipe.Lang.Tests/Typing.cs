using Xunit;

using Pipe.Lang.Core;
using Pipe.Lang.Core.Translator;
using Pipe.Lang.Core.Types;
using System.Collections.Generic;

namespace Pipe.Lang.Tests
{
    public class Typing
    {
        private void AssertTypeFromCode(string code, string identifier, IType type)
        {
            var prog = Util.GetConstructedTree(code);

            var env = new Environment();

            prog.Validate(env);

            Assert.Equal(type, env.Scopes.Types.GetBinding(identifier));
        }

        [Fact]
        public void CanAliasInbuilt()
        {
            var progCode = "type a = int; let b: a = 1;";
            var prog = Util.GetConstructedTree(progCode);

            prog.Validate(new Environment());
        }

        [Fact]
        public void CanAliasAliases()
        {
            var progCode = "type a = int; type b = a; let c: b = 1;";
            var prog = Util.GetConstructedTree(progCode);

            prog.Validate(new Environment());
        }

        [Fact]
        public void CanConstructFunctionTypes()
        {
            var expectedType = new FunctionType()
            {
                ReturnType = new IntType(),
                ParameterTypes = new List<IType>()
                {
                    new IntType(),
                    new IntType(),
                },
            };
            var progCode = "type a = (int, int) => int;";

            AssertTypeFromCode(progCode, "a", expectedType);
        }

        [Fact]
        public void CanConstructClassTypes()
        {
            var expectedType = new ClassType()
            {
                Name = "a",
                // Members = New Dictionary()
            };
            var progCode = "type a = {  };";

            AssertTypeFromCode(progCode, "a", expectedType);
        }
    }
}
