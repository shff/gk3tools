using System;
using System.Collections.Generic;
using System.Text;
using Tao.OpenAl;

namespace Gk3Main.Sound.AudioEngine
{
    class AudioManager
    {
        private struct SourceAndOwner
        {
            public AudioSource Source;
            public object Owner;
        }

        private const int _maxSources = 16;
        private static SourceAndOwner[] _sources = new SourceAndOwner[_maxSources];
        private static IntPtr _device;
        private static IntPtr _context;

        public static void Init()
        {
            _device = Tao.OpenAl.Alc.alcOpenDevice(null);
            _context = Tao.OpenAl.Alc.alcCreateContext(_device, IntPtr.Zero);
            Alc.alcMakeContextCurrent(_context);
            Al.alDistanceModel(Al.AL_INVERSE_DISTANCE_CLAMPED);

            for (int i = 0; i < _maxSources; i++)
            {
                _sources[i].Source = new AudioSource();
                _sources[i].Owner = null;
            }
        }

        public static void Shutdown()
        {
            for (int i = 0; i < _maxSources; i++)
            {
                _sources[i].Source.Dispose();
                _sources[i].Owner = null;
            }

            Alc.alcDestroyContext(_context);
            Alc.alcCloseDevice(_device);
        }

        public static AudioSource GetFreeSource(object owner)
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                if (_sources[i].Source.IsAvailable())
                {
                    _sources[i].Owner = owner;
                    return _sources[i].Source;
                }
            }

            return null;
        }

        public static void ReleaseSource(AudioSource source)
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                if (_sources[i].Source == source)
                {
                    _sources[i].Owner = null;
                    return;
                }
            }
        }

        public static bool IsSourceOwner(AudioSource source, object owner)
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                if (_sources[i].Source == source)
                    return _sources[i].Owner == owner;
            }

            return false;
        }
    }
}
