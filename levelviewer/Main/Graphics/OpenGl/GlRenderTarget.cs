using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlRenderTarget : RenderTarget
    {
        private int _fbo;
        private int _depthBuffer;
        private int _colorBuffer;
        private GlTexture _texture;

        public GlRenderTarget(int width, int height)
        {
            // generate the FBO
            Gl.glGenFramebuffersEXT(1, out _fbo);
            Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, _fbo);

            // generate a depth buffer
            Gl.glGenRenderbuffersEXT(1, out _depthBuffer);
            Gl.glBindRenderbufferEXT(Gl.GL_RENDERBUFFER_EXT, _depthBuffer);
            Gl.glRenderbufferStorageEXT(Gl.GL_RENDERBUFFER_EXT, Gl.GL_DEPTH_COMPONENT16, width, height);
            Gl.glFramebufferRenderbufferEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_DEPTH_ATTACHMENT_EXT, Gl.GL_RENDERBUFFER_EXT, _depthBuffer);

            // generate a color buffer
            Gl.glGenTextures(1, out _colorBuffer);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _colorBuffer);
            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, width, height, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, IntPtr.Zero);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
            Gl.glFramebufferTexture2DEXT(Gl.GL_FRAMEBUFFER_EXT, Gl.GL_COLOR_ATTACHMENT0_EXT, Gl.GL_TEXTURE_2D, _colorBuffer, 0);

            // all done... or are we...?
            int status = Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT);
            if (status != Gl.GL_FRAMEBUFFER_COMPLETE_EXT)
                throw new Exception("Unable to create RenderTarget: " + status.ToString());

            Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, 0);
        }

        public void Bind()
        {
            Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, _fbo);
        }

        public override TextureResource Texture
        {
            get
            {
                if (_texture == null)
                    _texture = new GlTexture("RenderTarget texture", _colorBuffer, true);

                return _texture;
            }
        }
    }
}
