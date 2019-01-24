#if !D3D_DISABLED

using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.Direct3D9;

namespace Gk3Main.Graphics.Direct3D9
{
    class Direct3D9Texture : TextureResource
    {
        private Texture _texture;

        /// <summary>
        /// Creates a 1x1 white texture
        /// </summary>
        internal Direct3D9Texture(bool loaded)
            : base("default_white", loaded)
        {
            // create a 1x1 white pixel
            _pixels = new byte[] { 255, 255, 255, 255 };
            _width = 1;
            _height = 1;

            convertToDirect3D9Texture(false, true);
        }

        public Direct3D9Texture(string name, BitmapSurface colorSurface)
            : base(name, colorSurface)
        {
            convertToDirect3D9Texture(true, true);
        }

        public Direct3D9Texture(string name, BitmapSurface surface, bool premultiplyAlpha)
            : base(name, surface)
        {
            if (premultiplyAlpha)
                this.premultiplyAlpha();

            convertToDirect3D9Texture(true, false);
        }

        internal Texture InternalTexture
        {
            get { return _texture; }
        }

        internal static void WritePixelsToTextureDataStream(SharpDX.DataStream stream, byte[] pixels, int actualWidth, int actualHeight)
        {
            for (int i = 0; i < actualHeight; i++)
            {
                stream.Write(pixels, i * actualWidth * 4, actualWidth * 4);
            }
        }

        private void convertToDirect3D9Texture(bool resizeToPowerOfTwo, bool clamp)
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

                pixels = fixupAlpha(newPixels, true);
            }
            else
            {
                pixels = fixupAlpha(null, true);
                if (pixels == null)
                    pixels = _pixels;
            }

            Direct3D9Renderer renderer = (Direct3D9Renderer)RendererManager.CurrentRenderer;
            _texture = new Texture(renderer.Direct3D9Device, _actualPixelWidth, _actualPixelHeight, 0, Usage.AutoGenerateMipMap, Format.A8R8G8B8, Pool.Managed);

            Surface s = _texture.GetSurfaceLevel(0);
            SharpDX.DataStream stream;
            SharpDX.DataRectangle r = s.LockRectangle(LockFlags.None, out stream);

            WritePixelsToTextureDataStream(stream, pixels, _actualPixelWidth, _actualPixelHeight);

            s.UnlockRectangle();
        }
    }
}

#endif