using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

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

        public Direct3D9Texture(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            convertToDirect3D9Texture(true, false);
        }

        public Direct3D9Texture(string name, System.IO.Stream stream, bool clamp)
            : base(name, stream)
        {
            convertToDirect3D9Texture(true, clamp);
        }

        public Direct3D9Texture(string name, System.IO.Stream colorStream, System.IO.Stream alphaStream)
            : base(name, colorStream, alphaStream)
        {
            convertToDirect3D9Texture(true, true);
        }

        public override void Bind()
        {
            //throw new NotImplementedException();
        }

        internal Texture InternalTexture
        {
            get { return _texture; }
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
            _texture = new Texture(renderer.Direct3D9Device, _actualPixelWidth, _actualPixelHeight, 0, Usage.None, Format.A8R8G8B8, Pool.Default);

            Texture tempTexture = new Texture(renderer.Direct3D9Device, _actualPixelWidth, _actualPixelHeight, 0, Usage.None, Format.A8R8G8B8, Pool.SystemMemory);
            Surface s = tempTexture.GetSurfaceLevel(0);
            SlimDX.DataRectangle r = s.LockRectangle(LockFlags.None);

            //SlimDX.DataRectangle r = tempTexture.LockRectangle(0, LockFlags.None);

            // r.Data.Write(pixels, 0, _actualPixelWidth * _actualPixelHeight * 4);

            for (int i = 0; i < _actualPixelHeight; i++)
            {
                // byte* sourceRow = &pixels[i * _actualPixelWidth * 4];
                // byte* row = &((byte*)rect.pBits)[rect.Pitch * i];

                r.Data.Write(pixels, i * _actualPixelWidth * 4, _actualPixelWidth * 4);

                for (int j = 0; j < _actualPixelWidth; j++)
                {
                    //r.Data.WriteByte(255);
                    // r.Data.WriteByte(255);
                    //r.Data.WriteByte(0);
                    // r.Data.WriteByte(255);

                    //   row[j * 4 + 0] = sourceRow[j * 4 + 2];
                    //   row[j * 4 + 1] = sourceRow[j * 4 + 1];
                    //   row[j * 4 + 2] = sourceRow[j * 4 + 0];
                    //   row[j * 4 + 3] = sourceRow[j * 4 + 3];
                }
            }


            s.UnlockRectangle();
            // tempTexture.UnlockRectangle(0);

            renderer.Direct3D9Device.UpdateTexture(tempTexture, _texture);
            tempTexture.Dispose();
        }
    }
}
