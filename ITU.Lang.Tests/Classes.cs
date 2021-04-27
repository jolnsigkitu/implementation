using Xunit;

using ITU.Lang.Core;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Tests
{
    public class Classes
    {
        [Fact]
        public void CanAccessProperty()
        {
            var progCode = "type a = { z: int; }; const b = new a(); b.z = 7;";
            var prog = Util.GetConstructedTree(progCode);
            prog.Validate(new Environment());
        }

        [Fact]
        public void ThrowsWhenPropertyDoesNotExist()
        {
            var progCode = "type a = { z: int; }; const b = new a(); b.invalid = 7;";
            var prog = Util.GetConstructedTree(progCode);

            var ex = Assert.Throws<TranspilationException>(() => prog.Validate(new Environment()));

            Assert.Contains("Cannot assign value to undefined member", ex.Message);
        }

        [Fact]
        public void CanAssignOnMemberDefinition()
        {
            var progCode = "type a = { z: int = 8; };";
            var prog = Util.GetConstructedTree(progCode);
            prog.Validate(new Environment());
        }

        [Fact]
        public void CanConstruct()
        {
            var progCode = "type a = { z: int; constructor(z: int) { this.z = z; } }; const b = new a(5); println(b.z);";
            var prog = Util.GetConstructedTree(progCode);
            prog.Validate(new Environment());
        }

        [Fact]
        public void CannotConstructWithWrongParameterCount()
        {
            var progCode = "type a = { z: int; constructor(z: int) { this.z = z; } }; const b = new a();";
            var prog = Util.GetConstructedTree(progCode);

            var ex = Assert.Throws<TranspilationException>(() => prog.Validate(new Environment()));

            Assert.Contains("Class constructor takes 1 parameters, but got 0", ex.Message);
        }

        /*
        type Account = <T> {  };

        type User = <T> {
            GetAccount<U>() => new Account<U>();
        };

        let a: Account<int> = new User<boolean>().GetAccount<int>();
        */
    }
}
