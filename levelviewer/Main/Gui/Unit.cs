using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public struct Unit
    {
        public Unit(float scale, int offset)
        {
            Scale = scale;
            Offset = offset;
        }

        public float Scale;
        public int Offset;
    }
}
