using System;
using System.Collections.Generic;
using System.Text;
using Tao.OpenGl;

namespace Viewer2
{
    static class Video
    {
        public static void Init(int width, int height)
        {
            #region Perspective view setup
            float ratio = (float)width / height;
            Gl.glViewport(0, 0, width, height);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(60.0f, ratio, 1.0f, 5000.0f);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Glu.gluLookAt(0, 0, 0, 0, 0, 1.0f, 0, 1.0f, 0);
            #endregion

            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_ALPHA_TEST);
            Gl.glAlphaFunc(Gl.GL_LESS, 0.1f);

            Gl.glEnable(Gl.GL_CULL_FACE);
            Gl.glFrontFace(Gl.GL_CW);
            Gl.glCullFace(Gl.GL_BACK);
        }
    }
}
