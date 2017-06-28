using System;
using OpenTK.Graphics.OpenGL;

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

            GL.BindTexture(TextureTarget.Texture2D, _glTexture);
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, _pixels);

            //Gl.glTexImage2D(TextureTarget.Texture2D, 0, Gl.GL_RGBA, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, _pixels); 
        }

        public GlTexture(OpenGLRenderer renderer, string name, BitmapSurface colorSurface)
            : base(name, colorSurface)
        {
            _renderer = renderer;

            convertToOpenGlTexture(true, true, true);
        }

        public GlTexture(OpenGLRenderer renderer, string name, BitmapSurface surface, bool mipmapped, bool premultiplyAlpha)
            : base(name, surface)
        {
            _renderer = renderer;

            // TODO: we aren't generating 

            if (premultiplyAlpha)
                this.premultiplyAlpha();

            convertToOpenGlTexture(true, false, mipmapped);
        }

        public void Bind(int index)
        {
            GL.BindTexture(TextureTarget.Texture2D, _glTexture);

            //SamplerState current = _renderer.SamplerStates[index];
            //ApplySamplerState(current, _hasMipmaps, TextureType.TwoD);
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


            GL.Enable(EnableCap.Texture2D);

            int[] textures = new int[1];
            textures[0] = 0;
            GL.GenTextures(1, textures);
            _glTexture = textures[0];
            GL.BindTexture(TextureTarget.Texture2D, _glTexture);

            _hasMipmaps = mipmapped;

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _actualPixelWidth, _actualPixelHeight,
                0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            if (mipmapped)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            else
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

          /*  GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);

            if (clamp)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            }*/
        }

        internal enum TextureType
        {
            TwoD,
            ThreeD,
            CubeMap
        }

        [Obsolete("Not needed since we're using sampler objects")]
        internal static void ApplySamplerState(SamplerState current, bool textureHasMipmaps, TextureType type)
        {
           /*
            TextureTarget target;
            if (type == TextureType.TwoD)
                target = TextureTarget.Texture2D;
            else if (type == TextureType.ThreeD)
                target = TextureTarget.Texture3D;
            else if (type == TextureType.CubeMap)
                target = TextureTarget.TextureCubeMap;
            else
                throw new ArgumentException("type");

            GL.TexParameter(target, TextureParameterName.TextureWrapS, convertTextureAddressMode(current.AddressU));
            GL.TexParameter(target, TextureParameterName.TextureWrapT, convertTextureAddressMode(current.AddressV));
            GL.TexParameter(target, TextureParameterName.TextureWrapR, convertTextureAddressMode(current.AddressW));

            if (textureHasMipmaps)
            {
                if (current.Filter == TextureFilter.Point)
                {
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)All.NearestMipmapNearest);
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                }
                else if (current.Filter == TextureFilter.Linear ||
                    current.Filter == TextureFilter.Anisoptropic)
                {
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)All.Linear);
                }
                else if (current.Filter == TextureFilter.PointMipLinear)
                {
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)All.NearestMipmapLinear);
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                }
                else if (current.Filter == TextureFilter.LinearMipPoint)
                {
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapNearest);
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)All.Linear);
                }
                else
                {
                    // TODO: implement the rest!
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)All.Linear);
                }
            }
            else
            {
                if (current.Filter == TextureFilter.Point ||
                    current.Filter == TextureFilter.PointMipLinear)
                {
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                }
                else if (current.Filter == TextureFilter.Linear ||
                    current.Filter == TextureFilter.LinearMipPoint ||
                    current.Filter == TextureFilter.Anisoptropic)
                {
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)All.Linear);
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)All.Linear);
                }
                else
                {
                    // TODO: implement the rest!
                    GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)All.Linear);
                    GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)All.Linear);
                }
            }

            if (current.Filter == TextureFilter.Anisoptropic)
            {
                GL.TexParameter(target, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, current.MaxAnisotropy);
            }
            else
            {
                GL.TexParameter(target, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, 1.0f);
            }*/
        }

        private static int convertTextureAddressMode(TextureAddressMode mode)
        {
            if (mode == TextureAddressMode.Clamp)
                return (int)All.ClampToEdge;
            else if (mode == TextureAddressMode.Mirror)
                return (int)All.MirroredRepeat;
            else
                return (int)All.Repeat;
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

            GL.Enable(EnableCap.Texture2D);
            GL.GenTextures(1, out _glTexture);

            GL.BindTexture(TextureTarget.Texture2D, _glTexture);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
           // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
        }

        public override void Update(byte[] pixels)
        {
            if (pixels.Length != _width * _height * 4)
                throw new ArgumentException("Pixel array is not the expected length");

            GL.BindTexture(TextureTarget.Texture2D, _glTexture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            _pixels = pixels;
        }

        public void Bind(int index)
        {
            GL.BindTexture(TextureTarget.Texture2D, _glTexture);

           // SamplerState current = _renderer.SamplerStates[index];
            //GlTexture.ApplySamplerState(current, false, GlTexture.TextureType.TwoD);
        }

        public int OpenGlTexture { get { return _glTexture; } }
    }
}
