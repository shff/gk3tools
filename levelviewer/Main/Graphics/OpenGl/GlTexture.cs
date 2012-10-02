using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlTexture : TextureResource
    {
        private int _glTexture;
        private bool _hasMipmaps;
        private SamplerState _state;
        private OpenGLRenderer _renderer;

        /// <summary>
        /// Creates a 1x1 white texture
        /// </summary>
        internal GlTexture(OpenGLRenderer renderer, bool loaded)
            : base("default_white", loaded)
        {
            _renderer = renderer;

            // create a 1x1 white pixel
            _pixels = new byte[] { 255, 255, 255, 255 };
            _width = 1;
            _height = 1;

            convertToOpenGlTexture(false, true, true);
        }

        internal GlTexture(OpenGLRenderer renderer, string name, int glTexture, int width, int height, bool loaded)
            : base(name, loaded)
        {
            _renderer = renderer;

            _glTexture = glTexture;

            _width = width;
            _height = height;
            _actualPixelWidth = width;
            _actualPixelHeight = height;
            _actualWidth = 1.0f;
            _actualHeight = 1.0f;

            _pixels = new byte[_width * _height * 4];

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);
            Gl.glGetTexImage(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, _pixels);

            //Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, _pixels); 
        }

        public GlTexture(OpenGLRenderer renderer, string name, System.IO.Stream stream)
            : base(name, stream)
        {
            _renderer = renderer;

            convertToOpenGlTexture(true, false, true);
        }

        public GlTexture(OpenGLRenderer renderer, string name, System.IO.Stream stream, bool clamp)
            : base(name, stream)
        {
            _renderer = renderer;

            convertToOpenGlTexture(true, clamp, true);
        }

        public GlTexture(OpenGLRenderer renderer, string name, System.IO.Stream colorStream, System.IO.Stream alphaStream)
            : base(name, colorStream, alphaStream)
        {
            _renderer = renderer;

            convertToOpenGlTexture(true, true, true);
        }

        public GlTexture(OpenGLRenderer renderer, string name, BitmapSurface surface, bool mipmapped)
            : base(name, surface)
        {
            _renderer = renderer;

            convertToOpenGlTexture(true, false, mipmapped);
        }

        public void Bind(int index)
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);

            SamplerState current = _renderer.SamplerStates[index];
            ApplySamplerState(current, _hasMipmaps, TextureType.TwoD);
        }

        public int OpenGlTexture { get { return _glTexture; } }

        private void convertToOpenGlTexture(bool resizeToPowerOfTwo, bool clamp, bool mipmapped)
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

            _hasMipmaps = mipmapped;
            if (mipmapped)
                Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGBA, _actualPixelWidth, _actualPixelHeight,
                    Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);
            else
                Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, _actualPixelWidth, _actualPixelHeight,
                    0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

            if (clamp)
            {
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
            }
        }

        internal enum TextureType
        {
            TwoD,
            ThreeD,
            CubeMap
        }

        internal static void ApplySamplerState(SamplerState current, bool textureHasMipmaps, TextureType type)
        {
            int target;
            if (type == TextureType.TwoD)
                target = Gl.GL_TEXTURE_2D;
            else if (type == TextureType.ThreeD)
                target = Gl.GL_TEXTURE_3D;
            else if (type == TextureType.CubeMap)
                target = Gl.GL_TEXTURE_CUBE_MAP;
            else
                throw new ArgumentException("type");

            Gl.glTexParameteri(target, Gl.GL_TEXTURE_WRAP_S, convertTextureAddressMode(current.AddressU));
            Gl.glTexParameteri(target, Gl.GL_TEXTURE_WRAP_T, convertTextureAddressMode(current.AddressV));
            Gl.glTexParameteri(target, Gl.GL_TEXTURE_WRAP_R, convertTextureAddressMode(current.AddressW));

            if (textureHasMipmaps)
            {
                if (current.Filter == TextureFilter.Point)
                {
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_NEAREST);
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
                }
                else if (current.Filter == TextureFilter.Linear)
                {
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                }
                else if (current.Filter == TextureFilter.PointMipLinear)
                {
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_LINEAR);
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
                }
                else if (current.Filter == TextureFilter.LinearMipPoint)
                {
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_NEAREST);
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                }
                else
                {
                    // TODO: implement the rest!
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                }
            }
            else
            {
                if (current.Filter == TextureFilter.Point ||
                    current.Filter == TextureFilter.PointMipLinear)
                {
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
                }
                else if (current.Filter == TextureFilter.Linear ||
                    current.Filter == TextureFilter.LinearMipPoint)
                {
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                }
                else
                {
                    // TODO: implement the rest!
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                    Gl.glTexParameteri(target, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
                }
            }

        }

        private static int convertTextureAddressMode(TextureAddressMode mode)
        {
            if (mode == TextureAddressMode.Clamp)
                return Gl.GL_CLAMP_TO_EDGE;
            else if (mode == TextureAddressMode.Mirror)
                return Gl.GL_MIRRORED_REPEAT;
            else
                return Gl.GL_REPEAT;
        }
    }

    public class GlUpdatableTexture : UpdatableTexture
    {
        private OpenGLRenderer _renderer;
        private int _glTexture;

        public GlUpdatableTexture(OpenGLRenderer renderer, string name, int width, int height)
            : base(name, width, height)
        {
            if (Gk3Main.Utils.IsPowerOfTwo(width) == false ||
                Gk3Main.Utils.IsPowerOfTwo(height) == false)
                throw new ArgumentException("Width and height must be power-of-two");

            _renderer = renderer;

            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glGenTextures(1, out _glTexture);

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
        }

        public override void Update(byte[] pixels)
        {
            if (pixels.Length != _width * _height * 4)
                throw new ArgumentException("Pixel array is not the expected length");

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, _width, _height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

            _pixels = pixels;
        }

        public void Bind(int index)
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _glTexture);

            SamplerState current = _renderer.SamplerStates[index];
            GlTexture.ApplySamplerState(current, false, GlTexture.TextureType.TwoD);
        }

        public int OpenGlTexture { get { return _glTexture; } }
    }
}
