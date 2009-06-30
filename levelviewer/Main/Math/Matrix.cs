using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Math
{
    public struct Matrix
    {
        public Matrix(float[] values)
        {
            if (values.Length != 16)
                throw new ArgumentException();

            M11 = values[0];
            M12 = values[1];
            M13 = values[2];
            M14 = values[3];
            M21 = values[4];
            M22 = values[5];
            M23 = values[6];
            M24 = values[7];
            M31 = values[8];
            M32 = values[9];
            M33 = values[10];
            M34 = values[11];
            M41 = values[12];
            M42 = values[13];
            M43 = values[14];
            M44 = values[15];
        }

        public static Matrix Identity
        {
            get
            {
                float[] values = new float[16];
                values[0] = 1.0f;
                values[5] = 1.0f;
                values[10] = 1.0f;
                values[15] = 1.0f;

                return new Matrix(values);
            }
        }

        public static Vector3 operator *(Matrix m, Vector3 v)
        {
            Vector3 result = new Vector3();

            result.X = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41;
            result.Y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42;
            result.Z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43;

            return result;
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            Matrix result;
            result.M11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41;
            result.M12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42;
            result.M13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43;
            result.M14 = m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44;
            result.M21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41;
            result.M22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42;
            result.M23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43;
            result.M24 = m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44;
            result.M31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41;
            result.M32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42;
            result.M33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43;
            result.M34 = m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44;
            result.M41 = m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41;
            result.M42 = m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42;
            result.M43 = m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43;
            result.M44 = m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44;

            return result;
        }


        /*public static Matrix Invert(Matrix matrix)
        {
            float n23 = matrix.M33 * matrix.M44 - matrix.M34 * matrix.M43;
            float n22 = matrix.M32 * matrix.M44 - matrix.M34 * matrix.M42;
            float n21 = matrix.M32 * matrix.M43 - matrix.M33 * matrix.m42;
            float n20 = matrix.M31 * matrix.M44 - matrix.M34 * matrix.M41;
            float n19 = matrix.M31 * matrix.M43 - matrix.M33 * 
        }*/

        public static Matrix LookAt(Vector3 position, Vector3 direction, Vector3 up)
        {
            Vector3 inverseDirection = new Vector3(-direction.X, -direction.Y, -direction.Z);
            direction = inverseDirection.Normalize();
            Vector3 right = up.Cross(inverseDirection);
            Vector3 trueUp = inverseDirection.Cross(right);

            Matrix m;

            m.M11 = right.X;
            m.M12 = trueUp.X;
            m.M13 = inverseDirection.X;
            m.M14 = 0;

            m.M21 = right.Y;
            m.M22 = trueUp.Y;
            m.M23 = inverseDirection.Y;
            m.M24 = 0;

            m.M31 = right.Z;
            m.M32 = trueUp.Z;
            m.M33 = inverseDirection.Z;
            m.M34 = 0;

            m.M41 = -(right.X * position.X + right.Y * position.Y + right.Z * position.Z);
            m.M42 = -(trueUp.X * position.X + trueUp.Y * position.Y + trueUp.Z * position.Z);
            m.M43 = -(inverseDirection.X * position.X + inverseDirection.Y * position.Y + inverseDirection.Z * position.Z);
            m.M44 = 1.0f;

            return m;
        }

        public static Matrix Perspective(float fov, float aspect, float near, float far)
        {
            float f = 1.0f / (float)System.Math.Tan(fov * 0.5f);

            Matrix m;
            m.M11 = f / aspect;
            m.M12 = 0;
            m.M13 = 0;
            m.M14 = 0;

            m.M21 = 0;
            m.M22 = f;
            m.M23 = 0;
            m.M24 = 0;

            m.M31 = 0;
            m.M32 = 0;
            m.M33 = far / (near - far);
            m.M34 = -1.0f;

            m.M41 = 0;
            m.M42 = 0;
            m.M43 = (near * far) / (near - far);
            m.M44 = 0;

            return m;
        }

        public static Matrix Translate(float x, float y, float z)
        {
            Matrix translation = Identity;
            translation.M41 = x;
            translation.M42 = y;
            translation.M43 = z;
            
            return translation;
        }

        public static Matrix Scale(float x, float y, float z)
        {
            Matrix scale = Identity;
            scale.M11 = x;
            scale.M22 = y;
            scale.M33 = z;

            return scale;
        }

        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M31;
        public float M32;
        public float M33;
        public float M34;
        public float M41;
        public float M42;
        public float M43;
        public float M44;
    }
}
