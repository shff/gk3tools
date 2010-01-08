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

        protected void loadFace(BinaryReader reader, out byte[] pixels, out int width, out int height)
        {
            bool containsAlpha;

            if (IsGk3Bitmap(reader))
                LoadGk3Bitmap(reader, out pixels, out width, out height, out containsAlpha);
            else
                LoadWindowsBitmap(reader, out pixels, out width, out height);
        }
    }
}
