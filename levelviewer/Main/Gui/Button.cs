using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class Button
    {
        public Button(IButtonContainer container, Resource.ResourceManager content, string downImage, string hoverImage, string upImage, string disabledImage, string clickedSound)
        {
            _downImage = content.Load<Graphics.TextureResource>(downImage);
            _hoverImage = content.Load<Graphics.TextureResource>(hoverImage);
            _upImage = content.Load<Graphics.TextureResource>(upImage);

            if (string.IsNullOrEmpty(disabledImage) == false)
                _disabledImage = content.Load<Graphics.TextureResource>(disabledImage);

            if (string.IsNullOrEmpty(clickedSound) == false)
                _clickedSound = content.Load<Sound.Sound>(clickedSound);

            _container = container;
            _enabled = true;
        }

        public Button(IButtonContainer container, Resource.ResourceManager content, string downImage, string hoverImage, string upImage, string disabledImage, string clickedSound, string tooltip)
        {
            _downImage = content.Load<Graphics.TextureResource>(downImage);
            _hoverImage = content.Load<Graphics.TextureResource>(hoverImage);
            _upImage = content.Load<Graphics.TextureResource>(upImage);

            if (string.IsNullOrEmpty(disabledImage) == false)
                _disabledImage = content.Load<Graphics.TextureResource>(disabledImage);

            if (string.IsNullOrEmpty(clickedSound) == false)
                _clickedSound = content.Load<Sound.Sound>(clickedSound);

            _tooltip = tooltip;
            if (tooltip != null)
            {
                _tooltipFont = content.Load<Gui.Font>("F_TOOLTIP.FON");
            }

            _container = container;
            _enabled = true;
        }

        public void OnMouseDown(int button)
        {
            if (button == 0)
            {
                if (IsMouseOverButton(_mouseX, _mouseY))
                {
                    _mouseDown = true;
                }
            }
        }

        public void OnMouseUp(int button)
        {
            if (button == 0)
            {
                if (_enabled && _mouseDown && IsMouseOverButton(_mouseX, _mouseY))
                {
                    if (_clickedSound != null)
                        _clickedSound.Play2D(Sound.SoundTrackChannel.UI);
                    
                    // clicked!
                    if (_onButtonClicked != null)
                        _onButtonClicked(this, new EventArgs());
                }

                _mouseDown = false;
            }
        }

        public void OnMouseMove(int ticks, int x, int y)
        {
            _timeAtLastMouseMove = ticks;

            _mouseX = x;
            _mouseY = y;

            if (IsMouseOverButton(x, y) == false)
                _tooltipVisible = false;
        }

        public void Render(Graphics.SpriteBatch sb, int tickCount)
        {
            if (_enabled)
            {
                if (_mouseDown)
                    sb.Draw(_downImage, new Math.Vector2(_screenX, _screenY));
                else if (IsMouseOverButton(_mouseX, _mouseY))
                    sb.Draw(_hoverImage, new Math.Vector2(_screenX, _screenY));
                else
                    sb.Draw(_upImage, new Math.Vector2(_screenX, _screenY));

                if (_tooltipVisible == false && tickCount > _timeAtLastMouseMove + 500 && IsMouseOverButton(_mouseX, _mouseY))
                    _tooltipVisible = true;
            }
            else
                sb.Draw(_disabledImage, new Math.Vector2(_screenX, _screenY));

            if (_tooltipVisible && _tooltipFont != null)
            {
                Graphics.Rect tooltipRect = _tooltipFont.GetPrintedRect(_tooltip);
                tooltipRect.X = _mouseX - 2;
                tooltipRect.Y = _mouseY + 32;
                tooltipRect.Width += 4;

                Graphics.TextureResource defaultWhite = Graphics.RendererManager.CurrentRenderer.DefaultTexture;
                sb.Draw(defaultWhite, tooltipRect, null, 0);

                _tooltipFont.Print(sb, _mouseX, _mouseY + 32, _tooltip);
            }
        }

        public Unit X
        {
            get { return _x; }
            set { _x = value; CalculateScreenCoordinates(); }
        }

        public Unit Y
        {
            get { return _y; }
            set { _y = value; CalculateScreenCoordinates(); }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public event EventHandler OnClick
        {
            add { _onButtonClicked += value; }
            remove { _onButtonClicked -= value; }
        }
        
        public bool IsMouseOverButton(int mouseX, int mouseY)
        {
            return (mouseX >= _screenX && mouseX < _screenX + _upImage.Width &&
                mouseY >= _screenY && mouseY < _screenY + _upImage.Height);
        }

        internal void CalculateScreenCoordinates()
        {
            Graphics.Viewport vp = Graphics.RendererManager.CurrentRenderer.Viewport;

            if (_container != null)
            {
                _screenX = (int)(vp.X + (_x.Scale + _container.X.Scale) * vp.Width + _x.Offset + _container.X.Offset);
                _screenY = (int)(vp.Y + (_y.Scale + _container.Y.Scale) * vp.Height + _y.Offset + _container.Y.Offset);
            }
            else
            {
                _screenX = (int)(vp.X + _x.Scale * vp.Width + _x.Offset);
                _screenY = (int)(vp.Y + _y.Scale * vp.Height + _y.Offset);
            }
        }

        
        private bool _mouseDown;
        private int _mouseX, _mouseY;

        private int _screenX, _screenY;
        private Unit _x, _y;
        private bool _enabled;

        private IButtonContainer _container;
        private Gui.Font _tooltipFont;
        private Graphics.TextureResource _downImage;
        private Graphics.TextureResource _hoverImage;
        private Graphics.TextureResource _upImage;
        private Graphics.TextureResource _disabledImage;
        private Sound.Sound _clickedSound;
        private string _tooltip;
        private int _timeAtLastMouseMove;
        private bool _tooltipVisible;

        private EventHandler _onButtonClicked;
    }

    public interface IButtonContainer
    {
        Unit X { get; }
        Unit Y { get; }
    }
}
