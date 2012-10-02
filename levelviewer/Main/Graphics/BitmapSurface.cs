using System;
using System.Collections.Generic;
using System.IO;

namespace Gk3Main.Graphics
{
    public class BitmapSurface
    {
        private byte[] _pixels;
        private bool _is8bit;
        private int _width, _height;
        private bool _containsAlpha;

        private const uint Gk3BitmapHeader = 0x4D6E3136;

        public BitmapSurface(Stream stream)
            : this(stream, true)
        {
        }

        public BitmapSurface(Stream stream, bool convertFromIndexed)
        {
            int currentStreamPosition = (int)stream.Position;
            System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);

            // determine whether this is a GK3 bitmap or a Windows bitmap
            uint header = reader.ReadUInt32();

            // rewind the stream to where it was when we first got it
            reader.BaseStream.Seek(currentStreamPosition, System.IO.SeekOrigin.Begin);

            if (header == Gk3BitmapHeader)
                loadGk3Bitmap(reader, out _pixels, out _width, out _height, out _containsAlpha);
            else
                loadWindowsBitmap(reader, convertFromIndexed, out _pixels, out _width, out _height, out _is8bit);
        }

        public BitmapSurface(int width, int height, byte[] pixels)
        {
            _width = width;
            _height = height;

            if (pixels == null)
                _pixels = new byte[_width * _height * 4];
            else
            {
                if (pixels.Length != _width * _height * 4)
                    throw new ArgumentException();

                _pixels = pixels;
            }

            _containsAlpha = true;
        }

        internal BitmapSurface(TextureResource texture)
        {
            _pixels = texture.Pixels;
            _width = texture.Width;
            _height = texture.Height;
            _is8bit = false;
            _containsAlpha = texture.ContainsAlpha;
        }

        public byte[] Pixels { get { return _pixels; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public bool Is8Bit { get { return _is8bit; } }

        public Color ReadColorAt(int x, int y)
        {
            if (x < 0 || x >= _width ||
                y < 0 || y >= _height)
                throw new ArgumentOutOfRangeException();

            if (_is8bit == false)
            {
                byte r = _pixels[(y * _width + x) * 4 + 0];
                byte g = _pixels[(y * _width + x) * 4 + 1];
                byte b = _pixels[(y * _width + x) * 4 + 2];
                byte a = _pixels[(y * _width + x) * 4 + 3];

                return new Color(r, g, b, a);
            }
            else
            {
                byte c = _pixels[(y * _width + x) + 0];
                return new Color(c, c, c, 255);
            }
        }

        public static void Copy(int destX, int destY, BitmapSurface destSurface, 
            int sourceX, int sourceY, int sourceWidth, int sourceHeight, BitmapSurface srcSurface)
        {
            for (int y = 0; y < sourceHeight; y++)
            {
                int destinationIndex = ((destY + y) * destSurface.Width + destX) * 4;
                Array.Copy(srcSurface.Pixels, ((sourceY + y) * srcSurface.Width + sourceX) * 4, destSurface.Pixels,
                    destinationIndex, sourceWidth * 4);
            }
        }

        private static void loadGk3Bitmap(System.IO.BinaryReader reader, out byte[] pixels, out int width, out int height, out bool containsAlpha)
        {
            const string errorMessage = "This is not a valid GK3 bitmap";

            containsAlpha = false;
            uint header = reader.ReadUInt32();

            if (header != Gk3BitmapHeader)
                throw new Resource.InvalidResourceFileFormat(errorMessage);

            height = reader.ReadUInt16();
            width = reader.ReadUInt16();

            pixels = new byte[width * height * 4];

            byte r, g, b;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int currentPixel = (y * width + x) * 4;
                    ushort pixel = reader.ReadUInt16();

                    convert565(pixel, out r, out g, out b);

                    pixels[currentPixel + 0] = r;
                    pixels[currentPixel + 1] = g;
                    pixels[currentPixel + 2] = b;
                    pixels[currentPixel + 3] = 255;
                }

                // do we need to skip a padding pixel?
                if ((width & 0x00000001) != 0)
                    reader.ReadUInt16();
            }
        }

        private static void loadWindowsBitmap(System.IO.BinaryReader reader, bool convertFromIndexed, out byte[] pixels, out int width, out int height, out bool is8bit)
        {
            const string errorMessage = "This is not a valid Windows bitmap";
            is8bit = false;

            uint startingPosition = (uint)reader.BaseStream.Position;

            ushort header = reader.ReadUInt16();

            if (header != 19778)
                throw new Resource.InvalidResourceFileFormat(errorMessage);

            uint size = reader.ReadUInt32();
            reader.ReadUInt16();
            reader.ReadUInt16();
            uint pixelOffset = reader.ReadUInt32();

            // info header
            reader.ReadUInt32();
            width = reader.ReadInt32();
            height = reader.ReadInt32();
            reader.ReadUInt16();
            ushort bitsPerPixel = reader.ReadUInt16();

            if (bitsPerPixel != 24 && bitsPerPixel != 8)
                throw new Resource.InvalidResourceFileFormat("Only 24-bit or 8-bit grayscale bitmaps supported");

            Color[] palette = null;
            if (bitsPerPixel == 8)
            {
                // we need to read palette info
                reader.BaseStream.Seek(4 * 4, SeekOrigin.Current);
                int paletteSize = reader.ReadInt32();
                if (paletteSize == 0) paletteSize = 256;

                reader.BaseStream.Seek(4, SeekOrigin.Current);
                
                // now read the palette
                palette = new Color[paletteSize];
                for (int i = 0; i < paletteSize; i++)
                {
                    palette[i].R = reader.ReadByte();
                    palette[i].G = reader.ReadByte();
                    palette[i].B = reader.ReadByte();
                    palette[i].A = 255;

                    reader.ReadByte();
                }

                if (convertFromIndexed == false)
                    is8bit = true;
            }

            // pixels
            reader.BaseStream.Seek(startingPosition + pixelOffset, System.IO.SeekOrigin.Begin);
            pixels = new byte[width * height * 4];

            for (int y = height - 1; y >= 0; y--)
            //for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int currentPixel = (y * width + x) * 4;

                    if (bitsPerPixel == 24)
                    {
                        byte r, g, b;
                        b = reader.ReadByte();
                        g = reader.ReadByte();
                        r = reader.ReadByte();

                        pixels[currentPixel + 0] = r;
                        pixels[currentPixel + 1] = g;
                        pixels[currentPixel + 2] = b;
                        pixels[currentPixel + 3] = 255;
                    }
                    else if (bitsPerPixel == 8)
                    {
                        byte pixel = reader.ReadByte();

                        pixels[currentPixel + 3] = 255;
                        if (convertFromIndexed)
                        {
                            pixels[currentPixel + 2] = palette[pixel].B;
                            pixels[currentPixel + 1] = palette[pixel].G;
                            pixels[currentPixel + 0] = palette[pixel].R;
                        }
                        else
                        {
                            pixels[currentPixel + 2] = pixel;
                            pixels[currentPixel + 1] = pixel;
                            pixels[currentPixel + 0] = pixel;
                        }
                    }
                    else
                    {
                        // are there even any grayscale bitmaps?
                        byte pixel = reader.ReadByte();

                        pixels[currentPixel + 3] = 255;
                        pixels[currentPixel + 2] = pixel;
                        pixels[currentPixel + 1] = pixel;
                        pixels[currentPixel + 0] = pixel;
                    }
                }

                // skip any extra bytes
                reader.ReadBytes(width % 4);
            }
        }

        private static void convert565(ushort pixel, out byte r, out byte g, out byte b)
        {
            int tr = ((pixel & 0xF800) >> 11);
            int tg = ((pixel & 0x07E0) >> 5);
            int tb = (pixel & 0x001F);

            // now scale the values up to max of 255
            r = (byte)(tr * 255 / 31);
            g = (byte)(tg * 255 / 63);
            b = (byte)(tb * 255 / 31);
        }
    }
}
