using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main
{
    public delegate bool ConsoleCommand(string[] args, Console console);

    public abstract class Console
    {
        public void WriteLine(string text, params object[] arg)
        {
            Write(text + Environment.NewLine, arg);
        }

        public abstract void Write(string text, params object[] arg);

        public void AddCommand(string command, ConsoleCommand callback)
        {
            _commands.Add(command, callback);
        }

        public void RunCommand(string command)
        {
            string[] args = command.Split();

            ConsoleCommand callback;
            if (_commands.TryGetValue(args[0], out callback) == false)
                WriteLine("No such command: {0}", args[0]);
            else
            {
                if (callback(args, this) == false)
                    WriteLine("Command returned an error: {0}", command);
            }
        }

        protected Dictionary<string, ConsoleCommand> _commands = new Dictionary<string, ConsoleCommand>();

        public static Console CurrentConsole
        {
            get { return _currentConsole; }
            set { _currentConsole = value; }
        }

        private static Console _currentConsole;
    }
}
