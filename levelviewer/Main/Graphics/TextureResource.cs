// Copyright (c) 2009 Brad Farris
// This file is part of the GK3 Scene Viewer.

// The GK3 Scene Viewer is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// The GK3 Scene Viewer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with the GK3 Scene Viewer; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;

namespace Gk3Main.Graphics
{
    public abstract class TextureResource : Resource.Resource
    {
        private const uint Gk3BitmapHeader = 0x4D6E3136;

        internal TextureResource(string name, bool loaded)
            : base(name, loaded)
        {
            // nothing... hopefully the child class is handling things
        }

        public TextureResource(string name, System.IO.Stream stream)
            : base(name, true)
        {
            int currentStreamPosition = (int)stream.Position;
            System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);

            // determine whether this is a GK3 bitmap or a Windows bitmap
            uint header = reader.ReadUInt32();

            // rewind the stream to where it was when we first got it
            reader.BaseStream.Seek(currentStreamPosition, System.IO.SeekOrigin.Begin);

            if (header == Gk3BitmapHeader)
                LoadGk3Bitmap(reader, out _pixels, out _width, out _height, out _containsAlpha);
            else
                LoadWindowsBitmap(reader, out _pixels, out _width, out _height);
        }

        public TextureResource(string name, System.IO.Stream colorMapStream, System.IO.Stream alphaMapStream)
            : base(name, true)
        {
            System.IO.BinaryReader colorReader = new System.IO.BinaryReader(colorMapStream);
            System.IO.BinaryReader alphaReader = new System.IO.BinaryReader(alphaMapStream);

            // load the color info
            if (IsGk3Bitmap(colorReader))
                LoadGk3Bitmap(colorReader, out _pixels, out _width, out _height, out _containsAlpha);
            else
                LoadWindowsBitmap(colorReader, out _pixels, out _width, out _height);

            // load the alpha info
            byte[] alphaPixels;
            int alphaWidth, alphaHeight;
            if (IsGk3Bitmap(alphaReader))
                LoadGk3Bitmap(alphaReader, out alphaPixels, out alphaWidth, out alphaHeight, out _containsAlpha);
            else
                LoadWindowsBitmap(alphaReader, out alphaPixels, out alphaWidth, out alphaHeight);

            if (alphaWidth != _width || alphaHeight != _height)
                throw new Resource.InvalidResourceFileFormat("Color and alpha map dimensions do not match");

            // merge color and alpha info
            for (int i = 0; i < _width * _height; i++)
            {
                _pixels[i * 4 + 3] = alphaPixels[i * 4 + 0];
            }
        }

        
        public override void Dispose()
        {
            // nothing
        }

        public abstract void Bind();

        public float ActualWidth { get { return _actualWidth; } }
        public float ActualHeight { get { return _actualHeight; } }

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public int ActualPixelWidth { get { return _actualPixelWidth; } }
        public int ActualPixelHeight { get { return _actualPixelHeight; } }

        public bool ContainsAlpha { get { return _containsAlpha; } }

        internal byte[] Pixels { get { return _pixels; } }

        /// <summary>
        /// Determines whether the bitmap is a GK3 bitmap or not.
        /// </summary>
        /// <param name="reader">A BinaryReader that *must* be positioned at the
        /// beginning of the bitmap.</param>
        /// <returns>True if this is a GK3 bitmap, false otherwise.</returns>
        protected static bool IsGk3Bitmap(System.IO.BinaryReader reader)
        {
            long currentPosition = reader.BaseStream.Position;

            // determine whether this is a GK3 bitmap or a Windows bitmap
            uint header = reader.ReadUInt32();

            // rewind the stream to where it was when we first got it
            reader.BaseStream.Seek(currentPosition, System.IO.SeekOrigin.Begin);

            // load the color info
            if (header == Gk3BitmapHeader)
                return true;

            return false;
        }

        protected static void LoadGk3Bitmap(System.IO.BinaryReader reader, out byte[] pixels, out int width, out int height, out bool containsAlpha)
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

                    if (r == 255 && g == 0 && b == 255)
                    {
                        pixels[currentPixel + 0] = 0;
                        pixels[currentPixel + 1] = 0;
                        pixels[currentPixel + 2] = 0;
                        pixels[currentPixel + 3] = 0;
                        containsAlpha = true;
                    }
                    else
                        pixels[currentPixel + 3] = 255;
                }

                // do we need to skip a padding pixel?
                if ((width & 0x00000001) != 0)
                    reader.ReadUInt16();
            }
        }

        protected static void LoadWindowsBitmap(System.IO.BinaryReader reader, out byte[] pixels, out int width, out int height)
        {
            const string errorMessage = "This is not a valid Windows bitmap";
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

            // pixels
            reader.BaseStream.Seek(startingPosition + pixelOffset, System.IO.SeekOrigin.Begin);
            pixels = new byte[width * height * 4];

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (bitsPerPixel == 24)
                    {
                        int currentPixel = (y * width + x) * 4;

                        byte r, g, b;
                        b = reader.ReadByte();
                        g = reader.ReadByte();
                        r = reader.ReadByte();

                        pixels[currentPixel + 0] = r;
                        pixels[currentPixel + 1] = g;
                        pixels[currentPixel + 2] = b;

                        if (r == 255 && g == 0 && b == 255)
                            pixels[currentPixel + 3] = 0;
                        else
                            pixels[currentPixel + 3] = 255;
                    }
                    else
                    {
                        int currentPixel = (y * width + x) * 4;
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

        #region Privates

        protected static int getNextPowerOfTwo(int n)
        {
            if ((n & (n - 1)) == 0)
                return n;

            n--;
            for (int i = 1; i < 32; i *= 2)
            {
                n = n | n >> i;
            }

            return n + 1;
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

        protected byte[] _pixels;
        protected int _width;
        protected int _height;

        protected float _actualWidth, _actualHeight;
        protected int _actualPixelWidth, _actualPixelHeight;
        protected bool _containsAlpha;

        private static TextureResource _defaultTexture;

        #endregion
    }

    public abstract class UpdatableTexture : TextureResource
    {
        public UpdatableTexture(string name, int width, int height)
            : base(name, true)
        {
            _pixels = new byte[width * height * 4];
            _width = width;
            _height = height;
        }

        public abstract void Update(byte[] pixels);
    }

    public class TextureResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            try
            {
                System.IO.Stream stream = FileSystem.Open(name);

                Resource.Resource resource = RendererManager.CurrentRenderer.CreateTexture(name, stream);

                stream.Close();

                return resource;
            }
            catch (System.IO.FileNotFoundException)
            {
                Logger.WriteError("Unable to find texture: {0}", name);

                return RendererManager.CurrentRenderer.ErrorTexture;
            }
        }

        public string[] SupportedExtensions { get { return new string[] { "BMP" }; } }
        public bool EmptyResourceIfNotFound { get { return true; } }
    }
}
