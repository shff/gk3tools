using System;
using System.Collections.Generic;
using Gk3Main.Graphics;

namespace Game
{
#if !D3D_DISABLED
    class Direct3D9RenderWindow : Gk3Main.Graphics.Direct3D9RenderWindow
    {
        OpenTK.NativeWindow _window;
        int _width, _height, _depth;
        bool _fullscreen;
        bool _closed;
        
        Gk3Main.Graphics.Direct3D9.Direct3D9Renderer _renderer;


        public Direct3D9RenderWindow(int width, int height, int depth, bool fullscreen)
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
            _window = new OpenTK.NativeWindow(_width, _height, "FreeGeeKayThree - Direct3D 9 renderer", _fullscreen ? OpenTK.GameWindowFlags.Fullscreen : OpenTK.GameWindowFlags.FixedWindow, mode, OpenTK.DisplayDevice.Default);
            _window.Visible = true;
            _window.Closed += (x, y) => _closed = true;

           // _window = SDL2.SDL.SDL_CreateWindow("FreeGeeKayThree - Direct3D 9 Renderer", SDL2.SDL.SDL_WINDOWPOS_CENTERED, SDL2.SDL.SDL_WINDOWPOS_CENTERED, _width, _height, _fullscreen ? SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN : 0);

            //Sdl.SDL_SetVideoMode(_width, _height, _depth, (_fullscreen ? Sdl.SDL_FULLSCREEN : 0));
            // Sdl.SDL_WM_SetCaption("FreeGeeKayThree - Direct3D 9 Renderer", "FreeGK3");

            // SDL_SysWMinfo wmInfo;
            // SDL_GetWMInfo(out wmInfo);

            _renderer = new Gk3Main.Graphics.Direct3D9.Direct3D9Renderer(this, Handle, _width, _height, false);

            return _renderer;
        }

        public override Gk3Main.Graphics.IRenderer Renderer
        {
            get { return _renderer; }
        }

        public override void Present()
        {
            _renderer.Present();
        }

        public override void Resize(int width, int height)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            _window.Close();
        }

        public override List<Gk3Main.Graphics.DisplayMode> GetSupportedDisplayModes()
        {
            List<Gk3Main.Graphics.DisplayMode> results = new List<Gk3Main.Graphics.DisplayMode>();

            foreach (var res in OpenTK.DisplayDevice.Default.AvailableResolutions)
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
                //SDL2.SDL.SDL_SysWMinfo wmInfo = new SDL2.SDL.SDL_SysWMinfo();
               // SDL2.SDL.SDL_VERSION(out wmInfo.version);
               // SDL2.SDL.SDL_GetWindowWMInfo(_window, ref wmInfo);

                //return wmInfo.info.win.window;
            }
        }
/* 
        private struct SDL_SysWMinfo
        {
            public Sdl.SDL_version version;
            public IntPtr window;
            public IntPtr hglrc;
        }

        [System.Runtime.InteropServices.DllImport("SDL")]
        private extern static int SDL_GetWMInfo(out SDL_SysWMinfo info);*/
    }
#endif
}
