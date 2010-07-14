using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace Gk3Main.Graphics.Direct3D9
{
    class Direct3D9UpdatableTexture : UpdatableTexture
    {
        private Texture _texture;

        public Direct3D9UpdatableTexture(string name, int width, int height)
            : base(name, width, height)
        {
            if (Gk3Main.Utils.IsPowerOfTwo(width) == false ||
                Gk3Main.Utils.IsPowerOfTwo(height) == false)
                throw new ArgumentException("Width and height must be power-of-two");

            Direct3D9Renderer renderer = (Direct3D9Renderer)RendererManager.CurrentRenderer;

            _texture = new Texture(renderer.Direct3D9Device, width, height, 0, Usage.AutoGenerateMipMap, Format.A8R8G8B8, Pool.Managed);
        }

        public override void Update(byte[] pixels)
        {
            if (pixels.Length != _width * _height * 4)
                throw new ArgumentException("Pixel array is not the expected length");
            
            // need to swap the R and B components
            for (int i = 0; i < _width * _height; i++)
            {
                byte temp = pixels[i * 4 + 0];
                pixels[i * 4 + 0] = pixels[i * 4 + 2];
                pixels[i * 4 + 2] = temp;
            }
            
            Surface s = _texture.GetSurfaceLevel(0);
            SlimDX.DataRectangle r = s.LockRectangle(LockFlags.None);

            Direct3D9Texture.WritePixelsToTextureDataStream(r.Data, pixels, _width, _height);

            s.UnlockRectangle();
        }

        public override void Bind()
        {
            // TODO
        }

        internal Texture InternalTexture
        {
            get { return _texture; }
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

                _cubeMap = new CubeTexture(device, width, 0, Usage.AutoGenerateMipMap, Format.X8R8G8B8, Pool.Managed);
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
                pixelsWithAlpha[i * 4 + 0] = pixels[i * 4 + 2];
                pixelsWithAlpha[i * 4 + 1] = pixels[i * 4 + 1];
                pixelsWithAlpha[i * 4 + 2] = pixels[i * 4 + 0];
                pixelsWithAlpha[i * 4 + 3] = pixels[i * 4 + 3];
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
        private BlendState _currentBlendState;
        private SamplerStateCollection _currentSamplerStates = new SamplerStateCollection();

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

            _currentSamplerStates.SamplerChanged += new SamplerStateCollection.SamplerChangedHandler(samplerStateChanged);

            // set default render states
            _device.SetRenderState(RenderState.CullMode, Cull.None);
            SamplerStates[0] = SamplerState.LinearWrap;
            SamplerStates[1] = SamplerState.LinearClamp;
            BlendState = BlendState.Opaque;
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

        public BlendState BlendState
        {
            get { return _currentBlendState; }
            set 
            {
                _currentBlendState = value;

                Blend sourceColor = convertBlendMode(value.ColorSourceBlend);
                Blend destColor = convertBlendMode(value.ColorDestinationBlend);
                Blend sourceAlpha = convertBlendMode(value.AlphaSourceBlend);
                Blend destAlpha = convertBlendMode(value.AlphaDestinationBlend);

                _device.SetRenderState(RenderState.SourceBlend, sourceColor);
                _device.SetRenderState(RenderState.DestinationBlend, destColor);
                _device.SetRenderState(RenderState.SourceBlendAlpha, sourceAlpha);
                _device.SetRenderState(RenderState.DestinationBlendAlpha, destAlpha);

                BlendOperation colorOp = convertBlendFunction(value.ColorBlendFunction);
                BlendOperation alphaOp = convertBlendFunction(value.AlphaBlendFunction);

                _device.SetRenderState(RenderState.BlendOperation, colorOp);
                _device.SetRenderState(RenderState.BlendOperationAlpha, alphaOp);
            }
        }

        public SamplerStateCollection SamplerStates
        {
            get { return _currentSamplerStates; }
        }

        private void samplerStateChanged(SamplerState newSampler, SamplerState oldSampler, int index)
        {
            // TODO: only modify the changed states
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.AddressU, convertTextureAddress(newSampler.AddressU));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.AddressV, convertTextureAddress(newSampler.AddressV));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.AddressW, convertTextureAddress(newSampler.AddressW));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.MinFilter, convertTextureFilter(newSampler.Filter, FilterType.Min));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.MagFilter, convertTextureFilter(newSampler.Filter, FilterType.Mag));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.MipFilter, convertTextureFilter(newSampler.Filter, FilterType.Mip));
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
            get
            {
                if (_defaultTexture == null)
                    _defaultTexture = new Direct3D9Texture(true);

                return _defaultTexture;
            }
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

        public void RenderIndices<T>(PrimitiveType type, int startIndex, int vertexCount, int[] indices, T[] vertices) where T: struct
        {
            SlimDX.Direct3D9.PrimitiveType d3dType;

            int primitiveCount;
            if (type == PrimitiveType.LineStrip)
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.LineStrip;
                primitiveCount = indices.Length - 1;
            }
            else if (type == PrimitiveType.Lines)
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.LineList;
                primitiveCount = indices.Length / 2;
            }
            else
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.TriangleList;
                primitiveCount = indices.Length / 3;
            }

            _device.DrawIndexedUserPrimitives(d3dType, 0, vertexCount,
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

        private static Blend convertBlendMode(BlendMode mode)
        {
            if (mode == BlendMode.One)
                return Blend.One;
            else if (mode == BlendMode.Zero)
                return Blend.Zero;
            else if (mode == BlendMode.SourceAlpha)
                return Blend.SourceAlpha;
            else if (mode == BlendMode.InverseSourceAlpha)
                return Blend.InverseSourceAlpha;
            else if (mode == BlendMode.DestinationAlpha)
                return Blend.DestinationAlpha;
            else
                return Blend.InverseDestinationAlpha;
        }

        private static BlendOperation convertBlendFunction(BlendFunction func)
        {
            if (func == BlendFunction.Add)
                return BlendOperation.Add;
            else if (func == BlendFunction.Subtract)
                return BlendOperation.Subtract;
            else if (func == BlendFunction.ReverseSubtract)
                return BlendOperation.ReverseSubtract;
            else if (func == BlendFunction.Min)
                return BlendOperation.Minimum;
            else
                return BlendOperation.Maximum;
        }

        private static TextureAddress convertTextureAddress(TextureAddressMode mode)
        {
            if (mode == TextureAddressMode.Clamp)
                return TextureAddress.Clamp;
            else if (mode == TextureAddressMode.Mirror)
                return TextureAddress.Mirror;
            else
                return TextureAddress.Wrap;
        }

        enum FilterType
        {
            Min,
            Mag,
            Mip
        }

        private static SlimDX.Direct3D9.TextureFilter convertTextureFilter(TextureFilter filter, FilterType type)
        {
            if (type == FilterType.Mip)
            {
                if (filter == TextureFilter.Point ||
                    filter == TextureFilter.LinearMipPoint ||
                    filter == TextureFilter.MinLinearMagPointMipPoint ||
                    filter == TextureFilter.MinPointMagLinearMipPoint)
                {
                    return SlimDX.Direct3D9.TextureFilter.Point;
                }
                else if (filter == TextureFilter.Linear ||
                    filter == TextureFilter.PointMipLinear ||
                    filter == TextureFilter.MinLinearMagPointMipLinear ||
                    filter == TextureFilter.MinPointMagLinearMipLinear)
                {
                    return SlimDX.Direct3D9.TextureFilter.Linear;
                }
            }
            else if (type == FilterType.Min)
            {
                if (filter == TextureFilter.Point ||
                    filter == TextureFilter.MinPointMagLinearMipLinear ||
                    filter == TextureFilter.MinPointMagLinearMipPoint ||
                    filter == TextureFilter.PointMipLinear)
                {
                    return SlimDX.Direct3D9.TextureFilter.Point;
                }
                else if (filter == TextureFilter.Linear ||
                    filter == TextureFilter.LinearMipPoint ||
                    filter == TextureFilter.MinLinearMagPointMipLinear ||
                    filter == TextureFilter.MinLinearMagPointMipPoint)
                {
                    return SlimDX.Direct3D9.TextureFilter.Linear;
                }
            }
            else if (type == FilterType.Mag)
            {
                if (filter == TextureFilter.Point ||
                    filter == TextureFilter.MinLinearMagPointMipLinear ||
                    filter == TextureFilter.MinLinearMagPointMipPoint ||
                    filter == TextureFilter.PointMipLinear)
                {
                    return SlimDX.Direct3D9.TextureFilter.Point;
                }
                else if (filter == TextureFilter.Linear ||
                    filter == TextureFilter.LinearMipPoint ||
                    filter == TextureFilter.MinPointMagLinearMipLinear ||
                    filter == TextureFilter.MinPointMagLinearMipPoint)
                {
                    return SlimDX.Direct3D9.TextureFilter.Linear;
                }
            }

            return SlimDX.Direct3D9.TextureFilter.None;
        }
    }
}
