using System.IO;

using Xunit;

using ITU.Lang.Core;
using ITU.Lang.Core.Translator;

namespace ITU.Lang.Tests
{
    public class FullRunTests
    {
        [Theory]
        [InlineData("baby.itu")]
        [InlineData("basic.itu")]
        [InlineData("babyLoop.itu")]
        [InlineData("loop.itu")]
        [InlineData("if-else.itu")]
        [InlineData("operators.itu")]
        [InlineData("precedence.itu")]
        [InlineData("functions.itu")]
        [InlineData("genericFunctions.itu")]
        [InlineData("classes.itu")]
        [InlineData("genericClasses.itu")]
        public void CanDoFile(string filePath)
        {
            string fileContent = File.ReadAllText("../../../../examples/" + filePath);

            var prog = Util.GetConstructedTree(fileContent);

            prog.Validate(new Environment());
            prog.ToString();
        }
    }
}
