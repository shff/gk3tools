using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    public struct Frustum
    {
        private Math.Vector4 _near;
        private Math.Vector4 _far;
        private Math.Vector4 _right;
        private Math.Vector4 _left;
        private Math.Vector4 _bottom;
        private Math.Vector4 _top;

        public Frustum(Math.Matrix modelViewProjection)
        {
            // need this to make the compiler happy
            _near = new Math.Vector4();
            _far = new Math.Vector4();
            _right = new Math.Vector4();
            _left = new Math.Vector4();
            _bottom = new Math.Vector4();
            _top = new Math.Vector4();

            Build(modelViewProjection);
        }

        public void Build(Math.Matrix modelViewProjection)
        {
            _right.X = modelViewProjection.M14 - modelViewProjection.M11;
            _right.Y = modelViewProjection.M24 - modelViewProjection.M21;
            _right.Z = modelViewProjection.M34 - modelViewProjection.M31;
            _right.W = modelViewProjection.M44 - modelViewProjection.M41;
            float len = (float)System.Math.Sqrt(_right.X * _right.X + _right.Y * _right.Y + _right.Z * _right.Z);
            _right.X /= len;
            _right.Y /= len;
            _right.Z /= len;
            _right.W /= len;

            _left.X = modelViewProjection.M14 + modelViewProjection.M11;
            _left.Y = modelViewProjection.M24 + modelViewProjection.M21;
            _left.Z = modelViewProjection.M34 + modelViewProjection.M31;
            _left.W = modelViewProjection.M44 + modelViewProjection.M41;
            len = (float)System.Math.Sqrt(_left.X * _left.X + _left.Y * _left.Y + _left.Z * _left.Z);
            _left.X /= len;
            _left.Y /= len;
            _left.Z /= len;
            _left.W /= len;

            _bottom.X = modelViewProjection.M14 + modelViewProjection.M12;
            _bottom.Y = modelViewProjection.M24 + modelViewProjection.M22;
            _bottom.Z = modelViewProjection.M34 + modelViewProjection.M32;
            _bottom.W = modelViewProjection.M44 + modelViewProjection.M42;
            len = (float)System.Math.Sqrt(_bottom.X * _bottom.X + _bottom.Y * _bottom.Y + _bottom.Z * _bottom.Z);
            _bottom.X /= len;
            _bottom.Y /= len;
            _bottom.Z /= len;
            _bottom.W /= len;

            _top.X = modelViewProjection.M14 - modelViewProjection.M12;
            _top.Y = modelViewProjection.M24 - modelViewProjection.M22;
            _top.Z = modelViewProjection.M34 - modelViewProjection.M32;
            _top.W = modelViewProjection.M44 - modelViewProjection.M42;
            len = (float)System.Math.Sqrt(_top.X * _top.X + _top.Y * _top.Y + _top.Z * _top.Z);
            _top.X /= len;
            _top.Y /= len;
            _top.Z /= len;
            _top.W /= len;


            _near.X = modelViewProjection.M13;
            _near.Y = modelViewProjection.M23;
            _near.Z = modelViewProjection.M33;
            _near.W = modelViewProjection.M43;
            len = (float)System.Math.Sqrt(_near.X * _near.X + _near.Y * _near.Y + _near.Z * _near.Z);
            _near.X /= len;
            _near.Y /= len;
            _near.Z /= len;
            _near.W /= len;

            _far.X = modelViewProjection.M14 - modelViewProjection.M13;
            _far.Y = modelViewProjection.M24 - modelViewProjection.M23;
            _far.Z = modelViewProjection.M34 - modelViewProjection.M33;
            _far.W = modelViewProjection.M44 - modelViewProjection.M43;
            len = (float)System.Math.Sqrt(_far.X * _far.X + _far.Y * _far.Y + _far.Z * _far.Z);
            _far.X /= len;
            _far.Y /= len;
            _far.Z /= len;
            _far.W /= len;
        }

        public bool IsSphereOutside(Math.Vector3 position, float radius)
        {
            // test near
            if (_near.X * position.X + _near.Y * position.Y + _near.Z * position.Z < -radius)
                return true;

            return false;
        }

        public bool IsSphereOutside(Math.Vector4 positionAndRadius)
        {
            // test near
            if (_near.X * positionAndRadius.X + _near.Y * positionAndRadius.Y + _near.Z * positionAndRadius.Z + _near.W < -positionAndRadius.W)
                return true;

            // test far
            if (_far.X * positionAndRadius.X + _far.Y * positionAndRadius.Y + _far.Z * positionAndRadius.Z + _far.W < -positionAndRadius.W)
                return true;

            // test right
            if (_right.X * positionAndRadius.X + _right.Y * positionAndRadius.Y + _right.Z * positionAndRadius.Z + _right.W < -positionAndRadius.W)
                return true;

            // test left
            if (_left.X * positionAndRadius.X + _left.Y * positionAndRadius.Y + _left.Z * positionAndRadius.Z + _left.W < -positionAndRadius.W)
                return true;

            // test bottom
            if (_bottom.X * positionAndRadius.X + _bottom.Y * positionAndRadius.Y + _bottom.Z * positionAndRadius.Z + _bottom.W < -positionAndRadius.W)
                return true;

            // test bottom
            if (_top.X * positionAndRadius.X + _top.Y * positionAndRadius.Y + _top.Z * positionAndRadius.Z + _top.W < -positionAndRadius.W)
                return true;

            return false;
        }
    }
}
