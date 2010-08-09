using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public enum MsgBoxType
    {
        YesNo,
        OK
    }

    public enum MsgBoxResult
    {
        Yes,
        No,
        Okay
    }

    public class MsgBoxResultEventArgs : EventArgs
    {
        public MsgBoxResult Result;
    }

    public class MsgBox : IButtonContainer, IGuiLayer
    {
        private MsgBoxType _type;
        private Font _font;
        private Button _yes;
        private Button _no;
        private Button _ok;

        private Graphics.TextureResource _bg;
        private Graphics.TextureResource _vert;
        private Graphics.TextureResource _horiz;
        private Graphics.TextureResource _ul;
        private Graphics.TextureResource _ur;
        private Graphics.TextureResource _ll;
        private Graphics.TextureResource _lr;

        private Graphics.Rect _rect;
        private float _fontHeight;
        private float _textOffsetX, _textOffsetY;
        private List<string> _lines = new List<string>();
        private EventHandler<MsgBoxResultEventArgs> _onResult;
        private bool _isActive;

        private const float _buttonPadding = 5.0f;

        public MsgBox(Resource.ResourceManager globalContent, string text, MsgBoxType type)
        {
            _type = type;
            _isActive = true;

            // we need to load all the positioning data and whatnot from a file...
            Resource.TextResource layout = new Gk3Main.Resource.TextResource("MSGBOX.TXT", FileSystem.Open("MSGBOX.TXT"));
            string[] lines = layout.Text.Split('\n');

            Dictionary<string, string> layoutInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in lines)
            {
                if (line.StartsWith(";") == false)
                {
                    int equal = line.IndexOf('=');
                    if (equal > 0)
                    {
                        layoutInfo.Add(line.Substring(0, equal).Trim(), line.Substring(equal + 1).Trim());
                    }
                }
            }

            _font = globalContent.Load<Font>(layoutInfo["Font"]);

            _yes = new Button(this, globalContent, layoutInfo["yesSpriteDown"], layoutInfo["yesSpriteHov"], layoutInfo["yesSpriteUp"], null, null);
            _no = new Button(this, globalContent, layoutInfo["noSpriteDown"], layoutInfo["noSpriteHov"], layoutInfo["noSpriteUp"], null, null);
            _ok = new Button(this, globalContent, layoutInfo["okSpriteDown"], layoutInfo["okSpriteHov"], layoutInfo["okSpriteUp"], null, null);
            _yes.OnClick += new EventHandler(onButtonClicked);
            _no.OnClick += new EventHandler(onButtonClicked);
            _ok.OnClick += new EventHandler(onButtonClicked);


            _bg = globalContent.Load<Graphics.TextureResource>("black");
            _vert = globalContent.Load<Graphics.TextureResource>(layoutInfo["vertSprite"]);
            _horiz = globalContent.Load<Graphics.TextureResource>(layoutInfo["horizSprite"]);
            _ur = globalContent.Load<Graphics.TextureResource>(layoutInfo["urCornerSprite"]);
            _ul = globalContent.Load<Graphics.TextureResource>(layoutInfo["ulCornerSprite"]);
            _lr = globalContent.Load<Graphics.TextureResource>(layoutInfo["lrCornerSprite"]);
            _ll = globalContent.Load<Graphics.TextureResource>(layoutInfo["llCornerSprite"]);

            tryParse2f(layoutInfo["minSize"], out _rect.Width, out _rect.Height);
            tryParse2f(layoutInfo["textOffset"], out _textOffsetX, out _textOffsetY);

            _rect = centerBox(Graphics.RendererManager.CurrentRenderer.Viewport, calculateBoxSize(text, _rect));

            if (_type == MsgBoxType.YesNo)
                positionButtons(true, true, false);
            else
                positionButtons(false, false, true);
        }

        public void Dismiss()
        {
            _isActive = false;
        }

        public event EventHandler<MsgBoxResultEventArgs> OnResult
        {
            add { _onResult += value; }
            remove { _onResult -= value; }
        }

        #region IGuiLayer stuff

        public void Render(Graphics.SpriteBatch sb, int tickCount)
        {
            // draw the edges
            sb.Draw(_horiz, new Gk3Main.Graphics.Rect(_rect.X, _rect.Y - _horiz.Height, _rect.Width, _horiz.Height), null, 0);
            sb.Draw(_horiz, new Gk3Main.Graphics.Rect(_rect.X, _rect.Y + _rect.Height, _rect.Width, _horiz.Height), null, 0);
            sb.Draw(_vert, new Gk3Main.Graphics.Rect(_rect.X - _vert.Width, _rect.Y, _vert.Width, _rect.Height), null, 0);
            sb.Draw(_vert, new Gk3Main.Graphics.Rect(_rect.X + _rect.Width, _rect.Y, _vert.Width, _rect.Height), null, 0);

            // draw the corners
            sb.Draw(_ul, new Gk3Main.Math.Vector2(_rect.X - _ul.Width, _rect.Y - _ul.Height));
            sb.Draw(_ur, new Gk3Main.Math.Vector2(_rect.X + _rect.Width, _rect.Y - _ur.Height));
            sb.Draw(_ll, new Gk3Main.Math.Vector2(_rect.X - _ll.Width, _rect.Y + _rect.Height));
            sb.Draw(_lr, new Gk3Main.Math.Vector2(_rect.X + _rect.Width, _rect.Y + _rect.Height));

            // draw the background
            sb.Draw(_bg, _rect, null, 0);

            // draw the text
            float posY = _rect.Y + _textOffsetY;
            foreach (string line in _lines)
            {
                _font.Print(sb, (int)(_rect.X + _textOffsetX), (int)posY, line);
                posY += _fontHeight;
            }

            // draw the buttons
            if (_type == MsgBoxType.YesNo)
            {
                _yes.Render(sb, tickCount);
                _no.Render(sb, tickCount);
            }
            else
            {
                _ok.Render(sb, tickCount);
            }
        }

        public void OnMouseMove(int ticks, int mx, int my)
        {
            if (_type == MsgBoxType.YesNo)
            {
                _yes.OnMouseMove(ticks, mx, my);
                _no.OnMouseMove(ticks, mx, my);
            }
            else
            {
                _ok.OnMouseMove(ticks, mx, my);
            }
        }

        public void OnMouseDown(int button, int mx, int my)
        {
            if (_type == MsgBoxType.YesNo)
            {
                _yes.OnMouseDown(button);
                _no.OnMouseDown(button);
            }
            else
            {
                _ok.OnMouseDown(button);
            }
        }

        public void OnMouseUp(int button, int mx, int my)
        {
            if (_type == MsgBoxType.YesNo)
            {
                _yes.OnMouseUp(button);
                _no.OnMouseUp(button);
            }
            else
            {
                _ok.OnMouseUp(button);
            }
        }

        public bool IsPopup { get { return true; } }

        public bool IsActive { get { return _isActive; } }

        #endregion

        #region IButtonContainer stuff

        public int ScreenX
        {
            get { return (int)_rect.X; }
        }

        public int ScreenY
        {
            get { return (int)_rect.Y; }
        }

        #endregion

        private void positionButtons(bool yesVisible, bool noVisible, bool okVisible)
        {
            float totalWidth = (yesVisible ? _yes.Width + _buttonPadding : 0)
                + (noVisible ? _no.Width + _buttonPadding : 0)
                + (okVisible ? _ok.Width + _buttonPadding : 0);

            int x = (int)(_rect.Width - totalWidth) / 2;
            int y = (int)(_rect.Height - _buttonPadding - _yes.Height);

            if (yesVisible)
            {
                _yes.X = new Unit(0, x);
                _yes.Y = new Unit(0, y);
                x += _yes.Width + (int)_buttonPadding;
            }

            if (noVisible)
            {
                _no.X = new Unit(0, x);
                _no.Y = new Unit(0, y);
                x += _no.Width + (int)_buttonPadding;
            }

            if (okVisible)
            {
                _ok.X = new Unit(0, x);
                _ok.Y = new Unit(0, y);
            }
        }

        private Graphics.Rect centerBox(Graphics.Viewport viewport, Graphics.Rect size)
        {
            Graphics.Rect result = size;
            result.X = (float)System.Math.Floor((viewport.Width - size.Width) / 2);
            result.Y = (float)System.Math.Floor((viewport.Height - size.Height) / 2);

            return result;
        }

        private Graphics.Rect calculateBoxSize(string text, Graphics.Rect minSize)
        {
            Math.Vector2 textSize = _font.MeasureString(text);
            float area = textSize.X * textSize.Y;

            const float goalRatio = 3.0f;
            float newHeight = (float)System.Math.Sqrt(area / goalRatio);
            float newWidth = newHeight * goalRatio;

            splitStrings(text, System.Math.Max(newWidth, minSize.Width - _textOffsetX * 2));

            Graphics.Rect result = new Graphics.Rect();
            result.Width = (float)System.Math.Ceiling(System.Math.Max(minSize.Width, newWidth + _textOffsetX * 2.0f));
            result.Height = (float)System.Math.Ceiling(System.Math.Max(minSize.Height, newHeight + _textOffsetY * 2.0f + _ok.Height + _buttonPadding * 2.0f));

            return result;
        }

        private void splitStrings(string text, float width)
        {
            _lines.Clear();

            string currentString = text;
            string[] words = text.Split(' ');

            float currentLineWidth = 0;
            string currentLine = string.Empty;
            float maxFontHeight = float.MinValue;
            float spaceWidth = _font.MeasureString(" ").X;
            bool needSpace = false;
            foreach (string word in words)
            {
                Math.Vector2 size = _font.MeasureString(word);
                if (currentLineWidth + size.X > width)
                {
                    _lines.Add(currentLine);
                    currentLine = word;
                    currentLineWidth = size.X + spaceWidth;
                }
                else
                {
                    if (needSpace) currentLine += ' ';
                    currentLine += word;
                    currentLineWidth += size.X + spaceWidth;
                    needSpace = true;
                }

                if (size.Y > maxFontHeight)
                    maxFontHeight = size.Y;
            }

            // add the final line
            _lines.Add(currentLine);
            _fontHeight = maxFontHeight;
        }

        private bool tryParse2f(string str, out float f1, out float f2)
        {
            int comma = str.IndexOf(',');

            f1 = f2 = 0;

            return float.TryParse(str.Substring(0, comma), out f1) &&
                float.TryParse(str.Substring(comma + 1), out f2);
        }

        private void onButtonClicked(object sender, EventArgs e)
        {
            if (_onResult != null)
            {
                MsgBoxResultEventArgs args = new MsgBoxResultEventArgs();

                if (sender == _ok)
                    args.Result = MsgBoxResult.Okay;
                else if (sender == _yes)
                    args.Result = MsgBoxResult.Yes;
                else if (sender == _no)
                    args.Result = MsgBoxResult.No;

                _onResult(this, args);
            }
        }
    }
}
