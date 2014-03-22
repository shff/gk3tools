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

        public override void SetData<T>(T[] data, int startIndex, int elementCount)
        {
            if (_usage == VertexBufferUsage.Static)
                throw new Exception("Can't update a vertex buffer created as Static");

            Gl.glGetError();
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _buffer);

            System.Runtime.InteropServices.GCHandle handle = System.Runtime.InteropServices.GCHandle.Alloc(data, System.Runtime.InteropServices.GCHandleType.Pinned);
            try
            {
                IntPtr pointer = handle.AddrOfPinnedObject();
                int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));

                Gl.glBufferData(Gl.GL_ARRAY_BUFFER, (IntPtr)(elementCount * size), Gk3Main.Utils.IncrementIntPtr(pointer, size * startIndex), convertUsage(_usage));
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }

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
