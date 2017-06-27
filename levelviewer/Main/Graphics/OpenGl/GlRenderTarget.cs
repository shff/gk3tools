using System;
using OpenTK.Graphics.OpenGL;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlRenderTarget : RenderTarget
    {
        private OpenGLRenderer _renderer;
        private int _fbo;
        private int _depthBuffer;
        private int _colorBuffer;
        private GlTexture _texture;
        private int _width, _height;

        public GlRenderTarget(OpenGLRenderer renderer, int width, int height)
        {
            // generate the FBO
            GL.GenFramebuffers(1, out _fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

            // generate a depth buffer
            GL.GenRenderbuffers(1, out _depthBuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent16, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthBuffer);

            // generate a color buffer
            GL.GenTextures(1, out _colorBuffer);
            GL.BindTexture(TextureTarget.Texture2D, _colorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _colorBuffer, 0);

            // all done... or are we...?
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Unable to create RenderTarget: " + status.ToString());

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            _width = width;
            _height = height;
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        }

        public override TextureResource Texture
        {
            get
            {
                if (_texture == null)
                    _texture = new GlTexture(_renderer, "RenderTarget texture", _colorBuffer, _width, _height, true);

                return _texture;
            }
        }
    }
}
