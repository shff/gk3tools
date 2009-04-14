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
    }

    public interface IRenderer
    {
        bool BlendEnabled { get; set; }
        bool AlphaTestEnabled { get; set; }
        bool DepthTestEnabled { get; set; }
        CullMode CullMode { get; set; }
        Viewport Viewport { get; set; }
    }
}
