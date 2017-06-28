using System;
using System.Collections.Generic;


namespace Game
{
    class OpenGLRenderWindow : Gk3Main.Graphics.OpenGLRenderWindow
    {
        bool _closed;
        int _width, _height, _depth;
        bool _fullscreen;
        bool _screenshotRequested;
        string _screenshotName;
        Gk3Main.Graphics.OpenGl.OpenGLRenderer _renderer;
        OpenTK.NativeWindow _window;
        OpenTK.Graphics.GraphicsContext _context;

        public OpenGLRenderWindow(int width, int height, int depth, bool fullscreen)
        {
            _width = width;
            _height = height;
            _depth = depth;
            _fullscreen = fullscreen;
        }

        public override Gk3Main.Graphics.IRenderer CreateRenderer()
        {
            if (_renderer != null)
                throw new InvalidOperationException("A renderer has already been created");

            OpenTK.Graphics.GraphicsMode mode = new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(32), _depth, 0, 0);
            _window = new OpenTK.NativeWindow(_width, _height, "FreeGeeKayThree - OpenGL 3.3 renderer", _fullscreen ? OpenTK.GameWindowFlags.Fullscreen : OpenTK.GameWindowFlags.FixedWindow, mode, OpenTK.DisplayDevice.Default);
            _window.Visible = true;
            _window.Closed += (x,y) => _closed = true;

            _context = new OpenTK.Graphics.GraphicsContext(mode, _window.WindowInfo, 3, 3, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible | OpenTK.Graphics.GraphicsContextFlags.Debug);
            _context.MakeCurrent(_window.WindowInfo);
            _context.LoadAll();

            _renderer = new Gk3Main.Graphics.OpenGl.OpenGLRenderer(this);

            return _renderer;
        }

        public override Gk3Main.Graphics.IRenderer Renderer
        {
            get { return _renderer; }
        }

        public override void Present()
        {
            _context.SwapBuffers();
        }

        public override void Resize(int width, int height)
        {
            throw new NotImplementedException();
        }

        public void RequestScreenshot()
        {
            _screenshotRequested = true;
        }
        public override List<Gk3Main.Graphics.DisplayMode> GetSupportedDisplayModes()
        {
            List<Gk3Main.Graphics.DisplayMode> results = new List<Gk3Main.Graphics.DisplayMode>();

            foreach(var res in OpenTK.DisplayDevice.Default.AvailableResolutions)
            {
                if (results.Exists(x => x.Width == res.Width && x.Height == res.Height) == false)
                    results.Add(new Gk3Main.Graphics.DisplayMode(res.Width, res.Height));
            }

            return results;
        }

        public override bool ProcessEvents()
        {
            _window.ProcessEvents();

            return !_closed;
        }

        public override void Close()
        {
            _window.Close();
        }

        public override void GetPosition(out int x, out int y)
        {
            x = _window.X;
            y = _window.Y;
        }

        public override Gk3Main.Graphics.MouseState GetMouseState()
        {
            Gk3Main.Graphics.MouseState ms;

            var m = OpenTK.Input.Mouse.GetCursorState();

            var p = _window.PointToClient(new System.Drawing.Point(m.X, m.Y));

            ms.X = p.X;
            ms.Y = p.Y;
            ms.Wheel = m.ScrollWheelValue;

            ms.LeftButton = m.LeftButton == OpenTK.Input.ButtonState.Pressed;
            ms.MiddleButton = m.MiddleButton == OpenTK.Input.ButtonState.Pressed;
            ms.RightButton = m.RightButton == OpenTK.Input.ButtonState.Pressed;
           
            return ms;
        }

        public override IntPtr Handle
        {
            get
            {
                return _window.WindowInfo.Handle;
            }
        }


        #region Interop stuff

        // TODO: make this interop stuff support platforms besides Windows

       /* private struct SDL_SysWMInfo
        {
            public Sdl.SDL_version version;
            public IntPtr window;
            public IntPtr hglrc;
        }

        [System.Runtime.InteropServices.DllImport("sdl")]
        private static extern int SDL_GetWMInfo(out SDL_SysWMInfo info);*/

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [System.Runtime.InteropServices.DllImport("opengl32.dll")]
        public static extern IntPtr wglGetProcAddress(string name);

        [System.Runtime.InteropServices.DllImport("OpenGL32")]
        private static extern IntPtr wglGetCurrentContext();

        [System.Runtime.InteropServices.DllImport("OpenGL32")]
        private static extern int wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

        [System.Runtime.InteropServices.DllImport("OpenGL32")]
        private static extern int wglDeleteContext(IntPtr hglrc);

        private delegate IntPtr PFNWGLCREATECONTEXTATTRIBSARBPROC(IntPtr hdc, IntPtr i, int[] flags);

        private const int WGL_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
        private const int WGL_CONTEXT_MINOR_VERSION_ARB = 0x2092;
        private const int WGL_CONTEXT_FLAGS_ARB = 0x2094;
        private const int WGL_CONTEXT_DEBUG_BIT_ARB = 0x001;
        private const int WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB = 0x002;

        #endregion
    }
}
