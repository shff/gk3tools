using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public abstract class RenderWindow
    {
        public abstract IRenderer CreateRenderer();
        public abstract void Present();
    }

    public abstract class Direct3D9RenderWindow : RenderWindow
    {
    }

    public abstract class OpenGLRenderWindow : RenderWindow
    {
    }
}
