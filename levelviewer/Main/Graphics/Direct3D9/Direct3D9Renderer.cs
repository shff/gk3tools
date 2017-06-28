#if !D3D_DISABLED

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
            //if (Gk3Main.Utils.IsPowerOfTwo(width) == false ||
            //    Gk3Main.Utils.IsPowerOfTwo(height) == false)
            //    throw new ArgumentException("Width and height must be power-of-two");

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

        internal Texture InternalTexture
        {
            get { return _texture; }
        }
    }

    public class Direct3D9CubeMap : CubeMapResource
    {
        private SlimDX.Direct3D9.CubeTexture _cubeMap;

        public Direct3D9CubeMap(Device device, string name, BitmapSurface front, BitmapSurface back, BitmapSurface left, BitmapSurface right,
            BitmapSurface up, BitmapSurface down)
            : base(name)
        {
            _cubeMap = new CubeTexture(device, front.Width, 0, Usage.AutoGenerateMipMap, Format.X8R8G8B8, Pool.Managed);
            writeFacePixels(CubeMapFace.PositiveX, front.Pixels, front.Width, front.Height);
            writeFacePixels(CubeMapFace.NegativeX, back.Pixels, back.Width, back.Height);
            writeFacePixels(CubeMapFace.PositiveZ, right.Pixels, right.Width, right.Height);
            writeFacePixels(CubeMapFace.NegativeZ, left.Pixels, left.Width, left.Height);
            writeFacePixels(CubeMapFace.PositiveY, up.Pixels, up.Width, up.Height);

            if (down != null)
            {
                writeFacePixels(CubeMapFace.NegativeY, down.Pixels, down.Width, down.Height);
            }
            else
            {
                // apparently the "down" face isn't needed. we'll just reuse the top.
                writeFacePixels(CubeMapFace.NegativeY, up.Pixels, up.Width, up.Height);
            }
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
        private int _numVertices;

        internal static Direct3D9VertexBuffer CreateBuffer<T>(VertexBufferUsage usage, SlimDX.Direct3D9.Device device, 
            T[] data, int numVertices, VertexElementSet vertexElements) where T: struct
        {
            Direct3D9VertexBuffer buffer = new Direct3D9VertexBuffer();
            buffer._declaration = vertexElements;
            buffer._numVertices = numVertices;
            buffer._buffer = new SlimDX.Direct3D9.VertexBuffer(device, numVertices * vertexElements.Stride, SlimDX.Direct3D9.Usage.WriteOnly, VertexFormat.None, Pool.Managed);
            buffer._usage = usage;

            if (data != null)
            {
                SlimDX.DataStream ds = buffer._buffer.Lock(0, numVertices * vertexElements.Stride, LockFlags.None);

                for (int i = 0; i < data.Length; i++)
                    ds.Write(data[i]);

                buffer._buffer.Unlock();
            }

            return buffer;
        }

        public override void Dispose()
        {
            _buffer.Dispose();
        }

        public override int NumVertices
        {
            get { return _numVertices; }
        }

        public override void SetData<T>(T[] data, int startIndex, int elementCount)
        {
            if (_usage == VertexBufferUsage.Static)
                throw new Exception("Cannot update a VertexBuffer that's Static");

            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            SlimDX.DataStream ds = _buffer.Lock(0, size * elementCount, LockFlags.None);

            ds.WriteRange(data, startIndex, elementCount); 

            _buffer.Unlock();
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
            _buffer = new SlimDX.Direct3D9.IndexBuffer(device, data.Length * sizeof(uint), Usage.WriteOnly, Pool.Managed, false);

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
        private RenderWindow _parentWindow;
        private Device _device;
        private PresentParameters _pp;
        private VertexElementSet _currentDeclaration;
        private TextureResource _defaultTexture;
        private TextureResource _errorTexture;
        private bool _renderToTextureSupported;
        private int _maxAnisotropy;
        private BlendState _currentBlendState;
        private Direct3D9VertexBuffer _currentVertexBuffer;
        private Direct3D9IndexBuffer _currentIndexBuffer;
        private SamplerStateCollection _currentSamplerStates = new SamplerStateCollection();

        public Direct3D9Renderer(RenderWindow parentWindow, IntPtr windowHandle, int width, int height, bool hosted)
        {
            _parentWindow = parentWindow;

            _pp = new PresentParameters();
            _pp.BackBufferWidth = width;
            _pp.BackBufferHeight = height;
            _pp.DeviceWindowHandle = windowHandle;
            _pp.EnableAutoDepthStencil = true;
            _pp.AutoDepthStencilFormat = Format.D16;
            if (hosted)
            {
                _pp.SwapEffect = SwapEffect.Flip;
                _pp.BackBufferCount = 1;
            }
            else
            {
                _pp.SwapEffect = SwapEffect.Discard;
                _pp.BackBufferCount = 2;
            }

            Direct3D d3d = new Direct3D();

            _device = new Device(d3d, 0, DeviceType.Hardware, windowHandle,
                CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve | CreateFlags.PureDevice, _pp);

            
            _device.VertexFormat = VertexFormat.None;

            _currentSamplerStates.SamplerChanged += new SamplerStateCollection.SamplerChangedHandler(samplerStateChanged);

            SamplerState.PointWrap = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, Filter = TextureFilter.Point, MaxAnisotropy = 4 });
            SamplerState.PointClamp = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp, Filter = TextureFilter.Point, MaxAnisotropy = 4 });
            SamplerState.LinearWrap = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, Filter = TextureFilter.Linear, MaxAnisotropy = 4 });
            SamplerState.LinearClamp = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp, Filter = TextureFilter.Linear, MaxAnisotropy = 4 });
            SamplerState.AnisotropicWrap = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Wrap, AddressV = TextureAddressMode.Wrap, AddressW = TextureAddressMode.Wrap, Filter = TextureFilter.Anisoptropic, MaxAnisotropy = 4 });
            SamplerState.AnisotropicClamp = CreateSampler(new SamplerStateDesc() { AddressU = TextureAddressMode.Clamp, AddressV = TextureAddressMode.Clamp, AddressW = TextureAddressMode.Clamp, Filter = TextureFilter.Anisoptropic, MaxAnisotropy = 4 });

            // set default render states
            _device.SetRenderState(RenderState.CullMode, Cull.None);
            SamplerStates[0] = SamplerState.LinearWrap;
            SamplerStates[1] = SamplerState.LinearClamp;
            BlendState = BlendState.Opaque;

            // TODO: load this from the caps!
            _maxAnisotropy = 0;
        }

        public void Reset(int width, int height)
        {
            _pp.BackBufferWidth = width;
            _pp.BackBufferHeight = height;

            _device.Reset(_pp);
        }

        #region Render states

        public SamplerState CreateSampler(SamplerStateDesc desc)
        {
            var newSamplerObject = new SamplerState(0, 0, desc);
            return newSamplerObject;
        }

        public bool BlendEnabled
        {
            get { return _device.GetRenderState(RenderState.AlphaBlendEnable) != 0; }
            set { _device.SetRenderState(RenderState.AlphaBlendEnable, value); }
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
            var desc = newSampler.Desc;
            // TODO: only modify the changed states
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.AddressU, convertTextureAddress(desc.AddressU));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.AddressV, convertTextureAddress(desc.AddressV));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.AddressW, convertTextureAddress(desc.AddressW));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.MinFilter, convertTextureFilter(desc.Filter, FilterType.Min));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.MagFilter, convertTextureFilter(desc.Filter, FilterType.Mag));
            _device.SetSamplerState(index, SlimDX.Direct3D9.SamplerState.MipFilter, convertTextureFilter(desc.Filter, FilterType.Mip));
        }

        #endregion Render states

        #region Texture creation
        public TextureResource CreateTexture(string name, BitmapSurface colorSurface, bool mipmapped)
        {
            return new Direct3D9Texture(name, colorSurface);
        }

        public TextureResource CreateTexture(string name, BitmapSurface surface, bool mipmapped, bool premultiplyAlpha)
        {
            return new Direct3D9Texture(name, surface, premultiplyAlpha);
        }

        public UpdatableTexture CreateUpdatableTexture(string name, int width, int height)
        {
            return new Direct3D9UpdatableTexture(name, width, height);
        }
        #endregion

        public CubeMapResource CreateCubeMap(string name, BitmapSurface front, BitmapSurface back, BitmapSurface left, BitmapSurface right,
            BitmapSurface up, BitmapSurface down)
        {
            return new Direct3D9CubeMap(_device, name, front, back, left, right, up, down);
        }

        public Effect CreateEffect(string name, System.IO.Stream stream)
        {
            return new Direct3D9Effect(name, stream);
        }

        public VertexBuffer CreateVertexBuffer<T>(VertexBufferUsage usage, T[] data, int numVertices, VertexElementSet declaration) where T: struct
        {
            return Direct3D9VertexBuffer.CreateBuffer(usage, _device, data, numVertices, declaration);
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

        internal VertexElementSet VertexDeclaration
        {
            set
            {
                _device.VertexDeclaration = value.D3D9Declaration;
                _currentDeclaration = value;
            }
        }

        public IndexBuffer Indices
        {
            get
            {
                return _currentIndexBuffer;
            }
            set
            {
                _currentIndexBuffer = (Direct3D9IndexBuffer)value;
                if (_currentIndexBuffer != null)
                    _device.Indices = _currentIndexBuffer.InternalBuffer;
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

        public void SetVertexBuffer(VertexBuffer buffer)
        {
            Direct3D9VertexBuffer vertexBuffer = (Direct3D9VertexBuffer)buffer;

            _device.SetStreamSource(0, vertexBuffer.InternalBuffer, 0, vertexBuffer.VertexElements.Stride);
            _device.VertexDeclaration = vertexBuffer.VertexElements.D3D9Declaration;

            _currentVertexBuffer = vertexBuffer;
        }

        public void RenderBuffers(VertexBuffer vertices, IndexBuffer indices)
        {
            Direct3D9VertexBuffer vertexBuffer = (Direct3D9VertexBuffer)vertices;
            Direct3D9IndexBuffer indexBuffer = (Direct3D9IndexBuffer)indices;

            _device.SetStreamSource(0, vertexBuffer.InternalBuffer, 0, vertices.VertexElements.Stride);

            if (indexBuffer != null)
            {
                _device.Indices = indexBuffer.InternalBuffer;

                _device.DrawIndexedPrimitives(SlimDX.Direct3D9.PrimitiveType.TriangleList, 0, 0, vertexBuffer.NumVertices / 3, 0, indices.Length / 3);
            }
            else
            {
                _device.DrawPrimitives(SlimDX.Direct3D9.PrimitiveType.TriangleList, 0, vertexBuffer.NumVertices / 3);
            }
        }

        public void RenderPrimitives(int firstVertex, int vertexCount)
        {
            _device.DrawPrimitives(SlimDX.Direct3D9.PrimitiveType.TriangleList, firstVertex, vertexCount / 3);
        }

        public void RenderIndexedPrimitives(int startIndex, int numPrimitives)
        {
            _device.DrawIndexedPrimitives(SlimDX.Direct3D9.PrimitiveType.TriangleList, 0, 0, _currentVertexBuffer.NumVertices, startIndex, numPrimitives);
        }

        public void RenderPrimitives<T>(PrimitiveType type, int startIndex, int vertexCount, T[] vertices, VertexElementSet declaration) where T: struct
        {
            if (declaration != _currentDeclaration)
            {
                VertexDeclaration = declaration;
            }

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

        public void RenderIndices(PrimitiveType type, int startIndex, int vertexCount, int[] indices)
        {

        }


        #endregion

        #region Capabilities
        public bool RenderToTextureSupported 
        { 
            get { return _renderToTextureSupported; } 
        }

        public int MaxAnisotropy { get { return _maxAnisotropy; } }

        #endregion Capabilities

        public ZClipMode ZClipMode
        {
            get { return ZClipMode.Zero; }
        }

        public string ShaderFilenameSuffix
        {
            get { return ".hlsl"; }
        }

        public RenderWindow ParentWindow
        {
            get { return _parentWindow; }
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

#endif