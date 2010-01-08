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
        static Utils()
        {
            _indices = new ushort[6];

            _indices[0] = 0;
            _indices[1] = 1;
            _indices[2] = 2;

            _indices[3] = 0;
            _indices[4] = 2;
            _indices[5] = 3;
        }

        public static void Blit(float x, float y, TextureResource texture)
        {
            bool wasIn2D = _in2D;
            if (_in2D == false) Go2D();

            texture.Bind();

            float u = 0;
            float v = 0;
            float uw = (float)texture.Width / texture.ActualPixelWidth;
            float vw = (float)texture.Height / texture.ActualPixelHeight;

            _workingBuffer1[0] = x;
            _workingBuffer1[1] = y;
            _workingBuffer2[0] = u;
            _workingBuffer2[1] = v;

            _workingBuffer1[2] = x + texture.Width;
            _workingBuffer1[3] = y;
            _workingBuffer2[2] = u + uw;
            _workingBuffer2[3] = v;

            _workingBuffer1[4] = _workingBuffer1[2];
            _workingBuffer1[5] = y + texture.Height;
            _workingBuffer2[4] = u + uw;
            _workingBuffer2[5] = v + vw;

            _workingBuffer1[6] = x;
            _workingBuffer1[7] = _workingBuffer1[5];
            _workingBuffer2[6] = u;
            _workingBuffer2[7] = v + vw;

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glVertexPointer(2, Gl.GL_FLOAT, 0, _workingBuffer1);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, _workingBuffer2);
            
            Gl.glDrawElements(Gl.GL_TRIANGLES, 6, Gl.GL_UNSIGNED_SHORT, _indices);

            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

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

            _workingBuffer1[0] = x;
            _workingBuffer1[1] = y;
            _workingBuffer2[0] = u;
            _workingBuffer2[1] = v;

            _workingBuffer1[2] = x + src.Width;
            _workingBuffer1[3] = y;
            _workingBuffer2[2] = u + uw;
            _workingBuffer2[3] = v;

            _workingBuffer1[4] = _workingBuffer1[2];
            _workingBuffer1[5] = y + src.Height;
            _workingBuffer2[4] = u + uw;
            _workingBuffer2[5] = v + vw;

            _workingBuffer1[6] = x;
            _workingBuffer1[7] = _workingBuffer1[5];
            _workingBuffer2[6] = u;
            _workingBuffer2[7] = v + vw;

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glVertexPointer(2, Gl.GL_FLOAT, 0, _workingBuffer1);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, _workingBuffer2);

            Gl.glDrawElements(Gl.GL_TRIANGLES, 6, Gl.GL_UNSIGNED_SHORT, _indices);

            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

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
            float screenWidth = (_viewport.Height * 4) / 3;
            float widescreenOffset = (_viewport.Width - screenWidth) / 2;

            float x1 = widescreenOffset + _viewport.X + dest.X * screenWidth;
            float y1 = _viewport.Y + dest.Y * _viewport.Height;
            float x2 = x1 + dest.Width * screenWidth;
            float y2 = y1 + dest.Height * _viewport.Height;


            _workingBuffer1[0] = x1;
            _workingBuffer1[1] = y1;
            _workingBuffer2[0] = u;
            _workingBuffer2[1] = v;

            _workingBuffer1[2] = x2;
            _workingBuffer1[3] = y1;
            _workingBuffer2[2] = u + uw;
            _workingBuffer2[3] = v;

            _workingBuffer1[4] = _workingBuffer1[2];
            _workingBuffer1[5] = y2;
            _workingBuffer2[4] = u + uw;
            _workingBuffer2[5] = v + vw;

            _workingBuffer1[6] = x1;
            _workingBuffer1[7] = _workingBuffer1[5];
            _workingBuffer2[6] = u;
            _workingBuffer2[7] = v + vw;

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glVertexPointer(2, Gl.GL_FLOAT, 0, _workingBuffer1);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, _workingBuffer2);

            Gl.glDrawElements(Gl.GL_TRIANGLES, 6, Gl.GL_UNSIGNED_SHORT, _indices);

            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

            if (wasIn2D == false) End2D();
        }

        public static void DrawRect(Rect dest)
        {
            bool wasIn2D = _in2D;
            if (_in2D == false) Go2D();

            TextureResource texture = RendererManager.CurrentRenderer.DefaultTexture;
            texture.Bind();


            _workingBuffer1[0] = dest.X;
            _workingBuffer1[1] = dest.Y;
            _workingBuffer2[0] = 0;
            _workingBuffer2[1] = 0;

            _workingBuffer1[2] = dest.X + dest.Width;
            _workingBuffer1[3] = dest.Y;
            _workingBuffer2[2] = 1.0f;
            _workingBuffer2[3] = 0;

            _workingBuffer1[4] = _workingBuffer1[2];
            _workingBuffer1[5] = dest.Y + dest.Height;
            _workingBuffer2[4] = 1.0f;
            _workingBuffer2[5] = 1.0f;

            _workingBuffer1[6] = dest.X;
            _workingBuffer1[7] = _workingBuffer1[5];
            _workingBuffer2[6] = 0;
            _workingBuffer2[7] = 1.0f;

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glVertexPointer(2, Gl.GL_FLOAT, 0, _workingBuffer1);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            Gl.glTexCoordPointer(2, Gl.GL_FLOAT, 0, _workingBuffer2);

            Gl.glDrawElements(Gl.GL_TRIANGLES, 6, Gl.GL_UNSIGNED_SHORT, _indices);

            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

            if (wasIn2D == false) End2D();
        }

        public static void DrawBoundingSphere(float x, float y, float z, float radius)
        {

        }

        public static void Go2D()
        {
            Go2D(Math.Vector4.One);
        }

        public static void Go2D(Math.Vector4 color)
        {
            if (_in2D) return;
            if (_2dEffect == null)
                _2dEffect = (Effect)Resource.ResourceManager.Load("2d.fx");

            
            IRenderer renderer = RendererManager.CurrentRenderer;
            renderer.BlendEnabled = true;
            renderer.AlphaTestEnabled = false;
            renderer.DepthTestEnabled = false;
            renderer.SetBlendFunctions(BlendMode.SourceAlpha, BlendMode.InverseSourceAlpha);

            _viewport = renderer.Viewport;

            _2dEffect.SetParameter("Viewport", _viewport.Vector);
            _2dEffect.SetParameter("Color", color);
            _2dEffect.Begin();
            _2dEffect.BeginPass(0);
           
            _in2D = true;
        }

        public static void End2D()
        {
            if (!_in2D) return;

            _2dEffect.EndPass();
            _2dEffect.End();

            IRenderer renderer = RendererManager.CurrentRenderer;
            renderer.BlendEnabled = false;
            renderer.AlphaTestEnabled = true;
            renderer.DepthTestEnabled = true;

            _in2D = false;
        }

        public static void WriteTga(string filename, byte[] pixels, int width, int height)
        {
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.OpenWrite(filename));

            // Write the TGA file header
            writer.Write((byte)0);                  // id length
            writer.Write((byte)0);                  // palette type
            writer.Write((byte)2);                  // image type
            writer.Write((short)0);                 // first color
            writer.Write((short)0);                 // color count
            writer.Write((byte)0);                  // palette entry size
            writer.Write((short)0);                 // left
            writer.Write((short)0);                 // top
            writer.Write((short)width);   // width
            writer.Write((short)height);   // height
            writer.Write((byte)32);                 // bits per pixel
            writer.Write((byte)8);                  // flags

            writer.Write(pixels);

            writer.Close();
        }

        private static bool _viewportInitialized;
        private static Viewport _viewport;
        private static float[] _workingBuffer1 = new float[4 * 2];
        private static float[] _workingBuffer2 = new float[4 * 2];
        private static ushort[] _indices;
        private static bool _in2D;
        private static Effect _2dEffect;
    }
}
