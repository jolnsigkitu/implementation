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
                Console.WriteLine("You need to provide the (relative) path to your file(s) as an argument.");
                return;
            }

            foreach (var fileName in args)
            {
                string fileContent = File.ReadAllText(fileName);

                var transpiler = new Transpiler();

                try
                {
                    string transpiledCode = transpiler.fromString(fileContent);

                    string csFileName = Path.ChangeExtension(fileName, ".cs");
                    File.WriteAllText(csFileName, transpiledCode);
                }
                catch (TranspilationException ex)
                {
                    throw new Exception($"An error occured while transpiling {fileName}:\n" + ex.Message, ex);

                    // System.Console.Error.WriteLine(ex.Message);
                    // var lines = ex.StackTrace.Split('\n');
                    // System.Console.Error.WriteLine(string.Join("\n", lines[0..5]));
                    // if (lines.Length - 5 > 0) System.Console.Error.WriteLine($"   ... {lines.Length - 5} more lines");
                    // System.Environment.Exit(1);
                }
            }
        }
    }
}
