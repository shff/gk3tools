using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    class Console : Gk3Main.Console
    {
        private static Console _instance;
        private static System.Text.StringBuilder _command = new System.Text.StringBuilder();

        struct ConsoleLine
        {
            public string Text;
            public Gk3Main.ConsoleSeverity Severity;
        }

        private static List<ConsoleLine> _lines = new List<ConsoleLine>();
        private static string _prevCommand;
        private static Gk3Main.Gui.FontInstance _font;
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

        public static void Load()
        {
            _font = Gk3Main.Gui.Font.Load(Gk3Main.Resource.ResourceManager.Global.Load<Gk3Main.Gui.FontSpec>("f_courier"));
        }

        public static void Render(Gk3Main.Graphics.SpriteBatch spriteBatch)
        {
            if (_visible)
            {
                spriteBatch.Begin();

                // draw a background
                {
                    var renderer = Gk3Main.Graphics.RendererManager.CurrentRenderer;
                    var bg = Gk3Main.Graphics.RendererManager.CurrentRenderer.DefaultTexture;
                    var r = new Gk3Main.Graphics.Rect(0, 0, renderer.Viewport.Width, (_numVisibleLines + 1) * _font.Font.LineHeight);
                    spriteBatch.Draw(bg, r, null, new Gk3Main.Graphics.Color(0, 0, 0, 0.7f), 0);
                }

                int startLine = Math.Max(0, _lines.Count - _numVisibleLines);

                int cursorY = 0;
                for (int i = startLine; i < _lines.Count; i++)
                {
                    Gk3Main.Graphics.Color color;
                    if (_lines[i].Severity == Gk3Main.ConsoleSeverity.Error)
                        color = Gk3Main.Graphics.Color.Red;
                    else if (_lines[i].Severity == Gk3Main.ConsoleSeverity.Warning)
                        color = Gk3Main.Graphics.Color.Orange;
                    else
                        color = Gk3Main.Graphics.Color.White;

                    var f = _font;
                    f.Color = color;
                    Gk3Main.Gui.Font.Print(spriteBatch, f, 0, cursorY, _lines[i].Text);

                    cursorY += _font.Font.LineHeight;
                }

                Gk3Main.Gui.Font.Print(spriteBatch, _font, 0, cursorY, _command.ToString());

                spriteBatch.End();
            }
        }

        public override void Write(Gk3Main.ConsoleSeverity severity, string text, params object[] arg)
        {
            string result = string.Format(text, arg);

            if (_wrap)
            {
                if (Gk3Main.Gui.Font.MeasureString(_font, result).X > _wrapWidth)
                    wrapLine(result, severity);
                else
                    _lines.Add(new ConsoleLine() { Severity = severity, Text = string.Format(text, arg) });
            }
            else
            {
                _lines.Add(new ConsoleLine() { Severity = severity, Text = string.Format(text, arg) });
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
            if (c == '\b') // backspace
            {
                if (_command.Length > 0)
                    _command.Remove(_command.Length - 1, 1);
            }
            else if (c == 10) // enter
            {
                var command = _command.ToString();
                _prevCommand = command;

                _instance.Write(Gk3Main.ConsoleSeverity.Normal, command);

                _instance.RunCommand(command);

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

        private static void wrapLine(string lineToWrap, Gk3Main.ConsoleSeverity severity)
        {
            string currentString = lineToWrap;
            StringBuilder currentLine = new StringBuilder();
            int index = 0;
            while (true)
            {
                float lineWidth = _wrapWidth;

                int prevSpace = -1;
                currentLine.Length = 0;
                while (Gk3Main.Gui.Font.MeasureString(_font.Font.Instance, currentLine).X < lineWidth && index < currentString.Length)
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
                        _lines.Add(new ConsoleLine() { Text = line, Severity = severity });
                        index--;
                    }
                    else
                    {
                        string line = currentLine.ToString().Substring(0, prevSpace);
                        _lines.Add(new ConsoleLine() { Text = line, Severity = severity });
                        index -= (currentLine.Length - prevSpace - 1);
                    }
                }
                else
                {
                    // apparently we're done! add this last line and exit
                    _lines.Add(new ConsoleLine() { Text = currentLine.ToString(), Severity = severity });
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
