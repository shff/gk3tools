using System;
using System.Collections.Generic;
using System.Text;

#if !SOUND_DISABLED
using Tao.OpenAl;
#endif

namespace Gk3Main.Sound
{
    public enum SoundTrackChannel
    {
        SFX,
        Ambient,
        Music,
        Dialog,
        UI
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
        private static IntPtr _device;
        private static IntPtr _context;
        private static int[] _sources = new int[_maxSources];

        public static void Init()
        {
            _device = Tao.OpenAl.Alc.alcOpenDevice(null);
            _context = Tao.OpenAl.Alc.alcCreateContext(_device, IntPtr.Zero);
            Alc.alcMakeContextCurrent(_context);
            Al.alDistanceModel(Al.AL_INVERSE_DISTANCE_CLAMPED);

            // TODO: set the main volume

            _channelSounds.Add(SoundTrackChannel.Ambient, new List<int>());
            _channelSounds.Add(SoundTrackChannel.Dialog, new List<int>());
            _channelSounds.Add(SoundTrackChannel.Music, new List<int>());
            _channelSounds.Add(SoundTrackChannel.SFX, new List<int>());
            _channelSounds.Add(SoundTrackChannel.UI, new List<int>());

            Al.alGenSources(_maxSources, _sources);
        }

        public static void Shutdown()
        {
            Al.alDeleteSources(_maxSources, _sources);
            Alc.alcDestroyContext(_context);
            Alc.alcCloseDevice(_device);
        }

        public static int CreateBufferFromFile(string file)
        {
            using (System.IO.Stream stream = FileSystem.Open(file))
            {
                WavFile wav = new WavFile(stream);

                int buffer;
                Al.alGenBuffers(1, out buffer);

                int format;
                if (wav.Channels == 1)
                {
                    if (wav.SampleSize == 8)
                        format = Al.AL_FORMAT_MONO8;
                    else
                        format = Al.AL_FORMAT_MONO16;
                }
                else
                {
                    if (wav.SampleSize == 8)
                        format = Al.AL_FORMAT_STEREO8;
                    else
                        format = Al.AL_FORMAT_STEREO16;
                }

                Al.alBufferData(buffer, format, wav.PcmData, wav.Length, wav.SampleRate);

                return buffer;
            }
        }

        public static PlayingSound PlaySound2DToChannel(Sound sound, SoundTrackChannel channel, bool clearChannel)
        {
            return PlaySound2DToChannel(sound, channel, clearChannel, false);
        }

        public static PlayingSound PlaySound2DToChannel(Sound sound, SoundTrackChannel channel, bool clearChannel, bool wait)
        {
            if (clearChannel)
                StopChannel(channel);

            int source = getFreeSource();
            if (source >= 0)
            {
                Al.alSourcei(source, Al.AL_BUFFER, sound.Buffer);
                Al.alSource3f(source, Al.AL_POSITION, 0, 0, 0);
                Al.alSourcei(source, Al.AL_SOURCE_RELATIVE, Al.AL_TRUE);
                Al.alSourcef(source, Al.AL_MAX_DISTANCE, sound.DefaultMaxDistance);
                Al.alSourcef(source, Al.AL_REFERENCE_DISTANCE, sound.DefaultMinDistance);
                Al.alSourcePlay(source);

                _channelSounds[channel].Add(source);

                return new PlayingSound(source, wait);
            }

            return new PlayingSound();
        }

        public static PlayingSound PlaySound3DToChannel(Sound sound, float x, float y, float z, SoundTrackChannel channel, bool clearChannel)
        {
            if (clearChannel)
                StopChannel(channel);

            int source = getFreeSource();
            if (source >= 0)
            {
                Al.alSourcei(source, Al.AL_BUFFER, sound.Buffer);
                Al.alSource3f(source, Al.AL_POSITION, x, y, z);
                Al.alSourcei(source, Al.AL_SOURCE_RELATIVE, Al.AL_FALSE);
                Al.alSourcef(source, Al.AL_MAX_DISTANCE, sound.DefaultMaxDistance);
                Al.alSourcef(source, Al.AL_REFERENCE_DISTANCE, sound.DefaultMinDistance);
                Al.alSourcePlay(source);

                _channelSounds[channel].Add(source);// BUG: this should be adding this sound to a collection!

                return new PlayingSound(source, false);
            }

            return new PlayingSound();
        }

        public static void StopChannel(SoundTrackChannel channel)
        {
            List<int> sounds;
            if (_channelSounds.TryGetValue(channel, out sounds))
            {
                foreach (int sound in sounds)
                {
                    Al.alSourceStop(sound);
                }

                _channelSounds[channel].Clear();
            }
        }

        public static void Stop(PlayingSound sound)
        {
            Al.alSourceStop(sound._Source);

            // BUG: remove the source from the channel
        }

        private static float[] _listenerOrientation = new float[6];
        public static void UpdateListener(Graphics.Camera camera)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            Math.Vector3 position = camera.Position;
            Math.Vector3 forward =  camera.Orientation * -Math.Vector3.Forward;
            Math.Vector3 up = camera.Orientation * Math.Vector3.Up;

            Al.alListener3f(Al.AL_POSITION, position.X, position.Y, position.Z);

            _listenerOrientation[0] = forward.X;
            _listenerOrientation[1] = forward.Y;
            _listenerOrientation[2] = forward.Z;
            _listenerOrientation[3] = up.X;
            _listenerOrientation[4] = up.Y;
            _listenerOrientation[5] = up.Z;
            Al.alListenerfv(Al.AL_ORIENTATION, _listenerOrientation);
        }

        private static int getFreeSource()
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                int state;
                Al.alGetSourcei(_sources[i], Al.AL_SOURCE_STATE, out state);
                if (state == Al.AL_STOPPED || state == Al.AL_INITIAL)
                {
                    return _sources[i];
                }
            }

            return -1;
        }

        private static Dictionary<SoundTrackChannel, List<int>> _channelSounds 
            = new Dictionary<SoundTrackChannel, List<int>>();
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

        public static void Stop(PlayingSound sound)
        {
            // nothing
        }

        public static void UpdateListener(Graphics.Camera camera)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            // nothing
        }
    }
#endif
}
