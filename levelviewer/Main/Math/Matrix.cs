using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Math
{
    class Matrix
    {
        public Matrix(float[] values)
        {
            if (values.Length != 12)
                throw new ArgumentException();

            _values = values;
        }

        public static Vector operator *(Matrix m, Vector v)
        {
            Vector result = new Vector();

            result.X = m._values[0] * v.X + m._values[3] * v.Y + m._values[6] * v.Z + m._values[9];
            result.Y = m._values[1] * v.X + m._values[4] * v.Y + m._values[7] * v.Z + m._values[10];
            result.Z = m._values[2] * v.X + m._values[5] * v.Y + m._values[8] * v.Z + m._values[11];

            return result;
        }

        private float[] _values;
    }
}
