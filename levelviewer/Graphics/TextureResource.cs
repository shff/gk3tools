using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;

namespace gk3levelviewer.Graphics
{
    class TextureResource : Resource.Resource
    {
        private const uint Gk3BitmapHeader = 0x4D6E3136;

        public TextureResource(string name, System.IO.Stream stream)
            : base(name)
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

            convertToOpenGlTexture(false);
        }

        private void convertToOpenGlTexture(bool resizeToPowerOfTwo)
        {
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glGenTextures(1, out _glTexture);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);

            Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGBA, _width, _height,
                Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, _pixels);
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

        #region Privates
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

            for (int y = 0; y < _height; y++)
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

        #endregion
    }

    class TextureResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            TextureResource resource = new TextureResource(name, stream);

            stream.Close();

            return resource;
        }

        public string[] SupportedExtensions { get { return new string[] { "BMP" }; } }
    }
}
