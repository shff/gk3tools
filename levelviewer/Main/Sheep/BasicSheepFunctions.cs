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

            SheepMachine.AddImport("SetLocation", new SheepFunctionDelegate(sheep_setLocation),
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("IsCurrentTime", _isCurrentTimeDelegate,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("GetGameVariableInt", _getGameVariableIntDelegate,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("GetEgoLocationCount", _getEgoLocationCount,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("GetEgoCurrentLocationCount", _getEgoCurrentLocationCount,
                SymbolType.Integer);

            SheepMachine.AddImport("GetNounVerbCount", _getNounVerbCount,
                SymbolType.Integer, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("DoesGraceHaveInvItem", _doesGraceHaveInvItemDelegate,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("DoesGabeHaveInvItem", _doesGabeHaveInvItemDelegate,
                SymbolType.Integer, SymbolType.String);
        }

        private static void sheep_PrintString(IntPtr vm)
        {
            Console.CurrentConsole.WriteLine(SheepMachine.PopStringOffStack(vm));
        }

        private static void sheep_setLocation(IntPtr vm)
        {
            string location = SheepMachine.PopStringOffStack(vm);

            Game.GameManager.SetLocation(location);
        }

        private static void sheep_IsCurrentTime(IntPtr vm)
        {
            string time = SheepMachine.PopStringOffStack(vm);

            if (Game.GameManager.GetTimeBlockString(Game.GameManager.CurrentTime).Equals(time, StringComparison.InvariantCultureIgnoreCase))
                SheepMachine.PushIntOntoStack(vm, 1);
            else
                SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_GetGameVariableInt(IntPtr vm)
        {
            string variable = SheepMachine.PopStringOffStack(vm);

            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_GetEgoCurrentLocationCount(IntPtr vm)
        {
            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_GetEgoLocationCount(IntPtr vm)
        {
            string location = SheepMachine.PopStringOffStack(vm);

            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_GetNounVerbCount(IntPtr vm)
        {
            string verb = SheepMachine.PopStringOffStack(vm);
            string noun = SheepMachine.PopStringOffStack(vm);
            

            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_DoesGraceHaveInvItem(IntPtr vm)
        {
            string item = SheepMachine.PopStringOffStack(vm);

            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_DoesGabeHaveInvItem(IntPtr vm)
        {
            string item = SheepMachine.PopStringOffStack(vm);

            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static SheepFunctionDelegate _printStringDelegate = new SheepFunctionDelegate(sheep_PrintString);
        private static SheepFunctionDelegate _isCurrentTimeDelegate = new SheepFunctionDelegate(sheep_IsCurrentTime);
        private static SheepFunctionDelegate _getGameVariableIntDelegate = new SheepFunctionDelegate(sheep_GetGameVariableInt);
        private static SheepFunctionDelegate _getEgoLocationCount = new SheepFunctionDelegate(sheep_GetEgoLocationCount);
        private static SheepFunctionDelegate _getEgoCurrentLocationCount = new SheepFunctionDelegate(sheep_GetEgoCurrentLocationCount);
        private static SheepFunctionDelegate _getNounVerbCount = new SheepFunctionDelegate(sheep_GetNounVerbCount);
        private static SheepFunctionDelegate _doesGraceHaveInvItemDelegate = new SheepFunctionDelegate(sheep_DoesGraceHaveInvItem);
        private static SheepFunctionDelegate _doesGabeHaveInvItemDelegate = new SheepFunctionDelegate(sheep_DoesGabeHaveInvItem);
        
    }
}
