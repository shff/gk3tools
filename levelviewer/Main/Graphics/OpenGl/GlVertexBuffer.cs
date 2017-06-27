using System;
using OpenTK.Graphics.OpenGL;

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

            GL.GenBuffers(1, out buffer._buffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer._buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(numVertices * vertexElements.Stride), data, (BufferUsageHint)convertUsage(usage));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            return buffer;
        }

        public override void Dispose()
        {
            GL.DeleteBuffers(1, ref _buffer);
        }

        public void Bind()
        {
            GL.GetError();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
            GlException.ThrowExceptionIfErrorExists();
        }

        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public override int NumVertices
        {
            get { return _numVertices; }
        }

        public override void SetData<T>(T[] data, int startIndex, int elementCount)
        {
            if (_usage == VertexBufferUsage.Static)
                throw new Exception("Can't update a vertex buffer created as Static");

            GL.GetError();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);

            System.Runtime.InteropServices.GCHandle handle = System.Runtime.InteropServices.GCHandle.Alloc(data, System.Runtime.InteropServices.GCHandleType.Pinned);
            try
            {
                IntPtr pointer = handle.AddrOfPinnedObject();
                int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));

                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(elementCount * size), Gk3Main.Utils.IncrementIntPtr(pointer, size * startIndex), (BufferUsageHint)convertUsage(_usage));
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
            int glUsage = (int)BufferUsageHint.StaticDraw;
            if (usage == VertexBufferUsage.Stream)
                glUsage = (int)BufferUsageHint.StreamDraw;
            else if (usage == VertexBufferUsage.Dynamic)
                glUsage = (int)BufferUsageHint.DynamicDraw;

            return glUsage;
        }
    }
}
