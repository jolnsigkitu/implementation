using System;
using System.IO;
using Pipe.Lang.Core;

namespace Pipe.Lang.CLI
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
                catch (Exception ex)
                {
                    throw new Exception($"An error occured while transpiling {fileName}:\n" + ex.Message, ex);

                }
            }
        }
    }
}
