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
    public delegate void SheepEndWaitDelegate(IntPtr vm);
    
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
                    SHP_SetVerbosity(_vm, 3);

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

                        int err = SHP_RunCode(_vm, ms.ToArray(), (int)ms.Length, function);
                        if (err != SHEEP_SUCCESS)
                            throw new SheepException("Unable to execute Sheep script");
                    }
                }
                else
                {
                    // rewind
                    s.Seek(0, System.IO.SeekOrigin.Begin);

                    using (System.IO.StreamReader reader = new System.IO.StreamReader(s))
                    {
                        string script = reader.ReadToEnd();

                        int err = SHP_RunScript(_vm, script, function);
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

        public static int RunSnippet(string snippet)
        {
            if (_vm != IntPtr.Zero)
            {
                int result;

                _output.Clear();

                Console.CurrentConsole.WriteLine(ConsoleVerbosity.Extreme, "Executing snippet: {0}", snippet);
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
            SHP_PopStringFromStack(vm, out result);
            return Marshal.PtrToStringAnsi(result);
        }

        public static void PushIntOntoStack(IntPtr vm, int i)
        {
            if (vm == IntPtr.Zero)
                throw new ArgumentException("vm");

            SHP_PushIntOntoStack(vm, i);
        }

        public static bool IsInWaitSection(IntPtr vm)
        {
            return SHP_IsInWaitSection(vm) != 0;
        }

        public static bool IsSuspended(IntPtr vm)
        {
            return SHP_IsSuspended(vm) != 0;
        }

        public static void Suspend(IntPtr vm)
        {
            SHP_Suspend(vm);
        }

        public static void AddWaitHandle(IntPtr vm, WaitHandle handle)
        {
            _currentWaitHandle.Add(handle);
        }

        public static void ResumeIfNoMoreBlockingWaits()
        {
            if (IsSuspended(_vm))
            {
                foreach (WaitHandle wait in _currentWaitHandle)
                {
                    if (wait.Finished == false)
                        return;
                }

                // all waits are done, so continue on with the script
                _currentWaitHandle.Clear();
                SHP_Resume(_vm);
            }
        }

        private static IntPtr _vm;
        private static List<WaitHandle> _currentWaitHandle = new List<WaitHandle>();

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

        private static void endWaitCallback(IntPtr vm)
        {
            foreach (WaitHandle handle in _currentWaitHandle)
            {
                if (handle.Finished == false)
                {
                    // still waiting on stuff to finish, so suspend the VM
                    SHP_Suspend(vm);
                    return;
                }
            }

            // everything is done!
            _currentWaitHandle.Clear();
        }

        private static CompilerOutputDelegate _compilerOutputDelegate;
        private static SheepEndWaitDelegate _endWaitDelegate;

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
        private static extern int SHP_PopIntFromStack(IntPtr vm, out int result);

        [DllImport("sheep")]
        private static extern int SHP_PopFloatFromStack(IntPtr vm, out float result);
        
        [DllImport("sheep")]
        private static extern int SHP_PopStringFromStack(IntPtr vm, out IntPtr result);

        [DllImport("sheep")]
        private static extern void SHP_PushIntOntoStack(IntPtr vm, int i);

        [DllImport("sheep")]
        private static extern int SHP_RunScript(IntPtr vm, string script, string function);

        [DllImport("sheep")]
        private static extern int SHP_RunCode(IntPtr vm, byte[] code, int length, string function);

        [DllImport("sheep")]
        private static extern int SHP_RunSnippet(IntPtr vm, string snippet, out int result);

        [DllImport("sheep")]
        private static extern void SHP_SetOutputCallback(IntPtr vm, IntPtr callback);

        [DllImport("sheep")]
        private static extern int SHP_IsInWaitSection(IntPtr vm);

        [DllImport("sheep")]
        private static extern int SHP_IsSuspended(IntPtr vm);

        [DllImport("sheep")]
        private static extern int SHP_Suspend(IntPtr vm);

        [DllImport("sheep")]
        private static extern int SHP_Resume(IntPtr vm);

        [DllImport("sheep")]
        private static extern void SHP_SetEndWaitCallback(IntPtr vm, IntPtr callback);

        [DllImport("sheep")]
        private static extern void SHP_SetVerbosity(IntPtr vm, int verbosity);

        #endregion Interops
    }
}
