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
                loadGk3Bitmap(reader);
            else
                loadWindowsBitmap(reader);

            convertToOpenGlTexture(true, false);
        }

        public TextureResource(string name, System.IO.Stream stream, bool clamp)
            : base(name, true)
        {
            int currentStreamPosition = (int)stream.Position;
            System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);

            // determine whether this is a GK3 bitmap or a Windows bitmap
            uint header = reader.ReadUInt32();
            
            // rewind the stream to where it was when we first got it
            reader.BaseStream.Seek(currentStreamPosition, System.IO.SeekOrigin.Begin);

            if (header == Gk3BitmapHeader)
                loadGk3Bitmap(reader);
            else
                loadWindowsBitmap(reader);

            convertToOpenGlTexture(true, clamp);
        }

        private void convertToOpenGlTexture(bool resizeToPowerOfTwo, bool clamp)
        {
            byte[] pixels = _pixels;
            int newWidth = _width;
            int newHeight = _height;

            _actualWidth = 1.0f;
            _actualHeight = 1.0f;

            if (resizeToPowerOfTwo &&
                ((_width & (_width - 1)) != 0 ||
                (_height & (_height - 1)) != 0))
            {
                newWidth = getNextPowerOfTwo(_width);
                newHeight = getNextPowerOfTwo(_height);

                _actualWidth = _width / (float)newWidth;
                _actualHeight = _height / (float)newHeight;

                pixels = new byte[newWidth * newHeight * 4];

                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        if (x < _width && y < _height)
                        {
                            pixels[(y * newWidth + x) * 4 + 0] = _pixels[(y * _width + x) * 4 + 0];
                            pixels[(y * newWidth + x) * 4 + 1] = _pixels[(y * _width + x) * 4 + 1];
                            pixels[(y * newWidth + x) * 4 + 2] = _pixels[(y * _width + x) * 4 + 2];
                            pixels[(y * newWidth + x) * 4 + 3] = _pixels[(y * _width + x) * 4 + 3];
                        }
                        else
                        {
                            pixels[(y * newWidth + x) * 4 + 0] = 0;
                            pixels[(y * newWidth + x) * 4 + 1] = 0;
                            pixels[(y * newWidth + x) * 4 + 2] = 0;
                            pixels[(y * newWidth + x) * 4 + 3] = 0;
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

            Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGBA, newWidth, newHeight,
                Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_NEAREST);
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

        public void Bind()
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);
        }

        public int OpenGlTexture { get { return _glTexture; } }

        public float ActualWidth { get { return _actualWidth; } }
        public float ActualHeight { get { return _actualHeight; } }

        #region Privates

        private int getNextPowerOfTwo(int n)
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

        private void loadGk3Bitmap(System.IO.BinaryReader reader)
        {
            const string errorMessage = "This is not a valid GK3 bitmap";

            uint header = reader.ReadUInt32();

            if (header != Gk3BitmapHeader)
                throw new Resource.InvalidResourceFileFormat(errorMessage);

            _height = reader.ReadUInt16();
            _width = reader.ReadUInt16();

            _pixels = new byte[_width * _height * 4];

            byte r, g, b;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int currentPixel = (y * _width + x) * 4;
                    ushort pixel = reader.ReadUInt16();

                    convert565(pixel, out r, out g, out b);

                    _pixels[currentPixel + 0] = r;
                    _pixels[currentPixel + 1] = g;
                    _pixels[currentPixel + 2] = b;

                    if (r == 255 && g == 0 && b == 255)
                        _pixels[currentPixel + 3] = 255;
                    else
                        _pixels[currentPixel + 3] = 0;
                }

                // do we need to skip a padding pixel?
                if ((_width & 0x00000001) != 0)
                    reader.ReadUInt16();
            }
        }

        private void convert565(ushort pixel, out byte r, out byte g, out byte b)
        {
            int tr = ((pixel & 0xF800) >> 11);
            int tg = ((pixel & 0x07E0) >> 5);
            int tb = (pixel & 0x001F);

            // now scale the values up to max of 255
            r = (byte)(tr * 255 / 31);
            g = (byte)(tg * 255 / 63);
            b = (byte)(tb * 255 / 31);
        }

        private void loadWindowsBitmap(System.IO.BinaryReader reader)
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
            _width = reader.ReadInt32();
            _height = reader.ReadInt32();
            reader.ReadUInt16();
            ushort bitsPerPixel = reader.ReadUInt16();

            if (bitsPerPixel != 24)
                throw new Resource.InvalidResourceFileFormat("Only 24-bit bitmaps supported");

            // pixels
            reader.BaseStream.Seek(startingPosition + pixelOffset, System.IO.SeekOrigin.Begin);
            _pixels = new byte[_width * _height * 4];

            for (int y = _height-1; y >= 0; y--)
            {
                for (int x = 0; x < _width; x++)
                {
                    int currentPixel = (y * _width + x) * 4;
                    _pixels[currentPixel + 2] = reader.ReadByte();
                    _pixels[currentPixel + 1] = reader.ReadByte();
                    _pixels[currentPixel + 0] = reader.ReadByte();
                }

                // skip any extra bytes
                reader.ReadBytes(_width % 4);
            }
        }

        private byte[] _pixels;
        private int _width;
        private int _height;
        private int _glTexture;

        private float _actualWidth, _actualHeight;

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
