using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public static class BillboardManager
    {
        private struct Billboard
        {
            public Math.Vector3 Position;
            public float Width, Height;
            public float U, V, USize, VSize;
            public TextureResource Texture;
        }

        private struct BillboardVertex
        {
            public float X, Y, Z;
            public float CornerX, CornerY;
        }

        private const int _maxBillboards = 50;
        private static Billboard[] _billboards = new Billboard[_maxBillboards];
        private static int _numBillboards;

        private static Effect _shader;
        private const int _billboardVertexStride = 4 * 7;
        private static VertexElementSet _elements;
        private static float[] _vertices = new float[_maxBillboards * _billboardVertexStride];
        private static int[] _indices = new int[_maxBillboards * 6];

        static BillboardManager()
        {
            // create the indices
            for (int i = 0; i < _maxBillboards; i++)
            {
                _indices[i * 6 + 0] = i * 4 + 0;
                _indices[i * 6 + 1] = i * 4 + 1;
                _indices[i * 6 + 2] = i * 4 + 2;

                _indices[i * 6 + 3] = i * 4 + 2;
                _indices[i * 6 + 4] = i * 4 + 1;
                _indices[i * 6 + 5] = i * 4 + 3;
            }

            // create the elements
            _elements = new VertexElementSet(new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Float3, VertexElementUsage.Position, 0),
                new VertexElement(3 * sizeof(float), VertexElementFormat.Float2, VertexElementUsage.TexCoord, 0),
                new VertexElement(5 * sizeof(float), VertexElementFormat.Float2, VertexElementUsage.TexCoord, 1)
            });
        }

        public static void AddBillboard(Math.Vector3 position, float width, float height,
            TextureResource texture)
        {
            if (_numBillboards < _maxBillboards - 1)
            {
                _billboards[_numBillboards].Position = position;
                _billboards[_numBillboards].Width = width;
                _billboards[_numBillboards].Height = height;
                _billboards[_numBillboards].Texture = texture;
                _billboards[_numBillboards].U = 0;
                _billboards[_numBillboards].V = 0;
                _billboards[_numBillboards].USize = 1.0f;
                _billboards[_numBillboards].VSize = 1.0f;

                _numBillboards++;
            }
        }

        public static void RenderBillboards(Camera camera)
        {
            if (_shader == null)
                _shader = (Effect)Resource.ResourceManager.Load("texturedBillboard.fx");

            // setup the vertex buffer
            // vertices are stored in this format:
            // position x,y,z | corner offset x,y
            for (int i = 0; i < _numBillboards; i++)
            {
                _vertices[i * _billboardVertexStride + 0] = _billboards[i].Position.X;
                _vertices[i * _billboardVertexStride + 1] = _billboards[i].Position.Y; 
                _vertices[i * _billboardVertexStride + 2] = _billboards[i].Position.Z;
                _vertices[i * _billboardVertexStride + 3] = -_billboards[i].Width * 0.5f;
                _vertices[i * _billboardVertexStride + 4] = _billboards[i].Height * 0.5f;
                _vertices[i * _billboardVertexStride + 5] = 0;
                _vertices[i * _billboardVertexStride + 6] = 0;

                _vertices[i * _billboardVertexStride + 7] = _billboards[i].Position.X;
                _vertices[i * _billboardVertexStride + 8] = _billboards[i].Position.Y; 
                _vertices[i * _billboardVertexStride + 9] = _billboards[i].Position.Z;
                _vertices[i * _billboardVertexStride + 10] = -_billboards[i].Width * 0.5f;
                _vertices[i * _billboardVertexStride + 11] = -_billboards[i].Height * 0.5f;
                _vertices[i * _billboardVertexStride + 12] = 0;
                _vertices[i * _billboardVertexStride + 13] = 1.0f;

                _vertices[i * _billboardVertexStride + 14] = _billboards[i].Position.X;
                _vertices[i * _billboardVertexStride + 15] = _billboards[i].Position.Y; 
                _vertices[i * _billboardVertexStride + 16] = _billboards[i].Position.Z;
                _vertices[i * _billboardVertexStride + 17] = _billboards[i].Width * 0.5f;
                _vertices[i * _billboardVertexStride + 18] = _billboards[i].Height * 0.5f;
                _vertices[i * _billboardVertexStride + 19] = 1.0f;
                _vertices[i * _billboardVertexStride + 20] = 0;

                _vertices[i * _billboardVertexStride + 21] = _billboards[i].Position.X;
                _vertices[i * _billboardVertexStride + 22] = _billboards[i].Position.Y; 
                _vertices[i * _billboardVertexStride + 23] = _billboards[i].Position.Z;
                _vertices[i * _billboardVertexStride + 24] = _billboards[i].Width * 0.5f;
                _vertices[i * _billboardVertexStride + 25] = -_billboards[i].Height * 0.5f;
                _vertices[i * _billboardVertexStride + 26] = 1.0f;
                _vertices[i * _billboardVertexStride + 27] = 1.0f;
            }

            RendererManager.CurrentRenderer.CullMode = CullMode.None;
            RendererManager.CurrentRenderer.AlphaTestEnabled = true;

            _shader.SetParameter("ModelView", camera.ModelView);
            _shader.SetParameter("Projection", camera.Projection);

            
            _shader.Begin();
            _shader.BeginPass(0);

            for (int i = 0; i < _numBillboards; i++)
            {
                _billboards[i].Texture.Bind();
                RendererManager.CurrentRenderer.RenderIndices(_elements, PrimitiveType.Triangles, i * 6, 6, _indices, _vertices);
            }

            _shader.EndPass();
            _shader.End();

            RendererManager.CurrentRenderer.CullMode = CullMode.CounterClockwise;
            RendererManager.CurrentRenderer.AlphaTestEnabled = false;

            // reset the billboard list
            _numBillboards = 0;
        }
    }
}
