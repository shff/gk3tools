using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    // obviously based on the SpriteBatch class from XNA
    public class SpriteBatch
    {
        struct Sprite
        {
            public TextureResource Texture;
            public Rect Source;
            public Rect Destination;
            public Color Color;
        }

        private List<Sprite> _sprites = new List<Sprite>();
        private static VertexElementSet _vertexDeclaration;
        private static Effect _2dEffect;
        private static uint[] _indices = new uint[] {0, 1, 2, 0, 2, 3};
        private static float[] _workingVertices = new float[4 * (2 + 2)];
        private static VertexBuffer _vertexBuffer;
        private static IndexBuffer _indexBuffer;

        public static void Init()
        {
            if (_vertexDeclaration == null)
            {
                _vertexDeclaration = new VertexElementSet(new VertexElement[] {
                    new VertexElement(0, VertexElementFormat.Float2, VertexElementUsage.Position, 0),
                    new VertexElement(2 * sizeof(float), VertexElementFormat.Float2, VertexElementUsage.TexCoord, 0)
                });
            }

            if (_2dEffect == null)
            {
                _2dEffect = Resource.ResourceManager.Global.Load<Effect>("2d.fx");
            }

            if (_vertexBuffer == null)
            {
                _vertexBuffer = RendererManager.CurrentRenderer.CreateVertexBuffer<float>(VertexBufferUsage.Stream, null, 4 * 100, _vertexDeclaration);
            }

            if (_indexBuffer == null)
            {
                _indexBuffer = RendererManager.CurrentRenderer.CreateIndexBuffer(_indices);
            }
        }

        public void Begin()
        {
            // nothing
        }

        public void End()
        {
            IRenderer renderer = RendererManager.CurrentRenderer;
            renderer.BlendEnabled = true;
            renderer.DepthTestEnabled = false;
            renderer.CullMode = CullMode.CounterClockwise;
            renderer.BlendState = BlendState.AlphaBlend;

            flush(renderer);

            renderer.BlendEnabled = false;
            renderer.DepthTestEnabled = true;
        }

        public void Draw(TextureResource texture, Math.Vector2 position)
        {
            Rect destination;
            destination.X = position.X;
            destination.Y = position.Y;

            destination.Width = texture.Width;
            destination.Height = texture.Height;

            Draw(texture, destination, null, 0);
        }

        public void Draw(TextureResource texture, Math.Vector2 position, Rect? source)
        {
            Rect destination;
            destination.X = position.X;
            destination.Y = position.Y;

            if (source.HasValue)
            {
                destination.Width = source.Value.Width;
                destination.Height = source.Value.Height;
            }
            else
            {
                destination.Width = texture.Width;
                destination.Height = texture.Height;
            }

            Draw(texture, destination, source, 0);
        }

        public void Draw(TextureResource texture, Rect destination, Rect? source, float layerDepth)
        {
            Sprite s;
            s.Texture = texture;
            s.Source = (source.HasValue ? source.Value : new Rect(0, 0, texture.Width, texture.Height));
            s.Destination = destination;
            s.Color = Color.White;

            _sprites.Add(s);
        }

        public void Draw(TextureResource texture, Math.Vector2 destination, Rect? source, Color color, float layerDepth)
        {
            Sprite s;
            s.Texture = texture;
            s.Source = (source.HasValue ? source.Value : new Rect(0, 0, texture.Width, texture.Height));
            s.Destination.X = destination.X;
            s.Destination.Y = destination.Y;
            s.Destination.Width = s.Source.Width;
            s.Destination.Height = s.Source.Height;
            s.Color = color;

            _sprites.Add(s);
        }

        public void Draw(TextureResource texture, Rect destination, Rect? source, Color color, float layerDepth)
        {
            Sprite s;
            s.Texture = texture;
            s.Source = (source.HasValue ? source.Value : new Rect(0, 0, texture.Width, texture.Height));
            s.Destination = destination;
            s.Color = color;

            _sprites.Add(s);
        }

        private void flush(IRenderer renderer)
        {
            _2dEffect.Bind();
            _2dEffect.SetParameter("Viewport", renderer.Viewport.Vector);
            _2dEffect.Begin();

            for (int i = 0; i < _sprites.Count; i++)
            {
                Sprite s = _sprites[i];

               // Utils.ScaleBlit(s.Destination, s.Texture, s.Source);
                //Utils.Blit(s.Destination.X, s.Destination.Y, s.Texture);

                float u = s.Source.X / s.Texture.ActualPixelWidth;
                float v = s.Source.Y / s.Texture.ActualPixelHeight;
                float tw = s.Source.Width / s.Texture.ActualPixelWidth;
                float th = s.Source.Height / s.Texture.ActualPixelHeight;

                float x = s.Destination.X;
                float y = s.Destination.Y;
                float width = s.Destination.Width;
                float height = s.Destination.Height;

                const int stride = 2 + 2;
                _workingVertices[0 * stride + 0] = x;
                _workingVertices[0 * stride + 1] = y;
                _workingVertices[0 * stride + 2] = u;
                _workingVertices[0 * stride + 3] = v;

                _workingVertices[1 * stride + 0] = x + width;
                _workingVertices[1 * stride + 1] = y;
                _workingVertices[1 * stride + 2] = u + tw;
                _workingVertices[1 * stride + 3] = v;

                _workingVertices[2 * stride + 0] = x + width;
                _workingVertices[2 * stride + 1] = y + height;
                _workingVertices[2 * stride + 2] = u + tw;
                _workingVertices[2 * stride + 3] = v + th;

                _workingVertices[3 * stride + 0] = x;
                _workingVertices[3 * stride + 1] = y + height;
                _workingVertices[3 * stride + 2] = u;
                _workingVertices[3 * stride + 3] = v + th;

                _vertexBuffer.SetData(_workingVertices, 0, 4 * stride);

                Math.Vector4 color = new Math.Vector4(s.Color.R / 255.0f, s.Color.G / 255.0f, s.Color.B / 255.0f, s.Color.A / 255.0f);


                _2dEffect.SetParameter("Color", color);
                _2dEffect.SetParameter("Diffuse", s.Texture, 0);
                _2dEffect.CommitParams();

                //s.Texture.Bind();

                RendererManager.CurrentRenderer.SetVertexBuffer(_vertexBuffer);
                RendererManager.CurrentRenderer.Indices = _indexBuffer;
                renderer.DrawIndexed(PrimitiveType.Triangles, 0, 0, _vertexBuffer.NumVertices, 0, 6);
                //renderer.RenderIndices(PrimitiveType.Triangles, 0, 4, _indices);
                //renderer.RenderIndices(PrimitiveType.Triangles, 0, 4, _indices, _workingVertices, _vertexDeclaration);
            }

            _2dEffect.End();
            _sprites.Clear();
        }
    }
}
