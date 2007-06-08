using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sheep
{
    static class BasicSheepFunctions
    {
        public static void Init()
        {
            SheepMachine.AddFunction("PrintString", 
                new SheepFunctionDelegate(sheep_PrintString));
            SheepMachine.AddFunction("IsCurrentEgo",
                new SheepFunctionDelegate(sheep_IsCurrentEgo));
            SheepMachine.AddFunction("IsCurrentTime",
                new SheepFunctionDelegate(sheep_IsCurrentTime));
        }

        private static int sheep_PrintString(Parameter[] parameters)
        {
            printParams("PrintString", parameters);

            throw new NotImplementedException();

            return 0;
        }

        private static int sheep_IsCurrentEgo(Parameter[] parameters)
        {
            printParams("IsCurrentEgo", parameters);

            throw new NotImplementedException();

            return 0;
        }

        private static int sheep_IsCurrentTime(Parameter[] parameters)
        {
            printParams("IsCurrentTime", parameters);

            throw new NotImplementedException();

            return 0;
        }

        private static void printParams(string function, Parameter[] parameters)
        {
            Console.CurrentConsole.WriteLine("Inside {0} with params:", function);

            foreach (Parameter param in parameters)
            {
                Console.CurrentConsole.Write("Type: {0} ", param.Type);

                if (param.Type == ParameterType.Integer)
                    Console.CurrentConsole.WriteLine(" value: {0}", param.Integer);
                else if (param.Type == ParameterType.Float)
                    Console.CurrentConsole.WriteLine(" value: {0}", param.Float);
                else if (param.Type == ParameterType.String)
                    Console.CurrentConsole.WriteLine(" value: {0}", param.String);
            }
        }
    }
}
