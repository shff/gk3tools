using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public struct AxisAlignedBoundingBox
    {
        public Math.Vector3 Min;
        public Math.Vector3 Max;

        private Math.Vector3[] _vertices;
        private static int[] _indices;
        private static Effect _effect;
        private static VertexElementSet _declaration;

        public static void Init()
        {
            _indices = new int[]
            {
                // bottom
                0, 1,
                1, 2,
                2, 3,
                3, 0,

                // top
                4, 5,
                5, 6,
                6, 7,
                7, 4,

                // sides
                0, 4,
                1, 5,
                2, 6,
                3, 7
            };

            _effect = Resource.ResourceManager.Global.Load<Effect>("wireframe.fx");

            _declaration = new VertexElementSet(new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Float3, VertexElementUsage.Position, 0)
            });
        }

        public AxisAlignedBoundingBox(Math.Vector3 min, Math.Vector3 max)
        {
            Min = min;
            Max = max;

            _vertices = null;
        }

        public AxisAlignedBoundingBox(float[] points)
        {
            Min.X = points[0];
            Min.Y = points[1];
            Min.Z = points[2];

            Max.X = points[3];
            Max.Y = points[4];
            Max.Z = points[5];

            _vertices = null;
        }

        public Math.Vector3 Center
        {
            get { return (Max + Min) * 0.5f; }
        }

        public void Render(Camera camera, Math.Matrix world)
        {
            if (_vertices == null)
                createVertices();

            Math.Matrix modelViewProjection = world * camera.ViewProjection;

            _effect.Bind();
            _effect.SetParameter("ModelViewProjection", modelViewProjection);
            _effect.Begin();

            //RendererManager.CurrentRenderer.RenderIndices(PrimitiveType.Lines, 0, 8, _indices, _vertices, _declaration);
            //RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.Lines, 0, 8, _vertices);
            //RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.Lines, 8, 8, _vertices);

            _effect.End();
        }

        public AxisAlignedBoundingBox Transform(Math.Matrix transform)
        {
            Math.Vector3 transformedBBMin = transform * Min;
            Math.Vector3 transformedBBMax = transform * Max;

            AxisAlignedBoundingBox newBBox = new AxisAlignedBoundingBox();
            newBBox.Min.X = System.Math.Min(transformedBBMin.X, transformedBBMax.X);
            newBBox.Min.Y = System.Math.Min(transformedBBMin.Y, transformedBBMax.Y);
            newBBox.Min.Z = System.Math.Min(transformedBBMin.Z, transformedBBMax.Z);
            newBBox.Max.X = System.Math.Max(transformedBBMin.X, transformedBBMax.X);
            newBBox.Max.Y = System.Math.Max(transformedBBMin.Y, transformedBBMax.Y);
            newBBox.Max.Z = System.Math.Max(transformedBBMin.Z, transformedBBMax.Z);

            return newBBox;
        }

        public bool TestRayAABBCollision(Math.Vector3 position, Math.Vector3 origin, Math.Vector3 direction, out float distance)
        {
            // TODO: the code really should be moved in here, rather
            // than in a utility method somewhere else

            float[] aabb = new float[] {Min.X, Min.Y, Min.Z, Max.X, Max.Y, Max.Z};
            return Gk3Main.Utils.TestRayAABBCollision(position, origin, direction, aabb, out distance);
        }

        private void createVertices()
        {
            _vertices = new Math.Vector3[8];

            // bottom
            _vertices[0].X = Min.X;
            _vertices[0].Y = Min.Y;
            _vertices[0].Z = Min.Z;

            _vertices[1].X = Max.X;
            _vertices[1].Y = Min.Y;
            _vertices[1].Z = Min.Z;

            _vertices[2].X = Max.X;
            _vertices[2].Y = Min.Y;
            _vertices[2].Z = Max.Z;

            _vertices[3].X = Min.X;
            _vertices[3].Y = Min.Y;
            _vertices[3].Z = Max.Z;

            // top
            _vertices[4].X = Min.X;
            _vertices[4].Y = Max.Y;
            _vertices[4].Z = Min.Z;

            _vertices[5].X = Max.X;
            _vertices[5].Y = Max.Y;
            _vertices[5].Z = Min.Z;

            _vertices[6].X = Max.X;
            _vertices[6].Y = Max.Y;
            _vertices[6].Z = Max.Z;

            _vertices[7].X = Min.X;
            _vertices[7].Y = Max.Y;
            _vertices[7].Z = Max.Z;
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
