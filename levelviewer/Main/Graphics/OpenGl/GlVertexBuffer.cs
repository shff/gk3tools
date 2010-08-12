using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlVertexBuffer : VertexBuffer
    {
        private int _buffer;
        private int _numVertices;

        internal static GlVertexBuffer CreateBuffer<T>(T[] data, int numVertices, VertexElementSet vertexElements) where T: struct
        {
            GlVertexBuffer buffer = new GlVertexBuffer();
            buffer._declaration = vertexElements;
            buffer._numVertices = numVertices;

            Gl.glGenBuffers(1, out buffer._buffer);
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, buffer._buffer);
            Gl.glBufferData(Gl.GL_ARRAY_BUFFER, (IntPtr)(numVertices * vertexElements.Stride), data, Gl.GL_STATIC_DRAW);

            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);

            return buffer;
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

        public override int NumVertices
        {
            get { return _numVertices; }
        }
    }
}
