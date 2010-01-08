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

            SheepMachine.AddImport("CallSheep", _callSheep,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("ChangeScore", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("ClearFlag", _clearFlag,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("ClearMood", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("ContinueDialogue", _continueDialogue,
                SymbolType.Void, SymbolType.Integer);

            SheepMachine.AddImport("ContinueDialogueNoFidgets", _continueDialogueNoFidgets,
                SymbolType.Void, SymbolType.Integer);

            SheepMachine.AddImport("CutToCameraAngle", _cutToCameraAngle,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("DisableCameraBoundaries", _dummyVoid,
                SymbolType.Void);

            SheepMachine.AddImport("DisableModelShadow", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("DoesEgoHaveInvItem", _doesEgoHaveInvItem,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("DoesGraceHaveInvItem", _doesGraceHaveInvItemDelegate,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("DoesGabeHaveInvItem", _doesGabeHaveInvItemDelegate,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("DoesModelExist", _doesModelExistDelegate,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("EnableCameraBoundaries", _dummyVoid,
                SymbolType.Void);

            SheepMachine.AddImport("EnableModelShadow", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("FinishedScreen", _finishedScreen,
                SymbolType.Void);

            SheepMachine.AddImport("ForceCutToCameraAngle", _cutToCameraAngle,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("GetFlag", _getFlag,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("GetGameVariableInt", _getGameVariableIntDelegate,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("GetChatCount", _getChatCount,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("GetEgoLocationCount", _getEgoLocationCount,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("GetEgoCurrentLocationCount", _getEgoCurrentLocationCount,
                SymbolType.Integer);

            SheepMachine.AddImport("GetNounVerbCount", _getNounVerbCount,
                SymbolType.Integer, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("GetRandomInt", _getRandomInt,
                SymbolType.Integer, SymbolType.Integer, SymbolType.Integer);

            SheepMachine.AddImport("GetTopicCount", _getTopicCount,
                SymbolType.Integer, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("GetTopicCountInt", _getTopicCountInt,
                SymbolType.Integer, SymbolType.Integer, SymbolType.Integer);

            SheepMachine.AddImport("HideModel", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("HideSceneModel", _hideSceneModel,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("IncNounVerbCount", _incNounVerbCount,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("IncNounVerbCountBoth", _incNounVerbCountBoth,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("InitEgoPosition", _initEgoPosition,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("InventoryInspect", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("IsActiveInvItem", _isActiveInvItem,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("IsActorAtLocation", _isActorAtLocation,
                SymbolType.Integer, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("IsActorNear", _isActorNear,
                SymbolType.Integer, SymbolType.String, SymbolType.String, SymbolType.Float);

            SheepMachine.AddImport("IsCurrentEgo", _isCurrentEgo,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("IsCurrentLocation", _isCurrentLocation,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("IsCurrentTime", _isCurrentTimeDelegate,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("IsTopLayerInventory", _isTopLayerInventory, 
                SymbolType.Integer);

            SheepMachine.AddImport("IsWalkingActorNear", _isWalkingActorNear,
                SymbolType.Integer, SymbolType.String, SymbolType.String, SymbolType.Float);

            SheepMachine.AddImport("InspectModelUsingAngle", _inspectModelUsingAngle,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("PlaySoundTrack", _playSoundTrack,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("SetActorLocation", _dummyStringString,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("SetActorPosition", _setActorPosition,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("SetCameraAngleType", _setCameraAngleType,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("SetDefaultDialogueCamera", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("SetFlag", _setFlag,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("SetForcedCameraCuts", _setForcedCameraCuts,
                SymbolType.Void, SymbolType.Integer);

            SheepMachine.AddImport("SetGameTimer", _setGameTimer,
                SymbolType.Void, SymbolType.String, SymbolType.String, SymbolType.Integer);

            SheepMachine.AddImport("SetGameVariableInt", _setGameVariableInt,
                SymbolType.Void, SymbolType.String, SymbolType.Integer);

            SheepMachine.AddImport("SetListenGas", _setListenGas,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("SetLocation", _setLocation,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("SetNounVerbCount", _setNounVerbCount,
                SymbolType.Void, SymbolType.String, SymbolType.String, SymbolType.Integer);

            SheepMachine.AddImport("SetNounVerbCountBoth", _setNounVerbCountBoth,
                SymbolType.Void, SymbolType.String, SymbolType.String, SymbolType.Integer);

            SheepMachine.AddImport("SetMood", _dummyStringString,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("SetTalkGas", _setTalkGas,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("ShowModel", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("ShowSceneModel", _showSceneModel,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("SetTimerSeconds", _setTimerSeconds,
                SymbolType.Void, SymbolType.Float);

            SheepMachine.AddImport("StartAnimation", _sheepStartAnimation,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("StartDialogue", _startDialogue,
                SymbolType.Void, SymbolType.String, SymbolType.Integer);

            SheepMachine.AddImport("StartDialogueNoFidgets", _startDialogueNoFidgets,
                SymbolType.Void, SymbolType.String, SymbolType.Integer);

            SheepMachine.AddImport("StartIdleFidget", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("StartMom", _startMom,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("StartMoveAnimation", _sheepStartMoveAnimation,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("StartVoiceOver", _startVoiceOver,
                SymbolType.Void, SymbolType.String, SymbolType.Integer);

            SheepMachine.AddImport("StopFidget", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("StopSoundTrack", _stopSoundTrack,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("TurnHead", _turnHead,
                SymbolType.Void, SymbolType.String, SymbolType.Integer, SymbolType.Integer, SymbolType.Integer);

            SheepMachine.AddImport("WalkerBoundaryBlockRegion", _walkerBoundaryBlockRegion,
                SymbolType.Void, SymbolType.Integer, SymbolType.Integer);

            SheepMachine.AddImport("WalkerBoundaryBlockModel", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("WalkerBoundaryUnblockModel", _dummyString,
                SymbolType.Void, SymbolType.String);

            SheepMachine.AddImport("WalkTo", _walkTo,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("WalkToAnimation", _dummyStringString,
                SymbolType.Void, SymbolType.String, SymbolType.String);

            SheepMachine.AddImport("WasEgoEverInLocation", _wasEgoEverInLocation,
                SymbolType.Integer, SymbolType.String);

            SheepMachine.AddImport("WasLastLocation", _wasLastLocation,
                SymbolType.Integer, SymbolType.String);
        }

        private static void sheep_PrintString(IntPtr vm)
        {
            Console.CurrentConsole.WriteLine(SheepMachine.PopStringOffStack(vm));
        }

        private static void sheep_CallSheep(IntPtr vm)
        {
            string function = SheepMachine.PopStringOffStack(vm);
            string file = SheepMachine.PopStringOffStack(vm);

            if (function.EndsWith("$") == false)
                function += "$";

            SheepMachine.RunSheep(string.Format("{0}.shp", file), function);
        }

        private static void sheep_clearFlag(IntPtr vm)
        {
            string flag = SheepMachine.PopStringOffStack(vm);
 
            // TODO!
        }

        private static void sheep_ContinueDialogue(IntPtr vm)
        {
            int numLines = SheepMachine.PopIntOffStack(vm);

            bool waiting = SheepMachine.IsInWaitSection(vm);
            WaitHandle handle = Game.DialogManager.ContinueDialogue(numLines, waiting);

            if (waiting && handle != null)
                SheepMachine.AddWaitHandle(vm, SheepMachine.GetCurrentContext(vm), handle);
        }

        private static void sheep_ContinueDialogueNoFidgets(IntPtr vm)
        {
            int numLines = SheepMachine.PopIntOffStack(vm);

            bool waiting = SheepMachine.IsInWaitSection(vm);
            WaitHandle handle = Game.DialogManager.ContinueDialogue(numLines, waiting);

            if (waiting && handle != null)
                SheepMachine.AddWaitHandle(vm, SheepMachine.GetCurrentContext(vm), handle);
        }

        private static void sheep_CutToCameraAngle(IntPtr vm)
        {
            string angleName = SheepMachine.PopStringOffStack(vm);

            SceneManager.SetCameraToCinematicCamera(angleName);
        }

        private static void sheep_setLocation(IntPtr vm)
        {
            string location = SheepMachine.PopStringOffStack(vm);

            Game.GameManager.SetLocation(location);
        }

        private static void sheep_IsActiveInvItem(IntPtr vm)
        {
            string item = SheepMachine.PopStringOffStack(vm);

            // TODO

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_IsActorAtLocation(IntPtr vm)
        {
            string location = SheepMachine.PopStringOffStack(vm);
            string actor = SheepMachine.PopStringOffStack(vm);

            // TODO

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_IsCurrentEgo(IntPtr vm)
        {
            string ego = SheepMachine.PopStringOffStack(vm);

            if (Game.GameManager.CurrentEgo == Gk3Main.Game.Ego.Gabriel &&
                ego.Equals("GABRIEL", StringComparison.OrdinalIgnoreCase))
                SheepMachine.PushIntOntoStack(vm, 1);
            else if (Game.GameManager.CurrentEgo == Gk3Main.Game.Ego.Grace &&
                ego.Equals("GRACE", StringComparison.OrdinalIgnoreCase))
                SheepMachine.PushIntOntoStack(vm, 1);
            else
                SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_IsCurrentLocation(IntPtr vm)
        {
            string location = SheepMachine.PopStringOffStack(vm);

            if (Game.GameManager.CurrentLocation.Equals(location, StringComparison.OrdinalIgnoreCase))
                SheepMachine.PushIntOntoStack(vm, 1);
            else
                SheepMachine.PushIntOntoStack(vm, 0);
        }


        private static void sheep_IsCurrentTime(IntPtr vm)
        {
            string time = SheepMachine.PopStringOffStack(vm);

            if (Game.GameManager.GetTimeBlockString(Game.GameManager.CurrentTime).Equals(time, StringComparison.InvariantCultureIgnoreCase))
                SheepMachine.PushIntOntoStack(vm, 1);
            else
                SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_IsTopLayerInventory(IntPtr vm)
        {
            // TODO

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_IsWalkingActorNear(IntPtr vm)
        {
            float distance = SheepMachine.PopFloatOffStack(vm);
            string position = SheepMachine.PopStringOffStack(vm);
            string actor = SheepMachine.PopStringOffStack(vm);

            // TODO

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_GetGameVariableInt(IntPtr vm)
        {
            string variable = SheepMachine.PopStringOffStack(vm);

            int value = Game.GameManager.GetIntegerGameVariable(variable);

            SheepMachine.PushIntOntoStack(vm, value);
        }

        private static void sheep_GetChatCount(IntPtr vm)
        {
            string noun = SheepMachine.PopStringOffStack(vm);

            int c = Game.GameManager.GetChatCount(noun);

            SheepMachine.PushIntOntoStack(vm, c);
        }

        private static void sheep_GetEgoCurrentLocationCount(IntPtr vm)
        {
            // TODO!

            SheepMachine.PushIntOntoStack(vm, 1);
        }

        private static void sheep_GetEgoLocationCount(IntPtr vm)
        {
            string location = SheepMachine.PopStringOffStack(vm);

            // TODO!
            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_GetFlag(IntPtr vm)
        {
            string name = SheepMachine.PopStringOffStack(vm);

            int flag = Game.GameManager.GetFlag(name) ? 1 : 0;

            SheepMachine.PushIntOntoStack(vm, flag);
        }

        private static void sheep_GetNounVerbCount(IntPtr vm)
        {
            string verb = SheepMachine.PopStringOffStack(vm);
            string noun = SheepMachine.PopStringOffStack(vm);

            int count = Game.GameManager.GetNounVerbCount(noun, verb);

            SheepMachine.PushIntOntoStack(vm, count);
        }

        private static void sheep_GetRandomInt(IntPtr vm)
        {
            int upper = SheepMachine.PopIntOffStack(vm);
            int lower = SheepMachine.PopIntOffStack(vm);

            int num = Utils.PickRandomNumber(lower, upper);

            SheepMachine.PushIntOntoStack(vm, num);
        }

        private static void sheep_GetTopicCount(IntPtr vm)
        {
            string verb = SheepMachine.PopStringOffStack(vm);
            string noun = SheepMachine.PopStringOffStack(vm);

            int count = Game.GameManager.GetTopicCount(noun, verb);

            SheepMachine.PushIntOntoStack(vm, count);
        }

        private static void sheep_GetTopicCountInt(IntPtr vm)
        {
            int verb = SheepMachine.PopIntOffStack(vm);
            int noun = SheepMachine.PopIntOffStack(vm);

            // TODO

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_DoesGraceHaveInvItem(IntPtr vm)
        {
            string item = SheepMachine.PopStringOffStack(vm);

            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_DoesModelExist(IntPtr vm)
        {
            string model = SheepMachine.PopStringOffStack(vm);

            // TODO!

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_DoesEgoHaveInvItem(IntPtr vm)
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

        private static void sheep_HideSceneModel(IntPtr vm)
        {
            string model = SheepMachine.PopStringOffStack(vm);

            SceneManager.SetSceneModelVisibility(model, false);
        }

        private static void sheep_IncNounVerbCount(IntPtr vm)
        {
            string verb = SheepMachine.PopStringOffStack(vm);
            string noun = SheepMachine.PopStringOffStack(vm);

            Game.GameManager.IncrementNounVerbCount(noun, verb);
        }

        private static void sheep_IncNounVerbCountBoth(IntPtr vm)
        {
            string verb = SheepMachine.PopStringOffStack(vm);
            string noun = SheepMachine.PopStringOffStack(vm);

            Game.GameManager.IncrementNounVerbCount(noun, verb, true);
            Game.GameManager.IncrementNounVerbCount(noun, verb, false);
        }

        private static void sheep_FinishedScreen(IntPtr vm)
        {
            // TODO!
            // NOTE: this function only exists in the demo.
        }

        private static void sheep_InitEgoPosition(IntPtr vm)
        {
            string position = SheepMachine.PopStringOffStack(vm);

            // TODO: put the current ego's model at the specified position
            SceneManager.SetEgoToSifPosition(position);

            // set the camera to the specified position
            SceneManager.SetCameraToSifPosition(position);
        }

        private static void sheep_InspectModelUsingAngle(IntPtr vm)
        {
            string angle = SheepMachine.PopStringOffStack(vm);
            string model = SheepMachine.PopStringOffStack(vm);

            SceneManager.SetCameraToCinematicCamera(angle);
        }

        private static void sheep_IsActorNear(IntPtr vm)
        {
            float distance = SheepMachine.PopFloatOffStack(vm);
            string position = SheepMachine.PopStringOffStack(vm);
            string actor = SheepMachine.PopStringOffStack(vm);

            // TODO

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_PlaySoundTrack(IntPtr vm)
        {
            string stk = SheepMachine.PopStringOffStack(vm);

            SceneManager.PlaySoundTrack(Utils.MakeEndsWith(stk, ".stk"));
        }

        private static void sheep_SetActorPosition(IntPtr vm)
        {
            string position = SheepMachine.PopStringOffStack(vm);
            string actor = SheepMachine.PopStringOffStack(vm);

            SceneManager.SetActorPosition(actor, position);
        }

        private static void sheep_SetCameraAngleType(IntPtr vm)
        {
            string camera = SheepMachine.PopStringOffStack(vm);
            string type = SheepMachine.PopStringOffStack(vm);

            // TODO!
        }

        private static void sheep_SetGameTimer(IntPtr vm)
        {
            int milliseconds = SheepMachine.PopIntOffStack(vm);
            string verb = SheepMachine.PopStringOffStack(vm);
            string noun = SheepMachine.PopStringOffStack(vm);

            Game.GameManager.AddGameTimer(noun, verb, milliseconds);
        }

        private static void sheep_SetListenGas(IntPtr vm)
        {
            string gas = SheepMachine.PopStringOffStack(vm);
            string actor = SheepMachine.PopStringOffStack(vm);

            // TODO
        }

        private static void sheep_StartMom(IntPtr vm)
        {
            string mom = SheepMachine.PopStringOffStack(vm);

            bool isInWaitSection = SheepMachine.IsInWaitSection(vm);
            if (isInWaitSection)
            {
                WaitHandle wait = Game.DialogManager.PlayMom(mom, true);
                SheepMachine.AddWaitHandle(vm, SheepMachine.GetCurrentContext(vm), wait);
            }
            else
            {
                Game.DialogManager.PlayMom(mom, false);
            }
        }

        private static void sheep_SetNounVerbCount(IntPtr vm)
        {
            int count = SheepMachine.PopIntOffStack(vm);
            string verb = SheepMachine.PopStringOffStack(vm);
            string noun = SheepMachine.PopStringOffStack(vm);

            Game.GameManager.SetNounVerbCount(noun, verb, count);
        }

        private static void sheep_SetNounVerbCountBoth(IntPtr vm)
        {
            int count = SheepMachine.PopIntOffStack(vm);
            string verb = SheepMachine.PopStringOffStack(vm);
            string noun = SheepMachine.PopStringOffStack(vm);

            Game.GameManager.SetNounVerbCount(noun, verb, true, count);
            Game.GameManager.SetNounVerbCount(noun, verb, false, count);
        }

        private static void sheep_SetFlag(IntPtr vm)
        {
            string flag = SheepMachine.PopStringOffStack(vm);

            Game.GameManager.SetFlag(flag);
        }

        private static void sheep_SetForcedCameraCuts(IntPtr vm)
        {
            int value = SheepMachine.PopIntOffStack(vm);

            // TODO
        }

        private static void sheep_SetGameVariableInt(IntPtr vm)
        {
            int value = SheepMachine.PopIntOffStack(vm);
            string name = SheepMachine.PopStringOffStack(vm);

            Game.GameManager.SetIntegerGameVariable(name, value);
        }

        private static void sheep_SetTalkGas(IntPtr vm)
        {
            string gas = SheepMachine.PopStringOffStack(vm);
            string actor = SheepMachine.PopStringOffStack(vm);

            // TODO
        }

        private static void sheep_SetTimerSeconds(IntPtr vm)
        {
            float seconds = SheepMachine.PopFloatOffStack(vm);

            if (SheepMachine.IsInWaitSection(vm))
                SheepMachine.AddWaitHandle(vm, SheepMachine.GetCurrentContext(vm), new Game.TimedWaitHandle((int)(seconds * 1000))); 
        }

        private static void sheep_ShowSceneModel(IntPtr vm)
        {
            string model = SheepMachine.PopStringOffStack(vm);

            // TODO
        }

        private static void sheep_StartAnimation(IntPtr vm)
        {
            string animation = SheepMachine.PopStringOffStack(vm);
            if (animation.EndsWith(".ANM", StringComparison.OrdinalIgnoreCase) == false)
                animation += ".ANM";

            //Game.AnmResource anm = (Game.AnmResource)Resource.ResourceManager.Load(animation);
            Game.MomResource anm = (Game.MomResource)Resource.ResourceManager.Load(animation);
            
            if (SheepMachine.IsInWaitSection(vm))
            {
                WaitHandle wait = Game.Animator.Add(anm, true);
                SheepMachine.AddWaitHandle(vm, SheepMachine.GetCurrentContext(vm), wait); 
            }
            else
            {
                Game.Animator.Add(anm, false);
            }
        }

        private static void sheep_StartDialogue(IntPtr vm)
        {
            int numLines = SheepMachine.PopIntOffStack(vm);
            string licensePlate = SheepMachine.PopStringOffStack(vm);

            bool waiting = SheepMachine.IsInWaitSection(vm);
            WaitHandle handle = Game.DialogManager.PlayDialogue(licensePlate, numLines, false, waiting);

            if (waiting && handle != null)
                SheepMachine.AddWaitHandle(vm, SheepMachine.GetCurrentContext(vm), handle);
        }

        private static void sheep_StartDialogueNoFidgets(IntPtr vm)
        {
            int numLines = SheepMachine.PopIntOffStack(vm);
            string licensePlate = SheepMachine.PopStringOffStack(vm);

            bool waiting = SheepMachine.IsInWaitSection(vm);
            WaitHandle handle = Game.DialogManager.PlayDialogue(licensePlate, numLines, false, waiting);

            if (waiting && handle != null)
                SheepMachine.AddWaitHandle(vm, SheepMachine.GetCurrentContext(vm), handle);
        }

        private static void sheep_StartMoveAnimation(IntPtr vm)
        {
            string animation = SheepMachine.PopStringOffStack(vm);

            // TODO
        }

        private static void sheep_StartVoiceOver(IntPtr vm)
        {
            int count = SheepMachine.PopIntOffStack(vm);
            string id = SheepMachine.PopStringOffStack(vm);

            // TODO!
            Game.YakResource yak = (Game.YakResource)Resource.ResourceManager.Load(string.Format("E{0}.YAK", id));

            if (SheepMachine.IsInWaitSection(vm))
            {
                WaitHandle wait = yak.PlayAndWait();
                SheepMachine.AddWaitHandle(vm, SheepMachine.GetCurrentContext(vm), wait);
            }
            else
            {
                yak.Play();
            }
        }

        private static void sheep_StopSoundTrack(IntPtr vm)
        {
            string stk = SheepMachine.PopStringOffStack(vm);

            SceneManager.StopSoundTrack(string.Format("{0}.stk", stk));
        }


        private static void sheep_TurnHead(IntPtr vm)
        {
            int duration = SheepMachine.PopIntOffStack(vm);
            int percentY = SheepMachine.PopIntOffStack(vm);
            int percentX = SheepMachine.PopIntOffStack(vm);
            string actor = SheepMachine.PopStringOffStack(vm);

            // TODO
        }

        private static void sheep_WalkerBoundaryBlockRegion(IntPtr vm)
        {
            int index1 = SheepMachine.PopIntOffStack(vm);
            int index2 = SheepMachine.PopIntOffStack(vm);

            // TODO!

        }

        private static void sheep_WalkTo(IntPtr vm)
        {
            string position = SheepMachine.PopStringOffStack(vm);
            string actor = SheepMachine.PopStringOffStack(vm);

            // TODO
        }

        private static void sheep_WasEgoEverInLocation(IntPtr vm)
        {
            string location = SheepMachine.PopStringOffStack(vm);

            // TODO

            SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_WasLastLocation(IntPtr vm)
        {
            string location = Sheep.SheepMachine.PopStringOffStack(vm);

            if (Game.GameManager.LastLocation != null &&
                Game.GameManager.LastLocation.Equals(location, StringComparison.OrdinalIgnoreCase))
                SheepMachine.PushIntOntoStack(vm, 1);
            else
                SheepMachine.PushIntOntoStack(vm, 0);
        }

        private static void sheep_DummyVoid(IntPtr vm)
        {
        }

        private static void sheep_DummyString(IntPtr vm)
        {
            string dummy = SheepMachine.PopStringOffStack(vm);
        }

        private static void sheep_DummyStringString(IntPtr vm)
        {
            string dummy2 = SheepMachine.PopStringOffStack(vm);
            string dummy1 = SheepMachine.PopStringOffStack(vm);
        }

        private static void sheep_DummyStringInt(IntPtr vm)
        {
            int integer = SheepMachine.PopIntOffStack(vm);
            string dummy = SheepMachine.PopStringOffStack(vm);
        }

        private static void sheep_DummyInt(IntPtr vm)
        {
            int integer = SheepMachine.PopIntOffStack(vm);
        }

        private static SheepFunctionDelegate _printStringDelegate = new SheepFunctionDelegate(sheep_PrintString);
        private static SheepFunctionDelegate _callSheep = new SheepFunctionDelegate(sheep_CallSheep);
        private static SheepFunctionDelegate _clearFlag = new SheepFunctionDelegate(sheep_clearFlag);
        private static SheepFunctionDelegate _continueDialogue = new SheepFunctionDelegate(sheep_ContinueDialogue);
        private static SheepFunctionDelegate _continueDialogueNoFidgets = new SheepFunctionDelegate(sheep_ContinueDialogueNoFidgets);
        private static SheepFunctionDelegate _cutToCameraAngle = new SheepFunctionDelegate(sheep_CutToCameraAngle);
        private static SheepFunctionDelegate _doesEgoHaveInvItem = new SheepFunctionDelegate(sheep_DoesEgoHaveInvItem);
        private static SheepFunctionDelegate _getFlag = new SheepFunctionDelegate(sheep_GetFlag);
        private static SheepFunctionDelegate _finishedScreen = new SheepFunctionDelegate(sheep_FinishedScreen);
        private static SheepFunctionDelegate _getGameVariableIntDelegate = new SheepFunctionDelegate(sheep_GetGameVariableInt);
        private static SheepFunctionDelegate _getChatCount = new SheepFunctionDelegate(sheep_GetChatCount);
        private static SheepFunctionDelegate _getEgoLocationCount = new SheepFunctionDelegate(sheep_GetEgoLocationCount);
        private static SheepFunctionDelegate _getEgoCurrentLocationCount = new SheepFunctionDelegate(sheep_GetEgoCurrentLocationCount);
        private static SheepFunctionDelegate _getNounVerbCount = new SheepFunctionDelegate(sheep_GetNounVerbCount);
        private static SheepFunctionDelegate _getRandomInt = new SheepFunctionDelegate(sheep_GetRandomInt);
        private static SheepFunctionDelegate _getTopicCount = new SheepFunctionDelegate(sheep_GetTopicCount);
        private static SheepFunctionDelegate _getTopicCountInt = new SheepFunctionDelegate(sheep_GetTopicCountInt);
        private static SheepFunctionDelegate _doesGraceHaveInvItemDelegate = new SheepFunctionDelegate(sheep_DoesGraceHaveInvItem);
        private static SheepFunctionDelegate _doesGabeHaveInvItemDelegate = new SheepFunctionDelegate(sheep_DoesGabeHaveInvItem);
        private static SheepFunctionDelegate _doesModelExistDelegate = new SheepFunctionDelegate(sheep_DoesModelExist);
        private static SheepFunctionDelegate _hideSceneModel = new SheepFunctionDelegate(sheep_HideSceneModel);
        private static SheepFunctionDelegate _initEgoPosition = new SheepFunctionDelegate(sheep_InitEgoPosition);
        private static SheepFunctionDelegate _incNounVerbCount = new SheepFunctionDelegate(sheep_IncNounVerbCount);
        private static SheepFunctionDelegate _incNounVerbCountBoth = new SheepFunctionDelegate(sheep_IncNounVerbCountBoth);
        private static SheepFunctionDelegate _inspectModelUsingAngle = new SheepFunctionDelegate(sheep_InspectModelUsingAngle);
        private static SheepFunctionDelegate _isActiveInvItem = new SheepFunctionDelegate(sheep_IsActiveInvItem);
        private static SheepFunctionDelegate _isActorAtLocation = new SheepFunctionDelegate(sheep_IsActorAtLocation);
        private static SheepFunctionDelegate _isActorNear = new SheepFunctionDelegate(sheep_IsActorNear);
        private static SheepFunctionDelegate _isCurrentEgo = new SheepFunctionDelegate(sheep_IsCurrentEgo);
        private static SheepFunctionDelegate _isCurrentLocation = new SheepFunctionDelegate(sheep_IsCurrentLocation);
        private static SheepFunctionDelegate _isCurrentTimeDelegate = new SheepFunctionDelegate(sheep_IsCurrentTime);
        private static SheepFunctionDelegate _isTopLayerInventory = new SheepFunctionDelegate(sheep_IsTopLayerInventory);
        private static SheepFunctionDelegate _isWalkingActorNear = new SheepFunctionDelegate(sheep_IsWalkingActorNear);
        private static SheepFunctionDelegate _playSoundTrack = new SheepFunctionDelegate(sheep_PlaySoundTrack);
        private static SheepFunctionDelegate _setActorPosition = new SheepFunctionDelegate(sheep_SetActorPosition);
        private static SheepFunctionDelegate _setCameraAngleType = new SheepFunctionDelegate(sheep_SetCameraAngleType);
        private static SheepFunctionDelegate _setFlag = new SheepFunctionDelegate(sheep_SetFlag);
        private static SheepFunctionDelegate _setForcedCameraCuts = new SheepFunctionDelegate(sheep_SetForcedCameraCuts);
        private static SheepFunctionDelegate _setGameTimer = new SheepFunctionDelegate(sheep_SetGameTimer);
        private static SheepFunctionDelegate _setGameVariableInt = new SheepFunctionDelegate(sheep_SetGameVariableInt);
        private static SheepFunctionDelegate _setListenGas = new SheepFunctionDelegate(sheep_SetListenGas);
        private static SheepFunctionDelegate _setLocation = new SheepFunctionDelegate(sheep_setLocation);
        private static SheepFunctionDelegate _setNounVerbCount = new SheepFunctionDelegate(sheep_SetNounVerbCount);
        private static SheepFunctionDelegate _setNounVerbCountBoth = new SheepFunctionDelegate(sheep_SetNounVerbCountBoth);
        private static SheepFunctionDelegate _setTalkGas = new SheepFunctionDelegate(sheep_SetTalkGas);
        private static SheepFunctionDelegate _setTimerSeconds = new SheepFunctionDelegate(sheep_SetTimerSeconds);
        private static SheepFunctionDelegate _showSceneModel = new SheepFunctionDelegate(sheep_ShowSceneModel);
        private static SheepFunctionDelegate _startDialogue = new SheepFunctionDelegate(sheep_StartDialogue);
        private static SheepFunctionDelegate _startDialogueNoFidgets = new SheepFunctionDelegate(sheep_StartDialogueNoFidgets);
        private static SheepFunctionDelegate _startVoiceOver = new SheepFunctionDelegate(sheep_StartVoiceOver);
        private static SheepFunctionDelegate _sheepStartAnimation = new SheepFunctionDelegate(sheep_StartAnimation);
        private static SheepFunctionDelegate _startMom = new SheepFunctionDelegate(sheep_StartMom);
        private static SheepFunctionDelegate _sheepStartMoveAnimation = new SheepFunctionDelegate(sheep_StartMoveAnimation);
        private static SheepFunctionDelegate _stopSoundTrack = new SheepFunctionDelegate(sheep_StopSoundTrack);
        private static SheepFunctionDelegate _turnHead = new SheepFunctionDelegate(sheep_TurnHead);
        private static SheepFunctionDelegate _walkerBoundaryBlockRegion = new SheepFunctionDelegate(sheep_WalkerBoundaryBlockRegion);
        private static SheepFunctionDelegate _walkTo = new SheepFunctionDelegate(sheep_WalkTo);
        private static SheepFunctionDelegate _wasEgoEverInLocation = new SheepFunctionDelegate(sheep_WasEgoEverInLocation);
        private static SheepFunctionDelegate _wasLastLocation = new SheepFunctionDelegate(sheep_WasLastLocation);

        private static SheepFunctionDelegate _dummyVoid = new SheepFunctionDelegate(sheep_DummyVoid);
        private static SheepFunctionDelegate _dummyString = new SheepFunctionDelegate(sheep_DummyString);
        private static SheepFunctionDelegate _dummyStringString = new SheepFunctionDelegate(sheep_DummyStringString);
        private static SheepFunctionDelegate _dummyStringInt = new SheepFunctionDelegate(sheep_DummyStringInt);
        private static SheepFunctionDelegate _dummyInt = new SheepFunctionDelegate(sheep_DummyInt);
        
    }
}
