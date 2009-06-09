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

            SheepMachine.AddImport("ClearFlag", new SheepFunctionDelegate(sheep_clearFlag),
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("CutToCameraAngle", new SheepFunctionDelegate(sheep_CutToCameraAngle),
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("FinishedScreen", _finishedScreen,
                SymbolType.Void);

            SheepMachine.AddImport("GetFlag", _getFlag,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("SetLocation", new SheepFunctionDelegate(sheep_setLocation),
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("InitEgoPosition", _initEgoPosition,
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

            SheepMachine.AddImport("SetCameraAngleType", _setCameraAngleType,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("SetNounVerbCount", _setNounVerbCount,
                SymbolType.Void, SymbolType.String, SymbolType.String, SymbolType.Integer);

            SheepMachine.AddImport("StartAnimation", _sheepStartAnimation,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("StartVoiceOver", _startVoiceOver,
                SymbolType.Void, SymbolType.String, SymbolType.Integer);

            SheepMachine.AddImport("WalkerBoundaryBlockRegion", _walkerBoundaryBlockRegion,
                SymbolType.Void, SymbolType.Integer, SymbolType.Integer);

            SheepMachine.AddImport("WasLastLocation", _wasLastLocation,
                SymbolType.Integer, SymbolType.String);
        }

        private static void sheep_PrintString(IntPtr vm)
        {
            Console.CurrentConsole.WriteLine(SheepMachine.PopStringOffStack(vm));
        }

        private static void sheep_clearFlag(IntPtr vm)
        {
            string flag = SheepMachine.PopStringOffStack(vm);
 
            // TODO!
        }

        private static void sheep_CutToCameraAngle(IntPtr vm)
        {
            string angleName = SheepMachine.PopStringOffStack(vm);

            // TODO!
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
            if (location.Equals("CSE", StringComparison.OrdinalIgnoreCase))
                SheepMachine.PushIntOntoStack(vm, 1);
            else
                SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_GetFlag(IntPtr vm)
        {
            string name = SheepMachine.PopStringOffStack(vm);

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

        private static void sheep_FinishedScreen(IntPtr vm)
        {
            // TODO!
            // NOTE: this function only exists in the demo.
        }

        private static void sheep_InitEgoPosition(IntPtr vm)
        {
            string position = SheepMachine.PopStringOffStack(vm);

            // TODO!
        }

        private static void sheep_SetCameraAngleType(IntPtr vm)
        {
            string camera = SheepMachine.PopStringOffStack(vm);
            string type = SheepMachine.PopStringOffStack(vm);

            // TODO!
        }

        private static void sheep_SetNounVerbCount(IntPtr vm)
        {
            int count = SheepMachine.PopIntOffStack(vm);
            string verb = SheepMachine.PopStringOffStack(vm);
            string noun = SheepMachine.PopStringOffStack(vm);
          
            // TODO!
        }

        private static void sheep_StartAnimation(IntPtr vm)
        {
            string animation = SheepMachine.PopStringOffStack(vm);

            // TODO!
        }

        private static void sheep_StartVoiceOver(IntPtr vm)
        {
            int count = SheepMachine.PopIntOffStack(vm);
            string id = SheepMachine.PopStringOffStack(vm);

            // TODO!
        }

        private static void sheep_WalkerBoundaryBlockRegion(IntPtr vm)
        {
            int index1 = SheepMachine.PopIntOffStack(vm);
            int index2 = SheepMachine.PopIntOffStack(vm);

            // TODO!

        }

        private static void sheep_WasLastLocation(IntPtr vm)
        {
            string location = Sheep.SheepMachine.PopStringOffStack(vm);

            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static SheepFunctionDelegate _printStringDelegate = new SheepFunctionDelegate(sheep_PrintString);
        private static SheepFunctionDelegate _clearFlag = new SheepFunctionDelegate(sheep_clearFlag);
        private static SheepFunctionDelegate _cutToCameraAngle = new SheepFunctionDelegate(sheep_CutToCameraAngle);
        private static SheepFunctionDelegate _getFlag = new SheepFunctionDelegate(sheep_GetFlag);
        private static SheepFunctionDelegate _initEgoPosition = new SheepFunctionDelegate(sheep_InitEgoPosition);
        private static SheepFunctionDelegate _isCurrentTimeDelegate = new SheepFunctionDelegate(sheep_IsCurrentTime);
        private static SheepFunctionDelegate _finishedScreen = new SheepFunctionDelegate(sheep_FinishedScreen);
        private static SheepFunctionDelegate _getGameVariableIntDelegate = new SheepFunctionDelegate(sheep_GetGameVariableInt);
        private static SheepFunctionDelegate _getEgoLocationCount = new SheepFunctionDelegate(sheep_GetEgoLocationCount);
        private static SheepFunctionDelegate _getEgoCurrentLocationCount = new SheepFunctionDelegate(sheep_GetEgoCurrentLocationCount);
        private static SheepFunctionDelegate _getNounVerbCount = new SheepFunctionDelegate(sheep_GetNounVerbCount);
        private static SheepFunctionDelegate _doesGraceHaveInvItemDelegate = new SheepFunctionDelegate(sheep_DoesGraceHaveInvItem);
        private static SheepFunctionDelegate _doesGabeHaveInvItemDelegate = new SheepFunctionDelegate(sheep_DoesGabeHaveInvItem);
        private static SheepFunctionDelegate _setCameraAngleType = new SheepFunctionDelegate(sheep_SetCameraAngleType);
        private static SheepFunctionDelegate _setNounVerbCount = new SheepFunctionDelegate(sheep_SetNounVerbCount);
        private static SheepFunctionDelegate _startVoiceOver = new SheepFunctionDelegate(sheep_StartVoiceOver);
        private static SheepFunctionDelegate _sheepStartAnimation = new SheepFunctionDelegate(sheep_StartAnimation);
        private static SheepFunctionDelegate _walkerBoundaryBlockRegion = new SheepFunctionDelegate(sheep_WalkerBoundaryBlockRegion);
        private static SheepFunctionDelegate _wasLastLocation = new SheepFunctionDelegate(sheep_WasLastLocation);
        
    }
}
