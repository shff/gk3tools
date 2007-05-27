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

namespace gk3levelviewer.Math
{
    class Quaternion
    {
        public Quaternion()
        {
            _v = new Vector();
            _s = 1.0f;
        }

        public Quaternion(float x, float y, float z, float s)
        {
            _v = new Vector(x, y, z);
            _s = s;
        }

        public Vector V { get { return _v; } }
        public float S { get { return _s; } }
        public Quaternion Conjugate
        {
            get { return new Quaternion(-_v.X, -_v.Y, -_v.Z, _s); }
        }

        public static Quaternion FromAxis(Vector v, float angle)
        {
            angle *= 0.5f;

            float sinAngle = (float)System.Math.Sin(angle);
            Vector vn = v.Normalize();

            return new Quaternion(vn.X * sinAngle, vn.Y * sinAngle, vn.Z * sinAngle,
                (float)System.Math.Cos(angle));
        }

        public static void ToAxis(Quaternion q, out Vector axis, out float angle)
        {
            float scale = q.V.Length;
            axis = new Vector(q.V.X / scale, q.V.Y / scale, q.V.Z / scale);
            angle = (float)System.Math.Acos(q.S) * 2.0f;
        }

        public static Quaternion operator *(Quaternion quat1, Quaternion quat2)
        {
            return new Quaternion(quat1.S * quat2.V.X + quat1.V.X * quat2.S + quat1.V.Y * quat2.V.Z - quat1.V.Z * quat2.V.Y,
                quat1.S * quat2.V.Y + quat1.V.Y * quat2.S + quat1.V.Z * quat2.V.X - quat1.V.X * quat2.V.Z,
                quat1.S * quat2.V.Z + quat1.V.Z * quat2.S + quat1.V.X * quat2.V.Y - quat1.V.Y * quat2.V.X,
                quat1.S * quat2.S - quat1.V.X * quat2.V.X - quat1.V.Y * quat2.V.Y - quat1.V.Z * quat2.V.Z);
        }

        public static Vector operator *(Quaternion quat, Vector v)
        {
            return (quat * new Quaternion(v.X, v.Y, v.Z, 0) * quat.Conjugate).V;
        }

        private Vector _v;
        private float _s;
    }
}
