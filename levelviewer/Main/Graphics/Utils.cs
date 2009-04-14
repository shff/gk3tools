﻿using System;
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
        public static void Blit(float x, float y, TextureResource texture)
        {
            bool wasIn2D = _in2D;
            if (_in2D == false) Go2D();

            texture.Bind();

            float u = 0;
            float v = 0;
            float uw = (float)texture.Width / texture.ActualPixelWidth;
            float vw = (float)texture.Height / texture.ActualPixelHeight;

            Gl.glBegin(Gl.GL_QUADS);

            Gl.glTexCoord2f(u, v);
            Gl.glVertex3f(x, y, 0);

            Gl.glTexCoord2f(u + uw, v);
            Gl.glVertex3f(x + texture.Width, y, 0);

            Gl.glTexCoord2f(u + uw, v + vw);
            Gl.glVertex3f(x + texture.Width, y + texture.Height, 0);

            Gl.glTexCoord2f(u, v + vw);
            Gl.glVertex3f(x, y + texture.Height, 0);

            Gl.glEnd();

            if (wasIn2D == false) End2D();
        }

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


        public static void ScaleBlit(Rect dest, TextureResource texture, Rect src)
        {
            bool wasIn2D = _in2D;
            if (_in2D == false) Go2D();

            texture.Bind();

            float u = src.X * texture.Width / texture.ActualPixelWidth;
            float v = src.Y * texture.Height / texture.ActualPixelHeight;
            float uw = src.Width * texture.Width / texture.ActualPixelWidth;
            float vw = src.Height * texture.Height / texture.ActualPixelHeight;

            // this keeps everything at a 4:3 ratio, even if it isn't IRL
            float screenWidth = (_viewport[3] * 4) / 3;
            float widescreenOffset = (_viewport[2] - screenWidth) / 2;

            float x1 = widescreenOffset + _viewport[0] + dest.X * screenWidth;
            float y1 = _viewport[1] + dest.Y * _viewport[3];
            float x2 = x1 + dest.Width * screenWidth;
            float y2 = y1 + dest.Height * _viewport[3];

            Gl.glBegin(Gl.GL_QUADS);

            Gl.glTexCoord2f(u, v);
            Gl.glVertex3f(x1, y1, 0);

            Gl.glTexCoord2f(u + uw, v);
            Gl.glVertex3f(x2, y1, 0);

            Gl.glTexCoord2f(u + uw, v + vw);
            Gl.glVertex3f(x2, y2, 0);

            Gl.glTexCoord2f(u, v + vw);
            Gl.glVertex3f(x1, y2, 0);

            Gl.glEnd();

            if (wasIn2D == false) End2D();
        }

        public static void Go2D()
        {
            if (_in2D) return;

            Gl.glPushAttrib(Gl.GL_ENABLE_BIT);
            IRenderer renderer = RendererManager.CurrentRenderer;
            renderer.BlendEnabled = true;
            renderer.AlphaTestEnabled = false;
            renderer.DepthTestEnabled = false;

            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

            if (_viewportInitialized == false)
            {
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, _viewport);
                _viewportInitialized = true;
            }

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();

            Gl.glOrtho(_viewport[0], _viewport[2], _viewport[3], _viewport[1], -1.0f, 1.0f);

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

        public static int[] Viewport
        {
            get
            {
                if (_viewportInitialized == false)
                {
                    Gl.glGetIntegerv(Gl.GL_VIEWPORT, _viewport);
                    _viewportInitialized = true;
                }

                return _viewport;
            }
        }

        private static bool _viewportInitialized;
        private static int[] _viewport = new int[4];
        private static bool _in2D;
    }
}
