using System;
using System.Collections.Generic;
using System.Text;

using Tao.Sdl;
using Tao.OpenGl;

namespace gk3levelviewer.Graphics
{
    static class Video
    {
        public static void SetScreenMode(int width, int height, int depth, bool fullscreen)
        {
            int flags = Sdl.SDL_OPENGL;

            Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO | Sdl.SDL_INIT_NOPARACHUTE);

            if (depth == 16)
            {
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 5);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 6);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 5);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, 16);
            }
            else if (depth == 24 || depth == 32)
            {
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 8);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 8);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 8);
                Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, depth);
            }

            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);

            if (fullscreen) flags |= Sdl.SDL_FULLSCREEN;
            Sdl.SDL_SetVideoMode(width, height, depth, flags);

            setupOpenGl(width, height);
        }

        public static void Present()
        {
            Sdl.SDL_GL_SwapBuffers();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
        }

        private static void setupOpenGl(int width, int height)
        {
            #region Perspective view setup
            float ratio = (float)width / height;
            Gl.glViewport(0, 0, width, height);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(60.0f, ratio, 0.5f, 5000.0f);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Glu.gluLookAt(0, 0, 0, 0, 0, 1.0f, 0, 1.0f, 0);
            #endregion

            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_ALPHA_TEST);
            Gl.glAlphaFunc(Gl.GL_LESS, 0.1f);

            Tao.OpenGl.GlExtensionLoader.LoadAllExtensions();
        }
    }
}
