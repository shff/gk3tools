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

        public abstract void SetParameter(string name, Math.Vector4 parameter);
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

    public interface IRenderer
    {
        bool BlendEnabled { get; set; }
        bool AlphaTestEnabled { get; set; }
        bool DepthTestEnabled { get; set; }
        CullMode CullMode { get; set; }
        Viewport Viewport { get; set; }

        Effect CreateEffect(string name, System.IO.Stream stream);
    }
}
