// Copyright (c) 2007 Brad Farris
// This file is part of the GK3 Scene Viewer.

// The GK3 Scene Viewer is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// The GK3 Scene Viewer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Foobar; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Math
{
    public struct Vector3
    {
        /*public Vector3()
        {
            _x = 0; _y = 0; _z = 0;
        }*/

        public Vector3(float x, float y, float z)
        {
            _x = x; _y = y; _z = z;
        }

        public float Dot(Vector3 v)
        {
            return _x * v._x + _y * v._y + _z * v._z;
        }

        public Vector3 Cross(Vector3 v)
        {
            return new Vector3(_y * v._z - _z * v._y,
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

        public Vector3 Normalize()
        {
            return this / Length;
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1._x + v2._x, v1._y + v2._y, v1._z + v2._z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1._x - v2._x, v1._y - v2._y, v1._z - v2._z); 
        }

        public static Vector3 operator *(Vector3 v, float s)
        {
            return new Vector3(v._x * s, v._y * s, v._z * s);
        }

        public static Vector3 operator /(Vector3 v, float s)
        {
            return new Vector3(v._x / s, v._y / s, v._z / s);
        }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        public override string ToString()
        {
            return "(" + _x + ", " + _y + ", " + _z + ")";
        }

        private float _x, _y, _z;
        
        public static Vector3 Forward
        {
            get { return new Vector3(0, 0, -1.0f); }
        }

        public static Vector3 Up
        {
            get { return new Vector3(0, 1.0f, 0); }
        }

        public static Vector3 Right
        {
            get { return new Vector3(1.0f, 0, 0); }
        }
    }

    public struct Vector4
    {
        public Vector4(float x, float y, float z, float w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public float X { get { return _x; } set { _x = value; } }
        public float Y { get { return _y; } set { _y = value; } }
        public float Z { get { return _z; } set { _z = value; } }
        public float W { get { return _w; } set { _w = value; } }

        private float _x, _y, _z, _w;
    }
}
