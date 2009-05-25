using System;
using System.Collections.Generic;
using System.Text;

#if !SOUND_DISABLED
using IrrKlang;
#endif

namespace Gk3Main.Sound
{
    public enum SoundTrackChannel
    {
        Music
    }

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

        public static void PlaySound2DToChannel(Sound sound, SoundTrackChannel channel)
        {
            StopChannel(channel);

            _channelSounds[channel] = Engine.Play2D(sound.Source, false, false, false);
        }

        public static void StopChannel(SoundTrackChannel channel)
        {
            ISound sound;
            if (_channelSounds.TryGetValue(channel, out sound))
            {
                sound.Stop();
                sound.Dispose();

                _channelSounds[channel] = null;
            }
        }

        internal static ISoundEngine Engine
        {
            get { return _engine; }
        }

        private static ISoundEngine _engine;

        private static int _numSources;
        private static int[] _sources = new int[_maxSources];

        private static Dictionary<SoundTrackChannel, ISound> _channelSounds 
            = new Dictionary<SoundTrackChannel, ISound>();
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
