using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlCubeMap : CubeMapResource
    {
        private int _glTexture;

        public GlCubeMap(string name, string front, string back, string left, string right,
            string up, string down)
            : base(name)
        {
            System.IO.Stream frontStream = null;
            System.IO.Stream backStream = null;
            System.IO.Stream leftStream = null;
            System.IO.Stream rightStream = null;
            System.IO.Stream upStream = null;
            System.IO.Stream downStream = null;

            frontStream = FileSystem.Open(front);
            backStream = FileSystem.Open(back);
            leftStream = FileSystem.Open(left);
            rightStream = FileSystem.Open(right);
            upStream = FileSystem.Open(up);

            try
            {
                downStream = FileSystem.Open(down);
            }
            catch (System.IO.FileNotFoundException)
            {
                // oh well, we tried.
            }

            try
            {
                Gl.glGetError();
                Gl.glGenTextures(1, out _glTexture);
                Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, _glTexture);

                int internalFormat = Gl.GL_RGBA;

                byte[] pixels;
                int width, height;
                loadFace(frontStream, out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_X, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                loadFace(backStream, out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_X, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                loadFace(rightStream, out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Z, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                loadFace(leftStream, out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Z, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                loadFace(upStream, out pixels, out width, out height);
                Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_POSITIVE_Y, 0, internalFormat, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);

                if (downStream != null)
                {
                    loadFace(downStream, out pixels, out width, out height);
                    Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y, 0, Gl.GL_RGBA, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);
                }
                else
                {
                    // apparently the "down" face isn't needed. we'll just reuse the top.
                    Gl.glTexImage2D(Gl.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y, 0, Gl.GL_RGBA, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);
                }

                Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
                Gl.glTexParameterf(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);

                Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
                Gl.glTexParameteri(Gl.GL_TEXTURE_CUBE_MAP, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);

                if (Gl.glGetError() != Gl.GL_NO_ERROR)
                    throw new InvalidOperationException();
            }
            finally
            {
                frontStream.Close();
                backStream.Close();
                leftStream.Close();
                rightStream.Close();
                upStream.Close();

                if (downStream != null)
                    downStream.Close();
            }
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
