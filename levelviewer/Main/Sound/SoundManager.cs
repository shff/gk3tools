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
        private static List<PlayingSound> _playingSounds = new List<PlayingSound>();

        public static void Init()
        {
            AudioEngine.AudioManager.Init();
        }

        public static void Shutdown()
        {
            AudioEngine.AudioManager.Shutdown();
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

        public static PlayingSound PlaySound2DToChannel(AudioEngine.SoundEffect sound, SoundTrackChannel channel, WaitHandle waitHandle = null)
        {
            PlayingSound s = new PlayingSound(sound.CreateInstance(), channel, waitHandle);
            s.Instance.SetPosition(true, Math.Vector3.Zero);
            s.Play();
            _playingSounds.Add(s);

            return s;
        }

        public static PlayingSound PlaySound3DToChannel(AudioEngine.SoundEffect sound, float x, float y, float z, SoundTrackChannel channel, WaitHandle waitHandle = null)
        {
            PlayingSound s = new PlayingSound(sound.CreateInstance(), channel, waitHandle);

            s.Instance.SetPosition(false, new Math.Vector3(x, y, z));
            s.Play();
            _playingSounds.Add(s);

            return s;

            /*AudioSource source = getFreeSource(null);
            if (source != null)
            {
                source.IsLooping

                Al.alSourcei(source, Al.AL_BUFFER, sound.Buffer);
                Al.alSource3f(source, Al.AL_POSITION, x, y, z);
                Al.alSourcei(source, Al.AL_SOURCE_RELATIVE, Al.AL_FALSE);
                Al.alSourcef(source, Al.AL_MAX_DISTANCE, sound.DefaultMaxDistance);
                Al.alSourcef(source, Al.AL_REFERENCE_DISTANCE, sound.DefaultMinDistance);
                Al.alSourcePlay(source);

                _channelSounds[channel].Add(source);// BUG: this should be adding this sound to a collection!

                return new PlayingSound(source, false);
            }

            return new PlayingSound();*/
        }

        public static void StopChannel(SoundTrackChannel channel)
        {
            for (int i = 0; i < _playingSounds.Count; i++)
            {
                if (_playingSounds[i].Channel == channel)
                {
                    _playingSounds[i].Stop();
                }
            }
        }

        private static float[] _listenerOrientation = new float[6];
        public static void Update(Graphics.Camera camera)
        {
            for (int i = 0; i < _playingSounds.Count; )
            {
                if (_playingSounds[i].Instance.State == AudioEngine.SoundState.Stopped)
                {
                    // the sound is stopped, so update the wait handle
                    if (_playingSounds[i].Wait != null)
                        _playingSounds[i].Wait.Finished = true;

                    // remove sounds ready to die
                    if (_playingSounds[i].Released)
                    {
                        _playingSounds[i].Dispose();
                        _playingSounds.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }

            if (camera != null)
            {
                Math.Vector3 position = camera.Position;
                Math.Vector3 forward = camera.Orientation * -Math.Vector3.Forward;
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
