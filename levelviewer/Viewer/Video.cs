using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using Tao.OpenGl;

namespace Viewer
{
    static class Video
    {
        public static void Init(int width, int height)
        {
            #region Perspective view setup
            float ratio = (float)width / height;
            Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport
                = new Gk3Main.Graphics.Viewport(0, 0, width, height);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(60.0f, ratio, 1.0f, 5000.0f);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glScalef(1.0f, 1.0f, -1.0f);
            Glu.gluLookAt(0, 0, 0, 0, 0, 1.0f, 0, 1.0f, 0);
            #endregion

            Gk3Main.Graphics.RendererManager.CurrentRenderer.DepthTestEnabled = true;
            Gk3Main.Graphics.RendererManager.CurrentRenderer.AlphaTestEnabled = true;

            Gl.glAlphaFunc(Gl.GL_GREATER, 0.9f);

            Gk3Main.Graphics.RendererManager.CurrentRenderer.CullMode = Gk3Main.Graphics.CullMode.CounterClockwise;
        }

        public static void SaveScreenshot(string filename)
        {
            Gk3Main.Graphics.Viewport viewport = Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport;

            byte[] pixels = new byte[viewport.Width * viewport.Height * 3];
            byte[] paddingArray = {0,0,0,0};
            
            Gl.glPixelStorei(Gl.GL_PACK_ALIGNMENT, 1);
            Gl.glReadPixels(viewport.X, viewport.Y, viewport.Width, viewport.Height, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, pixels);

            // now write the bitmap
            int padding = (viewport.Width * 3 % 4 == 0 ? 0 : 4 - viewport.Width * 3 % 4);
            int rowsize = viewport.Width * 3 + padding;
            using (System.IO.FileStream bitmap = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            {
                System.IO.BinaryWriter writer = new System.IO.BinaryWriter(bitmap);

                writer.Write((ushort)0x4d42);
                writer.Write((uint)(rowsize * viewport.Height + 54));
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((uint)54);

                writer.Write((uint)40);
                writer.Write((uint)viewport.Width);
                writer.Write((uint)viewport.Height);
                writer.Write((ushort)1);
                writer.Write((ushort)24);
                writer.Write((uint)0);
                writer.Write((uint)(rowsize * viewport.Height));
                writer.Write((uint)0);
                writer.Write((uint)0);
                writer.Write((uint)0);
                writer.Write((uint)0);

                // now write the pixels
                for (int i = 0; i < viewport.Height; i++)
                {
                    // swap B and R
                    for (int j = 0; j < viewport.Width; j++)
                    {
                        byte pixel = pixels[(i * viewport.Width + j) * 3 + 0];
                        pixels[(i * viewport.Width + j) * 3 + 0] = pixels[(i * viewport.Width + j) * 3 + 2];
                        pixels[(i * viewport.Width + j) * 3 + 2] = pixel;
                    }

                    writer.Write(pixels, i * viewport.Width * 3, viewport.Width * 3);

                    // add padding
                    writer.Write(paddingArray, 0, padding);
                }
            }
        }
    }
}
