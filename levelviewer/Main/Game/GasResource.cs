using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public class GasResource : Resource.TextResource
    {
        private Resource.ResourceManager _content;
        private bool _playing;
        private bool _suspended;
        private bool _timeSinceSuspend;
        private int _currentInstructionIndex;
        private WaitHandle _currentWait;
        private List<GasScriptLine> _lines = new List<GasScriptLine>();

        public GasResource(string name, System.IO.Stream stream, Resource.ResourceManager content)
            : base(name, stream)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            _content = content;
            parse(Text);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public void Play()
        {
            Logger.WriteInfo("Playing GAS script: " + this.Name, LoggerStream.Animation);

            _playing = true;
            _suspended = false;
            _currentInstructionIndex = 0;

            while (executeNextInstruction())
            {
                // nothing...
            }
        }

        public void Continue()
        {
            if (_suspended && _currentWait != null && _currentWait.Finished)
            {
                _suspended = false;
                _currentWait = null;
            }

            if (_suspended == false)
                while (executeNextInstruction())
                {
                    // nothing...
                }
        }

        public bool Playing
        {
            get { return _playing; }
        }

        public bool Suspended
        {
            get { return _suspended; }
        }

        #region Privates
        private bool executeNextInstruction()
        {
            // TODO
            GasCommand command = _lines[_currentInstructionIndex].Command;
            GasParam[] cparams = _lines[_currentInstructionIndex].Params;
            _currentInstructionIndex++;

            switch (command)
            {
                case GasCommand.Anim:
                    execAnim(cparams);
                    break;
                case GasCommand.Wait:
                    _currentWait = new TimedWaitHandle(cparams[0].IntegerValue * 1000);
                    _suspended = true;
                    break;
                case GasCommand.Loop:
                    _currentInstructionIndex = 0;
                    break;
            }

            // reached the end?
            if (_currentInstructionIndex >= _lines.Count)
            {
                _playing = false;
            }

            return _playing && !_suspended;
        }

        private void parse(string gas)
        {
            GasParser parser = new GasParser(gas);

            while (true)
            {
                string command;
                if (parser.ReadString(out command))
                {
                    // figure out which command this is
                    if (command.Equals("Anim", StringComparison.OrdinalIgnoreCase))
                        parseAnim(parser, true);
                    else if (command.Equals("OneOf", StringComparison.OrdinalIgnoreCase))
                        parseAnim(parser, false);
                    else if (command.Equals("Wait", StringComparison.OrdinalIgnoreCase))
                        parseWait(parser);
                    else if (command.Equals("Loop", StringComparison.OrdinalIgnoreCase))
                    {
                        GasScriptLine line;
                        line.Command = GasCommand.Loop;
                        line.Params = null;

                        _lines.Add(line);
                    }
                }

                if (parser.NextMeaningfulLine())
                    break;
            }
        }

        private bool parseAnim(GasParser parser, bool isAnim)
        {
            GasParam filename = new GasParam();
            GasParam moving = new GasParam();
            GasParam percent = new GasParam();

            if (parser.ReadString(out filename.StringValue) == false)
                return false;
            if (parser.ReadBoolean(out moving.BooleanValue) == false)
                moving.BooleanValue = false;
            if (parser.ReadInteger(out percent.IntegerValue) == false)
                percent.IntegerValue = 100;

            GasScriptLine line;
            line.Command = (isAnim ? GasCommand.Anim : GasCommand.OneOf);
            line.Params = new GasParam[] { filename, moving, percent };

            _lines.Add(line);

            return true;
        }

        private bool parseWait(GasParser parser)
        {
            GasParam min = new GasParam();
            GasParam max = new GasParam();
            GasParam percent = new GasParam();

            if (parser.ReadInteger(out min.IntegerValue) == false)
                return false;
            if (parser.ReadInteger(out max.IntegerValue) == false)
                max.IntegerValue = 0;
            if (parser.ReadInteger(out percent.IntegerValue) == false)
                percent.IntegerValue = 100;

            GasScriptLine line;
            line.Command = GasCommand.Wait;
            line.Params = new GasParam[] { min, max, percent };

            _lines.Add(line);

            return true;
        }


        #region Instruction handlers

        void execAnim(GasParam[] cparams)
        {
            string filename = cparams[0].StringValue;
            bool moving = (cparams.Length > 1 ? cparams[1].BooleanValue : false);
            int percent = (cparams.Length > 2 ? cparams[2].IntegerValue : 100);

            if (percent < 100)
            {
                // roll a dice to see if we're even going to execute this animation
                if (Utils.RollFloatingDie() > percent * 0.01f)
                    return;
            }

            //AnmResource anm = (AnmResource)Resource.ResourceManager.Load(string.Format("{0}.ANM", filename));
            //MomResource mom = (MomResource)Resource.ResourceManager.Load(string.Format("{0}.ANM", filename));
            MomResource mom = _content.Load<MomResource>(filename);
            _currentWait = Animator.Add(mom, true);

            _suspended = true;
        }

        #endregion

        private enum GasCommand
        {
            Anim,
            OneOf,
            Wait,
            Label,
            Goto,
            Loop,
            Set,
            Inc,
            Dec,
            If,
            WalkTo,
            ChooseWalk,
            Use,
            UseTalk,
            WhenNear,
            NewIdle,
            DLG,
            WhenNoLongerNear,
            WhenInView,
            Location,
            SetMood,
            ClearMood,
            Glance,
            TurnHead,
            LookAt,
            Expression
        }

        private struct GasParam
        {
            public string StringValue;
            public int IntegerValue;
            public bool BooleanValue;
        }

        private struct GasScriptLine
        {
            public GasCommand Command;
            public GasParam[] Params;
        }
        #endregion
    }

    public class GasResourceLoader : Resource.IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "GAS" }; }
        }

        public bool EmptyResourceIfNotFound { get { return false; } }

        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            if (name.IndexOf('.') < 0)
                name += ".GAS";

            System.IO.Stream stream = FileSystem.Open(name);

            GasResource resource = new GasResource(name, stream, content);

            stream.Close();

            return resource;
        }
    }

    class GasParser
    {
        private string _gas;
        private int _index;
        private bool _atNextLine;
        private bool _eof;

        public GasParser(string gas)
        {
            _gas = gas;
            _index = 0;
            _atNextLine = false;
            _eof = false;

            moveIndexToNextText();
            if (_gas[_index] == '/')
                NextMeaningfulLine();
        }

        public bool ReadString(out string s)
        {
            if (_atNextLine || _eof)
            {
                s = null;
                return false;
            }

            int nextSpaceOrEol = findNextSpaceOrEndOfLine();
            int nextComma = _gas.IndexOf(',', _index);

            if (nextSpaceOrEol < 0)
                s = _gas.Substring(_index).Trim();
            else
            {
                if (nextComma > 0 && nextComma < nextSpaceOrEol)
                {
                    s = _gas.Substring(_index, nextComma - _index);
                    _index = nextComma;
                }
                else
                {
                    s = _gas.Substring(_index, nextSpaceOrEol - _index);
                    _index = nextSpaceOrEol;
                }
            }

            moveIndexToNextText();

            return true;
        }

        public bool ReadFloat(out float f)
        {
            string s;
            if (ReadString(out s))
            {
                f = float.Parse(s);
                return true;
            }

            f = 0;
            return false;
        }

        public bool ReadInteger(out int i)
        {
            string s;
            if (ReadString(out s))
            {
                i = int.Parse(s);
                return true;
            }

            i = 0;
            return false;
        }

        public bool ReadBoolean(out bool b)
        {
            string s;
            if (ReadString(out s))
            {
                return bool.TryParse(s, out b);
            }

            b = false;
            return false;
        }

        public bool NextLine()
        {
            _atNextLine = false;

            int nextLine = _gas.IndexOf("\n", _index);

            if (nextLine < 0)
            {
                _eof = true;
            }
            else
            {
                _index = nextLine + 1;
                if (_index >= _gas.Length)
                    _eof = true;
                else
                    moveIndexToNextText();

                if (_index >= _gas.Length)
                    _eof = true;
            }

            return _eof;
        }

        public bool NextMeaningfulLine()
        {
            while (!NextLine())
            {
                if (_gas[_index] == '/')
                    continue;

                return false;
            }

            return true;
        }

        private static char[] _whitespaceChars = new char[] { ' ', '\r', '\n' };
        private int findNextSpaceOrEndOfLine()
        {
            // get the index of the next space or EOL
            return _gas.IndexOfAny(_whitespaceChars, _index);
        }

        private void moveIndexToNextText()
        {
            if (_gas[_index] == ',')
                _index++;

            while (char.IsWhiteSpace(_gas[_index]) && _index < _gas.Length)
            {
                if (_gas[_index] == '\n')
                {
                    _atNextLine = true;
                    break;
                }

                _index++;
            }
        }
    }
}
