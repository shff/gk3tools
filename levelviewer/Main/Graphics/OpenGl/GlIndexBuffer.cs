using System;
using OpenTK.Graphics.OpenGL;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlIndexBuffer : IndexBuffer
    {
        private int _buffer;
        private int _length;

        public GlIndexBuffer(uint[] data)
        {
            _length = data.Length;

            GL.GenBuffers(1, out _buffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _buffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(data.Length * sizeof(uint)), data, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public override void Dispose()
        {
            GL.DeleteBuffers(1, ref _buffer);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _buffer);
        }

        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public override int Length
        {
            get { return _length; }
        }
    }
}
