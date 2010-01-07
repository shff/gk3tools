using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class AnmWaitHandle : WaitHandle
    {
        private MomResource _mom;

        public AnmWaitHandle(MomResource mom)
        {
            _mom = mom;
        }

        public override bool Finished
        {
            get
            {
                return _mom.IsFinished;
            }
            set
            {
                base.Finished = value;
            }
        }
    }

    [Obsolete("Use MomResource instead (it handles ANM files too)", true)]
    class AnmResource : AnimationResource
    {
        List<Graphics.ActResource> _acts = new List<Gk3Main.Graphics.ActResource>();
        AnimationResourceSection _actionSection;
        AnimationResourceSection _mtexturesSection;
        AnimationResourceSection _soundsSection;
        private int _timeElapsedSinceStart;

        public AnmResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            foreach (AnimationResourceSection section in Sections)
            {
                if (section.SectionName.Equals("ACTIONS", StringComparison.OrdinalIgnoreCase))
                    _actionSection = section;
                else if (section.SectionName.Equals("MTEXTURES", StringComparison.OrdinalIgnoreCase))
                    _mtexturesSection = section;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (Graphics.ActResource act in _acts)
            {
                Resource.ResourceManager.Unload(act);
            }
        }

        public void Play()
        {
            _timeElapsedSinceStart = 0;

            // play any frame 0 stuff
            play(0, 0);
        }

        /*public WaitHandle PlayAndWait()
        {
            Play();

            return new AnmWaitHandle(this);
        }*/

        public void Step()
        {
            int elapsedTime = Game.GameManager.ElapsedTickCount;
            _timeElapsedSinceStart += elapsedTime;

            play(_timeElapsedSinceStart, elapsedTime);
        }

        public bool IsFinished
        {
            get { return _timeElapsedSinceStart > NumFrames * AnimationResource.MillisecondsPerFrame; }
        }

        public int TimeElapsedSinceStart
        {
            get { return _timeElapsedSinceStart; }
        }

        public AnimationResourceSection ActionsSection
        {
            get { return _actionSection; }
        }

        private void play(int timeSinceStart, int duration)
        {
            int startIndex, count;

            // play actions
            if (_actionSection != null)
            {
                GetAllFramesSince(_actionSection, timeSinceStart, duration, AnimationResource.MillisecondsPerFrame,
                    out startIndex, out count);

                for (int i = startIndex; i < startIndex + count; i++)
                {
                    string actName = _actionSection.Lines[i].Params[0].StringValue;

                    // load the action
                    Graphics.ActResource act = (Graphics.ActResource)Resource.ResourceManager.Load(string.Format("{0}.ACT", actName));
                    _acts.Add(act);

                    // TODO: play the action
                }
            }

            if (_mtexturesSection != null)
            {
                // TODO
            }

            if (_soundsSection != null)
            {

            }
        }
    }

    [Obsolete("Use MomResource instead (it handles ANM files too)", true)]
    public class AnmLoader : Resource.IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "ANM" }; }
        }

        public bool EmptyResourceIfNotFound { get { return false; } }

        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            AnmResource resource = new AnmResource(name, stream);

            stream.Close();

            return resource;
        }
    }
}
