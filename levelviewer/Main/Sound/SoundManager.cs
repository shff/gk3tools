using System;
using System.Collections.Generic;
using System.Text;

#if !SOUND_DISABLED
using IrrKlang;
#endif

namespace Gk3Main.Sound
{
    public class SoundException : Exception
    {
        public SoundException(string message)
            : base(message) {}
    }

#if !SOUND_DISABLED
    public class SoundManager
    {
        private const int _maxSources = 16;

        public static void Init()
        {
            _engine = new ISoundEngine();
            _engine.AddFileFactory(new BarnFileFactory());
            _engine.LoadPlugins(".");
        }

        public static void Shutdown()
        {

        }

        internal static ISoundEngine Engine
        {
            get { return _engine; }
        }

        private static ISoundEngine _engine;

        private static int _numSources;
        private static int[] _sources = new int[_maxSources];
    }

    internal class BarnFileFactory : IFileFactory
    {
        public System.IO.Stream openFile(string filename)
        {
            return FileSystem.Open(filename);
        }
    }
#else
    public class SoundManager
    {
        public static void Init()
        {
            // nothing
        }

        public static void Shutdown()
        {
            // nothing
        }
    }
#endif
}
