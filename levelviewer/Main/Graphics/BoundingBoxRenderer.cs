using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public class AxisAlignedBoundingBox
    {
        public Math.Vector3 Min;
        public Math.Vector3 Max;

        private float[] _vertices;
        private static int[] _indices;
        private static Effect _effect;

        static AxisAlignedBoundingBox()
        {
            _indices = new int[6 * 4];

            // set up the indices
            _indices[0] = 0;
            _indices[1] = 1;
            _indices[2] = 2;
            _indices[3] = 3;
            _indices[4] = 0;
        }

        public AxisAlignedBoundingBox(float[] points)
        {
            Min.X = points[0];
            Min.Y = points[1];
            Min.Z = points[2];

            Max.X = points[3];
            Max.Y = points[4];
            Max.Z = points[5];

            // side
            _vertices = new float[16 * 3];
            _vertices[0 * 3 + 0] = Min.X;
            _vertices[0 * 3 + 1] = Min.Y;
            _vertices[0 * 3 + 2] = Min.Z;

            _vertices[1 * 3 + 0] = points[0];
            _vertices[1 * 3 + 1] = points[4];
            _vertices[1 * 3 + 2] = points[2];

            _vertices[2 * 3 + 0] = points[0];
            _vertices[2 * 3 + 1] = points[4];
            _vertices[2 * 3 + 2] = points[5];

            _vertices[3 * 3 + 0] = points[0];
            _vertices[3 * 3 + 1] = points[1];
            _vertices[3 * 3 + 2] = points[5];

            // side
            _vertices[4 * 3 + 0] = points[0];
            _vertices[4 * 3 + 1] = points[1];
            _vertices[4 * 3 + 2] = points[2];

            _vertices[5 * 3 + 0] = points[3];
            _vertices[5 * 3 + 1] = points[1];
            _vertices[5 * 3 + 2] = points[2];

            _vertices[6 * 3 + 0] = points[3];
            _vertices[6 * 3 + 1] = points[1];
            _vertices[6 * 3 + 2] = points[5];

            _vertices[7 * 3 + 0] = points[0];
            _vertices[7 * 3 + 1] = points[1];
            _vertices[7 * 3 + 2] = points[5];

            // side
            _vertices[8 * 3 + 0] = points[3];
            _vertices[8 * 3 + 1] = points[1];
            _vertices[8 * 3 + 2] = points[2];

            _vertices[9 * 3 + 0] = points[3];
            _vertices[9 * 3 + 1] = points[4];
            _vertices[9 * 3 + 2] = points[2];

            _vertices[10 * 3 + 0] = points[3];
            _vertices[10 * 3 + 1] = points[4];
            _vertices[10 * 3 + 2] = points[5];

            _vertices[11 * 3 + 0] = points[3];
            _vertices[11 * 3 + 1] = points[1];
            _vertices[11 * 3 + 2] = points[5];

            // side
            _vertices[12 * 3 + 0] = points[0];
            _vertices[12 * 3 + 1] = points[4];
            _vertices[12 * 3 + 2] = points[2];

            _vertices[13 * 3 + 0] = points[3];
            _vertices[13 * 3 + 1] = points[4];
            _vertices[13 * 3 + 2] = points[2];

            _vertices[14 * 3 + 0] = points[3];
            _vertices[14 * 3 + 1] = points[4];
            _vertices[14 * 3 + 2] = points[5];

            _vertices[15 * 3 + 0] = points[0];
            _vertices[15 * 3 + 1] = points[4];
            _vertices[15 * 3 + 2] = points[5];
        }

        public void Render(Camera camera, Math.Matrix world)
        {
            if (_effect == null)
                _effect = (Effect)Resource.ResourceManager.Load("wireframe.fx");

            Math.Matrix modelViewProjection = world * camera.ViewProjection;

            _effect.SetParameter("ModelViewProjection", modelViewProjection);
            _effect.Begin();
            _effect.BeginPass(0);

            RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.LineStrip, 0, 8, _vertices);
            RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.LineStrip, 8, 8, _vertices);

            _effect.EndPass();
            _effect.End();
        }
    }

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
            AxisAlignedBoundingBox aabb = new AxisAlignedBoundingBox(boundingbox);
            aabb.Render(camera, Math.Matrix.Translate(offset));
        }

    }
}
