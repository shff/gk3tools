using System;
using System.Collections.Generic;
using Tao.Sdl;

namespace Game
{
    class OpenGLRenderWindow : Gk3Main.Graphics.OpenGLRenderWindow
    {
        int _width, _height, _depth;
        bool _fullscreen;
        Gk3Main.Graphics.OpenGl.OpenGLRenderer _renderer;

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

            if (_depth == 16)
            {
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 5);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 6);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 5);
            }
            else
            {
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 8);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 8);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 8);
            }
            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, 24);
            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);

            Sdl.SDL_SetVideoMode(_width, _height, _depth, Sdl.SDL_OPENGL | (_fullscreen ? Sdl.SDL_FULLSCREEN : 0));
            Sdl.SDL_WM_SetCaption("FreeGeeKayThree - OpenGL 3.0 renderer", "FreeGK3");

            SDL_SysWMInfo info;
            SDL_GetWMInfo(out info);
            IntPtr hdc = GetDC(info.window);

            PFNWGLCREATECONTEXTATTRIBSARBPROC wglCreateContextAttribsARB = (PFNWGLCREATECONTEXTATTRIBSARBPROC)Tao.OpenGl.Gl.GetDelegate("wglCreateContextAttribsARB", typeof(PFNWGLCREATECONTEXTATTRIBSARBPROC));
            if (wglCreateContextAttribsARB == null)
                throw new InvalidOperationException("OpenGL 3.x doesn't seem to be supported");

            int[] attribsList = new int[] {
                WGL_CONTEXT_MAJOR_VERSION_ARB, 3,
                WGL_CONTEXT_MINOR_VERSION_ARB, 0,
                WGL_CONTEXT_FLAGS_ARB, WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB,
                0
            };

            IntPtr context = wglCreateContextAttribsARB(hdc, IntPtr.Zero, attribsList);
            wglMakeCurrent(hdc, context);
            wglDeleteContext(info.hglrc);

            _renderer = new Gk3Main.Graphics.OpenGl.OpenGLRenderer(this);

            return _renderer;
        }

        public override void Present()
        {
            Sdl.SDL_GL_SwapBuffers();
        }

        public override List<Gk3Main.Graphics.DisplayMode> GetSupportedDisplayModes()
        {
            List<Gk3Main.Graphics.DisplayMode> results = new List<Gk3Main.Graphics.DisplayMode>();

            Sdl.SDL_Rect[] modes = Sdl.SDL_ListModes(IntPtr.Zero, Sdl.SDL_HWSURFACE | Sdl.SDL_FULLSCREEN);
            foreach (Sdl.SDL_Rect r in modes)
            {
                results.Add(new Gk3Main.Graphics.DisplayMode(r.w, r.h));
            }

            return results;
        }


        #region Interop stuff

        // TODO: make this interop stuff support platforms besides Windows

        private struct SDL_SysWMInfo
        {
            public Sdl.SDL_version version;
            public IntPtr window;
            public IntPtr hglrc;
        }

        [System.Runtime.InteropServices.DllImport("sdl")]
        private static extern int SDL_GetWMInfo(out SDL_SysWMInfo info);

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [System.Runtime.InteropServices.DllImport("OpenGL32")]
        private static extern int wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

        [System.Runtime.InteropServices.DllImport("OpenGL32")]
        private static extern int wglDeleteContext(IntPtr hglrc);

        private delegate IntPtr PFNWGLCREATECONTEXTATTRIBSARBPROC(IntPtr hdc, IntPtr i, int[] flags);

        private const int WGL_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
        private const int WGL_CONTEXT_MINOR_VERSION_ARB = 0x2092;
        private const int WGL_CONTEXT_FLAGS_ARB = 0x2094;
        private const int WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB = 0x002;

        #endregion
    }
}
