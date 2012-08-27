using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;

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

        public override Gk3Main.Graphics.IRenderer Renderer
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

        public override void Resize(int width, int height)
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

    class OpenGLRenderWindow : Gk3Main.Graphics.RenderWindow
    {
        private Direct3D9RenderControl _renderWindow;
        private Gk3Main.Graphics.IRenderer _renderer;
        private IntPtr _hdc;

        public OpenGLRenderWindow(Direct3D9RenderControl renderWindow)
        {
            _renderWindow = renderWindow;
        }

        public override Gk3Main.Graphics.IRenderer Renderer
        {
            get { return _renderer; }
        }

        public override void Present()
        {
            SwapBuffers(_hdc);
        }

        public override Gk3Main.Graphics.IRenderer CreateRenderer()
        {
            _hdc = GetDC(_renderWindow.Handle);
            setPixelFormat(_hdc);

            _renderer = new Gk3Main.Graphics.OpenGl.OpenGLRenderer(this);

            return _renderer;
        }

        public override List<Gk3Main.Graphics.DisplayMode> GetSupportedDisplayModes()
        {
            // we have no reason to support this since nothing should ever call this
            return null;
        }

        public override void Resize(int width, int height)
        {
            //_renderer.Viewport = new Gk3Main.Graphics.Viewport(0, 0, width, height);
        }

        private static void setPixelFormat(IntPtr hdc)
        {
            PIXELFORMATDESCRIPTOR format = new PIXELFORMATDESCRIPTOR();
            format.nSize = (ushort)Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR));
            format.nVersion = 1;
            format.dwFlags = PFD_DOUBLEBUFFER | PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL;
            format.PixelType = 24;
            format.cDepthBits = 32;

            int pixelFormat = ChoosePixelFormat(hdc, ref format);
            SetPixelFormat(hdc, pixelFormat, ref format);

            IntPtr context = wglCreateContext(hdc);
            wglMakeCurrent(hdc, context);
        }

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [System.Runtime.InteropServices.DllImport("OpenGL32")]
        private static extern int wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

        [System.Runtime.InteropServices.DllImport("OpenGL32")]
        private static extern IntPtr wglCreateContext(IntPtr hdc);

        private struct PIXELFORMATDESCRIPTOR
        {
            public UInt16 nSize;
            public UInt16 nVersion;
            public UInt32 dwFlags;
            public byte PixelType;
            public byte cColorBits;
            public byte cRedBits;
            public byte cRedShift;
            public byte cGreenBits;
            public byte cGreenShift;
            public byte cBlueBits;
            public byte cBlueShift;
            public byte cAlphaBits;
            public byte cAlphaShift;
            public byte cAccumBits;
            public byte cAccumRedBits;
            public byte cAccumGreenBits;
            public byte cAccumBlueBits;
            public byte cAccumAlphaBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public byte iLayerType;
            public byte bReserved;
            public UInt32 dwLayerMask;
            public UInt32 dwVisibleMask;
            public UInt32 dwDamageMask;
        }

        const int PFD_DOUBLEBUFFER = 0x01;
        const int PFD_DRAW_TO_WINDOW = 0x04;
        const int PFD_SUPPORT_OPENGL = 0x20;

        [DllImport("gdi32.dll")]
        private static extern int SetPixelFormat(IntPtr hDC, int n,
            [MarshalAs(UnmanagedType.Struct)] ref PIXELFORMATDESCRIPTOR pcPixelFormatDescriptor);

        [DllImport("gdi32.dll")]
        private static extern int ChoosePixelFormat(IntPtr hDC,
            [MarshalAs(UnmanagedType.Struct)] ref PIXELFORMATDESCRIPTOR pPixelFormatDescriptor);

        [DllImport("gdi32.dll")]
        private static extern int SwapBuffers(IntPtr hDC);
    }
}
