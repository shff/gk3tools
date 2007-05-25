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
