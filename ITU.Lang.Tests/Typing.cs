using Xunit;

using ITU.Lang.Core;
using ITU.Lang.Core.Translator;
using ITU.Lang.Core.Types;
using System.Collections.Generic;

namespace ITU.Lang.Tests
{
    public class Typing
    {
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
                ParameterTypes = new List<Type>()
                {
                    new IntType(),
                    new IntType(),
                },
            };
            var progCode = "type a = (int, int) => int;";
            var prog = Util.GetConstructedTree(progCode);

            var env = new Environment();

            prog.Validate(env);

            Assert.True(env.Scopes.Types.GetBinding("a").Equals(expectedType));
        }
    }
}
