using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sound
{
    public class SoundWaitHandle : WaitHandle
    {
        private IrrKlang.ISound _sound;

        public SoundWaitHandle(IrrKlang.ISound sound)
        {
            _sound = sound;
        }

        public override bool Finished
        {
            get
            {
                return _sound.Finished;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }

    public struct PlayingSound
    {
        private SoundWaitHandle _waitHandle;

#if !SOUND_DISABLED

        internal IrrKlang.ISound _PlayingSound;

        public PlayingSound(IrrKlang.ISound sound) : this(sound, false)
        {
        }

        public PlayingSound(IrrKlang.ISound sound, bool wait)
        {
            if (sound == null)
                throw new ArgumentNullException("sound");

            _PlayingSound = sound;

            if (wait)
                _waitHandle = new SoundWaitHandle(sound);
            else
                _waitHandle = null;
        }

        public bool Finished
        {
            get { return _PlayingSound.Finished; }
        }
#else
        public bool Finished
        {
            get { return true; }
        }
#endif

        public SoundWaitHandle WaitHandle
        {
            get { return _waitHandle; }
        }
    }


#if !SOUND_DISABLED
    public class Sound : Resource.Resource
    {
        public Sound(string name, System.IO.Stream stream)
            : base(name, true)
        {
            _sound = SoundManager.AddSoundSourceFromFile(name);
        }

        public override void Dispose()
        {
            SoundManager.RemoveSoundSource(_sound.Name);
            _sound = null;
        }

        public PlayingSound Play2D(SoundTrackChannel channel)
        {
            return Play2D(channel, false);
        }

        public PlayingSound Play2D(SoundTrackChannel channel, bool wait)
        {
            return SoundManager.PlaySound2DToChannel(this, channel, false, wait);
        }

        public PlayingSound Play3D(SoundTrackChannel channel, float x, float y, float z)
        {
            return SoundManager.PlaySound3DToChannel(this, x, y, z, channel, false);
        }

        internal IrrKlang.ISoundSource Source
        {
            get { return _sound; }
        }

        private IrrKlang.ISoundSource _sound;
       
    }

    public class SoundLoader : Resource.IResourceLoader
    {
        public string[]  SupportedExtensions
        {
	        get { return new string[] { "WAV" }; }
        }

        public bool EmptyResourceIfNotFound
        {
            get { return false; }
        }

        public Resource.Resource Load(string filename)
        {
            System.IO.Stream stream = FileSystem.Open(filename);

            Sound sound = new Sound(filename, stream);

            stream.Close();

            return sound;
        }

    }
#else
    public class Sound : Resource.Resource
    {
        public Sound(string name, System.IO.Stream stream)
            : base(name, true)
        {
            // nothing
        }

        public override void Dispose()
        {
            // nothing
        }

        public void Play2D()
        {
            // nothing
        }

        public PlayingSound Play2D(SoundTrackChannel channel)
        {
            return new PlayingSound();
        }

        public PlayingSound Play3D(SoundTrackChannel channel, float x, float y, float z)
        {
            return new PlayingSound();
        }

    }

    public class SoundLoader : Resource.IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "WAV" }; }
        }

        public bool EmptyResourceIfNotFound
        {
            get { return false; }
        }

        public Resource.Resource Load(string filename)
        {
            return new Sound(filename, null);
        }

    }
#endif
}
