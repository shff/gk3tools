using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace Gk3Main.Graphics.Direct3D9
{
    class Direct3D9UpdatableTexture : UpdatableTexture
    {
        public Direct3D9UpdatableTexture(string name, int width, int height)
            : base(name, width, height)
        {
            if (Gk3Main.Utils.IsPowerOfTwo(width) == false ||
                Gk3Main.Utils.IsPowerOfTwo(height) == false)
                throw new ArgumentException("Width and height must be power-of-two");
        }

        public override void Update(byte[] pixels)
        {
            // TODO
        }

        public override void Bind()
        {
            // TODO
        }
    }

    public class Direct3D9CubeMap : CubeMapResource
    {
        private SlimDX.Direct3D9.CubeTexture _cubeMap;

        internal Direct3D9CubeMap(Device device, string name, string front, string back, string left, string right,
            string up, string down)
            : base(name)
        {
            System.IO.Stream frontStream = null;
            System.IO.Stream backStream = null;
            System.IO.Stream leftStream = null;
            System.IO.Stream rightStream = null;
            System.IO.Stream upStream = null;
            System.IO.Stream downStream = null;

            frontStream = FileSystem.Open(front);
            backStream = FileSystem.Open(back);
            leftStream = FileSystem.Open(left);
            rightStream = FileSystem.Open(right);
            upStream = FileSystem.Open(up);

            try
            {
                downStream = FileSystem.Open(down);
            }
            catch (System.IO.FileNotFoundException)
            {
                // oh well, we tried.
            }

            try
            {
                byte[] pixels;
                int width, height;

                // load the first face so we can see the cube map dimensions
                loadFace(new System.IO.BinaryReader(frontStream), out pixels, out width, out height);

                _cubeMap = new CubeTexture(device, width, 0, Usage.None, Format.X8R8G8B8, Pool.Managed);
                writeFacePixels(CubeMapFace.PositiveX, pixels, width, height);

                // load the rest of the faces
                loadFace(new System.IO.BinaryReader(backStream), out pixels, out width, out height);
                writeFacePixels(CubeMapFace.NegativeX, pixels, width, height);

                loadFace(new System.IO.BinaryReader(rightStream), out pixels, out width, out height);
                writeFacePixels(CubeMapFace.PositiveZ, pixels, width, height);

                loadFace(new System.IO.BinaryReader(leftStream), out pixels, out width, out height);
                writeFacePixels(CubeMapFace.NegativeZ, pixels, width, height);

                loadFace(new System.IO.BinaryReader(upStream), out pixels, out width, out height);
                writeFacePixels(CubeMapFace.PositiveY, pixels, width, height);

                if (downStream != null)
                {
                    loadFace(new System.IO.BinaryReader(downStream), out pixels, out width, out height);
                    writeFacePixels(CubeMapFace.NegativeY, pixels, width, height);
                }
                else
                {
                    // apparently the "down" face isn't needed. we'll just reuse the top.
                    writeFacePixels(CubeMapFace.NegativeY, pixels, width, height);
                }
            }
            finally
            {
                frontStream.Close();
                backStream.Close();
                leftStream.Close();
                rightStream.Close();
                upStream.Close();

                if (downStream != null)
                    downStream.Close();
            }
        }

        public override void Bind()
        {
            throw new NotImplementedException();
        }

        public override void Unbind()
        {
            throw new NotImplementedException();
        }

        internal CubeTexture CubeMap
        {
            get { return _cubeMap; }
        }

        private void writeFacePixels(CubeMapFace face, byte[] pixels, int width, int height)
        {
            byte[] pixelsWithAlpha = new byte[width * height * 4];
            for (int i = 0; i < width * height; i++)
            {
            }

            SlimDX.DataRectangle r = _cubeMap.LockRectangle(face, 0, LockFlags.None);

            Direct3D9Texture.WritePixelsToTextureDataStream(r.Data, pixelsWithAlpha, width, height);

            _cubeMap.UnlockRectangle(face, 0);
        }
    }

    public class Direct3D9VertexBuffer : VertexBuffer
    {
        private SlimDX.Direct3D9.VertexBuffer _buffer;
        private int _length;

        internal Direct3D9VertexBuffer(SlimDX.Direct3D9.Device device, float[] data, int stride)
        {
            _stride = stride;
            _length = data.Length;

            _buffer = new SlimDX.Direct3D9.VertexBuffer(device, data.Length * sizeof(float), Usage.WriteOnly, VertexFormat.None, Pool.Default);

            SlimDX.DataStream ds = _buffer.Lock(0, _length * sizeof(float), LockFlags.None);

            for (int i = 0; i < data.Length; i++)
                ds.Write(data[i]);

            _buffer.Unlock();
        }

        public override void Dispose()
        {
            _buffer.Dispose();
        }

        public override int Length
        {
            get { return _length; }
        }

        internal SlimDX.Direct3D9.VertexBuffer InternalBuffer
        {
            get { return _buffer; }
        }
    }

    public class Direct3D9IndexBuffer : IndexBuffer
    {
        private int _length;
        private SlimDX.Direct3D9.IndexBuffer _buffer;

        internal Direct3D9IndexBuffer(SlimDX.Direct3D9.Device device, uint[] data)
        {
            _length = data.Length;
            _buffer = new SlimDX.Direct3D9.IndexBuffer(device, data.Length * sizeof(uint), Usage.WriteOnly, Pool.Default, false);

            SlimDX.DataStream ds = _buffer.Lock(0, _length * sizeof(uint), LockFlags.None);

            for (int i = 0; i < data.Length; i++)
                ds.Write(data[i]);

            _buffer.Unlock();
        }

        public override void Dispose()
        {
            _buffer.Dispose();
        }

        public override int Length
        {
            get { return _length; }
        }

        internal SlimDX.Direct3D9.IndexBuffer InternalBuffer
        {
            get { return _buffer; }
        }
    }

    public class Direct3D9Renderer : IRenderer
    {
        private Device _device;
        private int _currentDeclarationStride;
        private TextureResource _defaultTexture;
        private TextureResource _errorTexture;
        private bool _renderToTextureSupported;

        public Direct3D9Renderer(IntPtr windowHandle, int width, int height)
        {
            PresentParameters pp = new PresentParameters();
            pp.BackBufferWidth = width;
            pp.BackBufferHeight = height;
            pp.DeviceWindowHandle = windowHandle;
            pp.SwapEffect = SwapEffect.Discard;
            pp.EnableAutoDepthStencil = true;
            pp.AutoDepthStencilFormat = Format.D16;
            pp.BackBufferCount = 2;

            Direct3D d3d = new Direct3D();

            _device = new Device(d3d, 0, DeviceType.Hardware, windowHandle,
                CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve | CreateFlags.PureDevice, pp);

            _device.VertexFormat = VertexFormat.None;
            _device.SetRenderState(RenderState.CullMode, Cull.None);
        }

        #region Render states
        public bool BlendEnabled
        {
            get { return _device.GetRenderState(RenderState.AlphaBlendEnable) != 0; }
            set { _device.SetRenderState(RenderState.AlphaBlendEnable, value); }
        }

        public bool AlphaTestEnabled
        {
            get { return _device.GetRenderState(RenderState.AlphaTestEnable) != 0; }
            set { _device.SetRenderState(RenderState.AlphaTestEnable, value); }
        }

        public bool DepthTestEnabled
        {
            get { return _device.GetRenderState(RenderState.ZEnable) == (int)ZBufferType.UseZBuffer; }
            set { _device.SetRenderState(RenderState.ZEnable, (value ? ZBufferType.UseZBuffer : ZBufferType.DontUseZBuffer)); }
        }

        public bool DepthWriteEnabled
        {
            get { return _device.GetRenderState(RenderState.ZWriteEnable) != 0; }
            set { _device.SetRenderState(RenderState.ZWriteEnable, value); }
        }

        public CullMode CullMode
        {
            get
            {
                Cull cullMode = (Cull)_device.GetRenderState(RenderState.CullMode);

                if (cullMode == SlimDX.Direct3D9.Cull.None)
                    return CullMode.None;
                if (cullMode == Cull.Clockwise)
                    return CullMode.Clockwise;
                return CullMode.CounterClockwise;
            }
            set
            {
                Cull cullMode;
                if (value == CullMode.None)
                    cullMode = Cull.None;
                else if (value == CullMode.Clockwise)
                    cullMode = Cull.Clockwise;
                else
                    cullMode = Cull.Counterclockwise;

                _device.SetRenderState(RenderState.CullMode, cullMode);
            }
        }

        public CompareFunction AlphaTestFunction
        {
            get
            {
                Compare func = (Compare)_device.GetRenderState(RenderState.AlphaFunc);

                if (func == Compare.Always)
                    return CompareFunction.Always;
                else if (func == Compare.Never)
                    return CompareFunction.Never;
                else if (func == Compare.Equal)
                    return CompareFunction.Equal;
                else if (func == Compare.NotEqual)
                    return CompareFunction.NotEqual;
                else if (func == Compare.Greater)
                    return CompareFunction.Greater;
                else if (func == Compare.Less)
                    return CompareFunction.Less;
                else if (func == Compare.LessEqual)
                    return CompareFunction.LessOrEqual;
                else if (func == Compare.GreaterEqual)
                    return CompareFunction.GreaterOrEqual;

                throw new NotImplementedException("Unknown alpha test function");
            }
            set
            {
                if (value == CompareFunction.Always)
                    _device.SetRenderState(RenderState.AlphaFunc, Compare.Always);
                else if (value == CompareFunction.Greater)
                    _device.SetRenderState(RenderState.AlphaFunc, Compare.Greater);
                else if (value == CompareFunction.GreaterOrEqual)
                    _device.SetRenderState(RenderState.AlphaFunc, Compare.GreaterEqual);
            }
        }

        public float AlphaTestReference
        {
            get
            {
                int dword =_device.GetRenderState(RenderState.AlphaRef);

                return (dword & 0xff) / 255.0f;
            }
            set
            {
                int dword = (int)(value * 255.0f);

                _device.SetRenderState(RenderState.AlphaRef, dword);
            }
        }

        public void SetBlendFunctions(BlendMode source, BlendMode destination)
        {
            Blend d3dSource = Blend.Zero, d3dDest = Blend.Zero;

            switch (source)
            {
                case BlendMode.Zero:
                    d3dSource = Blend.Zero;
                    break;
                case BlendMode.One:
                    d3dSource = Blend.One;
                    break;
                case BlendMode.SourceAlpha:
                    d3dSource = Blend.SourceAlpha;
                    break;
                case BlendMode.InverseSourceAlpha:
                    d3dSource = Blend.InverseSourceAlpha;
                    break;
                case BlendMode.DestinationAlpha:
                    d3dSource = Blend.DestinationAlpha;
                    break;
                case BlendMode.InverseDestinationAlpha:
                    d3dSource = Blend.InverseDestinationAlpha;
                    break;
            }

            switch (destination)
            {
                case BlendMode.Zero:
                    d3dDest = Blend.Zero;
                    break;
                case BlendMode.One:
                    d3dDest = Blend.One;
                    break;
                case BlendMode.SourceAlpha:
                    d3dDest = Blend.SourceAlpha;
                    break;
                case BlendMode.InverseSourceAlpha:
                    d3dDest = Blend.InverseSourceAlpha;
                    break;
                case BlendMode.DestinationAlpha:
                    d3dDest = Blend.DestinationAlpha;
                    break;
                case BlendMode.InverseDestinationAlpha:
                    d3dDest = Blend.InverseDestinationAlpha;
                    break;
            }

            _device.SetRenderState(RenderState.SourceBlend, d3dSource);
            _device.SetRenderState(RenderState.DestinationBlend, d3dDest);
        }

        #endregion Render states

        #region Texture creation
        public TextureResource CreateTexture(string name, System.IO.Stream stream)
        {
            return new Direct3D9Texture(name, stream);
        }

        public TextureResource CreateTexture(string name, System.IO.Stream stream, bool clamp)
        {
            return new Direct3D9Texture(name, stream, clamp);
        }

        public TextureResource CreateTexture(string name, System.IO.Stream colorStream, System.IO.Stream alphaStream)
        {
            return new Direct3D9Texture(name, colorStream, alphaStream);
        }

        public UpdatableTexture CreateUpdatableTexture(string name, int width, int height)
        {
            return new Direct3D9UpdatableTexture(name, width, height);
        }
        #endregion

        public CubeMapResource CreateCubeMap(string name, string front, string back, string left, string right,
            string up, string down)
        {
            return new Direct3D9CubeMap(_device, name, front, back, left, right, up, down);
        }

        public Effect CreateEffect(string name, System.IO.Stream stream)
        {
            return new Direct3D9Effect(name, stream);
        }

        public VertexBuffer CreateVertexBuffer(float[] data, int stride)
        {
            return new Direct3D9VertexBuffer(_device, data, stride);
        }

        public IndexBuffer CreateIndexBuffer(uint[] data)
        {
            return new Direct3D9IndexBuffer(_device, data);
        }

        public RenderTarget CreateRenderTarget(int width, int height)
        {
            return null;
        }

        public void SetRenderTarget(RenderTarget renderTarget)
        {
            // TODO
        }

        public TextureResource ErrorTexture
        {
            get
            {
                if (_errorTexture == null)
                    _errorTexture = new Direct3D9Texture(false);

                return _errorTexture;
            }
        }

        public TextureResource DefaultTexture
        {
            get { throw new NotImplementedException(); }
        }

        public VertexElementSet VertexDeclaration
        {
            set
            {
                _device.VertexDeclaration = value.D3D9Declaration;
                _currentDeclarationStride = value.Stride;
            }
        }

        public void BeginScene()
        {
            _device.BeginScene();
        }

        public void EndScene()
        {
            _device.EndScene();
        }

        public void Present()
        {
            _device.Present();
        }

        public void Clear()
        {
            _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, 0, 1.0f, 0);
        }

        public Viewport Viewport
        {
            get
            {
                SlimDX.Direct3D9.Viewport vp = _device.Viewport;

                Viewport v = new Viewport();
                v.X = vp.X;
                v.Y = vp.Y;
                v.Width = vp.Width;
                v.Height = vp.Height;

                return v;
            }
            set
            {
                SlimDX.Direct3D9.Viewport vp = new SlimDX.Direct3D9.Viewport();
                vp.X = value.X;
                vp.Y = value.Y;
                vp.Width = value.Width;
                vp.Height = value.Height;
                vp.MinZ = 0;
                vp.MaxZ = 1.0f;

                _device.Viewport = vp;
            }
        }

        #region Rendering

        public void RenderBuffers(VertexBuffer vertices, IndexBuffer indices)
        {
            Direct3D9VertexBuffer vertexBuffer = (Direct3D9VertexBuffer)vertices;
            Direct3D9IndexBuffer indexBuffer = (Direct3D9IndexBuffer)indices;

            _device.SetStreamSource(0, vertexBuffer.InternalBuffer, 0, vertices.Stride);
            _device.Indices = indexBuffer.InternalBuffer;

            _device.DrawIndexedPrimitives(SlimDX.Direct3D9.PrimitiveType.TriangleList, 0, 0, vertexBuffer.Length / 3, 0, indices.Length / 3);
        }

        public void RenderPrimitives<T>(PrimitiveType type, int startIndex, int vertexCount, T[] vertices) where T: struct
        {
            SlimDX.Direct3D9.PrimitiveType d3dType;

            int primitiveCount;
            if (type == PrimitiveType.LineStrip)
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.LineStrip;
                primitiveCount = vertexCount - 1;
            }
            else
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.TriangleList;
                primitiveCount = vertexCount / 3;
            }

            _device.DrawUserPrimitives(d3dType, primitiveCount, vertices);
        }

        public void RenderIndices<T>(PrimitiveType type, int startIndex, int primitiveCount, int[] indices, T[] vertices) where T: struct
        {
            SlimDX.Direct3D9.PrimitiveType d3dType;
            int indexCount;
            if (type == PrimitiveType.LineStrip)
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.LineStrip;
                indexCount = primitiveCount + 1;
            }
            else if (type == PrimitiveType.Lines)
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.LineList;
                indexCount = primitiveCount * 2;
            }
            else
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.TriangleList;
                indexCount = primitiveCount * 3;
            }

            _device.DrawIndexedUserPrimitives(d3dType, 0, indexCount,
                primitiveCount, indices, Format.Index32, vertices, _currentDeclarationStride);
        }


        #endregion

        #region Capabilities
        public bool RenderToTextureSupported 
        { 
            get { return _renderToTextureSupported; } 
        }

        #endregion Capabilities

        public ZClipMode ZClipMode
        {
            get { return ZClipMode.Zero; }
        }

        public string ShaderFilenameSuffix
        {
            get { return ".hlsl"; }
        }

        internal Device Direct3D9Device
        {
            get { return _device; }
        }
    }
}
