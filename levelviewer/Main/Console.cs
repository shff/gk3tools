using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main
{
    public delegate bool ConsoleCommand(string[] args, Console console);

    public enum ConsoleVerbosity
    {
        Extreme = 0,
        Debug = 1,
        Polite = 2,
        Silent = 3
    }

    public abstract class Console
    {
        public void WriteLine(ConsoleVerbosity verbosity, string text, params object[] args)
        {
            Write(verbosity, text, args);
        }

        public void WriteLine(string text, params object[] arg)
        {
            Write(ConsoleVerbosity.Polite, text + Environment.NewLine, arg);
        }

        public abstract void Write(ConsoleVerbosity verbosity, string text, params object[] arg);

        public virtual void ReportError(string error)
        {
            WriteLine("Error: " + error);
        }

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

        public ConsoleVerbosity Verbosity
        {
            get { return _verbosity; }
            set { _verbosity = value; }
        }

        protected Dictionary<string, ConsoleCommand> _commands = new Dictionary<string, ConsoleCommand>();
        private ConsoleVerbosity _verbosity;

        public static Console CurrentConsole
        {
            get { return _currentConsole; }
            set { _currentConsole = value; }
        }

        private static Console _currentConsole = new NullConsole();
    }

    public class NullConsole : Console
    {
        public override void Write(ConsoleVerbosity verbosity, string text, params object[] arg)
        {
            // do nothing
        }
    }
}
