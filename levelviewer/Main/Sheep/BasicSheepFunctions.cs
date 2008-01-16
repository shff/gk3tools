using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sheep
{
    static class BasicSheepFunctions
    {
        public static void Init()
        {
            SheepMachine.AddImport("PrintString", _printStringDelegate,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("IsCurrentTime", _isCurrentTimeDelegate,
                SymbolType.Integer, SymbolType.String);
        }

        private static void sheep_PrintString(IntPtr vm)
        {
            Console.CurrentConsole.WriteLine(SheepMachine.PopStringOffStack(vm));
        }

        private static void sheep_IsCurrentTime(IntPtr vm)
        {
            string time = SheepMachine.PopStringOffStack(vm);

            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static SheepFunctionDelegate _printStringDelegate = new SheepFunctionDelegate(sheep_PrintString);
        private static SheepFunctionDelegate _isCurrentTimeDelegate = new SheepFunctionDelegate(sheep_IsCurrentTime);
    }
}
