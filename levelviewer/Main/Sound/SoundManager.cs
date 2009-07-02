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
        SFX,
        Ambient,
        Music,
        Dialog
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

            _channelSounds.Add(SoundTrackChannel.Ambient, new List<ISound>());
            _channelSounds.Add(SoundTrackChannel.Dialog, new List<ISound>());
            _channelSounds.Add(SoundTrackChannel.Music, new List<ISound>());
            _channelSounds.Add(SoundTrackChannel.SFX, new List<ISound>());
        }

        public static void Shutdown()
        {

        }

        public static PlayingSound PlaySound2DToChannel(Sound sound, SoundTrackChannel channel, bool clearChannel)
        {
            if (clearChannel)
                StopChannel(channel);

            ISound isound = Engine.Play2D(sound.Source, false, false, false);
            _channelSounds[channel].Add(isound);

            return new PlayingSound(isound);
        }

        public static PlayingSound PlaySound3DToChannel(Sound sound, float x, float y, float z, SoundTrackChannel channel, bool clearChannel)
        {
            if (clearChannel)
                StopChannel(channel);

            ISound isound = Engine.Play3D(sound.Source, x, y, z, false, false, false);
            _channelSounds[channel].Add(isound);// BUG: this should be adding this sound to a collection!

            return new PlayingSound(isound);
        }

        public static void StopChannel(SoundTrackChannel channel)
        {
            List<ISound> sounds;
            if (_channelSounds.TryGetValue(channel, out sounds))
            {
                foreach (ISound sound in sounds)
                {
                    sound.Stop();
                    sound.Dispose();
                }

                _channelSounds[channel].Clear();
            }
        }

        internal static ISoundEngine Engine
        {
            get { return _engine; }
        }

        private static ISoundEngine _engine;

        private static int _numSources;
        private static int[] _sources = new int[_maxSources];

        private static Dictionary<SoundTrackChannel, List<ISound>> _channelSounds 
            = new Dictionary<SoundTrackChannel, List<ISound>>();
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

        public static void StopChannel(SoundTrackChannel channel)
        {
            // nothing
        }
    }
#endif
}
