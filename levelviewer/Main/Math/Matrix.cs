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
            Vector3 result;

            result.X = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41;
            result.Y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42;
            result.Z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43;

            return result;
        }

        public static Vector4 operator *(Matrix m, Vector4 v)
        {
            Vector4 result;

            result.X = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + v.W * m.M41;
            result.Y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + v.W * m.M42;
            result.Z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + v.W * m.M43;
            result.W = v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34 + v.W * m.M44;

            return result;
        }

        [Obsolete("Use Multiply() instead since it's faster")]
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            Matrix result;
            Multiply(ref m1, ref m2, out result);

            return result;
        }

        public static void Multiply(ref Matrix m1, ref Matrix m2, out Matrix result)
        {
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
        }

        public static void Invert(ref Matrix matrix, out Matrix result)
        {
            // this is heavily based on the Mono Xna implementation
            // http://code.google.com/p/monoxna/

            // find determinants
            float det1 = matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;
            float det2 = matrix.M11 * matrix.M23 - matrix.M13 * matrix.M21;
            float det3 = matrix.M11 * matrix.M24 - matrix.M14 * matrix.M21;
            float det4 = matrix.M12 * matrix.M23 - matrix.M13 * matrix.M22;
            float det5 = matrix.M12 * matrix.M24 - matrix.M14 * matrix.M22;
            float det6 = matrix.M13 * matrix.M24 - matrix.M14 * matrix.M23;
            float det7 = matrix.M31 * matrix.M42 - matrix.M32 * matrix.M41;
            float det8 = matrix.M31 * matrix.M43 - matrix.M33 * matrix.M41;
            float det9 = matrix.M31 * matrix.M44 - matrix.M34 * matrix.M41;
            float det10 = matrix.M32 * matrix.M43 - matrix.M33 * matrix.M42;
            float det11 = matrix.M32 * matrix.M44 - matrix.M34 * matrix.M42;
            float det12 = matrix.M33 * matrix.M44 - matrix.M34 * matrix.M43;
            float major = det1 * det12 - det2 * det11 + det3 * det10 + det4 * det9 - det5 * det8 + det6 * det7;

            float invDetMatrix = 1.0f / major;

            // now do the rest
            result.M11 = (matrix.M22 * det12 - matrix.M23 * det11 + matrix.M24 * det10) * invDetMatrix;
            result.M12 = (-matrix.M12 * det12 + matrix.M13 * det11 - matrix.M14 * det10) * invDetMatrix;
            result.M13 = (matrix.M42 * det6 - matrix.M43 * det5 + matrix.M44 * det4) * invDetMatrix;
            result.M14 = (-matrix.M32 * det6 + matrix.M33 * det5 - matrix.M34 * det4) * invDetMatrix;
            result.M21 = (-matrix.M21 * det12 + matrix.M23 * det9 - matrix.M24 * det8) * invDetMatrix;
            result.M22 = (matrix.M11 * det12 - matrix.M13 * det9 + matrix.M14 * det8) * invDetMatrix;
            result.M23 = (-matrix.M41 * det6 + matrix.M43 * det3 - matrix.M44 * det2) * invDetMatrix;
            result.M24 = (matrix.M31 * det6 - matrix.M33 * det3 + matrix.M34 * det2) * invDetMatrix;
            result.M31 = (matrix.M21 * det11 - matrix.M22 * det9 + matrix.M24 * det7) * invDetMatrix;
            result.M32 = (-matrix.M11 * det11 + matrix.M12 * det9 - matrix.M14 * det7) * invDetMatrix;
            result.M33 = (matrix.M41 * det5 - matrix.M42 * det3 + matrix.M44 * det1) * invDetMatrix;
            result.M34 = (-matrix.M31 * det5 + matrix.M32 * det3 - matrix.M34 * det1) * invDetMatrix;
            result.M41 = (-matrix.M21 * det10 + matrix.M22 * det8 - matrix.M23 * det7) * invDetMatrix;
            result.M42 = (matrix.M11 * det10 - matrix.M12 * det8 + matrix.M13 * det7) * invDetMatrix;
            result.M43 = (-matrix.M41 * det4 + matrix.M42 * det2 - matrix.M43 * det1) * invDetMatrix;
            result.M44 = (matrix.M31 * det4 - matrix.M32 * det2 + matrix.M33 * det1) * invDetMatrix;
        }

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

        [Obsolete("Use PerspectiveLH instead")]
        public static Matrix Perspective(float fov, float aspect, float near, float far)
        {
            float f = 1.0f / (float)System.Math.Tan(fov * 0.5f);

            /*this is old right-handed code, but since
             * GK3 uses left-handed we decided not to use it anymore
             * 
             * Matrix m;
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
            m.M44 = 0;*/

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
            m.M33 = far / (far - near);
            m.M34 = 1.0f;

            m.M41 = 0;
            m.M42 = 0;
            m.M43 = (-near * far) / (far - near);
            m.M44 = 0;

            return PerspectiveLH(fov, aspect, near, far, true);
            //return m;
        }
    
        public static Matrix PerspectiveLH(float fov, float aspect, float near, float far, bool zNegOne)
        {
            float height = 2.0f * (float)System.Math.Tan(fov * 0.5f) * near;

            return perspectiveLH(height * aspect, height, near, far, zNegOne);
        }

        private static Matrix perspectiveLH(float width, float height, float near, float far, bool zNegOne)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            return perspectiveLH(-halfWidth, halfWidth, -halfHeight, halfHeight, near, far, zNegOne);
        }

        private static Matrix perspectiveLH(float left, float right, float bottom, float top, float near, float far, bool zNegOne)
        {
            Matrix m = Identity;

            const float s = 1.0f;
            float invWidth = 1.0f / (right - left);
            float invHeight = 1.0f / (top - bottom);
            float invDepth = 1.0f / (far - near);
            float near2 = 2.0f * near;

            m.M11 = near2 * invWidth;
            m.M22 = near2 * invHeight;
            m.M31 = -s * (right + left) * invWidth;
            m.M32 = -s * (top + bottom) * invHeight;
            m.M34 = s;
            m.M44 = 0;

            if (zNegOne)
            {
                m.M33 = s * (far + near) * invDepth;
                m.M43 = -2.0f * far * near * invDepth;
            }
            else
            {
                m.M33 = s * far * invDepth;
                m.M43 = -s * near * m.M33;
            }
            

            return m;
        }

        public static Matrix Translate(Vector3 position)
        {
            Matrix translation = Identity;
            translation.M41 = position.X;
            translation.M42 = position.Y;
            translation.M43 = position.Z;

            return translation;
        }

        [Obsolete("Use Translate(Vector3) instead")]
        public static Matrix Translate(float x, float y, float z)
        {
            Matrix translation = Identity;
            translation.M41 = x;
            translation.M42 = y;
            translation.M43 = z;
            
            return translation;
        }

        public static void Translate(ref Vector3 position, out Matrix result)
        {
            result = Identity;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
        }

        public static Matrix Scale(float x, float y, float z)
        {
            Matrix scale = Identity;
            scale.M11 = x;
            scale.M22 = y;
            scale.M33 = z;

            return scale;
        }

        public static Matrix RotateX(float angle)
        {
            float sine = (float)System.Math.Sin(angle);
            float cosine = (float)System.Math.Cos(angle);

            Matrix rotation = Identity;
            rotation.M22 = cosine;
            rotation.M23 = -sine;
            rotation.M32 = sine;
            rotation.M33 = cosine;

            return rotation;
        }

        public static Matrix RotateY(float angle)
        {
            float sine = (float)System.Math.Sin(angle);
            float cosine = (float)System.Math.Cos(angle);

            Matrix rotation = Identity;
            rotation.M11 = cosine;
            rotation.M13 = sine;
            rotation.M31 = -sine;
            rotation.M33 = cosine;

            return rotation;
        }

        public static Matrix RotateZ(float angle)
        {
            float sine = (float)System.Math.Sin(angle);
            float cosine = (float)System.Math.Cos(angle);

            Matrix rotation = Identity;
            rotation.M11 = cosine;
            rotation.M12 = -sine;
            rotation.M21 = sine;
            rotation.M22 = cosine;

            return rotation;
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
