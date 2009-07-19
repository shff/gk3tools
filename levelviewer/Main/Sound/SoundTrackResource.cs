using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sound
{

    enum SoundTrackNodeType
    {
        Sound,
        Prs,
        Wait
    }

    interface ISoundTrackNode
    {
        SoundTrackNodeType Type { get; }
        bool Enabled { get; set; }

        float Random { get; }
        int Repeat { get; set; }
        bool RepeatEnabled { get; }
    }

    class SoundTrackWaitNode : ISoundTrackNode
    {
        private int _minWait;
        private int _maxWait;
        private int _repeat;
        private bool _enableRepeat;
        private float _random = 1.0f;
        private bool _enabled = true;

        public SoundTrackWaitNode(Resource.InfoSection waitSection)
        {
            foreach (Resource.InfoLine line in waitSection.Lines)
            {
                // TODO: this could be more efficient
                if (line.TryGetIntAttribute("MinWaitMS", out _minWait) == false)
                {
                    if (line.TryGetIntAttribute("MaxWaitMS", out _maxWait) == false)
                    {
                        if (line.TryGetIntAttribute("Repeat", out _repeat) == false)
                        {
                            int r;
                            if (line.TryGetIntAttribute("Random", out r))
                                _random = r / 100.0f;
                        }
                    }
                }
                else if (_maxWait < _minWait)
                    _maxWait = _minWait;
            }

            if (_repeat > 0)
                _enableRepeat = true;
        }

        public SoundTrackNodeType Type { get { return SoundTrackNodeType.Wait; } }
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }

        public int MinWait { get { return _minWait; } }
        public int MaxWait { get { return _maxWait; } }
        public int Repeat { get { return _repeat; } set { _repeat = value; } }
        public bool RepeatEnabled { get { return _enableRepeat; } }
        public float Random { get { return _random; } }
    }

    enum SoundStopMethod
    {
        StopWhenComplete = 0,
        StopFade = 1,
        StopImmediately = 2
    }

    class SoundTrackSoundNode : ISoundTrackNode, IDisposable
    {
        protected string _name = "default";
        protected float _volume = 1.0f;
        protected int _repeat;
        protected bool _repeatEnabled;
        protected float _random = 1.0f;
        protected bool _loop;
        protected int _fadeIn;
        protected SoundStopMethod _stopMethod;
        protected int _fadeOut;
        protected bool _3d;
        protected int _minDist = 60;
        protected int _maxDist = 840;
        protected float _x;
        protected float _y;
        protected float _z;
        protected string _follow;
        protected bool _enabled = true;

        protected Sound _sound;
        protected PlayingSound _playingSound;

        public SoundTrackSoundNode(Resource.InfoSection soundSection)
        {
            foreach (Resource.InfoLine line in soundSection.Lines)
            {
                // TODO: this code is messy and could be more efficient
                string sdummy;
                int idummy;
                float fdummy;

                if (line.TryGetAttribute("Name", out sdummy))
                    _name = sdummy;
                else if (line.TryGetIntAttribute("Volume", out idummy))
                    _volume = idummy / 100.0f;
                else if (line.TryGetIntAttribute("Repeat", out idummy))
                    _repeat = idummy;
                else if (line.TryGetIntAttribute("Random", out idummy))
                    _random = idummy / 100.0f;
                else if (line.TryGetIntAttribute("Loop", out idummy))
                    _loop = idummy != 0;
                else if (line.TryGetIntAttribute("FadeInMS", out idummy))
                    _fadeIn = idummy;
                else if (line.TryGetIntAttribute("FadeOutMS", out idummy))
                    _fadeOut = idummy;
                else if (line.TryGetIntAttribute("StopMethod", out idummy))
                    _stopMethod = (SoundStopMethod)idummy;
                else if (line.TryGetIntAttribute("3D", out idummy))
                    _3d = idummy != 0;
                else if (line.TryGetIntAttribute("MinDist", out idummy))
                    _minDist = idummy;
                else if (line.TryGetIntAttribute("MaxDist", out idummy))
                    _maxDist = idummy;
                else if (line.TryGetFloatAttribute("X", out fdummy))
                    _x = fdummy;
                else if (line.TryGetFloatAttribute("Y", out fdummy))
                    _y = fdummy;
                else if (line.TryGetFloatAttribute("Z", out fdummy))
                    _z = fdummy;
                else if (line.TryGetAttribute("Follow", out sdummy))
                    _follow = sdummy;
            }


            // load the sound
            if (string.IsNullOrEmpty(_name) == false)
            {
                // make sure it ends in ".wav"
                string fileToLoad = _name;
                if (fileToLoad.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) == false)
                    fileToLoad = fileToLoad + ".wav";

                try
                {
                    _sound = (Sound)Resource.ResourceManager.Load(fileToLoad);

                    _sound.Source.DefaultMinDistance = _minDist;
                    _sound.Source.DefaultMaxDistance = _maxDist;
                    _sound.Source.DefaultVolume = _volume;
                }
                catch
                {
                    // for some reason it seems like it's possible for requested sounds
                    // not to exist!
                    Console.CurrentConsole.WriteLine(ConsoleVerbosity.Polite, 
                        "Unable to load sound referenced in STK: {0}", fileToLoad);
                }
            }

            if (_repeat > 0)
                _repeatEnabled = true;
        }

        public void Dispose()
        {
            if (_sound != null)
            {
                Resource.ResourceManager.Unload(_sound);
                _sound = null;
            }
        }

        public virtual SoundTrackNodeType Type { get { return SoundTrackNodeType.Sound; } }
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }
        public int Repeat { get { return _repeat; } set { _repeat = value; } }
        public bool Is3D { get { return _3d; } }
        public virtual bool RepeatEnabled { get { return _repeatEnabled; } set { _repeatEnabled = value; } }
        public float Random { get { return _random; } }
        public float X { get { return _x; } }
        public float Y { get { return _y; } }
        public float Z { get { return _z; } }

        public Sound Sound { get { return _sound; } }
        public PlayingSound PlayingSound { get { return _playingSound; } }
    }

    class SoundTrackPrsNode : SoundTrackSoundNode
    {
        public SoundTrackPrsNode(Resource.InfoSection section)
            : base(section)
        {
        }

        public override SoundTrackNodeType Type
        {
            get { return SoundTrackNodeType.Prs; }
        }

        public override bool RepeatEnabled
        {
            get
            {
                // PRS's can't repeat!
                return false;
            }
            set
            {
                // nothing
            }
        }
    }

    class SoundTrackResource : Resource.InfoResource
    {
        private SoundTrackChannel _channel;
        private List<ISoundTrackNode> _nodes = new List<ISoundTrackNode>();
        private int _currentNodeIndex;
        private int _timeAtStart;
        private bool _waiting;
        private int _timeToFinishWait;
        private PlayingSound? _playingSound;
        private bool _playing;

        public SoundTrackResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            foreach (Resource.InfoSection section in Sections)
            {
                if (section.Name.Equals("SoundTrack", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        string soundType;
                        if (line.TryGetAttribute("SoundType", out soundType))
                        {
                            if (soundType.Equals("SFX", StringComparison.OrdinalIgnoreCase))
                                _channel = SoundTrackChannel.SFX;
                            else if (soundType.Equals("Ambient", StringComparison.OrdinalIgnoreCase))
                                _channel = SoundTrackChannel.Ambient;
                            else if (soundType.Equals("Music", StringComparison.OrdinalIgnoreCase))
                                _channel = SoundTrackChannel.Music;
                            else if (soundType.Equals("Dialogue", StringComparison.OrdinalIgnoreCase))
                                _channel = SoundTrackChannel.Dialog;
                        }
                    }
                }
                else if (section.Name.Equals("Wait", StringComparison.OrdinalIgnoreCase))
                {
                    _nodes.Add(new SoundTrackWaitNode(section));
                }
                else if (section.Name.Equals("Sound", StringComparison.OrdinalIgnoreCase))
                {
                    _nodes.Add(new SoundTrackSoundNode(section));
                }
                else if (section.Name.Equals("PRS", StringComparison.OrdinalIgnoreCase))
                {
                    _nodes.Add(new SoundTrackPrsNode(section));
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (ISoundTrackNode node in _nodes)
            {
                if (node is IDisposable)
                {
                    ((IDisposable)node).Dispose();
                }
            }
        }

        public void Start(int currentTime)
        {
            _timeAtStart = currentTime;
            _currentNodeIndex = 0;
            _playing = true;
        }

        public void Step(int currentTime)
        {
            if (_waiting)
            {
                if (_timeToFinishWait > currentTime)
                {
                    _waiting = false;
                    _currentNodeIndex++;
                }
            }
            else if (_playingSound.HasValue)
            {
                if (_playingSound.Value.Finished)
                {
                    _playingSound = null;
                }
            }
            else
            {
                // move to the next enabled node
                if (findNextEnabledNode(_currentNodeIndex, out _currentNodeIndex) == false)
                {
                    _playing = false;
                    return;
                }

                if (_nodes[_currentNodeIndex].RepeatEnabled)
                {
                    _nodes[_currentNodeIndex].Repeat--;

                    if (_nodes[_currentNodeIndex].Repeat < 1)
                        _nodes[_currentNodeIndex].Enabled = false;
                }

                if (Utils.RollFloatingDie() < _nodes[_currentNodeIndex].Random)
                {
                    if (_nodes[_currentNodeIndex].Type == SoundTrackNodeType.Wait)
                    {
                        SoundTrackWaitNode node = (SoundTrackWaitNode)_nodes[_currentNodeIndex];

                        _waiting = true;

                        if (node.MaxWait == node.MinWait)
                            _timeToFinishWait = currentTime + node.MinWait;
                        else
                            _timeToFinishWait = currentTime + Utils.PickRandomNumber(node.MinWait, node.MaxWait);
                    }
                    else if (_nodes[_currentNodeIndex].Type == SoundTrackNodeType.Sound)
                    {
                        SoundTrackSoundNode node = (SoundTrackSoundNode)_nodes[_currentNodeIndex];

                        // TODO: set up the 3D position, fade in/out, etc
                        if (node.Sound != null)
                        {
                            if (node.Is3D)
                                _playingSound = node.Sound.Play3D(_channel, node.Z, node.Y, node.X);
                            else
                                _playingSound = node.Sound.Play2D(_channel);
                        }
                    }
                    else if (_nodes[_currentNodeIndex].Type == SoundTrackNodeType.Prs)
                    {
                        int prsCount = countPrs(_currentNodeIndex);

                        int prsToPlay = Utils.PickRandomNumber(0, prsCount);

                        SoundTrackSoundNode node = (SoundTrackSoundNode)_nodes[_currentNodeIndex + prsToPlay];

                        // TODO: set up the 3D position, fade in/out, etc
                        if (node.Sound != null)
                        {
                            if (node.Is3D)
                                _playingSound = node.Sound.Play3D(_channel, node.X, node.Y, node.Z);
                            else
                                _playingSound = node.Sound.Play2D(_channel);
                        }

                        // move to the last PRS node
                        _currentNodeIndex += prsCount - 1;
                    }
                }
            }
        }

        public bool Playing
        {
            get { return _playing; }
        }

        private bool findNextEnabledNode(int start, out int index)
        {
            for (int i = start + 1; i < _nodes.Count; i++)
            {
                if (_nodes[i].Enabled)
                {
                    index = i;
                    return true;
                }
            }

            // maybe we need to start from the top?
            for (int i = 0; i < start; i++)
            {
                if (_nodes[i].Enabled)
                {
                    index = i;
                    return true;
                }
            }

            // couldn't find it :(
            index = -1;
            return false;
        }

        private int countPrs(int start)
        {
            int count = 0;
            for (int i = start; i < _nodes.Count; i++)
                if (_nodes[i].Type == SoundTrackNodeType.Prs)
                    count++;
                else
                    break;

            return count;
        }
    }

    public class SoundTrackLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            return new SoundTrackResource(name, stream);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[] { "STK" };
    }
}
