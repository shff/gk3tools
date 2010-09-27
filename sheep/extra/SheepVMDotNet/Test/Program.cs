using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            /*if (args.Length > 0)
            {
                SheepVMDotNet.SheepFileReader reader = new SheepVMDotNet.SheepFileReader(args[0]);
                SheepVMDotNet.SheepMachine vm = new SheepVMDotNet.SheepMachine();

                addImportCallback(reader.Output, "PrintString", new SheepVMDotNet.ImportCallback(printString));

                vm.Run(reader.Output, "blah$");


                SheepVMDotNet.SheepCompiler.Compile("func(\"hi\") $ * 3 + 3 ");

            }*/

            SheepVMDotNet.SheepScanner scanner = new SheepVMDotNet.SheepScanner();
            scanner.Begin("x$ = d + 3 * -0.7880 + \"wo\\\"o!\";");

            SheepVMDotNet.ScannedToken t = scanner.GetNextToken();
            while (t.Type != SheepVMDotNet.SheepTokenType.None)
            {
                Console.WriteLine("Token: " + t.Type.ToString() + " (" + t.Text.ToString() + ")");
                t = scanner.GetNextToken();
            }


            SheepVMDotNet.SheepCompiler c = new SheepVMDotNet.SheepCompiler("symbols { int x$ = 4; float y$; string foo$ = \"woooo!\"; } code { blah$() { y$ = 45.0; } blah_2$() { } }");
            c.Print();

        }

        static void printString(SheepVMDotNet.SheepMachine vm)
        {
            Console.WriteLine(vm.PopStringFromStack());
        }

        static void addImportCallback(SheepVMDotNet.IntermediateOutput output, string name, SheepVMDotNet.ImportCallback callback)
        {
            for (int i = 0; i < output.Imports.Count; i++)
            {
                if (string.Equals(output.Imports[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    SheepVMDotNet.SheepImport import = output.Imports[i];
                    import.Callback = callback;
                    output.Imports[i] = import;
                    break;
                }
            }
        }
    }
}
