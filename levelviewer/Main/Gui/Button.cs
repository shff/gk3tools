using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class Button : IDisposable
    {
        public Button(string downImage, string hoverImage, string upImage, string disabledImage, string clickedSound)
        {
            _downImage = (Graphics.TextureResource)Resource.ResourceManager.Load(downImage);
            _hoverImage = (Graphics.TextureResource)Resource.ResourceManager.Load(hoverImage);
            _upImage = (Graphics.TextureResource)Resource.ResourceManager.Load(upImage);
            _disabledImage = (Graphics.TextureResource)Resource.ResourceManager.Load(disabledImage);

            if (string.IsNullOrEmpty(clickedSound) == false)
                _clickedSound = (Sound.Sound)Resource.ResourceManager.Load(clickedSound);

            _enabled = true;
        }

        public Button(string downImage, string hoverImage, string upImage, string disabledImage, string clickedSound, string tooltip)
        {
            _downImage = (Graphics.TextureResource)Resource.ResourceManager.Load(downImage);
            _hoverImage = (Graphics.TextureResource)Resource.ResourceManager.Load(hoverImage);
            _upImage = (Graphics.TextureResource)Resource.ResourceManager.Load(upImage);
            _disabledImage = (Graphics.TextureResource)Resource.ResourceManager.Load(disabledImage);

            if (string.IsNullOrEmpty(clickedSound) == false)
                _clickedSound = (Sound.Sound)Resource.ResourceManager.Load(clickedSound);

            _tooltip = tooltip;
            if (tooltip != null)
            {
                _tooltipFont = (Gui.Font)Resource.ResourceManager.Load("F_TOOLTIP.FON");
            }

            _enabled = true;
        }

        public void Dispose()
        {
            if (_downImage != null) Resource.ResourceManager.Unload(_downImage);
            if (_hoverImage != null) Resource.ResourceManager.Unload(_hoverImage);
            if (_upImage != null) Resource.ResourceManager.Unload(_upImage);
            if (_disabledImage != null) Resource.ResourceManager.Unload(_disabledImage);
            if (_clickedSound != null) Resource.ResourceManager.Unload(_clickedSound);
            if (_tooltipFont != null) Resource.ResourceManager.Unload(_tooltipFont);
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
                        _clickedSound.Play2D();
                    
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

        public void Render(int tickCount)
        {
            Graphics.Utils.Go2D();

            if (_enabled)
            {
                if (_mouseDown)
                    Graphics.Utils.Blit(_screenX, _screenY, _downImage);
                else if (IsMouseOverButton(_mouseX, _mouseY))
                    Graphics.Utils.Blit(_screenX, _screenY, _hoverImage);
                else
                    Graphics.Utils.Blit(_screenX, _screenY, _upImage);

                if (_tooltipVisible == false && tickCount > _timeAtLastMouseMove + 500 && IsMouseOverButton(_mouseX, _mouseY))
                    _tooltipVisible = true;
            }
            else
                Graphics.Utils.Blit(_screenX, _screenY, _disabledImage);

            Graphics.Utils.End2D();


            if (_tooltipVisible && _tooltipFont != null)
            {
                Graphics.Rect tooltipRect = _tooltipFont.GetPrintedRect(_tooltip);
                tooltipRect.X = _mouseX - 2;
                tooltipRect.Y = _mouseY + 32;
                tooltipRect.Width += 4;

                Graphics.Utils.DrawRect(tooltipRect);

                _tooltipFont.Print(_mouseX, _mouseY + 32, _tooltip);
            }

            
        }

        public Unit X
        {
            get { return _x; }
            set { _x = value; calculateScreenCoordinates(); }
        }

        public Unit Y
        {
            get { return _y; }
            set { _y = value; calculateScreenCoordinates(); }
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
   

        private void calculateScreenCoordinates()
        {
            int[] viewport = Graphics.Utils.Viewport;

            _screenX = (int)(viewport[0] + _x.Scale * viewport[2] + _x.Offset);
            _screenY = (int)(viewport[1] + _y.Scale * viewport[3] + _y.Offset);
        }

        
        private bool _mouseDown;
        private int _mouseX, _mouseY;

        private int _screenX, _screenY;
        private Unit _x, _y;
        private bool _enabled;

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
}
