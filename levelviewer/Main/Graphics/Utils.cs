using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public struct Rect
    {
        public float X, Y, Width, Height;

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public class Utils
    {
        public static void DrawBoundingSphere(float x, float y, float z, float radius)
        {

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

        public static Color ConvertFromWin32COLORREF(uint colorref)
        {
            Color result;
            result.R = (byte)(colorref & 0x000000ff);
            result.G = (byte)((colorref & 0x0000ff00) >> 8);
            result.B = (byte)((colorref & 0x00ff0000) >> 16);
            result.A = 255;

            return result;
        }
    }
}
