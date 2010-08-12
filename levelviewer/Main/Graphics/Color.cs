using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    struct Color
    {
        public byte R, G, B, A;

        public Color(int r, int g, int b)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = 255;
        }

        public Color(int r, int g, int b, int a)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = (byte)a;
        }

        public Color(float r, float g, float b)
        {
            R = (byte)(r * 255);
            G = (byte)(g * 255);
            B = (byte)(b * 255);
            A = 255;
        }

        public Color(float r, float g, float b, float a)
        {
            R = (byte)(r * 255);
            G = (byte)(g * 255);
            B = (byte)(b * 255);
            A = (byte)(a * 255);
        }

        public static bool operator ==(Color c1, Color c2)
        {
            return c1.R == c2.R &&
                c1.G == c2.G &&
                c1.B == c2.B &&
                c1.A == c2.A;
        }

        public static bool operator !=(Color c1, Color c2)
        {
            return c1.R != c2.R ||
                c1.G != c2.G ||
                c1.B != c2.B ||
                c1.A != c2.A;
        }

        public static Color White { get { return new Color(255, 255, 255); } }
        public static Color Black { get { return new Color(0, 0, 0); } }
        public static Color Blue { get { return new Color(0, 0, 255); } }
        public static Color Green { get { return new Color(0, 255, 0); } }
        public static Color Red { get { return new Color(255, 0, 0); } }
        public static Color Yellow { get { return new Color(255, 255, 0); } }
        public static Color Purple { get { return new Color(255, 0, 255); } }
        public static Color Gray { get { return new Color(128, 128, 128); } }
        public static Color Teal { get { return new Color(0, 255, 255); } }
    }
}
