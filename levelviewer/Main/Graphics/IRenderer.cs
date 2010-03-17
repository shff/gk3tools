using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public enum CullMode
    {
        Clockwise,
        CounterClockwise,
        None
    }

    public enum PrimitiveType
    {
        Triangles,
        LineStrip
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

    public enum BlendMode
    {
        Zero,
        One,
        SourceAlpha,
        InverseSourceAlpha,
        DestinationAlpha,
        InverseDestinationAlpha
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

        public abstract void Begin();
        public abstract void End();

        public abstract void BeginPass(int index);
        public abstract void EndPass();

        public abstract void SetParameter(string name, float parameter);
        public abstract void SetParameter(string name, Math.Vector4 parameter);
        public abstract void SetParameter(string name, Math.Matrix parameter);
        public abstract void SetParameter(string name, TextureResource parameter);
        public abstract void SetParameter(string name, CubeMapResource parameter);
        public abstract void UpdatePassParameters();

        public abstract void EnableTextureParameter(string name);
        public abstract void DisableTextureParameter(string name);
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

        public Gk3Main.Resource.Resource Load(string filename)
        {
            System.IO.Stream stream = FileSystem.Open(filename);

            IRenderer renderer = RendererManager.CurrentRenderer;
            Effect resource = renderer.CreateEffect(filename, stream);

            stream.Close();

            return resource;
        }

        #endregion
    }

    public abstract class VertexBuffer : IDisposable
    {
        protected int _stride;

        public abstract void Dispose();

        public int Stride { get { return _stride; } }
        public abstract int Length { get; }
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
            Direct3D9Renderer renderer = RendererManager.CurrentRenderer as Direct3D9Renderer;

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

    public interface IRenderer
    {
        bool BlendEnabled { get; set; }
        bool AlphaTestEnabled { get; set; }
        bool DepthTestEnabled { get; set; }
        bool DepthWriteEnabled { get; set; }
        CullMode CullMode { get; set; }
        Viewport Viewport { get; set; }
        CompareFunction AlphaTestFunction { get; set; }
        float AlphaTestReference { get; set; }

        TextureResource CreateTexture(string name, System.IO.Stream stream);
        TextureResource CreateTexture(string name, System.IO.Stream stream, bool clamp);
        TextureResource CreateTexture(string name, System.IO.Stream colorStream, System.IO.Stream alphaStream);
        UpdatableTexture CreateUpdatableTexture(string name, int width, int height);
        CubeMapResource CreateCubeMap(string name, string front, string back, string left, string right,
            string up, string down);
        Effect CreateEffect(string name, System.IO.Stream stream);

        TextureResource DefaultTexture { get; }
        TextureResource ErrorTexture { get; }

        VertexBuffer CreateVertexBuffer(float[] data, int stride);
        IndexBuffer CreateIndexBuffer(uint[] data);

        void SetBlendFunctions(BlendMode source, BlendMode destination);

        void RenderBuffers(VertexBuffer vertices, IndexBuffer indices);
        void RenderPrimitives(PrimitiveType type, int startIndex, int count, float[] vertices);

        void RenderIndices(PrimitiveType type, int startIndex, int primitiveCount, int[] indices, float[] vertices);

        VertexElementSet VertexDeclaration { set; }

        void BeginScene();
        void EndScene();
        void Clear();

        RenderTarget CreateRenderTarget(int width, int height);
        void SetRenderTarget(RenderTarget target);

        // caps
        bool RenderToTextureSupported { get; }
    }
}
