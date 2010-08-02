using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main
{
    public enum LoggerStream
    {
        Normal,
        Resource,
        Animation,
        Debug
    }

    public static class Logger
    {
        static Logger()
        {
            string mypath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            mypath = System.IO.Path.GetDirectoryName(mypath);

            _writer = new System.IO.StreamWriter(mypath 
                + System.IO.Path.DirectorySeparatorChar + "log.txt");

            _streams = new LoggerStreamInfo[]
            {
                // make sure these stay in the same order as they're
                // declared in the LoggerStream enum!
                new LoggerStreamInfo(LoggerStream.Normal),
                new LoggerStreamInfo(LoggerStream.Resource),
                new LoggerStreamInfo(LoggerStream.Animation),
                new LoggerStreamInfo(LoggerStream.Debug)
            };

            WriteInfo("Logging started");
#if DEBUG
            _streams[(int)LoggerStream.Normal].LocalEcho = true;
            _streams[(int)LoggerStream.Debug].LocalEcho = true;
#endif
        }

        public static void Close()
        {
            WriteInfo("Logging ended");
            _writer.Close();
            _writer = null;
        }

        public static void WriteError(string error, params object[] args)
        {
            if (_streams[(int)LoggerStream.Normal].LocalEcho) 
                Console.CurrentConsole.WriteLine(error, args);

            _writer.WriteLine(error, args);
        }

        public static void WriteInfo(string info, params object[] args)
        {
            WriteInfo(info, LoggerStream.Normal, args);
        }

        public static void WriteInfo(string info, LoggerStream stream, params object[] args)
        {
            if (_streams[(int)stream].Enabled == false)
                return;

            if (_streams[(int)stream].LocalEcho)
                Console.CurrentConsole.WriteLine(info, args);

            _writer.WriteLine(DateTime.Now.ToLongTimeString() + ": " + info, args);
        }

        public static void WriteDebug(string msg, params object[] args)
        {
            if (_streams[(int)LoggerStream.Normal].LocalEcho) 
                Console.CurrentConsole.WriteLine(msg, args);

            _writer.WriteLine(msg, args);
        }

        public static void SetLocalEcho(LoggerStream stream, bool echo)
        {
            _streams[(int)stream].LocalEcho = echo;
        }

        public static bool GetLocalEcho(LoggerStream stream)
        {
            return _streams[(int)stream].LocalEcho;
        }

        private struct LoggerStreamInfo
        {
            public LoggerStream Type;
            public bool LocalEcho;
            public bool Enabled;

            public LoggerStreamInfo(LoggerStream type)
            {
                Type = type;
                LocalEcho = false;
                Enabled = true;
            }
        }

        private static LoggerStreamInfo[] _streams;
        private static System.IO.StreamWriter _writer;
    }
}
