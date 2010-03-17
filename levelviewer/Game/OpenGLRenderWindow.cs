using System;
using Tao.Sdl;

namespace Game
{
    class OpenGLRenderWindow : Gk3Main.Graphics.OpenGLRenderWindow
    {
        int _width, _height, _depth;
        bool _fullscreen;
        Gk3Main.Graphics.OpenGLRenderer _renderer;

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
            Sdl.SDL_WM_SetCaption("FreeGeeKayThree", "FreeGK3");

            _renderer = new Gk3Main.Graphics.OpenGLRenderer();

            return _renderer;
        }

        public override void Present()
        {
            Sdl.SDL_GL_SwapBuffers();
        }
    }
}
