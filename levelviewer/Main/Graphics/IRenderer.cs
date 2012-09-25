using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public enum ZClipMode
    {
        NegativeOne,
        Zero
    }

    public enum CullMode
    {
        Clockwise,
        CounterClockwise,
        None
    }

    public enum PrimitiveType
    {
        Triangles,
        LineStrip,
        Lines
    }

    public enum CompareFunction
    {
        Always,
        Never,

        Greater,
        Less,

        Equal,
        LessOrEqual,
        GreaterOrEqual,
        NotEqual
    }

    public struct Viewport
    {
        private int _x, _y, _width, _height;

        public Viewport(int x, int y, int width, int height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public float Aspect
        {
            get { return (float)_width / _height; }
        }

        public Math.Vector3 Unproject(Math.Vector3 source,
            ref Math.Matrix projection, ref Math.Matrix view, ref Math.Matrix world)
        {
            Math.Vector4 result;
            result.X = ((source.X - X) * 2 / Width) - 1;
            result.Y = 1 - ((source.Y - Y) * 2 / Height);
            result.Z = source.Z;
            result.W = 1.0f;

            Math.Matrix invProj, invView, invWorld;
            Math.Matrix.Invert(ref projection, out invProj);
            Math.Matrix.Invert(ref view, out invView);
            Math.Matrix.Invert(ref world, out invWorld);

            result = invProj * result;
            result = invView * result;
            result = invWorld * result;
            result = result / result.W;

            return new Math.Vector3(result.X, result.Y, result.Z);
        }

        public Math.Vector4 Vector
        {
            get { return new Math.Vector4(_x, _y, _width, _height); }
        }

    }

    public abstract class Effect : Resource.TextResource
    {
        public Effect(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            // nothing
        }

        public virtual void Bind() { }
        public abstract void Begin();
        public abstract void CommitParams();
        public abstract void End();

        public abstract void SetParameter(string name, float parameter);
        public abstract void SetParameter(string name, Math.Vector4 parameter);
        public abstract void SetParameter(string name, Math.Matrix parameter);
        public abstract void SetParameter(string name, Color parameter);
        public abstract void SetParameter(string name, TextureResource parameter, int index);
        public abstract void SetParameter(string name, CubeMapResource parameter, int index);
    }

    public class EffectLoader : Resource.IResourceLoader
    {
        #region IResourceLoader Members

        public string[] SupportedExtensions
        {
            get { return new string[] { "fx" }; }
        }

        public bool EmptyResourceIfNotFound
        {
            get { return false; }
        }

        public Gk3Main.Resource.Resource Load(string filename, Resource.ResourceManager content)
        {
            IRenderer renderer = RendererManager.CurrentRenderer;
            System.IO.Stream stream = FileSystem.Open(filename + renderer.ShaderFilenameSuffix);
            Effect resource = renderer.CreateEffect(filename, stream);

            stream.Close();

            return resource;
        }

        #endregion
    }

    public enum VertexBufferUsage
    {
        Static,
        Stream,
        Dynamic
    }

    public abstract class VertexBuffer : IDisposable
    {
        protected VertexBufferUsage _usage;
        protected VertexElementSet _declaration;

        public abstract void Dispose();

        public VertexElementSet VertexElements { get { return _declaration; } }
        public abstract int NumVertices { get; }

        public VertexBufferUsage Usage { get { return _usage; } }
        public abstract void UpdateData<T>(T[] data, int numVertices) where T : struct;
    }

    public abstract class IndexBuffer : IDisposable
    {
        public abstract void Dispose();

        public abstract int Length { get; }
    }

    public enum VertexElementFormat
    {
        Float = 1,
        Float2,
        Float3,
        Float4
    }

    public enum VertexElementUsage
    {
        Position,
        TexCoord,
        Normal,
        Color
    }

    public struct VertexElement
    {
        public VertexElement(int offset, VertexElementFormat format, 
            VertexElementUsage usage, int usageIndex)
        {
            Offset = offset;
            Format = format;
            Usage = usage;
            UsageIndex = usageIndex;
        }

        public int Offset;
        public VertexElementFormat Format;
        public VertexElementUsage Usage;
        public int UsageIndex;
    }

    public class VertexElementSet
    {
        public VertexElementSet(VertexElement[] elements)
        {
            Stride = (int)elements[elements.Length-1].Offset + 
                (int)elements[elements.Length - 1].Format * sizeof(float);

            Elements = elements;

#if !D3D_DISABLED
            Direct3D9.Direct3D9Renderer renderer = RendererManager.CurrentRenderer as Direct3D9.Direct3D9Renderer;

            if (renderer != null)
            {
                SlimDX.Direct3D9.VertexElement[] d3d9Elements = new SlimDX.Direct3D9.VertexElement[elements.Length + 1];
                for (int i = 0; i < elements.Length; i++)
                {
                    d3d9Elements[i].Offset = (short)elements[i].Offset;

                    if (elements[i].Format == VertexElementFormat.Float)
                        d3d9Elements[i].Type = SlimDX.Direct3D9.DeclarationType.Float1;
                    else if (elements[i].Format == VertexElementFormat.Float2)
                        d3d9Elements[i].Type = SlimDX.Direct3D9.DeclarationType.Float2;
                    else if (elements[i].Format == VertexElementFormat.Float3)
                        d3d9Elements[i].Type = SlimDX.Direct3D9.DeclarationType.Float3;
                    else if (elements[i].Format == VertexElementFormat.Float4)
                        d3d9Elements[i].Type = SlimDX.Direct3D9.DeclarationType.Float4;

                    if (elements[i].Usage == VertexElementUsage.Position)
                        d3d9Elements[i].Usage = SlimDX.Direct3D9.DeclarationUsage.Position;
                    else if (elements[i].Usage == VertexElementUsage.TexCoord)
                        d3d9Elements[i].Usage = SlimDX.Direct3D9.DeclarationUsage.TextureCoordinate;
                    else if (elements[i].Usage == VertexElementUsage.Normal)
                        d3d9Elements[i].Usage = SlimDX.Direct3D9.DeclarationUsage.Normal;
                    else if (elements[i].Usage == VertexElementUsage.Color)
                        d3d9Elements[i].Usage = SlimDX.Direct3D9.DeclarationUsage.Color;

                    d3d9Elements[i].Stream = 0;
                    d3d9Elements[i].UsageIndex = (byte)elements[i].UsageIndex;
                    d3d9Elements[i].Method = SlimDX.Direct3D9.DeclarationMethod.Default;
                }
                d3d9Elements[d3d9Elements.Length - 1] = SlimDX.Direct3D9.VertexElement.VertexDeclarationEnd;
                D3D9Declaration = new SlimDX.Direct3D9.VertexDeclaration(renderer.Direct3D9Device, d3d9Elements);
            }
#endif
        }

        public VertexElement[] Elements;
#if !D3D_DISABLED
        public SlimDX.Direct3D9.VertexDeclaration D3D9Declaration;
#endif
        public int Stride;
    }

    public abstract class RenderTarget
    {
        public abstract TextureResource Texture
        {
            get;
        }
    }

    #region Blending
    public enum BlendMode
    {
        Zero,
        One,
        SourceAlpha,
        InverseSourceAlpha,
        DestinationAlpha,
        InverseDestinationAlpha
    }

    public enum BlendFunction
    {
        Add,
        Max,
        Min,
        ReverseSubtract,
        Subtract
    }

    public struct BlendState
    {
        private BlendMode _colorSourceBlend;
        private BlendMode _colorDestinationBlend;
        private BlendFunction _colorBlendFunction;
        private BlendMode _alphaSourceBlend;
        private BlendMode _alphaDestinationBlend;
        private BlendFunction _alphaBlendFunction;

        private BlendState(BlendMode colorSource, BlendMode colorDest, BlendFunction colorFunc,
            BlendMode alphaSource, BlendMode alphaDest, BlendFunction alphaFunc)
        {
            _colorSourceBlend = colorSource;
            _colorDestinationBlend = colorDest;
            _colorBlendFunction = colorFunc;
            _alphaSourceBlend = alphaSource;
            _alphaDestinationBlend = alphaDest;
            _alphaBlendFunction = alphaFunc;
        }

        public BlendMode ColorSourceBlend
        {
            get { return _colorSourceBlend; }
            set { _colorSourceBlend = value; }
        }

        public BlendMode ColorDestinationBlend
        {
            get { return _colorDestinationBlend; }
            set { _colorDestinationBlend = value; }
        }

        public BlendFunction ColorBlendFunction
        {
            get { return _colorBlendFunction; }
            set { _colorBlendFunction = value; }
        }

        public BlendMode AlphaSourceBlend
        {
            get { return _alphaSourceBlend; }
            set { _alphaSourceBlend = value; }
        }

        public BlendMode AlphaDestinationBlend
        {
            get { return _alphaDestinationBlend; }
            set { _alphaDestinationBlend = value; }
        }

        public BlendFunction AlphaBlendFunction
        {
            get { return _alphaBlendFunction; }
            set { _alphaBlendFunction = value; }
        }

        public static BlendState Opaque = new BlendState(BlendMode.One, BlendMode.Zero, BlendFunction.Add,
            BlendMode.One, BlendMode.Zero, BlendFunction.Add);

        public static BlendState AlphaBlend = new BlendState(BlendMode.SourceAlpha, BlendMode.InverseSourceAlpha, BlendFunction.Add,
            BlendMode.SourceAlpha, BlendMode.InverseSourceAlpha, BlendFunction.Add);
    }
    #endregion

    #region Sampler states

    public enum TextureAddressMode
    {
        Clamp,
        Mirror,
        Wrap
    }

    public enum TextureFilter
    {
        Linear,
        Point,
        Anisoptropic,
        
        LinearMipPoint,
        PointMipLinear,
        MinLinearMagPointMipLinear,
        MinLinearMagPointMipPoint,
        MinPointMagLinearMipLinear,
        MinPointMagLinearMipPoint
    }

    public class SamplerStateCollection
    {
        private SamplerState[] _states;

        public SamplerStateCollection()
        {
            const int maxSamplers = 8;
            _states = new SamplerState[maxSamplers];

            for (int i = 0; i < maxSamplers; i++)
                _states[i] = SamplerState.PointWrap;
        }

        public SamplerState this[int index]
        {
            get { return _states[index]; }
            set 
            {
                SamplerState oldState = _states[index];
                _states[index] = value;

                if (SamplerChanged != null)
                    SamplerChanged(value, oldState, index);
            }
        }

        internal delegate void SamplerChangedHandler(SamplerState newState, SamplerState oldState, int index);
        internal event SamplerChangedHandler SamplerChanged;
    }

    public struct SamplerState
    {
        private TextureAddressMode _addressU;
        private TextureAddressMode _addressV;
        private TextureAddressMode _addressW;
        private TextureFilter _filter;

        public SamplerState(TextureAddressMode addressU, TextureAddressMode addressV, TextureAddressMode addressW, TextureFilter filter)
        {
            _addressU = addressU;
            _addressV = addressV;
            _addressW = addressW;
            _filter = filter;
        }

        public TextureAddressMode AddressU
        {
            get { return _addressU; }
            set { _addressU = value; }
        }

        public TextureAddressMode AddressV
        {
            get { return _addressV; }
            set { _addressV = value; }
        }

        public TextureAddressMode AddressW
        {
            get { return _addressW; }
            set { _addressW = value; }
        }

        public TextureFilter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public static SamplerState PointWrap =
            new SamplerState(TextureAddressMode.Wrap, TextureAddressMode.Wrap, TextureAddressMode.Wrap, TextureFilter.Point);
        public static SamplerState PointClamp =
            new SamplerState(TextureAddressMode.Clamp, TextureAddressMode.Clamp, TextureAddressMode.Clamp, TextureFilter.Point);
        public static SamplerState LinearWrap =
            new SamplerState(TextureAddressMode.Wrap, TextureAddressMode.Wrap, TextureAddressMode.Wrap, TextureFilter.Linear);
        public static SamplerState LinearClamp =
            new SamplerState(TextureAddressMode.Clamp, TextureAddressMode.Clamp, TextureAddressMode.Clamp, TextureFilter.Linear);
    }
    #endregion

    public interface IRenderer
    {
        ZClipMode ZClipMode { get; }

        bool BlendEnabled { get; set; }
        bool DepthTestEnabled { get; set; }
        bool DepthWriteEnabled { get; set; }
        CullMode CullMode { get; set; }
        Viewport Viewport { get; set; }

        TextureResource CreateTexture(string name, System.IO.Stream stream);
        TextureResource CreateTexture(string name, System.IO.Stream stream, bool clamp);
        TextureResource CreateTexture(string name, System.IO.Stream colorStream, System.IO.Stream alphaStream);
        UpdatableTexture CreateUpdatableTexture(string name, int width, int height);

        [Obsolete]
        CubeMapResource CreateCubeMap(string name, string front, string back, string left, string right,
            string up, string down);
        CubeMapResource CreateCubeMap(string name, BitmapSurface front, BitmapSurface back, BitmapSurface left, BitmapSurface right,
            BitmapSurface up, BitmapSurface down);
        Effect CreateEffect(string name, System.IO.Stream stream);

        TextureResource DefaultTexture { get; }
        TextureResource ErrorTexture { get; }

        VertexBuffer CreateVertexBuffer<T>(VertexBufferUsage usage, T[] data, int numVertices, VertexElementSet declaration) where T: struct;
        IndexBuffer CreateIndexBuffer(uint[] data);

        BlendState BlendState { get; set; }
        SamplerStateCollection SamplerStates { get; }

        IndexBuffer Indices { get; set; }
        void SetVertexBuffer(VertexBuffer buffer);

        void RenderPrimitives(int startVertex, int numVertices);
        void RenderIndexedPrimitives(int startIndex, int numPrimitives);

        [Obsolete]
        void RenderPrimitives<T>(PrimitiveType type, int startIndex, int vertexCount, T[] vertices, VertexElementSet declaration) where T: struct;

        [Obsolete]
        void RenderIndices<T>(PrimitiveType type, int startIndex, int vertexCount, int[] indices, T[] vertices, VertexElementSet declaration) where T: struct;
        

        void BeginScene();
        void EndScene();
        void Clear();

        RenderTarget CreateRenderTarget(int width, int height);
        void SetRenderTarget(RenderTarget target);

        // caps
        bool RenderToTextureSupported { get; }

        string ShaderFilenameSuffix { get; }

        RenderWindow ParentWindow { get; }
    }

    public static class RendererManager
    {
        private static IRenderer _renderer = null;

        // HACK: too much stuff depends on this property. Plus it should be read-only.
        public static IRenderer CurrentRenderer
        {
            get { return _renderer; }
            set { _renderer = value; }
        }
    }
}
