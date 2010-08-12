using System;
using System.IO;

namespace Gk3Main.Graphics
{
    public abstract class CubeMapResource : TextureResource
    {
        public CubeMapResource(string name)
            : base(name, false)
        {
        }

        public override void Dispose()
        {
        }

        public abstract void Unbind();

        protected void loadFace(Stream stream, out byte[] pixels, out int width, out int height)
        {
            BitmapSurface face = new BitmapSurface(stream);

            pixels = face.Pixels;
            width = face.Width;
            height = face.Height;
        }
    }
}
