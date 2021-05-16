using System.IO;

using Xunit;

using Pipe.Lang.Core;
using Pipe.Lang.Core.Translator;

namespace Pipe.Lang.Tests
{
    public class FullRunTests
    {
        [Theory]
        [InlineData("basic.pipe")]
        [InlineData("basicLoop.pipe")]
        [InlineData("loop.pipe")]
        [InlineData("if-else.pipe")]
        [InlineData("operators.pipe")]
        [InlineData("precedence.pipe")]
        [InlineData("functions.pipe")]
        [InlineData("genericFunctions.pipe")]
        [InlineData("classes.pipe")]
        [InlineData("genericClasses.pipe")]
        [InlineData("genericIteration.pipe")]
        public void CanDoFile(string filePath)
        {
            string fileContent = File.ReadAllText("../../../../examples/" + filePath);

            var prog = Util.GetConstructedTree(fileContent);

            prog.Validate(new Environment());
            prog.ToString();
        }
    }
}
