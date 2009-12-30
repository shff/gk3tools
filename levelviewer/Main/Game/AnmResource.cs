using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class AnmWaitHandle : WaitHandle
    {
        private AnmResource _anm;

        public AnmWaitHandle(AnmResource anm)
        {
            _anm = anm;
        }

        public override bool Finished
        {
            get
            {
                return _anm.IsFinished;
            }
            set
            {
                base.Finished = value;
            }
        }
    }

    class AnmResource : AnimationResource
    {
        List<Graphics.ActResource> _acts = new List<Gk3Main.Graphics.ActResource>();
        AnimationResourceSection _actionSection;
        private int _timeElapsedSinceStart;

        public AnmResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            foreach (AnimationResourceSection section in Sections)
            {
                if (section.SectionName.Equals("ACTIONS", StringComparison.OrdinalIgnoreCase))
                    _actionSection = section;
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

        public WaitHandle PlayAndWait()
        {
            Play();

            return new AnmWaitHandle(this);
        }

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

        private void play(int timeSinceStart, int duration)
        {
            int startIndex, count;

            // play actions
            GetAllFramesSince(_actionSection, timeSinceStart, duration, out startIndex, out count);

            for (int i = startIndex; i < startIndex + count; i++)
            {
                string actName = _actionSection.Lines[i].Params[0].StringValue;

                // load the action
                Graphics.ActResource act = (Graphics.ActResource)Resource.ResourceManager.Load(string.Format("{0}.ACT", actName));
                _acts.Add(act);

                // TODO: play the action
                
            }
        }
    }

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
