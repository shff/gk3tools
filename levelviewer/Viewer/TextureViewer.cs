using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Viewer
{
    public partial class TextureViewer : Form
    {
        public TextureViewer()
        {
            InitializeComponent();
        }

        public void DisplayGk3Bitmap(string name)
        {
            System.IO.Stream file = Gk3Main.FileSystem.Open(name);
            Gk3Main.Graphics.BitmapSurface bmp = new Gk3Main.Graphics.BitmapSurface(file);
            file.Dispose();

            System.Runtime.InteropServices.GCHandle pixels = System.Runtime.InteropServices.GCHandle.Alloc(bmp.Pixels, System.Runtime.InteropServices.GCHandleType.Pinned);
            Bitmap img = new Bitmap(bmp.Width, bmp.Height, bmp.Width * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb, pixels.AddrOfPinnedObject());
            pixels.Free();

            pictureBox1.Image = swapRedAndBlueChannels(img);

            img.Dispose();
        }

        public void DisplaySurfaceLightmap(Gk3Main.Graphics.BspResource bsp, Gk3Main.Graphics.LightmapResource lightmaps, int surfaceIndex)
        {
            // find the extends of the surface
            float minU = float.MaxValue, minV = float.MaxValue, maxU = float.MinValue, maxV = float.MinValue;

            for (int i = 0; i < bsp.Surfaces[surfaceIndex].numTriangles * 3; i++)
            {
                float u = bsp.LightmapCoords[(bsp.Surfaces[surfaceIndex].VertexIndex + i) * 2 + 0];
                float v = bsp.LightmapCoords[(bsp.Surfaces[surfaceIndex].VertexIndex + i) * 2 + 1];

                if (u > maxU) maxU = u;
                if (u < minU) minU = u;
                if (v > maxV) maxV = v;
                if (v < minV) minV = v;
            }

            const int scale = 4;
            
            System.Runtime.InteropServices.GCHandle pixels = System.Runtime.InteropServices.GCHandle.Alloc(lightmaps.Maps[surfaceIndex].Pixels, System.Runtime.InteropServices.GCHandleType.Pinned);
            Bitmap original = new Bitmap(lightmaps.Maps[surfaceIndex].Width, lightmaps.Maps[surfaceIndex].Height, lightmaps.Maps[surfaceIndex].Width * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb, pixels.AddrOfPinnedObject());
            pixels.Free();

            // scale the image
            int newWidth = Math.Max(256, original.Width);
            int newHeight = Math.Max(256, original.Height);

           Bitmap result = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppRgb);
            result.MakeTransparent();
            //Bitmap result = original;

            using (Graphics g = Graphics.FromImage(result))
            {
             //   g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
               g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
              g.DrawImage(original, 0, 0, newWidth, newHeight);

                using (Pen p = new Pen(Color.White, 3.0f))
                {
                    for (int i = 0; i < bsp.Surfaces[surfaceIndex].numTriangles; i++)
                    {
                        float u1 = bsp.LightmapCoords[(bsp.Surfaces[surfaceIndex].VertexIndex + i * 3 + 0) * 2 + 0];
                        float v1 = bsp.LightmapCoords[(bsp.Surfaces[surfaceIndex].VertexIndex + i * 3 + 0) * 2 + 1];

                        float u2 = bsp.LightmapCoords[(bsp.Surfaces[surfaceIndex].VertexIndex + i * 3 + 1) * 2 + 0];
                        float v2 = bsp.LightmapCoords[(bsp.Surfaces[surfaceIndex].VertexIndex + i * 3 + 1) * 2 + 1];

                        float u3 = bsp.LightmapCoords[(bsp.Surfaces[surfaceIndex].VertexIndex + i * 3 + 2) * 2 + 0];
                        float v3 = bsp.LightmapCoords[(bsp.Surfaces[surfaceIndex].VertexIndex + i * 3 + 2) * 2 + 1];

                        g.DrawLines(p, new Point[] {
                            new Point((int)(u1 * newWidth), (int)(v1 * newHeight)),
                            new Point((int)(u2 * newWidth), (int)(v2 * newHeight)),
                            new Point((int)(u3 * newWidth), (int)(v3 * newHeight)),
                            new Point((int)(u1 * newWidth), (int)(v1 * newHeight)),
                        });
                    }
                }
            }

            pictureBox1.Image = result;
        }

        // stolen from http://www.codeproject.com/Questions/167235/How-to-swap-Red-and-Blue-channels-on-bitmap
        private Bitmap swapRedAndBlueChannels(Bitmap bitmap)
        {
            var imageAttr = new ImageAttributes();
            imageAttr.SetColorMatrix(new ColorMatrix(
                                         new[]
                                             {
                                                 new[] {0.0F, 0.0F, 1.0F, 0.0F, 0.0F},
                                                 new[] {0.0F, 1.0F, 0.0F, 0.0F, 0.0F},
                                                 new[] {1.0F, 0.0F, 0.0F, 0.0F, 0.0F},
                                                 new[] {0.0F, 0.0F, 0.0F, 1.0F, 0.0F},
                                                 new[] {0.0F, 0.0F, 0.0F, 0.0F, 1.0F}
                                             }
                                         ));
            var temp = new Bitmap(bitmap.Width, bitmap.Height);
            GraphicsUnit pixel = GraphicsUnit.Pixel;
            using (Graphics g = Graphics.FromImage(temp))
            {
                g.DrawImage(bitmap, Rectangle.Round(bitmap.GetBounds(ref pixel)), 0, 0, bitmap.Width, bitmap.Height,
                            GraphicsUnit.Pixel, imageAttr);
            }

            return temp;
        }
    }
}
