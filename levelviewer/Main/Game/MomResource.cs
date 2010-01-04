using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class MomResource : AnimationResource
    {
        private AnimationResourceSection _actionSection;
        private AnimationResourceSection _soundSection;
        private AnimationResourceSection _gk3Section;
        private List<Sound.Sound> _sounds = new List<Gk3Main.Sound.Sound>();
        private int _timeElapsedSinceStart;

        public MomResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            foreach (AnimationResourceSection section in Sections)
            {
                if (section.SectionName.Equals("ACTIONS", StringComparison.OrdinalIgnoreCase))
                    _actionSection = section;
                else if (section.SectionName.Equals("SOUNDS", StringComparison.OrdinalIgnoreCase))
                {
                    _soundSection = section;

                    // preload the sounds
                    foreach (AnimationResourceSectionLine line in section.Lines)
                    {
                        string soundName = line.Params[0].StringValue;
                        _sounds.Add(new Sound.Sound(soundName, FileSystem.Open(soundName)));
                    }
                }
                else if (section.SectionName.Equals("GK3", StringComparison.OrdinalIgnoreCase))
                    _gk3Section = section;
            }
        }

        public void Play()
        {
            _timeElapsedSinceStart = 0;

            // play any frame 0 stuff
            play(0, 0);
        }

        public void Step()
        {
            int elapsedTime = Game.GameManager.ElapsedTickCount;
            _timeElapsedSinceStart += elapsedTime;

            play(_timeElapsedSinceStart, elapsedTime);
        }

        public bool IsFinished
        {
            get { return _timeElapsedSinceStart > NumFrames * AnimationResource.MillisecondsPerFrame;  }
        }

        private void play(int timeSinceStart, int duration)
        {
            int startIndex, count;

            // play sounds
            GetAllFramesSince(_soundSection, timeSinceStart, duration, out startIndex, out count);

            for (int i = startIndex; i < startIndex + count; i++)
            {
                // the indices in the little sound list *should* match "i"
                _sounds[i].Play2D(Sound.SoundTrackChannel.SFX);
            }

            // play the dialog
            GetAllFramesSince(_gk3Section, timeSinceStart, duration, out startIndex, out count);

            for (int i = startIndex; i < startIndex + count; i++)
            {
                string command = _gk3Section.Lines[i].Params[0].StringValue;

                if (command.Equals("DIALOGUE", StringComparison.OrdinalIgnoreCase))
                {
                    string yak = _gk3Section.Lines[i].Params[1].StringValue;
                    DialogManager.PlayDialogue(yak, 1, yak.StartsWith("E", StringComparison.OrdinalIgnoreCase), false);
                }
            }
        }
    }

    public class MomLoader : Resource.IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "MOM" }; }
        }

        public bool EmptyResourceIfNotFound { get { return false; } }

        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            MomResource resource = new MomResource(name, stream);

            stream.Close();

            return resource;
        }
    }
}
