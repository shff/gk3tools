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

    public struct VertexElementSet
    {
        public VertexElementSet(VertexElement[] elements)
        {
            Stride = 0;
            foreach (VertexElement element in elements)
            {
                Stride += element.Offset;
            }

            // add the size of the last element
            Stride += (int)elements[elements.Length - 1].Format * sizeof(float);

            Elements = elements;
        }

        public VertexElement[] Elements;
        public int Stride;
    }

    public interface IRenderer
    {
        bool BlendEnabled { get; set; }
        bool AlphaTestEnabled { get; set; }
        bool DepthTestEnabled { get; set; }
        CullMode CullMode { get; set; }
        Viewport Viewport { get; set; }
        CompareFunction AlphaTestFunction { get; set; }
        float AlphaTestReference { get; set; }

        Effect CreateEffect(string name, System.IO.Stream stream);

        VertexBuffer CreateVertexBuffer(float[] data, int stride);
        IndexBuffer CreateIndexBuffer(uint[] data);

        void RenderBuffers(VertexBuffer vertices, IndexBuffer indices);
        void RenderPrimitives(PrimitiveType type, int startIndex, int count, float[] vertices);

        void RenderIndices(VertexElementSet elements, PrimitiveType type, int startIndex, int count, int[] indices, float[] vertices);

        void Clear();
    }
}
