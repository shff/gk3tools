using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public enum Timeblock
    {
        Day1_10AM,
        Day1_12PM,
        Day1_02PM,
        Day1_04PM,
        Day1_06PM,

        Day2_07AM,
        Day2_10AM,
        Day2_12PM,
        Day2_02PM,
        Day2_05PM,
        Day2_02AM,

        Day3_07AM,
        Day3_10AM,
        Day3_12PM,
        Day3_03PM,
        Day3_06PM,
        Day3_09PM
    }

    public enum Ego
    {
        None,
        Gabriel,
        Grace
    }

    public static class GameManager
    {
        public static int TickCount
        {
            get { return _tickCount; }
        }

        public static int ElapsedTickCount
        {
            get { return _tickCount - _prevTickCount; }
        }

        public static float SecsPerFrame
        {
            get
            {
                return (_tickCount - _prevTickCount) * 0.001f;
            }
        }

        public static Timeblock CurrentTime
        {
            get { return _currentTime; }
            set { _currentTime = value; }
        }

        public static Ego CurrentEgo
        {
            get { return _currentEgo; }
            set { _currentEgo = value; }
        }

        public static string CurrentLocation
        {
            get { return _location; }
            set { _location = value; }
        }

        public static string GetTimeBlockString(Timeblock timeblock)
        {
            switch (timeblock)
            {
                case Timeblock.Day1_10AM: return TimeCodes.Day1_10AM;
                case Timeblock.Day1_12PM: return TimeCodes.Day1_12PM;
                case Timeblock.Day1_02PM: return TimeCodes.Day1_02PM;
                case Timeblock.Day1_04PM: return TimeCodes.Day1_04PM;
                case Timeblock.Day1_06PM: return TimeCodes.Day1_06PM;
                
                case Timeblock.Day2_07AM: return TimeCodes.Day2_07AM;
                case Timeblock.Day2_10AM: return TimeCodes.Day2_10AM;
                case Timeblock.Day2_12PM: return TimeCodes.Day2_12PM;
                case Timeblock.Day2_02PM: return TimeCodes.Day2_02PM;
                case Timeblock.Day2_05PM: return TimeCodes.Day2_05PM;
                case Timeblock.Day2_02AM: return TimeCodes.Day2_02AM;

                case Timeblock.Day3_07AM: return TimeCodes.Day3_07AM;
                case Timeblock.Day3_10AM: return TimeCodes.Day3_10AM;
                case Timeblock.Day3_12PM: return TimeCodes.Day3_12PM;
                case Timeblock.Day3_03PM: return TimeCodes.Day3_03PM;
                case Timeblock.Day3_06PM: return TimeCodes.Day3_06PM;
                case Timeblock.Day3_09PM: return TimeCodes.Day3_09PM;
            }

            throw new ArgumentException("Invalid TimeBlock");
        }

        public static void InjectTickCount(int tickCount)
        {
            // don't let TOO much time pass...
            if (tickCount - _prevTickCount > 500)
                tickCount = _prevTickCount + 500;

            _prevTickCount = _tickCount;
            _tickCount = tickCount;
        }

        public static void SetLocation(string location)
        {
            SceneManager.LoadSif(location, GetTimeBlockString(_currentTime));
            List<NounVerbCase> nvcs = SceneManager.GetNounVerbCasesForNoun("SCENE");

            _lastLocation = _location;
            _location = location;

            foreach (NounVerbCase nvc in nvcs)
            {
                if (nvc.Verb.Equals("ENTER", StringComparison.OrdinalIgnoreCase))
                {
                    Sheep.SheepMachine.RunCommand(nvc.Script);
                }
            }
        }

        public static int GetNounVerbCount(string noun, string verb, bool isGabe)
        {
            int count;
            if (_nounVerbCounts.TryGetValue(new NounVerbCombination(noun, verb, isGabe), out count))
                return count;

            return 0;
        }

        public static int GetNounVerbCount(string noun, string verb)
        {
            return GetNounVerbCount(noun, verb, CurrentEgo == Ego.Gabriel);
        }

        public static void SetNounVerbCount(string noun, string verb, bool isGabe, int count)
        {
            _nounVerbCounts[new NounVerbCombination(noun, verb, isGabe)] = count;
        }

        public static void SetNounVerbCount(string noun, string verb, int count)
        {
            SetNounVerbCount(noun, verb, CurrentEgo == Ego.Gabriel, count);   
        }

        public static void IncrementNounVerbCount(string noun, string verb, bool isGabe)
        {
            NounVerbCombination nv = new NounVerbCombination(noun, verb, CurrentEgo == Ego.Gabriel);
            if (_nounVerbCounts.ContainsKey(nv) == false)
                _nounVerbCounts[nv] = 1;
            else
            {
                int c = _nounVerbCounts[nv];
                _nounVerbCounts[nv] = c + 1;
            }
        }

        public static void IncrementNounVerbCount(string noun, string verb)
        {
            IncrementNounVerbCount(noun, verb, CurrentEgo == Ego.Gabriel);
        }

        public static int GetIntegerGameVariable(string variable)
        {
            int value;
            if (_integerGameVariables.TryGetValue(variable, out value))
                return value;

            return 0;
        }

        public static void SetIntegerGameVariable(string variable, int value)
        {
            _integerGameVariables[variable] = value;
        }

        public static int GetChatCount(string noun)
        {
            if (_chatCounts.ContainsKey(noun) == false)
                return 0;

            return _chatCounts[noun];
        }

        public static void IncrementChatCount(string noun)
        {
            if (_chatCounts.ContainsKey(noun) == false)
                _chatCounts[noun] = 1;
            else
            {
                int c = _chatCounts[noun];
                _chatCounts[noun] = c + 1;
            }
        }

        public static int GetTopicCount(string noun, string verb)
        {
            string nounverb = noun + "$" + verb;
            if (_topicCounts.ContainsKey(nounverb) == false)
                return 0;

            return _topicCounts[nounverb];
        }

        public static void IncrementTopicCount(string noun, string verb)
        {
            string nounverb = noun + "$" + verb;
            if (_topicCounts.ContainsKey(nounverb) == false)
                _topicCounts[nounverb] = 1;
            else
            {
                int c = _topicCounts[nounverb];
                _topicCounts[nounverb] = c + 1;
            }
        }

        public static void SetFlag(string name)
        {
            if (_flags.ContainsKey(name))
                _flags[name] = true;
            else
            {
                _flags.Add(name, true);
            }
        }

        public static bool GetFlag(string name)
        {
            if (_flags.ContainsKey(name))
                return _flags[name];

            return false;
        }

        public static void AddGameTimer(string noun, string verb, int duration)
        {
            Game.GameTimer timer;
            timer.Noun = noun;
            timer.Verb = verb;
            timer.Duration = duration;
            timer.TimeAtExpiration = duration + TickCount;

            // find the appropriate spot for the timer
            for(LinkedListNode<Game.GameTimer> node = _gameTimers.First;
                node != null; node = node.Next)
            {
                // keep the list sorted by time of expiration
                if (node.Value.TimeAtExpiration > timer.TimeAtExpiration)
                {
                    _gameTimers.AddBefore(node, timer);
                    return;
                }
            }

            // still here? just add to the end
            _gameTimers.AddLast(timer);
        }

        public static Game.GameTimer? GetNextExpiredGameTimer()
        {
            for (LinkedListNode<Game.GameTimer> node = _gameTimers.First;
                node != null; node = node.Next)
            {
                if (node.Value.TimeAtExpiration < TickCount)
                {
                    _gameTimers.Remove(node);
                    return node.Value;
                }
            }

            return null;
        }

        public static void Load()
        {
            _verbs = new Verbs("verbs.txt", FileSystem.Open("verbs.txt"));
            _strings = new LocalizedStrings("estrings.txt", FileSystem.Open("estrings.txt"));
            FaceDefinitions.Load();
        }

        public static Verbs Verbs
        {
            get { return _verbs; }
        }

        public static LocalizedStrings Strings
        {
            get { return _strings; }
        }

        public static string LastLocation
        {
            get { return _lastLocation; }
        }

        public static Graphics.Camera CreateCameraWithDefaults()
        {
            return CreateCameraWithDefaults(Math.Constants.SixtyDegreesInRadians);
        }

        public static Graphics.Camera CreateCameraWithDefaults(float fov)
        {
            return new Gk3Main.Graphics.Camera(fov, Graphics.RendererManager.CurrentRenderer.Viewport.Aspect, 5.0f, 5000.0f);
        }

        private static int _tickCount, _prevTickCount;
        private static Timeblock _currentTime = Timeblock.Day1_10AM;
        private static Ego _currentEgo;
        private static Verbs _verbs;
        private static string _location;
        private static string _lastLocation;
        private static LocalizedStrings _strings;
        private static Dictionary<NounVerbCombination, int> _nounVerbCounts = new Dictionary<NounVerbCombination,int>(new NounVerbComparison());
        private static Dictionary<string, int> _integerGameVariables = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, int> _chatCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, int> _topicCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, bool> _flags = new Dictionary<string, bool>();
        private static LinkedList<GameTimer> _gameTimers = new LinkedList<GameTimer>();
        }
}
