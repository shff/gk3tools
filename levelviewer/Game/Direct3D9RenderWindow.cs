using System;
using System.Collections.Generic;
using Tao.Sdl;

namespace Game
{
#if !D3D_DISABLED
    class Direct3D9RenderWindow : Gk3Main.Graphics.Direct3D9RenderWindow
    {
        int _width, _height, _depth;
        bool _fullscreen;
        
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

            Sdl.SDL_SetVideoMode(_width, _height, _depth, (_fullscreen ? Sdl.SDL_FULLSCREEN : 0));
            Sdl.SDL_WM_SetCaption("FreeGeeKayThree - Direct3D 9 Renderer", "FreeGK3");

            SDL_SysWMinfo wmInfo;
            SDL_GetWMInfo(out wmInfo);

            _renderer = new Gk3Main.Graphics.Direct3D9.Direct3D9Renderer(this, wmInfo.window, _width, _height, false);

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

        public override List<Gk3Main.Graphics.DisplayMode> GetSupportedDisplayModes()
        {
            List<Gk3Main.Graphics.DisplayMode> results = new List<Gk3Main.Graphics.DisplayMode>();

            // we're relying on SDL for this instead of directly asking Direct3D.
            // not sure if that will always work in every case...
            Sdl.SDL_Rect[] modes = Sdl.SDL_ListModes(IntPtr.Zero, Sdl.SDL_HWSURFACE | Sdl.SDL_FULLSCREEN);
            foreach (Sdl.SDL_Rect r in modes)
            {
                results.Add(new Gk3Main.Graphics.DisplayMode(r.w, r.h));
            }

            return results;
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
#endif
}
