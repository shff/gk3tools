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
            Gl.glViewport(0, 0, width, height);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(60.0f, ratio, 1.0f, 5000.0f);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Glu.gluLookAt(0, 0, 0, 0, 0, 1.0f, 0, 1.0f, 0);
            #endregion

            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_ALPHA_TEST);
            Gl.glAlphaFunc(Gl.GL_GREATER, 0.9f);

            Gl.glEnable(Gl.GL_CULL_FACE);
            Gl.glFrontFace(Gl.GL_CW);
            Gl.glCullFace(Gl.GL_BACK);
        }

        public static void SaveScreenshot(string filename)
        {
            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            byte[] pixels = new byte[viewport[2] * viewport[3] * 3];
            byte[] paddingArray = {0,0,0,0};
            
            Gl.glPixelStorei(Gl.GL_PACK_ALIGNMENT, 1);
            Gl.glReadPixels(viewport[0], viewport[1], viewport[2], viewport[3], Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, pixels);

            // now write the bitmap
            int padding = (viewport[2] * 3 % 4 == 0 ? 0 : 4 - viewport[2] * 3 % 4);
            int rowsize = viewport[2] * 3 + padding;
            using (System.IO.FileStream bitmap = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            {
                System.IO.BinaryWriter writer = new System.IO.BinaryWriter(bitmap);

                writer.Write((ushort)0x4d42);
                writer.Write((uint)(rowsize * viewport[3] + 54));
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((uint)54);

                writer.Write((uint)40);
                writer.Write((uint)viewport[2]);
                writer.Write((uint)viewport[3]);
                writer.Write((ushort)1);
                writer.Write((ushort)24);
                writer.Write((uint)0);
                writer.Write((uint)(rowsize * viewport[3]));
                writer.Write((uint)0);
                writer.Write((uint)0);
                writer.Write((uint)0);
                writer.Write((uint)0);

                // now write the pixels
                for (int i = 0; i < viewport[3]; i++)
                {
                    // swap B and R
                    for (int j = 0; j < viewport[2]; j++)
                    {
                        byte pixel = pixels[(i * viewport[2] + j) * 3 + 0];
                        pixels[(i * viewport[2] + j) * 3 + 0] = pixels[(i * viewport[2] + j) * 3 + 2];
                        pixels[(i * viewport[2] + j) * 3 + 2] = pixel;
                    }

                    writer.Write(pixels, i * viewport[2] * 3, viewport[2] * 3);

                    // add padding
                    writer.Write(paddingArray, 0, padding);
                }
            }
        }
    }
}
