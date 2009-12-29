using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class YakResource : AnimationResource
    {
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
                        _sounds.Add(new Sound.Sound(soundName, FileSystem.Open(soundName)));
                    }
                }
                else if (section.SectionName.Equals("GK3", StringComparison.OrdinalIgnoreCase))
                {
                    // look for then DIALOGUECUE
                    for (int i = 0; i < section.Lines.Count; i++)
                    {
                        if (section.Lines[i].Params[0].StringValue != null &&
                            section.Lines[i].Params[0].StringValue.Equals("DIALOGUECUE", StringComparison.OrdinalIgnoreCase))
                            _cueTime = section.Lines[i].FrameNum * MillisecondsPerFrame;
                    }
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
                return _playingSound.HasValue == true &&
                    _playingSound.Value.Finished == false; 
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
