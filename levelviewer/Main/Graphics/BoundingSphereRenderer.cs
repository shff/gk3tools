using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public class BoundingSphereRenderer
    {
        private static Effect _effect;
        private const int _resolution = 12;
        private static float[] _vertices;

        static BoundingSphereRenderer()
        {
            _vertices = new float[(_resolution + 1) * 3 * 3];

            float step = Math.Constants.TwoPi / _resolution;
            int index = 0;

            // XY plane
            for (float i = 0; i < Math.Constants.TwoPi; i += step)
            {
                _vertices[index * 3 + 0] = (float)System.Math.Cos(i);
                _vertices[index * 3 + 1] = (float)System.Math.Sin(i);
                _vertices[index * 3 + 2] = 0;

                index++;
            }

            // XZ plane
            for (float i = 0; i < Math.Constants.TwoPi; i += step)
            {
                _vertices[index * 3 + 0] = (float)System.Math.Cos(i);
                _vertices[index * 3 + 1] = 0;
                _vertices[index * 3 + 2] = (float)System.Math.Sin(i);

                index++;
            }

            // YZ plane
            for (float i = 0; i < Math.Constants.TwoPi; i += step)
            {
                _vertices[index * 3 + 0] = 0;
                _vertices[index * 3 + 1] = (float)System.Math.Cos(i);
                _vertices[index * 3 + 2] = (float)System.Math.Sin(i);

                index++;
            }
        }

        public static void Render(Camera camera, float x, float y, float z, float radius)
        {
            if (_effect == null)
                _effect = (Effect)Resource.ResourceManager.Load("wireframe.fx");

            _effect.Begin();
            _effect.BeginPass(0);
            
            _effect.SetParameter("ModelViewProjection", Math.Matrix.Scale(radius, radius, radius) * Math.Matrix.Translate(x, y, z) * camera.View * camera.Projection);

            int count = (_resolution + 1);
            RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.LineStrip, 0, count, _vertices);
            RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.LineStrip, count, count, _vertices);
            RendererManager.CurrentRenderer.RenderPrimitives(PrimitiveType.LineStrip, count * 2, count, _vertices);

            _effect.EndPass();
            _effect.End();
        }
    }
}
