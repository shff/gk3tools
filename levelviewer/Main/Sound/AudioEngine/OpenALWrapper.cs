using System;
using System.Collections.Generic;
using System.Text;
using Tao.OpenAl;

namespace Gk3Main.Sound.AudioEngine
{
    public enum SoundState
    {
        Paused,
        Playing,
        Stopped
    }

    class AudioSource : IDisposable
    {
        private int _source;
        private int _buffer;
        private bool _isLooping;
        private Math.Vector3 _position;
        private bool _positionRelative;

        public AudioSource()
        {
            Tao.OpenAl.Al.alGenSources(1, out _source);

            Al.alSourcef(_source, Al.AL_REFERENCE_DISTANCE, 100.0f);
        }

        public void Dispose()
        {
            Al.alDeleteSources(1, ref _source);
            _source = 0;
        }

        public bool IsAvailable()
        {
            int state;
            Tao.OpenAl.Al.alGetSourcei(_source, Tao.OpenAl.Al.AL_SOURCE_STATE, out state);
            return state == Tao.OpenAl.Al.AL_STOPPED || state == Tao.OpenAl.Al.AL_INITIAL;
        }

        public void SetBuffer(int buffer)
        {
            _buffer = buffer;
        }

        public void Play(float volume)
        {
            Al.alSourcei(_source, Al.AL_SOURCE_RELATIVE, _positionRelative ? 1 : 0);
            Al.alSource3f(_source, Al.AL_POSITION, _position.X, _position.Y, _position.Z);
            Al.alSourcei(_source, Al.AL_LOOPING, _isLooping ? Al.AL_TRUE : Al.AL_FALSE);
            Al.alSourcei(_source, Al.AL_BUFFER, _buffer);
            Al.alSourcef(_source, Al.AL_GAIN, volume);
            //Al.alSourcef(_source, Al.AL_REFERENCE_DISTANCE, AudioManager::m_distanceScale);
               
            Al.alSourcePlay(_source);
        }


        public void Pause()
        {
            Al.alSourcePause(_source);
        }

        public void Stop()
        {
            Al.alSourceStop(_source);
        }

        public bool IsLooping
        {
            get
            {
                return _isLooping;
            }
            set
            {
                _isLooping = value;

                Al.alSourcei(_source, Al.AL_LOOPING, value ? 1 : 0);
            }
        }

        public void SetPosition(bool relative, Math.Vector3 position)
        {
            _positionRelative = relative;
            _position = position;

            Al.alSourcei(_source, Al.AL_SOURCE_RELATIVE, relative ? 1 : 0);
            Al.alSource3f(_source, Al.AL_POSITION, position.X, position.Y, position.Z);
        }

        public SoundState State
        {
            get
            {
                int val;
                Al.alGetSourcei(_source, Al.AL_SOURCE_STATE, out val);

                if (val == Al.AL_PLAYING)
                    return SoundState.Playing;
                if (val == Al.AL_PAUSED)
                    return SoundState.Paused;

                return SoundState.Stopped;
            }
        }
    }
}
