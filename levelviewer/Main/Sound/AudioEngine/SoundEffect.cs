using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sound.AudioEngine
{
    public class SoundEffect : Resource.Resource
    {
        private bool _disposed = false;
        private int _buffer;
        private float _minDistance = 1.0f;
        private float _maxDistance = float.MaxValue;
        
        public SoundEffect(string name, System.IO.Stream stream)
            : base(name, true)
        {
            _buffer = SoundManager.CreateBufferFromFile(name);
        }

        public override void Dispose()
        {
            _disposed = true;
            OpenTK.Audio.OpenAL.AL.DeleteBuffers(1, ref _buffer);
            _buffer = 0;
        }

        public bool Play()
        {
            return Play(1.0f);
        }

        public bool Play(float volume)
        {
            AudioSource source = AudioManager.GetFreeSource(null);
            if (source == null) return false;

            source.IsLooping = false;
            source.SetBuffer(_buffer);
            source.Play(volume);

            return true;
        }

        public SoundEffectInstance CreateInstance()
        {
            return new SoundEffectInstance(this);
        }

        internal int Buffer { get { return _buffer; } }
    }
}
