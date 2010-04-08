using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace Gk3Main.Graphics
{
    class Direct3D9Texture : TextureResource
    {
        private Texture _texture;

        /// <summary>
        /// Creates a 1x1 white texture
        /// </summary>
        internal Direct3D9Texture(bool loaded)
            : base("default_white", loaded)
        {
            // create a 1x1 white pixel
            _pixels = new byte[] { 255, 255, 255, 255 };
            _width = 1;
            _height = 1;

            convertToDirect3D9Texture(false, true);
        }

        public Direct3D9Texture(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            convertToDirect3D9Texture(true, false);
        }

        public Direct3D9Texture(string name, System.IO.Stream stream, bool clamp)
            : base(name, stream)
        {
            convertToDirect3D9Texture(true, clamp);
        }

        public Direct3D9Texture(string name, System.IO.Stream colorStream, System.IO.Stream alphaStream)
            :base(name, colorStream, alphaStream)
        {
            convertToDirect3D9Texture(true, true);
        }

        public override void Bind()
        {
            //throw new NotImplementedException();
        }

        internal Texture InternalTexture
        {
            get { return _texture; }
        }

        private void convertToDirect3D9Texture(bool resizeToPowerOfTwo, bool clamp)
        {
            byte[] pixels = _pixels;
            _actualPixelWidth = _width;
            _actualPixelHeight = _height;

            _actualWidth = 1.0f;
            _actualHeight = 1.0f;

            if (resizeToPowerOfTwo &&
                ((_width & (_width - 1)) != 0 ||
                (_height & (_height - 1)) != 0))
            {
                byte[] newPixels;
                ConvertToPowerOfTwo(pixels, _width, _height, out newPixels, out _actualPixelWidth, out _actualPixelHeight);

                _actualWidth = _width / (float)_actualPixelWidth;
                _actualHeight = _height / (float)_actualPixelHeight;

                pixels = fixupAlpha(newPixels, true);
            }
            else
            {
                pixels = fixupAlpha(null, true);
                if (pixels == null)
                    pixels = _pixels;
            }

            Direct3D9Renderer renderer = (Direct3D9Renderer)RendererManager.CurrentRenderer;
            _texture = new Texture(renderer.Direct3D9Device, _actualPixelWidth, _actualPixelHeight, 0, Usage.None, Format.A8R8G8B8, Pool.Default);

            Texture tempTexture = new Texture(renderer.Direct3D9Device, _actualPixelWidth, _actualPixelHeight, 0, Usage.None, Format.A8R8G8B8, Pool.SystemMemory);
            Surface s = tempTexture.GetSurfaceLevel(0);
            SlimDX.DataRectangle r = s.LockRectangle(LockFlags.None);
            
            //SlimDX.DataRectangle r = tempTexture.LockRectangle(0, LockFlags.None);

            // r.Data.Write(pixels, 0, _actualPixelWidth * _actualPixelHeight * 4);

            for (int i = 0; i < _actualPixelHeight; i++)
            {
               // byte* sourceRow = &pixels[i * _actualPixelWidth * 4];
               // byte* row = &((byte*)rect.pBits)[rect.Pitch * i];

                r.Data.Write(pixels, i * _actualPixelWidth * 4, _actualPixelWidth * 4);
                
                for (int j = 0; j < _actualPixelWidth; j++)
                {
                    //r.Data.WriteByte(255);
                   // r.Data.WriteByte(255);
                    //r.Data.WriteByte(0);
                   // r.Data.WriteByte(255);

                 //   row[j * 4 + 0] = sourceRow[j * 4 + 2];
                 //   row[j * 4 + 1] = sourceRow[j * 4 + 1];
                 //   row[j * 4 + 2] = sourceRow[j * 4 + 0];
                 //   row[j * 4 + 3] = sourceRow[j * 4 + 3];
                }
            }


            s.UnlockRectangle();
           // tempTexture.UnlockRectangle(0);

            renderer.Direct3D9Device.UpdateTexture(tempTexture, _texture);
            tempTexture.Dispose();
        }
    }

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
        public Direct3D9CubeMap(string name, string front, string back, string left, string right,
            string up, string down)
            : base(name)
        {
            // nothing... yet...
        }

        public override void Bind()
        {
            throw new NotImplementedException();
        }

        public override void Unbind()
        {
            throw new NotImplementedException();
        }
    }

    class Direct3D9Effect : Effect
    {
        SlimDX.Direct3D9.Effect _effect;

        public Direct3D9Effect(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            Direct3D9Renderer renderer = (Direct3D9Renderer)RendererManager.CurrentRenderer;
            string errors;
            _effect = SlimDX.Direct3D9.Effect.FromString(renderer.Direct3D9Device, Text, null, null, null, ShaderFlags.None, null, out errors);

            _effect.Technique = _effect.GetTechnique(0);
        }

        public override void Begin()
        {
            _effect.Begin();
            _effect.BeginPass(0);
        }

        public override void End()
        {
            _effect.EndPass();
            _effect.End();
        }

        #region Parameters
        public override void SetParameter(string name, float parameter)
        {
            EffectHandle param = _effect.GetParameter(null, name);
            _effect.SetValue(param, parameter);
        }

        public override void SetParameter(string name, Math.Vector4 parameter)
        {
            EffectHandle param = _effect.GetParameter(null, name);
            _effect.SetValue(param, parameter);
        }

        public override void SetParameter(string name, Gk3Main.Math.Matrix parameter)
        {
            EffectHandle param = _effect.GetParameter(null, name);
            _effect.SetValue(param, parameter);
        }

        public override void SetParameter(string name, TextureResource parameter, int index)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            Direct3D9Texture d3dTexture = (Direct3D9Texture)parameter;

            EffectHandle param = _effect.GetParameter(null, name);
            _effect.SetTexture(param, d3dTexture.InternalTexture);
            // TODO: how is this done?
            //_effect.SetValue(param, d3dTexture.Direct3D9Texture);
        }

        public override void SetParameter(string name, CubeMapResource parameter, int index)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            // TODO: how is this done?
        }

        #endregion
    }

    class Direct3D9VertexBuffer : VertexBuffer
    {
        public Direct3D9VertexBuffer(float[] data, int stride)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override int Length
        {
            get { throw new NotImplementedException(); }
        }
    }

    class Direct3D9IndexBuffer : IndexBuffer
    {
        public Direct3D9IndexBuffer(uint[] data)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override int Length
        {
            get { throw new NotImplementedException(); }
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
            return new Direct3D9CubeMap(name, front, back, left, right, up, down);
        }

        public Effect CreateEffect(string name, System.IO.Stream stream)
        {
            return new Direct3D9Effect(name, stream);
        }

        public VertexBuffer CreateVertexBuffer(float[] data, int stride)
        {
            return new Direct3D9VertexBuffer(data, stride);
        }

        public IndexBuffer CreateIndexBuffer(uint[] data)
        {
            return new Direct3D9IndexBuffer(data);
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
            _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, 0, 0, 0);
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
            throw new NotImplementedException();
        }

        public void RenderPrimitives(PrimitiveType type, int startIndex, int count, float[] vertices)
        {
            SlimDX.Direct3D9.PrimitiveType d3dType;

            if (type == PrimitiveType.LineStrip)
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.LineStrip;
            }
            else
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.TriangleList;
            }

            _device.DrawUserPrimitives(d3dType, count, vertices);
        }

        public void RenderIndices(PrimitiveType type, int startIndex, int count, int[] indices, float[] vertices)
        {
            SlimDX.Direct3D9.PrimitiveType d3dType;

            if (type == PrimitiveType.LineStrip)
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.LineStrip;
            }
            else
            {
                d3dType = SlimDX.Direct3D9.PrimitiveType.TriangleList;
            }

            _device.DrawIndexedUserPrimitives(d3dType, 0, vertices.Length / (_currentDeclarationStride / sizeof(float)),
                count, indices, Format.Index32, vertices, _currentDeclarationStride);
        }


        #endregion

        #region Capabilities
        public bool RenderToTextureSupported 
        { 
            get { return _renderToTextureSupported; } 
        }

        #endregion Capabilities

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
