using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Math
{
    public struct Plane2D
    {
        public Math.Vector2 Normal;
        public float Distance;

        public static Plane2D CreateFromEdge(Vector2 p1, Vector2 p2)
        {
            Plane2D result;
            
            // calc the distance
            Math.Vector2 line = (p1 - p2);
            Math.Vector2 w = Math.Vector2.Zero - p2;
            float c1 = w.Dot(line);
            float c2 = line.Dot(line);
            float b = c1 / c2;
            Math.Vector2 pb = p2 + line * b;
            result.Distance = pb.Length;
            
            // calc the normal
            result.Normal = line.Normalize();
            float t = result.Normal.X;
            result.Normal.X = result.Normal.Y;
            result.Normal.Y = -t;

            return result;
        }

        public static bool IsPointInFrontOfPlane(Plane2D plane, Vector2 point)
        {
            return plane.Normal.Dot(point) + plane.Distance >= 0;
        }
    }
}
