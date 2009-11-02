using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public class BoundingBoxRenderer
    {
        private static Effect _effect;
        private static float[] _vertices;
        private static int[] _indices;

        static BoundingBoxRenderer()
        {
            _vertices = new float[16 * 3];
            _indices = new int[6 * 4];

            // set up the indices
            _indices[0] = 0;
            _indices[1] = 1;
            _indices[2] = 2;
            _indices[3] = 3;
            _indices[4] = 0;
            
        }

        public static void Render(Camera camera, Math.Vector3 offset, float[] boundingbox)
        {
            // TODO: ewww, this could be better...

            // side
            _vertices[0 * 3 + 0] = boundingbox[0] + offset.X;
            _vertices[0 * 3 + 1] = boundingbox[1] + offset.Y;
            _vertices[0 * 3 + 2] = boundingbox[2] + offset.Z;

            _vertices[1 * 3 + 0] = boundingbox[0] + offset.X;
            _vertices[1 * 3 + 1] = boundingbox[4] + offset.Y;
            _vertices[1 * 3 + 2] = boundingbox[2] + offset.Z;

            _vertices[2 * 3 + 0] = boundingbox[0] + offset.X;
            _vertices[2 * 3 + 1] = boundingbox[4] + offset.Y;
            _vertices[2 * 3 + 2] = boundingbox[5] + offset.Z;

            _vertices[3 * 3 + 0] = boundingbox[0] + offset.X;
            _vertices[3 * 3 + 1] = boundingbox[1] + offset.Y;
            _vertices[3 * 3 + 2] = boundingbox[5] + offset.Z;

            // side
            _vertices[4 * 3 + 0] = boundingbox[0] + offset.X;
            _vertices[4 * 3 + 1] = boundingbox[1] + offset.Y;
            _vertices[4 * 3 + 2] = boundingbox[2] + offset.Z;

            _vertices[5 * 3 + 0] = boundingbox[3] + offset.X;
            _vertices[5 * 3 + 1] = boundingbox[1] + offset.Y;
            _vertices[5 * 3 + 2] = boundingbox[2] + offset.Z;

            _vertices[6 * 3 + 0] = boundingbox[3] + offset.X;
            _vertices[6 * 3 + 1] = boundingbox[1] + offset.Y;
            _vertices[6 * 3 + 2] = boundingbox[5] + offset.Z;

            _vertices[7 * 3 + 0] = boundingbox[0] + offset.X;
            _vertices[7 * 3 + 1] = boundingbox[1] + offset.Y;
            _vertices[7 * 3 + 2] = boundingbox[5] + offset.Z;

            // side
            _vertices[8 * 3 + 0] = boundingbox[3] + offset.X;
            _vertices[8 * 3 + 1] = boundingbox[1] + offset.Y;
            _vertices[8 * 3 + 2] = boundingbox[2] + offset.Z;

            _vertices[9 * 3 + 0] = boundingbox[3] + offset.X;
            _vertices[9 * 3 + 1] = boundingbox[4] + offset.Y;
            _vertices[9 * 3 + 2] = boundingbox[2] + offset.Z;

            _vertices[10 * 3 + 0] = boundingbox[3] + offset.X;
            _vertices[10 * 3 + 1] = boundingbox[4] + offset.Y;
            _vertices[10 * 3 + 2] = boundingbox[5] + offset.Z;

            _vertices[11 * 3 + 0] = boundingbox[3] + offset.X;
            _vertices[11 * 3 + 1] = boundingbox[1] + offset.Y;
            _vertices[11 * 3 + 2] = boundingbox[5] + offset.Z;

            // side
            _vertices[12 * 3 + 0] = boundingbox[0] + offset.X;
            _vertices[12 * 3 + 1] = boundingbox[4] + offset.Y;
            _vertices[12 * 3 + 2] = boundingbox[2] + offset.Z;

            _vertices[13 * 3 + 0] = boundingbox[3] + offset.X;
            _vertices[13 * 3 + 1] = boundingbox[4] + offset.Y;
            _vertices[13 * 3 + 2] = boundingbox[2] + offset.Z;

            _vertices[14 * 3 + 0] = boundingbox[3] + offset.X;
            _vertices[14 * 3 + 1] = boundingbox[4] + offset.Y;
            _vertices[14 * 3 + 2] = boundingbox[5] + offset.Z;

            _vertices[15 * 3 + 0] = boundingbox[0] + offset.X;
            _vertices[15 * 3 + 1] = boundingbox[4] + offset.Y;
            _vertices[15 * 3 + 2] = boundingbox[5] + offset.Z;

            RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.LineStrip, 0, 8, _vertices);
            RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.LineStrip, 8, 8, _vertices);
        }

    }
}
