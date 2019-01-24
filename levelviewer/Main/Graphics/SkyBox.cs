using System;

namespace Gk3Main.Graphics
{
    public class SkyBox
    {
        private static VertexBuffer _vertices;
        private static IndexBuffer _indices;
        private static Effect _skyboxEffect;

        private CubeMapResource _cubeMap;
        private TextureResource _sun;
        private TextureResource _sunMask;

        private float _azimuth;
        private Math.Vector3 _sunDirection;
        private const int _stride = 3;
        private const float _size = 500.0f;

        public static void Init(Resource.ResourceManager globalContent)
        {
            VertexElementSet declaration = new VertexElementSet(new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Float3, VertexElementUsage.Position, 0)
            });

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
                2, 1, 0,
                2, 3, 1,

                // right
                7, 3, 2,
                6, 7, 2,

                // left
                0, 1, 5,
                4, 0, 5,

                // up
                3, 5, 1,
                7, 5, 3,

                // back
                7, 6, 4,
                5, 7, 4,

                // bottom
                0, 4, 2,
                2, 4, 6
            };

            _vertices = RendererManager.CurrentRenderer.CreateVertexBuffer(VertexBufferUsage.Static, vertices, 8, declaration);
            _indices = RendererManager.CurrentRenderer.CreateIndexBuffer(indices);

            _skyboxEffect = globalContent.Load<Effect>("skybox.fx");
        }

        public SkyBox(string name, BitmapSurface front, BitmapSurface back, BitmapSurface left, BitmapSurface right,
            BitmapSurface up, BitmapSurface down, float azimuth)
        {
            _cubeMap = Graphics.RendererManager.CurrentRenderer.CreateCubeMap(name, front, back, left, right, up, down);
            _azimuth = azimuth;
        }

        public void AddSun(Math.Vector3 direction, Math.Vector3 color, bool memory)
        {
            const int sunSize = 64;

            Game.Radiosity.GenerateOmniLight(sunSize, color, out _sun, out _sunMask);

            _sunDirection = direction;
        }

        public void Render(Camera camera)
        {
            if (SceneManager.CurrentFilterMode == TextureFilterMode.None)
            {
                RendererManager.CurrentRenderer.SamplerStates[0] = SamplerState.PointClamp;
                RendererManager.CurrentRenderer.SamplerStates[1] = SamplerState.PointClamp;
            }
            else
            {
                RendererManager.CurrentRenderer.SamplerStates[0] = SamplerState.LinearClamp;
                RendererManager.CurrentRenderer.SamplerStates[1] = SamplerState.LinearClamp;
            }

            RendererManager.CurrentRenderer.DepthTestEnabled = false;
            RendererManager.CurrentRenderer.SetVertexBuffer(_vertices);
            RendererManager.CurrentRenderer.Indices = _indices;

            _skyboxEffect.Bind();

            _skyboxEffect.SetParameter("Diffuse", _cubeMap, 0);

            Math.Matrix modelViewProjection = Math.Matrix.RotateY(_azimuth) * Math.Matrix.Translate(camera.Position.X, camera.Position.Y, camera.Position.Z) * camera.View * camera.Projection;
            _skyboxEffect.SetParameter("ModelViewProjection", modelViewProjection);

            
            _skyboxEffect.Begin();

            Graphics.RendererManager.CurrentRenderer.DrawIndexed(PrimitiveType.Triangles, 0, 0, _vertices.NumVertices, 0, _indices.Length);

            _skyboxEffect.End();

            if (_sun != null)
            {
                BillboardManager.AddBillboard(camera.Position + -_sunDirection * 500.0f, 100.0f, 100.0f, _sun, _sunMask);
                BillboardManager.RenderBillboardsWithAlpha(camera);
            }

            RendererManager.CurrentRenderer.DepthTestEnabled = true;
        }

        [Obsolete]
        internal static void AddSun(Math.Vector3 direction, Math.Vector3 color, float radius,
            BitmapSurface front, BitmapSurface back, BitmapSurface left, BitmapSurface right, BitmapSurface up, bool memory)
        {
            // process each surface of the skybox and figure out how much "sun" is in that texel
            addSunToSurface(direction, color, radius, new Math.Vector3(1.0f, 0, 0), Math.Vector3.Up, front, memory);
            addSunToSurface(direction, color, radius, new Math.Vector3(-1.0f, 0, 0), Math.Vector3.Up, back, memory);
            addSunToSurface(direction, color, radius, new Math.Vector3(0, 0, 1.0f), Math.Vector3.Up, right, memory);
            addSunToSurface(direction, color, radius, new Math.Vector3(0, 0, -1.0f), Math.Vector3.Up, left, memory);
            addSunToSurface(direction, color, radius, new Math.Vector3(0, 1.0f, 0), Math.Vector3.Forward, up, memory);
        }

        private static void addSunToSurface(Math.Vector3 direction, Math.Vector3 color, float radius,
            Math.Vector3 faceNormal, Math.Vector3 faceUp, BitmapSurface faceSurface, bool memory)
        {
            Math.Vector3 faceRight = faceNormal.Cross(faceUp);
            float t = -0.5f / faceNormal.Dot(-direction);

            if (t > 0) return; // the sun never hits this plane

            Math.Vector3 sunP = -direction * t;
            for (int y = 0; y < faceSurface.Height; y++)
            {
                for (int x = 0; x < faceSurface.Width; x++)
                {
                    // calc the distance from this pixel to the sun
                    float u = (float)x / faceSurface.Width - 0.5f;
                    float v = (float)y / faceSurface.Height - 0.5f;

                    Math.Vector3 texelP = faceUp * v + faceRight * u + -faceNormal * 0.5f;

                    float distance = Math.Vector3.Distance(texelP, sunP);

                    if (distance < radius)
                    {
                        if (!memory)
                        {
                            faceSurface.Pixels[(y * faceSurface.Width + x) * 4 + 0] = 255;
                            faceSurface.Pixels[(y * faceSurface.Width + x) * 4 + 1] = 0;
                            faceSurface.Pixels[(y * faceSurface.Width + x) * 4 + 2] = 0;
                        }
                        else
                        {
                            Color c = faceSurface.ReadColorAt(x, y);

                            uint ptr = (uint)c.R | ((uint)c.G << 8) | ((uint)c.B << 16) | ((uint)c.A << 24);

                            UIntPtr ptr2 = (UIntPtr)ptr;

                            unsafe
                            {
                                float* f = (float*)ptr2.ToPointer();
                                f[0] = color.X;
                                f[1] = color.Y;
                                f[2] = color.Z;
                            }
                        }
                    }
                }
            }
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
