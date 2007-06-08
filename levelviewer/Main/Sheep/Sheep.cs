using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sheep
{
    public delegate int SheepFunctionDelegate(Parameter[] parameters);

    enum SheepParameterType
    {
        String,
        Integer,
        Float
    }

    struct SheepFunction
    {
        public string Name;
        public SheepParameterType[] Parameters;
        public SheepFunctionDelegate Callback;
    }

    public static class SheepMachine
    {
        static SheepMachine()
        {
            BasicSheepFunctions.Init();
        }

        public static void CallSheep(string filename, string function)
        {
            SheepScript script = new SheepScript(filename, _state);
            script.Execute(function);
        }

        internal static void CallVoidSystemFunction(string name, Parameter[] parameters)
        {
            try
            {
                Console.CurrentConsole.WriteLine("Calling system function \"{0}\"", name);

                SheepFunction function;
                if (_imports.TryGetValue(name, out function) == false)
                    Console.CurrentConsole.WriteLine("Function not found: " + name);
                else
                    function.Callback(parameters);
            }
            catch (NotImplementedException)
            {
                Console.CurrentConsole.WriteLine("{0} not implemented", name);
            }
        }

        internal static int CallIntSystemFunction(string name, Parameter[] parameters)
        {
            try
            {
                Console.CurrentConsole.WriteLine("Calling int system function \"{0}\"", name);

                SheepFunction function;
                if (_imports.TryGetValue(name, out function) == false)
                    Console.CurrentConsole.WriteLine("Function not found: " + name);
                else
                    return function.Callback(parameters);
            }
            catch(NotImplementedException)
            {
                Console.CurrentConsole.WriteLine("{0} not implemented", name);
                
            }
            return 0;
        }

        internal static string CallStringSystemFunction(string name, Parameter[] parameters)
        {
            try
            {
                Console.CurrentConsole.WriteLine("Calling int system function \"{0}\"", name);

                SheepFunction function;
                if (_imports.TryGetValue(name, out function) == false)
                    Console.CurrentConsole.WriteLine("Function not found: " + name);
                else
                    function.Callback(parameters);
            }
            catch (NotImplementedException)
            {
                Console.CurrentConsole.WriteLine("{0} not implemented", name);
            }
            return "";
        }

        public static void AddFunction(string name, SheepFunctionDelegate callback)
        {
            SheepFunction function;

            function.Name = name;
            function.Parameters = null;
            function.Callback = callback;

            _imports.Add(name, function);
        }

        public static bool EvaluateBoolean(string expression)
        {
            return false;
        }

        public static int ExecuteRaw(byte[] code)
        {
            int instructionPtr = 0;

            while (instructionPtr < code.Length)
            {
                byte op = code[instructionPtr++];
                int param;

                switch (op)
                {
                    case 0x00:
                        // do nothing
                        break;
                    case 0x02:
                        param = getIntFromBytes(code, ref instructionPtr);
                        // do nothing
                        break;
                    case 0x03:
                        param = getIntFromBytes(code, ref instructionPtr);
                        // do nothing
                        break;
                    case 0x06:
                        param = getIntFromBytes(code, ref instructionPtr);
                        instructionPtr = param;
                        break;
                    case 0x08:
                        param = getIntFromBytes(code, ref instructionPtr);
                        // do nothing
                        break;
                    case 0x09:
                        // do nothing
                        break;
                    case 0x0A:
                        // do nothing
                        break;
                    case 0x0B:
                        return 0;
                    case 0x13:
                        param = getIntFromBytes(code, ref instructionPtr);
                        // do nothing
                        break;
                    case 0x15:
                        param = getIntFromBytes(code, ref instructionPtr);
                        // do nothing
                        break;
                    case 0x16:
                        // do nothing
                        break;
                    case 0x21:
                        // do nothing
                        break;
                    case 0x2D:
                        param = getIntFromBytes(code, ref instructionPtr);
                        // do nothing
                        break;
                    case 0x30:
                        // do nothing
                        break;
                    case 0x31:
                        // do nothing
                        break;
                    case 0x33:
                        // do nothing
                        break;
                    default:
                        throw new NotImplementedException("Instruction " +
                            op.ToString("X") + " not implemented");
                }
            }

            return -1;
        }

        private static int getIntFromBytes(byte[] bytes, ref int index)
        {
            int i = BitConverter.ToInt32(bytes, index);

            index += 4;

            return i;
        }

        private static SheepStateManager _state = new SheepStateManager();
        private static Dictionary<string, SheepFunction> _imports = 
            new Dictionary<string, SheepFunction>();
    }
}
