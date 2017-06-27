using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sound
{
    public class SoundWaitHandle : WaitHandle
    {
#if !SOUND_DISABLED
        private int _source;

        public SoundWaitHandle(int source)
        {
            _source = source;
        }

        public override bool Finished
        {
            get
            {
                int state;
                OpenTK.Audio.OpenAL.AL.GetSource(_source, OpenTK.Audio.OpenAL.ALGetSourcei.SourceState, out state);

                return state == (int)OpenTK.Audio.OpenAL.ALSourceState.Stopped || state == (int)OpenTK.Audio.OpenAL.ALSourceState.Initial;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
#else
        public override bool Finished
        {
            get { return true; }
            set { }
        }
#endif
    }

    public class PlayingSound : IDisposable
    {
        private AudioEngine.SoundEffectInstance _instance;
        private WaitHandle _waitHandle;
        private SoundTrackChannel _channel;
        private bool _released;

        public PlayingSound(AudioEngine.SoundEffectInstance instance, SoundTrackChannel channel, WaitHandle waitHandle)
        {
            _instance = instance;
            _channel = channel;
            _waitHandle = waitHandle;
            _released = false;
        }

        public void Dispose()
        {
            _instance.Dispose();
            _instance = null;
        }

        public void Release()
        {
            _released = true;
        }

        public void Play()
        {
            _instance.Play();
        }

        public void Stop()
        {
            _instance.Stop();

            if (_waitHandle != null)
                _waitHandle.Finished = true;
        }

        internal bool Released { get { return _released; } }
        internal AudioEngine.SoundEffectInstance Instance { get { return _instance; } }
        internal WaitHandle Wait { get { return _waitHandle; } }
        public SoundTrackChannel Channel { get { return _channel; } }
    }


#if !SOUND_DISABLED

    /*
    [Obsolete]
    public class Sound : Resource.Resource
    {
        private bool _disposed = false;
        private int _buffer;
        private float _defaultMinDistance = 1.0f;
        private float _defaultMaxDistance = float.MaxValue;
        private float _defaultVolume;

        public Sound(string name, System.IO.Stream stream)
            : base(name, true)
        {
            _buffer = SoundManager.CreateBufferFromFile(name);
            //_sound = SoundManager.AddSoundSourceFromFile(name);

           // if (_sound == null)
            //    throw new Exception("Sound source returned from Irrklang was null");
        }

        public override void Dispose()
        {
            _disposed = true;
            Tao.OpenAl.Al.alDeleteBuffers(1, ref _buffer);
            _buffer = 0;
        }

        public PlayingSound Play2D(SoundTrackChannel channel)
        {
            return Play2D(channel, false);
        }

        public PlayingSound Play2D(SoundTrackChannel channel, bool wait)
        {
            if (_disposed) throw new ObjectDisposedException("Sound");

            return SoundManager.PlaySound2DToChannel(this, channel, false, wait);
        }

        public PlayingSound Play3D(SoundTrackChannel channel, float x, float y, float z)
        {
            if (_disposed) throw new ObjectDisposedException("Sound");

            return SoundManager.PlaySound3DToChannel(this, x, y, z, channel, false);
        }

        internal float DefaultMinDistance
        {
            get { return _defaultMinDistance; }
            set { _defaultMinDistance = value; }
        }

        internal float DefaultMaxDistance
        {
            get { return _defaultMaxDistance; }
            set { _defaultMaxDistance = value; }
        }

        internal float DefaultVolume
        {
            get { return _defaultVolume; }
            set { _defaultVolume = value; }
        }

        internal int Buffer
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException("Sound");

                return _buffer;
            }
        }
    }*/

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

        public Resource.Resource Load(string filename, Resource.ResourceManager content)
        {
            if (filename.IndexOf('.') < 0)
                filename += ".WAV";

            System.IO.Stream stream = FileSystem.Open(filename);

            AudioEngine.SoundEffect sound = new AudioEngine.SoundEffect(filename, stream);

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

        public PlayingSound Play2D(SoundTrackChannel channel, bool wait)
        {
            return new PlayingSound(wait);
        }

        public PlayingSound Play3D(SoundTrackChannel channel, float x, float y, float z)
        {
            return new PlayingSound();
        }

        internal float DefaultMinDistance
        {
            get { return 0; }
            set { }
        }

        internal float DefaultMaxDistance
        {
            get { return 0; }
            set { }
        }

        internal float DefaultVolume
        {
            get { return 0; }
            set { }
        }

        internal IntPtr Source
        {
            get
            {
                return IntPtr.Zero;
            }
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
