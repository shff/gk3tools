using System;
using Tao.Sdl;

namespace Game
{
    class Direct3D9RenderWindow : Gk3Main.Graphics.Direct3D9RenderWindow
    {
        int _width, _height, _depth;
        bool _fullscreen;
        
        Gk3Main.Graphics.Direct3D9Renderer _renderer;


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

            Sdl.SDL_SetVideoMode(_width, _height, _depth, (_fullscreen ? Sdl.SDL_FULLSCREEN : 0));
            Sdl.SDL_WM_SetCaption("FreeGeeKayThree - Direct3D 9 Renderer", "FreeGK3");

            SDL_SysWMinfo wmInfo;
            SDL_GetWMInfo(out wmInfo);

            _renderer = new Gk3Main.Graphics.Direct3D9Renderer(wmInfo.window, _width, _height);

            return _renderer;
        }

        public override void Present()
        {
            _renderer.Present();
        }

        private struct SDL_SysWMinfo
        {
            public Sdl.SDL_version version;
            public IntPtr window;
            public IntPtr hglrc;
        }

        [System.Runtime.InteropServices.DllImport("SDL")]
        private extern static int SDL_GetWMInfo(out SDL_SysWMinfo info);
    }
}
