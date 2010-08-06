using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public struct DisplayMode
    {
        public DisplayMode(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width;
        public int Height;
    }

    public abstract class RenderWindow
    {
        public abstract IRenderer CreateRenderer();
        public abstract void Present();

        public abstract List<DisplayMode> GetSupportedDisplayModes();
    }

    public abstract class Direct3D9RenderWindow : RenderWindow
    {
    }

    public abstract class OpenGLRenderWindow : RenderWindow
    {
    }
}
