using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Viewer
{
    class Direct3D9RenderWindow : Gk3Main.Graphics.RenderWindow
    {
        private Direct3D9RenderControl _renderWindow;
        private Gk3Main.Graphics.Direct3D9.Direct3D9Renderer _renderer;
        private int _maxWidth, _maxHeight;

        public Direct3D9RenderWindow(Direct3D9RenderControl renderWindow)
        {
            _renderWindow = renderWindow;
        }

        public Gk3Main.Graphics.IRenderer Renderer
        {
            get { return _renderer; }
        }

        public override void Present()
        {
            _renderer.Present();
        }

        public override Gk3Main.Graphics.IRenderer CreateRenderer()
        {
            _maxWidth = Screen.PrimaryScreen.Bounds.Width;
            _maxHeight = Screen.PrimaryScreen.Bounds.Height;

            _renderer = new Gk3Main.Graphics.Direct3D9.Direct3D9Renderer(this, _renderWindow.Handle, _maxWidth, _maxHeight, true);

            return _renderer;
        }

        public override List<Gk3Main.Graphics.DisplayMode> GetSupportedDisplayModes()
        {
            // we have no reason to support this since nothing should ever call this
            return null;
        }

        public void Resize(int width, int height)
        {
            width = Math.Min(width, _maxWidth);
            height = Math.Min(height, _maxHeight);

            //_renderer.Viewport = new Gk3Main.Graphics.Viewport(0, 0, width, height);
        }
    }

    class Direct3D9RenderControl : Control
    {
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // nothing
        }
    }
}
