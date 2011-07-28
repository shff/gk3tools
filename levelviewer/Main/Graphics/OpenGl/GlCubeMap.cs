using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlCubeMap : CubeMapResource
    {
        private int _glTexture;

        public GlCubeMap(string name, BitmapSurface front, BitmapSurface back, BitmapSurface left, BitmapSurface right,
            BitmapSurface up, BitmapSurface down)
            : base(name)
        {
            Gl.glGenTextures(1, out _glTexture);
            Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, _glTexture);

            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_X, 0, Gl.GL_RGBA, front.Width, front.Height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, front.Pixels);
            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_X, 0, Gl.GL_RGBA, back.Width, back.Height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, back.Pixels);
            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Z, 0, Gl.GL_RGBA, right.Width, right.Height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, right.Pixels);
            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Z, 0, Gl.GL_RGBA, left.Width, left.Height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, left.Pixels);
            Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Y, 0, Gl.GL_RGBA, up.Width, up.Height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, up.Pixels);

            if (down != null)
            {
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y, 0, Gl.GL_RGBA, down.Width, down.Height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, down.Pixels);
            }
            else
            {
                // apparently the "down" face isn't needed. we'll just reuse the top.
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y, 0, Gl.GL_RGBA, up.Width, up.Height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, up.Pixels);
            }

            Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

            Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
            Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);

            if (Gl.glGetError() != Gl.GL_NO_ERROR)
                throw new InvalidOperationException();
        }

        public override void Bind()
        {
            Gl.glEnable(Gl.GL_TEXTURE_CUBE_MAP);
            Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, _glTexture);
            //Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, 0);
        }

        public override void Unbind()
        {
            Gl.glDisable(Gl.GL_TEXTURE_CUBE_MAP);
        }

        public int OpenGlTexture { get { return _glTexture; } }
    }
}
