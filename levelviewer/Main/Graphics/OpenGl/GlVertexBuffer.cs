using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlVertexBuffer : VertexBuffer
    {
        private int _buffer;
        private int _numVertices;

        internal static GlVertexBuffer CreateBuffer<T>(VertexBufferUsage usage, T[] data, int numVertices, VertexElementSet vertexElements) where T: struct
        {
            GlVertexBuffer buffer = new GlVertexBuffer();
            buffer._declaration = vertexElements;
            buffer._numVertices = numVertices;
            buffer._usage = usage;

            Gl.glGenBuffers(1, out buffer._buffer);
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, buffer._buffer);
            Gl.glBufferData(Gl.GL_ARRAY_BUFFER, (IntPtr)(numVertices * vertexElements.Stride), data, convertUsage(usage));

            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);

            return buffer;
        }

        public override void Dispose()
        {
            Gl.glDeleteBuffers(1, ref _buffer);
        }

        public void Bind()
        {
            Gl.glGetError();
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _buffer);
            GlException.ThrowExceptionIfErrorExists();
        }

        public void Unbind()
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);
        }

        public override int NumVertices
        {
            get { return _numVertices; }
        }

        public override void UpdateData<T>(T[] data, int numVertices)
        {
            if (_usage == VertexBufferUsage.Static)
                throw new Exception("Can't update a vertex buffer created as Static");

            Gl.glGetError();
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _buffer);
            Gl.glBufferData(Gl.GL_ARRAY_BUFFER, (IntPtr)(numVertices * _declaration.Stride), data, convertUsage(_usage));
            GlException.ThrowExceptionIfErrorExists();
        }

        private static int convertUsage(VertexBufferUsage usage)
        {
            int glUsage = Gl.GL_STATIC_DRAW;
            if (usage == VertexBufferUsage.Stream)
                glUsage = Gl.GL_STREAM_DRAW;
            else if (usage == VertexBufferUsage.Dynamic)
                glUsage = Gl.GL_DYNAMIC_DRAW;

            return glUsage;
        }
    }
}
