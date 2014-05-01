using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    class Console : Gk3Main.Console
    {
        private static Console _instance;
        private static System.Text.StringBuilder _command = new System.Text.StringBuilder();
        private static List<string> _lines = new List<string>();
        private static string _prevCommand;
        private static Gk3Main.Gui.Font _font;
        private static bool _wrap;
        private static float _wrapWidth;
        private static bool _visible = true;

        private const int _numVisibleLines = 15;

        public static Console Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Console();

                return _instance;
            }
        }

        public static bool Wrap
        {
            get { return _wrap; }
            set { _wrap = value; }
        }

        public static float WrapWidth
        {
            get { return _wrapWidth; }
            set { _wrapWidth = value; }
        }

        public static bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public static void Load(Gk3Main.Resource.ResourceManager content)
        {
            _font = content.Load<Gk3Main.Gui.Font>("f_courier");
        }

        public static void Render(Gk3Main.Graphics.SpriteBatch spriteBatch)
        {
            if (_visible)
            {
                spriteBatch.Begin();

                int startLine = Math.Max(0, _lines.Count - _numVisibleLines);

                int cursorY = 0;
                for (int i = startLine; i < _lines.Count; i++)
                {
                    _font.Print(spriteBatch, 0, cursorY, _lines[i]);

                    cursorY += _font.LineHeight;
                }

                _font.Print(spriteBatch, 0, cursorY, _command.ToString());

                spriteBatch.End();
            }
        }

        public override void Write(Gk3Main.ConsoleVerbosity verbosity, string text, params object[] arg)
        {
            string result = string.Format(text, arg);

            if (_wrap && _font != null)
            {
                if (_font.MeasureString(result).X > _wrapWidth)
                    wrapLine(result);
                else
                    _lines.Add(string.Format(text, arg));
            }
            else
            {
                _lines.Add(string.Format(text, arg));
            }
            
            System.Console.WriteLine(result);
        }

        public static void KeyPress(Keys key)
        {
            if (key == Keys.Up ||
                key == Keys.Down ||
                key == Keys.Right ||
                key == Keys.Left)
            {
                ControlButtonPressed(key);
            }
            else
            {
                char c = convertKey(key);

                AppendToCurrentCommand(c);
            }
        }

        public static void AppendToCurrentCommand(char c)
        {
            if (c == '\b')
            {
                if (_command.Length > 2)
                    _command.Remove(_command.Length - 1, 1);
            }
            else if (c == 10)
            {
                _prevCommand = _command.ToString();

                _instance.Write(Gk3Main.ConsoleVerbosity.Polite, _prevCommand);

                _instance.RunCommand(_prevCommand);

                _command.Length = 0;
            }
            else if (c != 0)
                _command.Append(c);
        }

        public static void ControlButtonPressed(Keys button)
        {
            if (button == Keys.Up)
            {
                _command.Length = 0;
                _command.Append(_prevCommand);
            }
        }

        private static void wrapLine(string lineToWrap)
        {
            string currentString = lineToWrap;
            StringBuilder currentLine = new StringBuilder();
            int index = 0;
            while (true)
            {
                float lineWidth = _wrapWidth;

                int prevSpace = -1;
                currentLine.Length = 0;
                while (_font.MeasureString(currentLine).X < lineWidth && index < currentString.Length)
                {
                    if (currentString[index] == ' ')
                        prevSpace = currentLine.Length;

                    currentLine.Append(currentString[index++]);
                }

                // uh oh, too many characters! back up until we find a space
                if (index < currentString.Length)
                {
                    if (prevSpace == -1)
                    {
                        // no spaces found, so just break in the middle of the line
                        string line = currentLine.ToString();
                        line = line.Substring(0, line.Length - 1);
                        _lines.Add(line);
                        index--;
                    }
                    else
                    {
                        string line = currentLine.ToString().Substring(0, prevSpace);
                        _lines.Add(line);
                        index -= (currentLine.Length - prevSpace - 1);
                    }
                }
                else
                {
                    // apparently we're done! add this last line and exit
                    _lines.Add(currentLine.ToString());
                    break;
                }
            }
        }

        private static char convertKey(Keys keys)
        {
            bool shift = Input.CurrentKeys.IsKeyDown(Keys.LeftShift) ||
                Input.CurrentKeys.IsKeyDown(Keys.RightShift);

            // A - Z
            if (keys >= Keys.A && keys <= Keys.Z)
            {
                if (shift)
                {
                    return (char)keys;
                }
                else
                {
                    return (char)('a' + ((char)keys - 'A'));
                }
            }

            // 0 - 9
            else if (keys >= Keys.D0 && keys <= Keys.D9)
            {
                if (shift)
                {
                    // this assumes a standard US Querty keyboard!
                    if (keys == Keys.D1)
                        return '!';
                    if (keys == Keys.D9)
                        return '(';
                    if (keys == Keys.D0)
                        return ')';
                }
                else
                {
                    return (char)keys;
                }
            }
            //else if (keys == Keys.OemPipe)
            //    return '\\';
            //else if (keys == Keys.OemOpenBrackets)
            //    return '[';
            //else if (keys == Keys.OemCloseBrackets)
            //    return ']';

            // special characters
            else if (keys == Keys.Enter)
                return '\n';
            else if (keys == Keys.Back)
                return '\b';
            else if (keys == Keys.Space)
                return ' ';
            else if (keys == Keys.OemSemicolon)
            {
                if (shift)
                    return ':';
                else
                    return ';';
            }
            else if (keys == Keys.OemPlus)
                return '+';
            else if (keys == Keys.OemComma)
                return ',';
            else if (keys == Keys.OemMinus)
                return '-';
            else if (keys == Keys.OemPeriod)
            {
                if (shift)
                    return '>';
                else
                    return '.';
            }
            else if (keys == Keys.OemQuestion)
            {
                if (shift)
                    return '?';
                else
                    return '/';
            }
            else if (keys == Keys.OemPipe)
            {
                if (shift)
                    return '|';
                else
                    return '\\';
            }
            else if (keys == Keys.OemQuotes)
            {
                if (shift)
                    return '"';
                else
                    return '\'';
            }

            return (char)0;
        }
    }
}
