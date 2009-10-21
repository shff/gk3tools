using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public class GasResource
    {
        private List<GasScriptLine> _lines = new List<GasScriptLine>();

        public static void Test()
        {
            string blah =
                // "  // this is a comment\n" +
                "    COMMAND Foo,4, true\n" +
                "// poops and strawberries\n" +
                "  ANIM poo, FALSE, 50\n" +
                "anim fart,FALSE,100\n" +
                "anim foot,trUe\n" +
                "anim ardvark\n" +
                "FART foo, bar, baz";

            GasParser parser = new GasParser(blah);

            string command;
            string dummy;
            float dummyf;
            bool dummyb;
            parser.ReadString(out command);
            parser.ReadString(out dummy);
            parser.ReadFloat(out dummyf);
            parser.ReadBoolean(out dummyb);

            parser.NextMeaningfulLine();

            parser.ReadString(out command);

            GasResource gr = new GasResource();
            gr.Parse(blah);
        }

        public void Parse(string gas)
        {
            GasParser parser = new GasParser(gas);

            while (true)
            {
                string command;
                if (parser.ReadString(out command))
                {
                    // figure out which command this is
                    if (command.Equals("Anim", StringComparison.OrdinalIgnoreCase))
                        parseAnim(parser);
                }

                if (parser.NextMeaningfulLine())
                    break;
            }
        }

        private bool parseAnim(GasParser parser)
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
            line.Command = GasCommand.Anim;
            line.Params = new GasParam[] { filename, moving, percent };

            _lines.Add(line);

            return true;
        }

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
                s = _gas.Substring(_index);
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
                b = bool.Parse(s);
                return true;
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

        private static char[] _whitespaceChars = new char[] { ' ', '\n' };
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
