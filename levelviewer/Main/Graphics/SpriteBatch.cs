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
        }

        private List<Sprite> _sprites = new List<Sprite>();
        private static VertexElementSet _vertexDeclaration;
        private static Effect _2dEffect;
        private static int[] _indices = new int[] {0, 1, 2, 0, 2, 3};
        private static float[] _workingVertices = new float[4 * (2 + 2)];

        public static void Init(Resource.ResourceManager content)
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
                _2dEffect = content.Load<Effect>("2d.fx");
            }
        }

        public void Begin()
        {
            RendererManager.CurrentRenderer.VertexDeclaration = _vertexDeclaration;
        }

        public void End()
        {
            IRenderer renderer = RendererManager.CurrentRenderer;
            renderer.BlendEnabled = true;
            renderer.AlphaTestEnabled = false;
            renderer.DepthTestEnabled = false;
            renderer.CullMode = CullMode.CounterClockwise;
            renderer.BlendState = BlendState.AlphaBlend;

            flush(renderer);

            renderer.BlendEnabled = false;
            renderer.AlphaTestEnabled = true;
            renderer.DepthTestEnabled = true;
        }

        public void Draw(TextureResource texture, Math.Vector2 position)
        {
            Rect destination;
            destination.X = (int)position.X;
            destination.Y = (int)position.Y;

            destination.Width = texture.Width;
            destination.Height = texture.Height;

            Draw(texture, destination, null, 0);
        }

        public void Draw(TextureResource texture, Math.Vector2 position, Rect? source)
        {
            Rect destination;
            destination.X = (int)position.X;
            destination.Y = (int)position.Y;

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

            _sprites.Add(s);
        }

        private void flush(IRenderer renderer)
        {
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

                Math.Vector4 color = new Gk3Main.Math.Vector4(1.0f, 1.0f, 1.0f, 1.0f);

                _2dEffect.Bind();
                _2dEffect.SetParameter("Viewport", renderer.Viewport.Vector);
                _2dEffect.SetParameter("Color", color);
                _2dEffect.SetParameter("Diffuse", s.Texture, 0);
                _2dEffect.Begin();

                //s.Texture.Bind();

                renderer.RenderIndices(PrimitiveType.Triangles, 0, 4, _indices, _workingVertices);

                _2dEffect.End();
            }

            _sprites.Clear();
        }
    }
}
