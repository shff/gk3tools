using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main
{
    public static class Logger
    {
        static Logger()
        {
            _writer = new System.IO.StreamWriter("log.txt");
        }

        public static void Close()
        {
            _writer.Close();
            _writer = null;
        }

        public static void WriteError(string error, params object[] args)
        {
            if (_localEcho) Console.WriteLine(error, args);

            _writer.WriteLine(error, args);
        }

        public static void WriteInfo(string info, params object[] args)
        {
            if (_localEcho) Console.WriteLine(info, args);

            _writer.WriteLine(info, args);
        }

        public static void WriteDebug(string msg, params object[] args)
        {
            if (_localEcho) Console.WriteLine(msg, args);

            _writer.WriteLine(msg, args);
        }

        public static bool LocalEcho
        {
            get { return _localEcho; }
            set { _localEcho = value; }
        }

        private static bool _localEcho = true;
        private static System.IO.StreamWriter _writer;
    }
}
