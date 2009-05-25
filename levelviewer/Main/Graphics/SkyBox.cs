using System;

namespace Gk3Main.Graphics
{
    class SkyBox
    {
        private VertexBuffer _vertices;
        private IndexBuffer _indices;

        private CubeMapResource _cubeMap;
        private Effect _skyboxEffect;

        private float _azimuth;
        private const int _stride = 3;
        private const float _size = 500.0f;

        public SkyBox(string name, string front, string back, string left, string right,
            string up, string down, float azimuth)
        {
            _cubeMap = new CubeMapResource(name, front, back, left, right, up, down);
            _azimuth = azimuth;

            float[] vertices = new float[8 * _stride];

            Math.Vector3 rightDir = Math.Vector3.Right;
            Math.Vector3 forwardDir = Math.Vector3.Forward;
            Math.Vector3 upDir = Math.Vector3.Up;

            // front bottom left
            vectorAdd(-rightDir, -upDir, forwardDir, _size,
                out vertices[0 * _stride + 0], out vertices[0 * _stride + 1], out vertices[0 * _stride + 2]);

            // front top left
            vectorAdd(-rightDir, upDir, forwardDir, _size,
                out vertices[1 * _stride + 0], out vertices[1 * _stride + 1], out vertices[1 * _stride + 2]);

            // front bottom right
            vectorAdd(rightDir, -upDir, forwardDir, _size,
                out vertices[2 * _stride + 0], out vertices[2 * _stride + 1], out vertices[2 * _stride + 2]);

            // front top right
            vectorAdd(rightDir, upDir, forwardDir, _size,
                out vertices[3 * _stride + 0], out vertices[3 * _stride + 1], out vertices[3 * _stride + 2]);

            // back bottom left
            vectorAdd(-rightDir, -upDir, -forwardDir, _size,
                out vertices[4 * _stride + 0], out vertices[4 * _stride + 1], out vertices[4 * _stride + 2]);

            // back top left
            vectorAdd(-rightDir, upDir, -forwardDir, _size,
                out vertices[5 * _stride + 0], out vertices[5 * _stride + 1], out vertices[5 * _stride + 2]);

            // back bottom right
            vectorAdd(rightDir, -upDir, -forwardDir, _size,
                out vertices[6 * _stride + 0], out vertices[6 * _stride + 1], out vertices[6 * _stride + 2]);

            // back top right
            vectorAdd(rightDir, upDir, -forwardDir, _size,
                out vertices[7 * _stride + 0], out vertices[7 * _stride + 1], out vertices[7 * _stride + 2]);

            uint[] indices = new uint[]
            {
                // front
                0, 1, 2,
                1, 3, 2,

                // right
                2, 3, 7,
                2, 7, 6,

                // left
                5, 1, 0,
                5, 0, 4,

                // up
                1, 5, 3,
                3, 5, 7,

                // back
                4, 6, 7,
                4, 7, 5
            };

            _vertices = RendererManager.CurrentRenderer.CreateVertexBuffer(vertices, 0);
            _indices = RendererManager.CurrentRenderer.CreateIndexBuffer(indices);

            _skyboxEffect = (Effect)Resource.ResourceManager.Load("skybox.fx");
        }

        public void Render(Camera camera)
        {
            RendererManager.CurrentRenderer.DepthTestEnabled = false;

            _skyboxEffect.EnableTextureParameter("Diffuse");
            _skyboxEffect.SetParameter("Diffuse", _cubeMap);

            Math.Matrix modelViewProjection = Math.Matrix.Translate(camera.Position.X, camera.Position.Y, camera.Position.Z) * camera.ModelView * camera.Projection;
            _skyboxEffect.SetParameter("ModelViewProjection", modelViewProjection);

            _cubeMap.Bind();
            
            _skyboxEffect.Begin();
            _skyboxEffect.BeginPass(0);

            Graphics.RendererManager.CurrentRenderer.RenderBuffers(_vertices, _indices);

            _skyboxEffect.EndPass();
            _skyboxEffect.End();

            _skyboxEffect.DisableTextureParameter("Diffuse");

            RendererManager.CurrentRenderer.DepthTestEnabled = true;
        }

        private static void vectorAdd(Math.Vector3 one, Math.Vector3 two, Math.Vector3 three, float scale,
            out float r1, out float r2, out float r3)
        {
            Math.Vector3 r = one * scale + two * scale + three * scale;

            r1 = r.X;
            r2 = r.Y;
            r3 = r.Z;
        }
    }
}