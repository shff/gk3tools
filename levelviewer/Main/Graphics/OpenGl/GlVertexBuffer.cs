using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlVertexBuffer : VertexBuffer
    {
        private int _buffer;
        private int _length;

        public GlVertexBuffer(float[] data, int stride)
        {
            _stride = stride;
            _length = data.Length;

            Gl.glGenBuffers(1, out _buffer);
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _buffer);
            Gl.glBufferData(Gl.GL_ARRAY_BUFFER, (IntPtr)(data.Length * sizeof(float)), data, Gl.GL_STATIC_DRAW);

            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);
        }

        public override void Dispose()
        {
            Gl.glDeleteBuffers(1, ref _buffer);
        }

        public void Bind()
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _buffer);
        }

        public void Unbind()
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);
        }

        public override int Length
        {
            get { return _length; }
        }
    }
}
