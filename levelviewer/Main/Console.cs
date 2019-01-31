using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main
{
    public delegate bool ConsoleCommand(string[] args, Console console);

    public enum ConsoleSeverity
    {
        Debug = 1,
        Info = 2,
        Normal = 3,
        Warning = 4,
        Error = 5
    }

    public abstract class Console
    {
        public void WriteLine(ConsoleSeverity severity, string text, params object[] args)
        {
            Write(severity, text, args);
        }

        public void WriteLine(string text, params object[] arg)
        {
            Write(ConsoleSeverity.Normal, text + Environment.NewLine, arg);
        }

        public abstract void Write(ConsoleSeverity severity, string text, params object[] arg);

        public virtual void ReportError(string error)
        {
            WriteLine(ConsoleSeverity.Error, "Error: " + error);
        }

        public void AddCommand(string command, ConsoleCommand callback)
        {
            _commands.Add(command, callback);
        }

        public void RunCommand(string command)
        {
            _previousCommand = command;

            string[] args = command.Split();

            if (args[0].Equals("dumpCommands", StringComparison.OrdinalIgnoreCase) ||
                args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                dumpCommands();
                return;
            }
            else if (args[0].Equals("sheep", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length == 1)
                    WriteLine("No statement found.");
                else
                    runSheep(command.Substring(5));

                return;
            }
            else if (args[0].Equals("printsearchpath", StringComparison.OrdinalIgnoreCase))
            {
                printSearchPath();

                return;
            }

            ConsoleCommand callback;
            if (_commands.TryGetValue(args[0], out callback) == false)
                WriteLine("No such command: {0}", args[0]);
            else
            {
                if (callback(args, this) == false)
                    WriteLine("Command returned an error: {0}", command);
            }
        }

        public string PreviousCommand
        {
            get { return _previousCommand; }
        }

        private void dumpCommands()
        {
            WriteLine("Known commands:");
            foreach (KeyValuePair<string, ConsoleCommand> command in _commands)
            {
                WriteLine("\t{0}", command.Key);
            }
            WriteLine("\tDumpCommands");
            WriteLine("\tPrintSearchPath");
            WriteLine("\tSheep - executes a sheep statement");
        }

        private void runSheep(string statement)
        {
            WriteLine("Executing command: " + statement);

            try
            {
                Sheep.SheepMachine.RunCommand(statement);
            }
            catch (Sheep.SheepException ex)
            {
                WriteLine("Error: " + ex.Message);
            }
        }

        private void printSearchPath()
        {
            foreach (var item in FileSystem.SearchPath)
            {
                WriteLine("\t" + item.Name);
            }
        }

        protected Dictionary<string, ConsoleCommand> _commands = new Dictionary<string, ConsoleCommand>(StringComparer.OrdinalIgnoreCase);
        private string _previousCommand;

        public static Console CurrentConsole
        {
            get { return _currentConsole; }
            set { _currentConsole = value; }
        }

        private static Console _currentConsole = new NullConsole();
    }

    public class NullConsole : Console
    {
        public override void Write(ConsoleSeverity severity, string text, params object[] arg)
        {
            // do nothing
        }
    }
}
