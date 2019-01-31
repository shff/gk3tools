using System;
using OpenTK.Graphics.OpenGL;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlCubeMap : CubeMapResource
    {
        private OpenGLRenderer _renderer;
        private int _glTexture;

        public GlCubeMap(OpenGLRenderer renderer, string name, BitmapSurface front, BitmapSurface back, BitmapSurface left, BitmapSurface right,
            BitmapSurface up, BitmapSurface down)
            : base(name)
        {
            _renderer = renderer;

            GL.GetError();

            GL.GenTextures(1, out _glTexture);
            GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);

            const PixelFormat format = PixelFormat.Rgba;
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgba, front.Width, front.Height, 0, format, PixelType.UnsignedByte, front.Pixels);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Rgba, back.Width, back.Height, 0, format, PixelType.UnsignedByte, back.Pixels);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Rgba, right.Width, right.Height, 0, format, PixelType.UnsignedByte, right.Pixels);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Rgba, left.Width, left.Height, 0, format, PixelType.UnsignedByte, left.Pixels);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Rgba, up.Width, up.Height, 0, format, PixelType.UnsignedByte, up.Pixels);

            if (down != null)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgba, down.Width, down.Height, 0, format, PixelType.UnsignedByte, down.Pixels);
            }
            else
            {
                // apparently the "down" face isn't needed. we'll just reuse the top.
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgba, up.Width, up.Height, 0, format, PixelType.UnsignedByte, up.Pixels);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)All.Linear);
                             
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);

            if (GL.GetError() != ErrorCode.NoError)
                throw new InvalidOperationException();
        }

        public void Bind(int index)
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, _glTexture);
            //Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, 0);

            SamplerState current = _renderer.SamplerStates[index];
            GlTexture.ApplySamplerState(current, false, GlTexture.TextureType.CubeMap);
        }

        public override void Unbind()
        {
            GL.Disable(EnableCap.TextureCubeMap);
        }

        public int OpenGlTexture { get { return _glTexture; } }
    }
}
