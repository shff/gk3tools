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

    public struct MouseState
    {
        public int X, Y, Wheel;
        public bool LeftButton, MiddleButton, RightButton;
    }

    public abstract class RenderWindow
    {
        public abstract IRenderer CreateRenderer();
        public abstract void Present();
        public abstract void Resize(int width, int height);

        public abstract List<DisplayMode> GetSupportedDisplayModes();
        public abstract IRenderer Renderer { get; }

        public abstract bool ProcessEvents();
        public abstract void Close();

        public abstract IntPtr Handle { get; }

        public abstract void GetPosition(out int x, out int y);
        public abstract MouseState GetMouseState(); 

        public event EventHandler<EventArgs> Closed;
    }

    public abstract class Direct3D9RenderWindow : RenderWindow
    {
    }

    public abstract class OpenGLRenderWindow : RenderWindow
    {
    }
}
