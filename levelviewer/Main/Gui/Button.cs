using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class Button : IDisposable
    {
        public Button(string downImage, string hoverImage, string upImage, string disabledImage)
        {
            _downImage = (Graphics.TextureResource)Resource.ResourceManager.Load(downImage);
            _hoverImage = (Graphics.TextureResource)Resource.ResourceManager.Load(hoverImage);
            _upImage = (Graphics.TextureResource)Resource.ResourceManager.Load(upImage);
            _disabledImage = (Graphics.TextureResource)Resource.ResourceManager.Load(disabledImage);
            _wooshSound = (Sound.Sound)Resource.ResourceManager.Load("SIDBUTN-1.WAV");

            _enabled = true;
        }

        public void Dispose()
        {
            if (_downImage != null) Resource.ResourceManager.Unload(_downImage);
            if (_hoverImage != null) Resource.ResourceManager.Unload(_hoverImage);
            if (_upImage != null) Resource.ResourceManager.Unload(_upImage);
            if (_disabledImage != null) Resource.ResourceManager.Unload(_disabledImage);
            if (_wooshSound != null) Resource.ResourceManager.Unload(_wooshSound);
        }

        public void SetMousePosition(int x, int y)
        {
            _mouseX = x;
            _mouseY = y;
        }

        public void OnMouseDown(int button)
        {
            if (button == 0)
            {
                if (isMouseOverButton())
                {
                    _mouseDown = true;
                }
            }
        }

        public void OnMouseUp(int button)
        {
            if (button == 0)
            {
                if (_enabled && _mouseDown && isMouseOverButton())
                {
                    _wooshSound.Play2D();
                    
                    // clicked!
                    if (_onButtonClicked != null)
                        _onButtonClicked(this, new EventArgs());
                }

                _mouseDown = false;
            }
        }

        public void Render()
        {
            if (_enabled)
            {
                if (_mouseDown)
                    Graphics.Utils.Blit(_screenX, _screenY, _downImage);
                else if (isMouseOverButton())
                    Graphics.Utils.Blit(_screenX, _screenY, _hoverImage);
                else
                    Graphics.Utils.Blit(_screenX, _screenY, _upImage);
            }
            else
                Graphics.Utils.Blit(_screenX, _screenY, _disabledImage);
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

        private void calculateScreenCoordinates()
        {
            int[] viewport = Graphics.Utils.Viewport;

            _screenX = (int)(viewport[0] + _x.Scale * viewport[2] + _x.Offset);
            _screenY = (int)(viewport[1] + _y.Scale * viewport[3] + _y.Offset);
        }

        private bool isMouseOverButton()
        {
            return (_mouseX >= _screenX && _mouseX < _screenX + _upImage.Width &&
                _mouseY >= _screenY && _mouseY < _screenY + _upImage.Height);
        }

        private bool _mouseDown;
        private int _mouseX, _mouseY;

        private int _screenX, _screenY;
        private Unit _x, _y;
        private bool _enabled;

        private Graphics.TextureResource _downImage;
        private Graphics.TextureResource _hoverImage;
        private Graphics.TextureResource _upImage;
        private Graphics.TextureResource _disabledImage;
        private Sound.Sound _wooshSound;

        private EventHandler _onButtonClicked;
    }
}
