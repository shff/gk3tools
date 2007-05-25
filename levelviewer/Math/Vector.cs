using System;
using System.Collections.Generic;
using System.Text;

namespace gk3levelviewer.Math
{
    class Vector
    {
        public Vector()
        {
            _x = 0; _y = 0; _z = 0;
        }

        public Vector(float x, float y, float z)
        {
            _x = x; _y = y; _z = z;
        }

        public float Dot(Vector v)
        {
            return _x * v._x + _y * v._y + _z * v._z;
        }

        public Vector Cross(Vector v)
        {
            return new Vector(_y * v._z - _z * v._y,
                _z * v._x - _x * v._z,
                _x * v._y - _y * v._x);
        }

        public float X { get { return _x; } set { _x = value; } }
        public float Y { get { return _y; } set { _y = value; } }
        public float Z { get { return _z; } set { _z = value; } }

        public float Length
        {
            get { return (float)System.Math.Sqrt(_x * _x + _y * _y + _z * _z); }
        }

        public Vector Normalize()
        {
            return this / Length;
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1._x + v2._x, v1._y + v2._y, v1._z + v2._z);
        }

        public static Vector operator *(Vector v, float s)
        {
            return new Vector(v._x * s, v._y * s, v._z * s);
        }

        public static Vector operator /(Vector v, float s)
        {
            return new Vector(v._x / s, v._y / s, v._z / s);
        }

        public override string ToString()
        {
            return "(" + _x + ", " + _y + ", " + _z + ")";
        }

        private float _x, _y, _z;
    }
}
