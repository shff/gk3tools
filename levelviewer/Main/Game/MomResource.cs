using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class MomResource : AnimationResource
    {
        private struct MomAct
        {
            public Graphics.ActResource Act;
            public Graphics.ModelResource Model;
            public bool IsAbsolute;
        }

        private AnimationResourceSection _actionSection;
        private AnimationResourceSection _modelVisibilitySection;
        private AnimationResourceSection _soundSection;
        private AnimationResourceSection _gk3Section;
        private List<Sound.Sound> _sounds = new List<Gk3Main.Sound.Sound>();
        private List<MomAct?> _acts = new List<MomAct?>();
        private int _timeElapsedSinceStart;
        private bool _playingFirstFrame;

        public MomResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            foreach (AnimationResourceSection section in Sections)
            {
                if (section.SectionName.Equals("ACTIONS", StringComparison.OrdinalIgnoreCase))
                    _actionSection = section;
                else if (section.SectionName.Equals("MVISIBILITY", StringComparison.OrdinalIgnoreCase))
                    _modelVisibilitySection = section;
                else if (section.SectionName.Equals("SOUNDS", StringComparison.OrdinalIgnoreCase))
                {
                    _soundSection = section;

                    // preload the sounds
                    foreach (AnimationResourceSectionLine line in section.Lines)
                    {
                        string soundName = line.Params[0].StringValue;
                        Sound.Sound sound = (Sound.Sound)Resource.ResourceManager.Load(soundName);
                        _sounds.Add(sound);
                    }
                }
                else if (section.SectionName.Equals("GK3", StringComparison.OrdinalIgnoreCase))
                    _gk3Section = section;
            }
        }

        public void Play()
        {
            Play(false);
        }

        public void Play(bool firstFrameOnly)
        {
            Logger.WriteInfo("Playing animation {0}", LoggerStream.Animation, this.Name);

            if (_playingFirstFrame && !firstFrameOnly)
                Step();
            else
            {
                _timeElapsedSinceStart = 0;
                play(0, 0);
            }
            
            _playingFirstFrame = firstFrameOnly;
        }

        public WaitHandle PlayAndWait()
        {
            Play();

            return new AnmWaitHandle(this);
        }

        public void Step()
        {
            // if we were only supposed to play the first frame
            // then don't do anything
            if (_playingFirstFrame) return;

            int elapsedTime = Game.GameManager.ElapsedTickCount;
            _timeElapsedSinceStart += elapsedTime;
            play(_timeElapsedSinceStart, elapsedTime);
        }

        public bool IsFinished
        {
            get { return _timeElapsedSinceStart > NumFrames * MillisecondsPerFrame;  }
        }

        private void play(int timeSinceStart, int duration)
        {
            int startIndex, count;

            // play model visibility
            if (_modelVisibilitySection != null)
            {
                GetAllFramesSince(_modelVisibilitySection, timeSinceStart, duration, MillisecondsPerFrame,
                    out startIndex, out count);

                for (int i = startIndex; i < startIndex + count; i++)
                {
                    string model = _modelVisibilitySection.Lines[i].Params[0].StringValue;
                    string onoff = _modelVisibilitySection.Lines[i].Params[1].StringValue;
                    bool visible = onoff.Equals("on", StringComparison.OrdinalIgnoreCase);

                    SceneManager.SetSceneModelVisibility(model, visible);
                }
            }

            // play sounds
            if (_soundSection != null)
            {
                GetAllFramesSince(_soundSection, timeSinceStart, duration, MillisecondsPerFrame,
                    out startIndex, out count);

                for (int i = startIndex; i < startIndex + count; i++)
                {
                    // the indices in the little sound list *should* match "i"
                    _sounds[i].Play2D(Sound.SoundTrackChannel.SFX);
                }
            }

            // play the dialog
            if (_gk3Section != null)
            {
                GetAllFramesSince(_gk3Section, timeSinceStart, duration, MillisecondsPerFrame,
                    out startIndex, out count);

                for (int i = startIndex; i < startIndex + count; i++)
                {
                    string command = _gk3Section.Lines[i].Params[0].StringValue;

                    if (command.Equals("DIALOGUE", StringComparison.OrdinalIgnoreCase))
                    {
                        string yak = _gk3Section.Lines[i].Params[1].StringValue;
                        DialogManager.PlayDialogue(yak, 1, yak.StartsWith("E", StringComparison.OrdinalIgnoreCase), false);
                    }
                    else if (command.Equals("LIPSYNCH", StringComparison.OrdinalIgnoreCase))
                    {
                        string param2 = _gk3Section.Lines[i].Params[1].StringValue;

                        Actor actor = SceneManager.GetActor(param2);
                        if (actor == null)
                            continue; // couldn't find this actor for some reason, so give up

                        string param3 = _gk3Section.Lines[i].Params[2].StringValue;
                        actor.SetMouth(param3);
                    }
                }
            }

            // add any new ACT files
            if (_actionSection != null)
            {
                GetAllFramesSince(_actionSection, timeSinceStart, duration, MillisecondsPerFrame,
                    out startIndex, out count);

                for (int i = startIndex; i < startIndex + count; i++)
                {
                    string actName = _actionSection.Lines[i].Params[0].StringValue;
                    if (actName.EndsWith(".ACT", StringComparison.OrdinalIgnoreCase) == false)
                        actName += ".ACT";

                    MomAct act;
                    act.Act = (Graphics.ActResource)Resource.ResourceManager.Load(actName);
                    act.Model = SceneManager.GetSceneModel(act.Act.ModelName);

                    // check if this is an absolute animation or contains a transformation
                    act.IsAbsolute = _actionSection.Lines[i].Params.Count > 1;

                    if (act.Model == null)
                    {
                        Resource.ResourceManager.Unload(act.Act);
                        continue;
                    }
                    
                    // add the act file to the list
                    bool added = false;
                    for (int j = 0; j < _acts.Count; j++)
                    {
                        if (_acts[j].HasValue == false)
                        {
                            _acts[j] = act;
                            added = true;
                            break;
                        }
                    }
                    if (added == false)
                        _acts.Add(act);
                }
            }

            // animate models using ACT files
            for (int i = 0; i < _acts.Count; i++)
            {
                if (_acts[i].HasValue)
                {
                    if (_acts[i].Value.Act.Animate(_acts[i].Value.Model, timeSinceStart, duration, true, _acts[i].Value.IsAbsolute) == false)
                        _acts[i] = null;
                }
            }
        }
    }

    public class MomLoader : Resource.IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "MOM", "ANM" }; }
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
