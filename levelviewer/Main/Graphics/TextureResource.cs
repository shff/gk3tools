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

        internal TextureResource(string name, BitmapSurface surface)
            : base(name, true)
        {
            _width = surface.Width;
            _height = surface.Height;
            _pixels = surface.Pixels;
        }
        
        public override void Dispose()
        {
            // nothing
        }

        public float ActualWidth { get { return _actualWidth; } }
        public float ActualHeight { get { return _actualHeight; } }

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public int ActualPixelWidth { get { return _actualPixelWidth; } }
        public int ActualPixelHeight { get { return _actualPixelHeight; } }

        public bool ContainsAlpha { get { return _containsAlpha; } }
        public bool PremultipliedAlhpa { get { return _premultipliedAlpha; } }

        /// <summary>
        /// Gets the ORIGINAL pixels as loaded from the bitmap, without any processing
        /// </summary>
        internal byte[] Pixels { get { return _pixels; } }

        protected static void ConvertToPowerOfTwo(byte[] pixels, int width, int height, 
            out byte[] newPixels, out int newWidth, out int newHeight)
        {
            newWidth = getNextPowerOfTwo(width);
            newHeight = getNextPowerOfTwo(height);

            newPixels = new byte[newWidth * newHeight * 4];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    if (x < width && y < height)
                    {
                        newPixels[(y * newWidth + x) * 4 + 0] = pixels[(y * width + x) * 4 + 0];
                        newPixels[(y * newWidth + x) * 4 + 1] = pixels[(y * width + x) * 4 + 1];
                        newPixels[(y * newWidth + x) * 4 + 2] = pixels[(y * width + x) * 4 + 2];
                        newPixels[(y * newWidth + x) * 4 + 3] = pixels[(y * width + x) * 4 + 3];
                    }
                    else
                    {
                        newPixels[(y * newWidth + x) * 4 + 0] = 0;
                        newPixels[(y * newWidth + x) * 4 + 1] = 0;
                        newPixels[(y * newWidth + x) * 4 + 2] = 0;
                        newPixels[(y * newWidth + x) * 4 + 3] = 0;
                    }
                }
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

        /// <summary>
        /// Fixes alpha. If resizedPixels is null then it uses _pixels.
        /// It returns the array of modified pixels. It may also return
        /// null if no modifications were made and resizedPixels was not provided.
        /// </summary>
        protected byte[] fixupAlpha(byte[] resizedPixels, bool swapRandB)
        {
            byte[] result, source;
            int width, height;

            if (resizedPixels == null)
            {
                result = null;
                source = _pixels;
                width = _width;
                height = _height;
            }
            else
            {
                result = resizedPixels;
                source = resizedPixels;
                width = _actualPixelWidth;
                height = _actualPixelHeight;
            }

            for (int i = 0; i < width * height; i++)
            {
                if (source[i * 4 + 0] == 255 &&
                    source[i * 4 + 1] == 0 &&
                    source[i * 4 + 2] == 255)
                {
                    if (result == null)
                    {
                        result = new byte[width * height * 4];
                        Array.Copy(source, result, source.Length);
                    }

                    result[i * 4 + 0] = 0;
                    result[i * 4 + 1] = 0;
                    result[i * 4 + 2] = 0;
                    result[i * 4 + 3] = 0;
                }
                else if (swapRandB)
                {
                    if (result == null)
                    {
                        result = new byte[width * height * 4];
                        Array.Copy(source, result, source.Length);
                    }

                    byte temp = result[i * 4 + 0];
                    result[i * 4 + 0] = result[i * 4 + 2];
                    result[i * 4 + 2] = temp;
                }
            }

            return result;
        }

        protected void premultiplyAlpha()
        {
            if (_premultipliedAlpha) return;

            for (int i = 0; i < _width * _height; i++)
            {
                float alpha = 255.0f / _pixels[i * 4 + 3];
                _pixels[i * 4 + 0] = (byte)(_pixels[i * 4 + 0] * alpha);
                _pixels[i * 4 + 1] = (byte)(_pixels[i * 4 + 1] * alpha);
                _pixels[i * 4 + 2] = (byte)(_pixels[i * 4 + 2] * alpha);
            }

            _premultipliedAlpha = true;
        }

        protected byte[] _pixels;
        protected int _width;
        protected int _height;

        protected float _actualWidth, _actualHeight;
        protected int _actualPixelWidth, _actualPixelHeight;
        protected bool _containsAlpha;
        protected bool _premultipliedAlpha;

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
        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            try
            {
                if (name.IndexOf('.') < 0)
                    name += ".BMP";

                System.IO.Stream stream = FileSystem.Open(name);

                BitmapSurface surface = new BitmapSurface(stream);
                Resource.Resource resource = RendererManager.CurrentRenderer.CreateTexture(name, surface, true, true);

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
