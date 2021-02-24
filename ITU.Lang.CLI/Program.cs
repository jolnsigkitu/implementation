using System;
using System.IO;
using ITU.Lang.Core;

namespace ITU.Lang.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You need to provide the (relative) path to your file as an argument.");
                return;
            }
            string fileName = args[0];
            string csFileName = Path.ChangeExtension(fileName, ".cs");
            string fileContent = File.ReadAllText(fileName);
            var transpiler = new Transpiler();
            string transpiledCode = transpiler.fromString(fileContent);
            File.WriteAllText(csFileName, transpiledCode);
        }
    }
}
