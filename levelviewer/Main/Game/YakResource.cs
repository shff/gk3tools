using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class YakResource : AnimationResource
    {
        private string _speaker;
        private AnimationResourceSection _gk3Section;
        private List<Sound.Sound> _sounds = new List<Gk3Main.Sound.Sound>();
        private Sound.PlayingSound? _playingSound;
        private int _timeAtPlayStart;
        private int _cueTime = -1;

        public YakResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            foreach (AnimationResourceSection section in Sections)
            {
                if (section.SectionName.Equals("SOUNDS", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (AnimationResourceSectionLine line in section.Lines)
                    {
                        string soundName = line.Params[0].StringValue;
                        _sounds.Add((Sound.Sound)Resource.ResourceManager.Load(soundName, "WAV"));
                    }
                }
                else if (section.SectionName.Equals("GK3", StringComparison.OrdinalIgnoreCase))
                {
                    // look for then DIALOGUECUE and SPEAKER
                    for (int i = 0; i < section.Lines.Count; i++)
                    {
                        string param1 = section.Lines[i].Params[0].StringValue;

                        if (param1 != null)
                        {
                            if (param1.Equals("DIALOGUECUE", StringComparison.OrdinalIgnoreCase))
                                _cueTime = section.Lines[i].FrameNum * MillisecondsPerFrame;
                            else if (param1.Equals("SPEAKER", StringComparison.OrdinalIgnoreCase))
                            {
                                string speaker = section.Lines[i].Params[1].StringValue;
                                if (speaker.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase) == false)
                                    _speaker = speaker;
                            }
                        }
                    }

                    _gk3Section = section;
                }
            }
        }

        public override void Dispose()
        {
            foreach (Sound.Sound sound in _sounds)
            {
                sound.Dispose();
            }

            _sounds = null;

            base.Dispose();
        }

        public void Play()
        {
            if (_sounds.Count > 0)
            {
                if (_playingSound.HasValue == true)
                {
                    Sound.SoundManager.Stop(_playingSound.Value);
                }

                _timeAtPlayStart = GameManager.TickCount;
                _playingSound = _sounds[0].Play2D(Sound.SoundTrackChannel.Dialog);
            }
        }

        public WaitHandle PlayAndWait()
        {
            if (_sounds.Count > 0)
            {
                if (_playingSound.HasValue == true)
                {
                    Sound.SoundManager.Stop(_playingSound.Value);
                }

                _timeAtPlayStart = GameManager.TickCount;
                _playingSound = _sounds[0].Play2D(Sound.SoundTrackChannel.Dialog, true);

                return _playingSound.Value.WaitHandle;
            }

            return null;
        }

        /// <summary>
        /// Gets whether the YAK is still playing
        /// </summary>
        public bool IsPlaying
        {
            get 
            {
                int duration = NumFrames * MillisecondsPerFrame;

                // the sound must be finished AND we must have covered all the frames
                return (_playingSound.HasValue == false || _playingSound.Value.Finished == false) && 
                    GameManager.TickCount <= _timeAtPlayStart + duration;
            }
        }

        /// <summary>
        /// Gets whether the YAK has progressed far enough that another YAK can begin.
        /// </summary>
        /// <remarks>
        /// It's possible for YAKs to overlap. Just because a YAK is finished doesn't
        /// mean it isn't still playing.
        /// </remarks>
        public bool IsFinished
        {
            get 
            {
                if (_cueTime >= 0)
                    return GameManager.TickCount > _timeAtPlayStart + _cueTime;
                
                return !IsPlaying;
            }
        }

        /// <summary>
        /// Gets the name of the Actor doing the speaking.
        /// Is null if voiceover.
        /// </summary>
        public string Speaker
        {
            get { return _speaker; }
        }

        public AnimationResourceSection Gk3Section
        {
            get { return _gk3Section; }
        }

        public int TimeAtPlayStart
        {
            get { return _timeAtPlayStart; }
        }
    }

    public class YakLoader : Resource.IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "YAK" }; }
        }

        public bool EmptyResourceIfNotFound { get { return false; } }

        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            YakResource resource = new YakResource(name, stream);

            stream.Close();

            return resource;
        }
    }
}
