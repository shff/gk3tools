using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlIndexBuffer : IndexBuffer
    {
        private int _buffer;
        private int _length;

        public GlIndexBuffer(uint[] data)
        {
            _length = data.Length;

            Gl.glGenBuffers(1, out _buffer);
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, _buffer);
            Gl.glBufferData(Gl.GL_ELEMENT_ARRAY_BUFFER, (IntPtr)(data.Length * sizeof(uint)), data, Gl.GL_STATIC_DRAW);

            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public override void Dispose()
        {
            Gl.glDeleteBuffers(1, ref _buffer);
        }

        public void Bind()
        {
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, _buffer);
        }

        public void Unbind()
        {
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public override int Length
        {
            get { return _length; }
        }
    }
}
