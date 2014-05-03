using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sound.AudioEngine
{
    public class SoundEffectInstance : IDisposable
    {
        private AudioSource _source;
        private int _buffer;
        private bool _looped;
        private float _gain;
        private Math.Vector3 _position;
        private bool _positionRelative;

        internal SoundEffectInstance(SoundEffect parent)
        {
            _buffer = parent.Buffer;
            _gain = 1.0f;
        }

        public void Dispose()
        {
            if (isSourceValid())
                AudioManager.ReleaseSource(_source);
        }

        public void Play()
        {
            if (isSourceValid() == false)
            {
                _source = AudioManager.GetFreeSource(this);
                if (_source == null) return;

                _source.SetBuffer(_buffer);
            }

            _source.SetPosition(_positionRelative, _position);
            _source.Play(_gain);
        }

        public void Stop()
        {
            if (isSourceValid() == false) return;

            _source.Stop();

            AudioManager.ReleaseSource(_source);
            _source = null;
        }

        public void Pause()
        {
            if (isSourceValid() == false) return;

            _source.Pause();
        }

        public void SetPosition(bool relative, Math.Vector3 position)
        {
            _position = position;
            _positionRelative = relative;

            if (isSourceValid())
                _source.SetPosition(relative, position);
        }

        public SoundState State
        {
            get
            {
                if (isSourceValid())
                {
                    return _source.State;
                }

                return SoundState.Stopped;
            }
        }

        private bool isSourceValid()
        {
            return _source != null && AudioManager.IsSourceOwner(_source, this);
        }
    }
}
