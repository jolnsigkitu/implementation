using Xunit;

using ITU.Lang.Core;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Tests
{
    public class VariableAssignment
    {
        [Fact]
        public void CanReassignLet()
        {
            var progCode = "let a = 1; a = 2;";
            var prog = Util.GetConstructedTree(progCode);

            prog.Validate(new Environment());
        }

        [Fact]
        public void CannotReassignConst()
        {
            var progCode = "const a = 1; a = 2;";
            var prog = Util.GetConstructedTree(progCode);

            Assert.Throws<TranspilationException>(() => prog.Validate(new Environment()));
        }

        [Fact]
        public void CannotMismatchAnnotationAndValue()
        {
            var progCode = "const a: boolean = 1;";
            var prog = Util.GetConstructedTree(progCode);

            Assert.Throws<TranspilationException>(() => prog.Validate(new Environment()));
        }

        // this.invalid = lastName;
    }
}
