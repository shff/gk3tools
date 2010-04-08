using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlTexture : TextureResource
    {
        private int _glTexture;

        /// <summary>
        /// Creates a 1x1 white texture
        /// </summary>
        internal GlTexture(bool loaded)
            : base("default_white", loaded)
        {
            // create a 1x1 white pixel
            _pixels = new byte[] { 255, 255, 255, 255 };
            _width = 1;
            _height = 1;

            convertToOpenGlTexture(false, true);
        }

        internal GlTexture(string name, int glTexture, bool loaded)
            : base(name, loaded)
        {
            _glTexture = glTexture;
        }

        public GlTexture(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            convertToOpenGlTexture(true, false);
        }

        public GlTexture(string name, System.IO.Stream stream, bool clamp)
            : base(name, stream)
        {
            convertToOpenGlTexture(true, clamp);
        }

        public GlTexture(string name, System.IO.Stream colorStream, System.IO.Stream alphaStream)
            : base(name, colorStream, alphaStream)
        {
            convertToOpenGlTexture(true, true);
        }

        public override void Bind()
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);
        }

        public int OpenGlTexture { get { return _glTexture; } }

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
                byte[] newPixels;
                ConvertToPowerOfTwo(pixels, _width, _height, out newPixels, out _actualPixelWidth, out _actualPixelHeight);

                _actualWidth = _width / (float)_actualPixelWidth;
                _actualHeight = _height / (float)_actualPixelHeight;

                pixels = fixupAlpha(newPixels, false);
            }
            else
            {
                pixels = fixupAlpha(null, false);
                if (pixels == null)
                    pixels = _pixels;
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
    }
}
