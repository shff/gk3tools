#ifndef MATRIX_H
#define MATRIX_H

class Vector3
{
public:
    float x, y, z;
};

class Matrix
{
public:

    float M11, M12, M13, M14,
        M21, M22, M23, M24,
        M31, M32, M33, M34,
        M41, M42, M43, M44;

    Matrix()
    {
        M11 = M22 = M33 = M44 = 1.0f;
        M12 = M13 = M14 =
            M21 = M23 = M24 =
            M31 = M32 = M34 =
            M41 = M42 = M43 = 0;
    }

    Matrix(float values[])
    {
        M11 = values[0];
        M12 = values[1];
        M13 = values[2];
        M14 = 0;
        M21 = values[3];
        M22 = values[4];
        M23 = values[5];
        M24 = 0;
        M31 = values[6];
        M32 = values[7];
        M33 = values[8];
        M34 = 0;
        M41 = values[9];
        M42 = values[10];
        M43 = values[11];
        M44 = 1.0f;
    }

    Vector3 Multiply(Vector3 v)
    {
        Vector3 result;

        result.x = v.x * M11 + v.y * M21 + v.z * M31 + M41;
        result.y = v.x * M12 + v.y * M22 + v.z * M32 + M42;
        result.z = v.x * M13 + v.y * M23 + v.z * M33 + M43;

        return result;
    }

    static void Invert(const Matrix& matrix, Matrix& result)
    {
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
};

#endif // MATRIX_H
