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
        public static void Initialize()
        {
            if (_vm == IntPtr.Zero)
            {


                _vm = SHP_CreateNewVM();


                _compilerOutputDelegate = new CompilerOutputDelegate(compilerOutputCallback);
                
                SHP_SetOutputCallback(_vm,
                    Marshal.GetFunctionPointerForDelegate(_compilerOutputDelegate));
               // SHP_SetOutputCallback(_vm, _compilerOutputDelegate);

                BasicSheepFunctions.Init();
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
            IntPtr import = SHP_AddImport(_vm, name, returnType, Marshal.GetFunctionPointerForDelegate(callback));

            if (import == IntPtr.Zero)
                throw new SheepException("Unable to add import");

            foreach (SymbolType parameterType in parameters)
                SHP_AddImportParameter(import, parameterType);
        }

        public static void RunSheep(string filename, string function)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(FileSystem.Open(filename)))
            {
                string script = reader.ReadToEnd();

                SHP_RunScript(_vm, script, function);
            }
        }

        public static int RunSnippet(string snippet)
        {
            int result;

            _output.Clear();

            int err = SHP_RunSnippet(_vm, string.Format("snippet{{ {0} }}", snippet), out result);

            if (err != 0)
                throw new SheepException("Unable to execute snippet");

            return result;
        }

        public static int PopIntOffStack(IntPtr vm)
        {
            return SHP_PopIntFromStack(vm);
        }

        public static float PopFloatOffStack(IntPtr vm)
        {
            return SHP_PopFloatFromStack(vm);
        }

        public static string PopStringOffStack(IntPtr vm)
        {
            return Marshal.PtrToStringAnsi(SHP_PopStringFromStack(vm));
        }

        public static void PushIntOntoStack(IntPtr vm, int i)
        {
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
        [DllImport("sheepc")]
        private static extern IntPtr SHP_CreateNewVM();

        [DllImport("sheepc")]
        private static extern void SHP_DestroyVM(IntPtr vm);

        [DllImport("sheepc")]
        private static extern IntPtr SHP_AddImport(IntPtr vm, string name, SymbolType returnType, IntPtr callback);

        [DllImport("sheepc")]
        private static extern void SHP_AddImportParameter(IntPtr import, SymbolType parameterType);

        [DllImport("sheepc")]
        private static extern int SHP_PopIntFromStack(IntPtr vm);

        [DllImport("sheepc")]
        private static extern float SHP_PopFloatFromStack(IntPtr vm);
        
        [DllImport("sheepc")]
        private static extern IntPtr SHP_PopStringFromStack(IntPtr vm);

        [DllImport("sheepc")]
        private static extern void SHP_PushIntOntoStack(IntPtr vm, int i);

        [DllImport("sheepc")]
        private static extern int SHP_RunScript(IntPtr vm, string script, string function);

        [DllImport("sheepc")]
        private static extern int SHP_RunSnippet(IntPtr vm, string snippet, out int result);

        [DllImport("sheepc")]
        private static extern void SHP_SetOutputCallback(IntPtr vm, IntPtr callback);

        #endregion Interops
    }
}
