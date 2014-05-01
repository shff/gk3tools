using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Gk3Main.Sheep
{
    public class SheepException : Exception
    {
        public SheepException(string message)
            : base(message)
        {
        }

        public SheepException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public delegate void SheepFunctionDelegate(IntPtr vm);
    public delegate void SheepEndWaitDelegate(IntPtr vm, IntPtr context);
    
    public enum SymbolType
    {
        Void,
        Integer,
        Float,
        String
    }

    public static class SheepMachine
    {
        private const string SheepUnavailableError = "The Sheep VM is unavailable";

        public static void Initialize()
        {
            if (_compiler == IntPtr.Zero)
            {
                _compiler = shp_CreateNewCompiler(1);
            }

            if (_vm == IntPtr.Zero)
            {
                try
                {
                    _vm = SHP_CreateNewVM(1);
                    SHP_SetVerbosity(_vm, 1);

                    _compilerOutputDelegate = new CompilerOutputDelegate(compilerOutputCallback);
                    _endWaitDelegate = new SheepEndWaitDelegate(endWaitCallback);

                    SHP_SetOutputCallback(_vm,
                        Marshal.GetFunctionPointerForDelegate(_compilerOutputDelegate));
                    SHP_SetEndWaitCallback(_vm,
                        Marshal.GetFunctionPointerForDelegate(_endWaitDelegate));

                    BasicSheepFunctions.Init();
                }
                catch (DllNotFoundException ex)
                {
                    throw new SheepException("Unable to load the Sheep library.", ex);
                }
            }
        }

        public static void Shutdown()
        {
            if (_vm != IntPtr.Zero)
            {
                SHP_DestroyVM(_vm);
                _vm = IntPtr.Zero;
            }
        }

        public static void AddImport(string name, SheepFunctionDelegate callback, SymbolType returnType, params SymbolType[] parameters)
        {
            if (_vm != IntPtr.Zero)
            {
                SHP_SetImportCallback(_vm, name, Marshal.GetFunctionPointerForDelegate(callback));

                shp_DefineImportFunction(_compiler, name, returnType, parameters, parameters.Length);
            }
            else
            {
                throw new SheepException(SheepUnavailableError);
            }
        }

        public static void RunSheep(IntPtr parentContext, string filename, string function)
        {
            if (_vm != IntPtr.Zero)
            {

                System.IO.Stream s = FileSystem.Open(filename);
                
                // determine wether this is binary or not
                if (s.ReadByte() == 0x47)
                {
                    // rewind
                    s.Seek(0, System.IO.SeekOrigin.Begin);

                    byte[] buffer = new byte[1024];
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(s))
                    {
                        while (true)
                        {
                            int read = reader.Read(buffer, 0, buffer.Length);
                            if (read <= 0) break;

                            ms.Write(buffer, 0, read);
                        }

                        IntPtr script;
                        int err = shp_LoadScriptFromBytecode(ms.ToArray(), (int)ms.Length, out script);

                        if (err != SHEEP_SUCCESS)
                            throw new SheepException("Unable to load sheep bytecode");

                        IntPtr context;
                        if (parentContext == IntPtr.Zero)
                            err = shp_PrepareScriptForExecution(_vm, script, function, out context);
                        else
                            err = shp_PrepareScriptForExecutionWithParent(_vm, script, function, parentContext, out context);
                        if (err != SHEEP_SUCCESS)
                            throw new SheepException("Unable to execute Sheep script");

                        err = shp_Execute(context);
                        if (err != SHEEP_SUCCESS && err != SHEEP_SUSPENDED)
                        {
                            SHP_PrintStackTrace(_vm);
                            throw new SheepException("Unable to execute Sheep script");
                        }
                    }
                }
                else
                {
                    // rewind
                    s.Seek(0, System.IO.SeekOrigin.Begin);

                    using (System.IO.StreamReader reader = new System.IO.StreamReader(s))
                    {
                        string script = reader.ReadToEnd();

                        IntPtr result;
                        int err = shp_CompileScript(_compiler, script, out result);

                        if (err != SHEEP_SUCCESS)
                            throw new SheepException("Unable to compile Sheep script");

                        IntPtr context;
                        err = shp_PrepareScriptForExecution(_vm, result, function, out context);
                        if (err != SHEEP_SUCCESS)
                            throw new SheepException("Unable to execute Sheep script");

                        err = shp_Execute(context);
                        if (err != SHEEP_SUCCESS)
                            throw new SheepException("Unable to execute Sheep script");
                    }
                }

                s.Close();
            }
            else
            {
                throw new SheepException(SheepUnavailableError);
            }
        }

        public static int RunSnippet(string snippet, Game.Nouns noun, Game.Verbs verb)
        {
            if (_vm != IntPtr.Zero)
            {
                int result;

                _output.Clear();

                Console.CurrentConsole.WriteLine(ConsoleVerbosity.Extreme, "Executing snippet: {0}", snippet);

                string script = string.Format("symbols {{ int result$; int n$; int v$; }} code {{ main$() {{ result$ = {0}; }} }}", snippet);

                IntPtr compiledSnippet;
                if (shp_CompileScript(_compiler, script, out compiledSnippet) != SHEEP_SUCCESS)
                    throw new SheepException("Unable to compile snippet");

                IntPtr context;
                if (shp_PrepareScriptForExecution(_vm, compiledSnippet, "main$", out context) != SHEEP_SUCCESS)
                    throw new SheepException("Unable to execute snippet");

                if (shp_SetVariableI(context, 1, (int)noun) != SHEEP_SUCCESS ||
                    shp_SetVariableI(context, 2, (int)verb) != SHEEP_SUCCESS)
                    throw new SheepException("Unable to execute snippet");

                int err = shp_Execute(context);

                if (err != 0)
                {
                    Logger.WriteError("Error ({0}) received when attempting to execute snippet: {1}", err, snippet);
                    throw new SheepException("Unable to execute snippet");
                }

                if (shp_GetVariableI(context, 0, out result) != SHEEP_SUCCESS)
                    throw new SheepException("Unable to get result of snippet");

                return result;
            }
            else
            {
                throw new SheepException(SheepUnavailableError);
            }
        }

        public static int RunSnippet(string snippet)
        {
            return RunSnippet(snippet, Game.Nouns.N_NONE, Game.Verbs.V_NONE);
        }

        public static void RunCommand(string command)
        {
            string sheep = string.Format("code {{ main$() {{ {0}; }} }}", command);

            if (_vm != IntPtr.Zero)
            {
                _output.Clear();

                IntPtr result;
                int err = shp_CompileScript(_compiler, sheep, out result);
                if (err != SHEEP_SUCCESS)
                    throw new SheepException("Unable to compile sheep command: " + err.ToString());

                IntPtr context;
                err = shp_PrepareScriptForExecution(_vm, result, "main$", out context);
                if (err != SHEEP_SUCCESS)
                    throw new SheepException("Unable to execute command: " + err.ToString());

                err = shp_Execute(context);

                if (err != SHEEP_SUCCESS && err != SHEEP_SUSPENDED)
                {
                    Logger.WriteError("Error ({0}) received when attempting to execute Sheep command: {1}", err, command);
                    throw new SheepException("Unable to execute command: " + err.ToString());
                }
            }
            else
            {
                throw new SheepException(SheepUnavailableError);
            }
        }

        public static void RunScript(string script, string function)
        {
            IntPtr result;
            int err = shp_CompileScript(_compiler, script, out result);
            if (err != SHEEP_SUCCESS)
                throw new SheepException("Unable to compile sheep script: " + err.ToString());

            IntPtr context;
            err = shp_PrepareScriptForExecution(_vm, result, function, out context);
            if (err != SHEEP_SUCCESS)
                throw new SheepException("Unable to execute Sheep script");

            err = shp_Execute(context);
            if (err != SHEEP_SUCCESS)
                throw new SheepException("Unable to execute Sheep script");
        }

        public static void ReleaseVMContext(IntPtr context)
        {
            shp_ReleaseVMContext(context);
        }

        public static int GetVMContextState(IntPtr context)
        {
            return shp_GetVMContextState(context);
        }

        public static int PopIntOffStack(IntPtr vm)
        {
            if (vm == IntPtr.Zero)
                throw new ArgumentException("vm");

            int result;
            SHP_PopIntFromStack(vm, out result);

            return result;
        }

        public static float PopFloatOffStack(IntPtr vm)
        {
            if (vm == IntPtr.Zero)
                throw new ArgumentException("vm");

            float result;
            SHP_PopFloatFromStack(vm, out result);

            return result;
        }

        public static string PopStringOffStack(IntPtr vm)
        {
            if (vm == IntPtr.Zero)
                throw new ArgumentException("vm");

            IntPtr result;
            int r = SHP_PopStringFromStack(vm, out result);
            if (r != 0)
                throw new SheepException("Unable to pop string: " + r.ToString());

            return Marshal.PtrToStringAnsi(result);
        }

        public static void PushIntOntoStack(IntPtr vm, int i)
        {
            if (vm == IntPtr.Zero)
                throw new ArgumentException("vm");

            SHP_PushIntOntoStack(vm, i);
        }

        public static bool IsInWaitSection(IntPtr context)
        {
            return SHP_IsInWaitSection(context) != 0;
        }

        public static void Suspend(IntPtr context)
        {
            SHP_Suspend(context);
        }

        public static void AddWaitHandle(IntPtr context, WaitHandle handle)
        {
            if (handle == null)
                throw new ArgumentNullException("handle");

            if (_waitHandles.ContainsKey(context) == false)
                _waitHandles.Add(context, new List<WaitHandle>());

            _waitHandles[context].Add(handle);
        }

        public static void ResumeIfNoMoreBlockingWaits()
        {
            List<IntPtr> deadWaits = new List<IntPtr>();

            foreach (KeyValuePair<IntPtr, List<WaitHandle>> wait in _waitHandles)
            {
                bool canResume = true;
                foreach (WaitHandle l in wait.Value)
                {
                    if (l.Finished == false)
                    {
                        canResume = false;
                        break;
                    }
                }

                if (canResume)
                {
                    // remove the context from the list
                    wait.Value.Clear();
                        
                    // add it to the list of dead waits so it can be resumed
                    deadWaits.Add(wait.Key);
                }
            }

            // remove old waits
            foreach (IntPtr wait in deadWaits)
            {
                // resume
                int result = shp_Execute(wait);
                if (result != SHEEP_SUCCESS && result != SHEEP_SUSPENDED)
                    throw new SheepException("Unable to resume");

                // remove dead waits as long as no new waits
                // were added after resuming
                if (_waitHandles[wait].Count == 0) 
                    _waitHandles.Remove(wait);
            }
        }

        public static void CancelAllWaits()
        {
            // TODO
        }

        public static void GetVersion(out int major, out int minor, out int rev)
        {
            SHP_Version v = shp_GetVersion();

            major = v.Major;
            minor = v.Minor;
            rev = v.Revision;
        }

        private static IntPtr _vm;
        private static IntPtr _compiler;
        private static Dictionary<IntPtr, List<WaitHandle>> _waitHandles = new Dictionary<IntPtr, List<WaitHandle>>();

        struct CompilerOutput
        {
            public int LineNumber;
            public string Text;
        }

        private static List<CompilerOutput> _output = new List<CompilerOutput>();

        [StructLayout(LayoutKind.Sequential)]
        struct SHP_CompilerOutput
        {
            public int LineNumber;
            public IntPtr Output;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void CompilerOutputDelegate(int lineNumber, IntPtr message);
        private static void compilerOutputCallback(int lineNumber, IntPtr message)
        {
            CompilerOutput co;
            co.LineNumber = lineNumber;
            co.Text = Marshal.PtrToStringAnsi(message);

            _output.Add(co);
        }

        private static void endWaitCallback(IntPtr vm, IntPtr context)
        {
            if (_waitHandles.ContainsKey(context))
            {
                foreach (WaitHandle wait in _waitHandles[context])
                {
                    if (wait.Finished == false)
                    {
                        // still waiting on stuff to finish, so suspend the VM
                        SHP_Suspend(context);
                        return;
                    }
                }

                // everything is done!
                _waitHandles[context].Clear();
            }
        }

        private static CompilerOutputDelegate _compilerOutputDelegate;
        private static SheepEndWaitDelegate _endWaitDelegate;

        #region Interops

        const int SHEEP_SUCCESS = 0;
        const int SHEEP_ERROR = -1;
        const int SHEEP_SUSPENDED = 2;

        private struct SHP_Version
        {
            public short Major;
            public short Minor;
            public short Revision;
        }

        [DllImport("sheep")]
        private static extern IntPtr SHP_CreateNewVM(int languageVersion);

        [DllImport("sheep")]
        private static extern void SHP_DestroyVM(IntPtr vm);

        [DllImport("sheep")]
        private static extern void SHP_SetImportCallback(IntPtr vm, string name, IntPtr callback);

        [DllImport("sheep")]
        private static extern void SHP_AddImportParameter(IntPtr import, SymbolType parameterType);

        [DllImport("sheep")]
        private static extern int shp_GetVMContextState(IntPtr context);

        [DllImport("sheep")]
        private static extern void shp_ReleaseVMContext(IntPtr context);

        [DllImport("sheep")]
        private static extern int SHP_PopIntFromStack(IntPtr context, out int result);

        [DllImport("sheep")]
        private static extern int SHP_PopFloatFromStack(IntPtr context, out float result);
        
        [DllImport("sheep")]
        private static extern int SHP_PopStringFromStack(IntPtr context, out IntPtr result);

        [DllImport("sheep")]
        private static extern void SHP_PushIntOntoStack(IntPtr context, int i);

        [DllImport("sheep")]
        private static extern int SHP_RunSnippet(IntPtr vm, string snippet, out int result);

        [DllImport("sheep")]
        private static extern int SHP_RunNounVerbSnippet(IntPtr vm, string snippet, int noun, int verb, out int result);

        [DllImport("sheep")]
        private static extern int shp_PrepareScriptForExecution(IntPtr vm, IntPtr script, string function, out IntPtr context);

        [DllImport("sheep")]
        private static extern int shp_PrepareScriptForExecutionWithParent(IntPtr vm, IntPtr script, string function, IntPtr parent, out IntPtr context);

        [DllImport("sheep")]
        private static extern int shp_GetNumVariables(IntPtr context);

        [DllImport("sheep")]
        private static extern int shp_GetVariableName(IntPtr context, int index, out string name);

        [DllImport("sheep")]
        private static extern int shp_GetVariableI(IntPtr context, int index, out int value);

        [DllImport("sheep")]
        private static extern int shp_GetVariableF(IntPtr context, int index, out float value);

        [DllImport("sheep")]
        private static extern int shp_SetVariableI(IntPtr context, int index, int value);

        [DllImport("sheep")]
        private static extern int shp_SetVariableF(IntPtr context, int index, float value);

        [DllImport("sheep")]
        private static extern void SHP_SetOutputCallback(IntPtr vm, IntPtr callback);

        [DllImport("sheep")]
        private static extern int SHP_IsInWaitSection(IntPtr vm);

        [DllImport("sheep")]
        private static extern int SHP_Suspend(IntPtr context);

        [DllImport("sheep")]
        private static extern int shp_Execute(IntPtr context);

        [DllImport("sheep")]
        private static extern void SHP_SetEndWaitCallback(IntPtr vm, IntPtr callback);

        [DllImport("sheep")]
        private static extern void SHP_SetVerbosity(IntPtr vm, int verbosity);

        [DllImport("sheep")]
        private static extern void SHP_PrintStackTrace(IntPtr vm);

        [DllImport("sheep")]
        private static extern IntPtr shp_CreateNewCompiler(int languageVersion);

        [DllImport("sheep")]
        private static extern void shp_DestroyCompiler(IntPtr compiler);

        [DllImport("sheep")]
        private static extern int shp_DefineImportFunction(IntPtr compiler, string name, SymbolType returnType, SymbolType[] parameters, int numParameters);

        [DllImport("sheep")]
        private static extern int shp_CompileScript(IntPtr compiler, string script, out IntPtr result);

        [DllImport("sheep")]
        private static extern int shp_LoadScriptFromBytecode(byte[] bytecode, int length, out IntPtr result);

        [DllImport("sheep")]
        private static extern void shp_ReleaseSheepScript(IntPtr script);


        [DllImport("sheep")]
        private static extern SHP_Version shp_GetVersion();

        #endregion Interops
    }
}
