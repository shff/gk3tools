using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;

namespace Gk3Main.Graphics
{
    public struct Rect
    {
        public float X, Y, Width, Height;
    }

    public class Utils
    {
        public static void Blit(float x, float y, TextureResource texture, Rect src)
        {
            bool wasIn2D = _in2D;
            if (_in2D == false) Go2D();

            texture.Bind();

            float u = src.X / texture.ActualPixelWidth;
            float v = src.Y / texture.ActualPixelHeight;
            float uw = src.Width / texture.ActualPixelWidth;
            float vw = src.Height / texture.ActualPixelHeight;

            Gl.glBegin(Gl.GL_QUADS);

            Gl.glTexCoord2f(u, v);
            Gl.glVertex3f(x, y, 0);

            Gl.glTexCoord2f(u + uw, v);
            Gl.glVertex3f(x + src.Width, y, 0);

            Gl.glTexCoord2f(u + uw, v + vw);
            Gl.glVertex3f(x + src.Width, y + src.Height, 0);

            Gl.glTexCoord2f(u, v + vw);
            Gl.glVertex3f(x, y + src.Height, 0);

            Gl.glEnd();

            if (wasIn2D == false) End2D();
        }

        public static void Go2D()
        {
            if (_in2D) return;

            Gl.glPushAttrib(Gl.GL_ENABLE_BIT);
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glDisable(Gl.GL_ALPHA_TEST);
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();

            Gl.glOrtho(viewport[0], viewport[2], viewport[3], viewport[1], -1.0f, 1.0f);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            
            _in2D = true;
        }

        public static void End2D()
        {
            if (!_in2D) return;

            Gl.glPopAttrib();
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);

            Gl.glPopAttrib();

            _in2D = false;
        }
            

        private static bool _in2D;
    }
}
