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
            if (args.Length > 0)
            {
                SheepVMDotNet.SheepFileReader reader = new SheepVMDotNet.SheepFileReader(args[0]);
                SheepVMDotNet.SheepMachine vm = new SheepVMDotNet.SheepMachine();

                addImportCallback(reader.Output, "PrintString", new SheepVMDotNet.ImportCallback(printString));

                vm.Run(reader.Output, "blah$");


            }
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
