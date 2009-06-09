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
            if (_vm == IntPtr.Zero)
            {
                try
                {
                    _vm = SHP_CreateNewVM();

                    _compilerOutputDelegate = new CompilerOutputDelegate(compilerOutputCallback);

                    SHP_SetOutputCallback(_vm,
                        Marshal.GetFunctionPointerForDelegate(_compilerOutputDelegate));
                    // SHP_SetOutputCallback(_vm, _compilerOutputDelegate);

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
                IntPtr import = SHP_AddImport(_vm, name, returnType, Marshal.GetFunctionPointerForDelegate(callback));

                if (import == IntPtr.Zero)
                    throw new SheepException("Unable to add import");

                foreach (SymbolType parameterType in parameters)
                    SHP_AddImportParameter(import, parameterType);
            }
            else
            {
                throw new SheepException(SheepUnavailableError);
            }
        }

        public static void RunSheep(string filename, string function)
        {
            if (_vm != IntPtr.Zero)
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(FileSystem.Open(filename)))
                {
                    string script = reader.ReadToEnd();

                    if (SHP_RunScript(_vm, script, function) != SHEEP_SUCCESS)
                        throw new SheepException("Unable to execute Sheep script");
                }
            }
            else
            {
                throw new SheepException(SheepUnavailableError);
            }
        }

        public static int RunSnippet(string snippet)
        {
            if (_vm != IntPtr.Zero)
            {
                int result;

                _output.Clear();

                int err = SHP_RunSnippet(_vm, string.Format("snippet {{ {0} }}", snippet), out result);

                if (err != 0)
                    throw new SheepException("Unable to execute snippet");

                return result;
            }
            else
            {
                throw new SheepException(SheepUnavailableError);
            }
        }

        public static void RunCommand(string command)
        {
            string sheep = string.Format("code {{ main$() {{ {0} }} }}", command);

            if (_vm != IntPtr.Zero)
            {
                int result;

                _output.Clear();

                int err = SHP_RunScript(_vm, sheep, "main$");

                if (err != 0)
                    throw new SheepException("Unable to execute snippet");
            }
            else
            {
                throw new SheepException(SheepUnavailableError);
            }
        }

        public static int PopIntOffStack(IntPtr vm)
        {
            if (vm == IntPtr.Zero)
                throw new ArgumentException("vm");

            return SHP_PopIntFromStack(vm);
        }

        public static float PopFloatOffStack(IntPtr vm)
        {
            if (vm == IntPtr.Zero)
                throw new ArgumentException("vm");

            return SHP_PopFloatFromStack(vm);
        }

        public static string PopStringOffStack(IntPtr vm)
        {
            if (vm == IntPtr.Zero)
                throw new ArgumentException("vm");

            return Marshal.PtrToStringAnsi(SHP_PopStringFromStack(vm));
        }

        public static void PushIntOntoStack(IntPtr vm, int i)
        {
            if (vm == IntPtr.Zero)
                throw new ArgumentException("vm");

            SHP_PushIntOntoStack(vm, i);
        }

        private static IntPtr _vm;

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
        private static CompilerOutputDelegate _compilerOutputDelegate;

        #region Interops

        const int SHEEP_SUCCESS = 0;
        const int SHEEP_ERROR = -1;

        [DllImport("sheep")]
        private static extern IntPtr SHP_CreateNewVM();

        [DllImport("sheep")]
        private static extern void SHP_DestroyVM(IntPtr vm);

        [DllImport("sheep")]
        private static extern IntPtr SHP_AddImport(IntPtr vm, string name, SymbolType returnType, IntPtr callback);

        [DllImport("sheep")]
        private static extern void SHP_AddImportParameter(IntPtr import, SymbolType parameterType);

        [DllImport("sheep")]
        private static extern int SHP_PopIntFromStack(IntPtr vm);

        [DllImport("sheep")]
        private static extern float SHP_PopFloatFromStack(IntPtr vm);
        
        [DllImport("sheep")]
        private static extern IntPtr SHP_PopStringFromStack(IntPtr vm);

        [DllImport("sheep")]
        private static extern void SHP_PushIntOntoStack(IntPtr vm, int i);

        [DllImport("sheep")]
        private static extern int SHP_RunScript(IntPtr vm, string script, string function);

        [DllImport("sheep")]
        private static extern int SHP_RunSnippet(IntPtr vm, string snippet, out int result);

        [DllImport("sheep")]
        private static extern void SHP_SetOutputCallback(IntPtr vm, IntPtr callback);

        #endregion Interops
    }
}
