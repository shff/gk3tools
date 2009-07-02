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
            _vertices = new float[8 * 3];
            _indices = new int[6 * 4];

            // set up the indices
            _indices[0] = 0;
            _indices[1] = 1;
            _indices[2] = 2;
            _indices[3] = 3;

            
        }

        public static void Render(Camera camera, float[] boundingbox)
        {
            RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.LineStrip, 0, 2, boundingbox);
        }

    }
}
