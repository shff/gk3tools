// Copyright (c) 2007 Brad Farris
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
// along with Foobar; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;

namespace Gk3Main.Graphics
{
    public class TextureResource : Resource.Resource
    {
        private const uint Gk3BitmapHeader = 0x4D6E3136;

        public static TextureResource DefaultTexture
        {
            get
            {
                if (_defaultTexture == null)
                {
                    _defaultTexture = new TextureResource("empty");
                }

                return _defaultTexture; 
            }
        }

        /// <summary>
        /// Creates a empty texture
        /// </summary>
        /// <param name="name"></param>
        public TextureResource(string name)
            : base(name, false)
        {
            _width = 1;
            _height = 1;
            _glTexture = 0;
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
                LoadGk3Bitmap(reader, out _pixels, out _width, out _height);
            else
                LoadWindowsBitmap(reader, out _pixels, out _width, out _height);

            convertToOpenGlTexture(true, false);
        }

        public TextureResource(string name, System.IO.Stream stream, bool clamp)
            : base(name, true)
        {
            System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);

            if (IsGk3Bitmap(reader))
                LoadGk3Bitmap(reader, out _pixels, out _width, out _height);
            else
                LoadWindowsBitmap(reader, out _pixels, out _width, out _height);

            convertToOpenGlTexture(true, clamp);
        }

        public TextureResource(string name, System.IO.Stream colorMapStream, System.IO.Stream alphaMapStream)
            : base(name, true)
        {
            System.IO.BinaryReader colorReader = new System.IO.BinaryReader(colorMapStream);
            System.IO.BinaryReader alphaReader = new System.IO.BinaryReader(alphaMapStream);

            // load the color info
            if (IsGk3Bitmap(colorReader))
                LoadGk3Bitmap(colorReader, out _pixels, out _width, out _height);
            else
                LoadWindowsBitmap(colorReader, out _pixels, out _width, out _height);

            // load the alpha info
            byte[] alphaPixels;
            int alphaWidth, alphaHeight;
            if (IsGk3Bitmap(alphaReader))
                LoadGk3Bitmap(alphaReader, out alphaPixels, out alphaWidth, out alphaHeight);
            else
                LoadWindowsBitmap(alphaReader, out alphaPixels, out alphaWidth, out alphaHeight);

            if (alphaWidth != _width || alphaHeight != _height)
                throw new Resource.InvalidResourceFileFormat("Color and alpha map dimensions do not match");

            // merge color and alpha info
            for (int i = 0; i < _width * _height; i++)
            {
                _pixels[i * 4 + 3] = alphaPixels[i * 4 + 0];
            }

            convertToOpenGlTexture(true, true);
        }

        private void convertToOpenGlTexture(bool resizeToPowerOfTwo, bool clamp)
        {
            byte[] pixels = _pixels;
            _actualPixelWidth = _width;
            _actualPixelHeight = _height;

            _actualWidth = 1.0f;
            _actualHeight = 1.0f;

            if (resizeToPowerOfTwo &&
                ((_width & (_width - 1)) != 0 ||
                (_height & (_height - 1)) != 0))
            {
                _actualPixelWidth = getNextPowerOfTwo(_width);
                _actualPixelHeight = getNextPowerOfTwo(_height);

                _actualWidth = _width / (float)_actualPixelWidth;
                _actualHeight = _height / (float)_actualPixelHeight;

                pixels = new byte[_actualPixelWidth * _actualPixelHeight * 4];

                for (int y = 0; y < _actualPixelHeight; y++)
                {
                    for (int x = 0; x < _actualPixelWidth; x++)
                    {
                        if (x < _width && y < _height)
                        {
                            pixels[(y * _actualPixelWidth + x) * 4 + 0] = _pixels[(y * _width + x) * 4 + 0];
                            pixels[(y * _actualPixelWidth + x) * 4 + 1] = _pixels[(y * _width + x) * 4 + 1];
                            pixels[(y * _actualPixelWidth + x) * 4 + 2] = _pixels[(y * _width + x) * 4 + 2];
                            pixels[(y * _actualPixelWidth + x) * 4 + 3] = _pixels[(y * _width + x) * 4 + 3];
                        }
                        else
                        {
                            pixels[(y * _actualPixelWidth + x) * 4 + 0] = 0;
                            pixels[(y * _actualPixelWidth + x) * 4 + 1] = 0;
                            pixels[(y * _actualPixelWidth + x) * 4 + 2] = 0;
                            pixels[(y * _actualPixelWidth + x) * 4 + 3] = 0;
                        }
                    }
                }
            }

            Gl.glEnable(Gl.GL_TEXTURE_2D);

            int[] textures = new int[1];
            textures[0] = 0;
            Gl.glGenTextures(1, textures);
            _glTexture = textures[0];
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);

            Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGBA, _actualPixelWidth, _actualPixelHeight,
                Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

            if (clamp)
            {
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
            }
        }

        public override void Dispose()
        {
            // nothing
        }

        public virtual void Bind()
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);
        }

        public int OpenGlTexture { get { return _glTexture; } }

        public float ActualWidth { get { return _actualWidth; } }
        public float ActualHeight { get { return _actualHeight; } }

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public int ActualPixelWidth { get { return _actualPixelWidth; } }
        public int ActualPixelHeight { get { return _actualPixelHeight; } }

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

        protected static void LoadGk3Bitmap(System.IO.BinaryReader reader, out byte[] pixels, out int width, out int height)
        {
            const string errorMessage = "This is not a valid GK3 bitmap";

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
                        pixels[currentPixel + 3] = 0;
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
                        pixels[currentPixel + 3] = 255;
                        pixels[currentPixel + 2] = reader.ReadByte();
                        pixels[currentPixel + 1] = reader.ReadByte();
                        pixels[currentPixel + 0] = reader.ReadByte();
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

        private static int getNextPowerOfTwo(int n)
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

        
        private byte[] _pixels;
        private int _width;
        private int _height;
        protected int _glTexture;

        private float _actualWidth, _actualHeight;
        private int _actualPixelWidth, _actualPixelHeight;

        private static TextureResource _defaultTexture;

        #endregion
    }

    public class TextureResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            try
            {
                System.IO.Stream stream = FileSystem.Open(name);

                TextureResource resource = new TextureResource(name, stream);

                stream.Close();

                return resource;
            }
            catch (System.IO.FileNotFoundException)
            {
                Logger.WriteError("Unable to find texture: {0}", name);

                return new TextureResource(name);
            }
        }

        public string[] SupportedExtensions { get { return new string[] { "BMP" }; } }
        public bool EmptyResourceIfNotFound { get { return true; } }
    }
}
