using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Audio.OpenAL;

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
            AL.GenSources(1, out _source);

            AL.Source(_source, ALSourcef.ReferenceDistance, 100.0f);
        }

        public void Dispose()
        {
            AL.DeleteSources(1, ref _source);
            _source = 0;
        }

        public bool IsAvailable()
        {
            int state;
            AL.GetSource(_source, ALGetSourcei.SourceState, out state);
            return state == (int)ALSourceState.Stopped || state == (int)ALSourceState.Initial;
        }

        public void SetBuffer(int buffer)
        {
            _buffer = buffer;
        }

        public void Play(float volume)
        {
            AL.Source(_source, ALSourceb.SourceRelative, _positionRelative);
            AL.Source(_source, ALSource3f.Position, _position.X, _position.Y, _position.Z);
            AL.Source(_source, ALSourceb.Looping, _isLooping);
            AL.Source(_source, ALSourcei.Buffer, _buffer);
            AL.Source(_source, ALSourcef.Gain, volume);
            //Al.alSourcef(_source, Al.AL_REFERENCE_DISTANCE, AudioManager::m_distanceScale);

            AL.SourcePlay(_source);
        }


        public void Pause()
        {
            AL.SourcePause(_source);
        }

        public void Stop()
        {
            AL.SourceStop(_source);
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

                AL.Source(_source, ALSourceb.Looping, value);
            }
        }

        public void SetPosition(bool relative, Math.Vector3 position)
        {
            _positionRelative = relative;
            _position = position;

            AL.Source(_source, ALSourceb.SourceRelative, relative);
            AL.Source(_source, ALSource3f.Position, position.X, position.Y, position.Z);
        }

        public SoundState State
        {
            get
            {
                int val;
                AL.GetSource(_source, ALGetSourcei.SourceState, out val);

                if (val == (int)ALSourceState.Playing)
                    return SoundState.Playing;
                if (val == (int)ALSourceState.Paused)
                    return SoundState.Paused;

                return SoundState.Stopped;
            }
        }
    }
}
