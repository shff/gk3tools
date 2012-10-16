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
            public TextureResource Texture2;
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
        private static Effect _alphaShader;
        private const int _billboardVertexStride = 4 * 7;
        private static VertexElementSet _elements;
        private static VertexBuffer _vertices;
        private static IndexBuffer _indices;
        private static float[] _workingVertices = new float[_maxBillboards * _billboardVertexStride];

        public static void Init(Resource.ResourceManager globalContent)
        {
            // create the indices
            uint[] indices = new uint[_maxBillboards * 6];
            for (uint i = 0; i < _maxBillboards; i++)
            {
                indices[i * 6 + 0] = i * 4 + 0;
                indices[i * 6 + 1] = i * 4 + 1;
                indices[i * 6 + 2] = i * 4 + 2;
                
                indices[i * 6 + 3] = i * 4 + 2;
                indices[i * 6 + 4] = i * 4 + 1;
                indices[i * 6 + 5] = i * 4 + 3;
            }

            // create the elements
            _elements = new VertexElementSet(new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Float3, VertexElementUsage.Position, 0),
                new VertexElement(3 * sizeof(float), VertexElementFormat.Float2, VertexElementUsage.TexCoord, 0),
                new VertexElement(5 * sizeof(float), VertexElementFormat.Float2, VertexElementUsage.TexCoord, 1)
            });

            _vertices = RendererManager.CurrentRenderer.CreateVertexBuffer(VertexBufferUsage.Dynamic, (float[])null, _maxBillboards * _billboardVertexStride, _elements);
            _indices = RendererManager.CurrentRenderer.CreateIndexBuffer(indices);

            _shader = globalContent.Load<Effect>("texturedBillboard.fx");

            try
            {
                _alphaShader = globalContent.Load<Effect>("radiosity_omnilight.fx");
            }
            catch (System.IO.FileNotFoundException)
            {
                // oh well, that shader just isn't available
                // (this will happen if using Direct3D renderer)
            }
        }

        public static void AddBillboard(Math.Vector3 position, float width, float height,
            TextureResource texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");

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

        internal static void AddBillboard(Math.Vector3 position, float width, float height,
            TextureResource diffuse, TextureResource alpha)
        {
            if (diffuse == null)
                throw new ArgumentNullException("diffuse");

            if (_numBillboards < _maxBillboards - 1)
            {
                _billboards[_numBillboards].Position = position;
                _billboards[_numBillboards].Width = width;
                _billboards[_numBillboards].Height = height;
                _billboards[_numBillboards].Texture = diffuse;
                _billboards[_numBillboards].Texture2 = alpha;
                _billboards[_numBillboards].U = 0;
                _billboards[_numBillboards].V = 0;
                _billboards[_numBillboards].USize = 1.0f;
                _billboards[_numBillboards].VSize = 1.0f;

                _numBillboards++;
            }
        }

        public static void RenderBillboards(Camera camera)
        {
            render(camera, false);
        }

        internal static void RenderBillboardsWithAlpha(Camera camera)
        {
            render(camera, true);
        }

        private static void render(Camera camera, bool useAlphaShader)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            // setup the vertex buffer
            // vertices are stored in this format:
            // position x,y,z | corner offset x,y
            for (int i = 0; i < _numBillboards; i++)
            {
                _workingVertices[i * _billboardVertexStride + 0] = _billboards[i].Position.X;
                _workingVertices[i * _billboardVertexStride + 1] = _billboards[i].Position.Y;
                _workingVertices[i * _billboardVertexStride + 2] = _billboards[i].Position.Z;
                _workingVertices[i * _billboardVertexStride + 3] = -_billboards[i].Width * 0.5f;
                _workingVertices[i * _billboardVertexStride + 4] = _billboards[i].Height * 0.5f;
                _workingVertices[i * _billboardVertexStride + 5] = 0;
                _workingVertices[i * _billboardVertexStride + 6] = 0;

                _workingVertices[i * _billboardVertexStride + 7] = _billboards[i].Position.X;
                _workingVertices[i * _billboardVertexStride + 8] = _billboards[i].Position.Y;
                _workingVertices[i * _billboardVertexStride + 9] = _billboards[i].Position.Z;
                _workingVertices[i * _billboardVertexStride + 10] = -_billboards[i].Width * 0.5f;
                _workingVertices[i * _billboardVertexStride + 11] = -_billboards[i].Height * 0.5f;
                _workingVertices[i * _billboardVertexStride + 12] = 0;
                _workingVertices[i * _billboardVertexStride + 13] = 1.0f;

                _workingVertices[i * _billboardVertexStride + 14] = _billboards[i].Position.X;
                _workingVertices[i * _billboardVertexStride + 15] = _billboards[i].Position.Y;
                _workingVertices[i * _billboardVertexStride + 16] = _billboards[i].Position.Z;
                _workingVertices[i * _billboardVertexStride + 17] = _billboards[i].Width * 0.5f;
                _workingVertices[i * _billboardVertexStride + 18] = _billboards[i].Height * 0.5f;
                _workingVertices[i * _billboardVertexStride + 19] = 1.0f;
                _workingVertices[i * _billboardVertexStride + 20] = 0;

                _workingVertices[i * _billboardVertexStride + 21] = _billboards[i].Position.X;
                _workingVertices[i * _billboardVertexStride + 22] = _billboards[i].Position.Y;
                _workingVertices[i * _billboardVertexStride + 23] = _billboards[i].Position.Z;
                _workingVertices[i * _billboardVertexStride + 24] = _billboards[i].Width * 0.5f;
                _workingVertices[i * _billboardVertexStride + 25] = -_billboards[i].Height * 0.5f;
                _workingVertices[i * _billboardVertexStride + 26] = 1.0f;
                _workingVertices[i * _billboardVertexStride + 27] = 1.0f;
            }

            CullMode originalCullMode = RendererManager.CurrentRenderer.CullMode;
            RendererManager.CurrentRenderer.CullMode = CullMode.None;

            _vertices.UpdateData(_workingVertices, _numBillboards * 4);

            RendererManager.CurrentRenderer.Indices = _indices;
            RendererManager.CurrentRenderer.SetVertexBuffer(_vertices);

            Effect shader;
            

            for (int i = 0; i < _numBillboards; i++)
            {
                if (!useAlphaShader || _billboards[i].Texture2 == null)
                    shader = _shader;
                else
                    shader = _alphaShader;

                shader.Bind();
                shader.SetParameter("ModelView", camera.View);
                shader.SetParameter("Projection", camera.Projection);
                shader.SetParameter("Diffuse", _billboards[i].Texture, 0);

                if (useAlphaShader)
                    shader.SetParameter("Alpha", _billboards[i].Texture2, 1);

                shader.Begin();

                RendererManager.CurrentRenderer.RenderIndexedPrimitives(i * 6, 2);

                shader.End();
            }

            RendererManager.CurrentRenderer.CullMode = originalCullMode;

            // reset the billboard list
            _numBillboards = 0;
        }
    }
}
