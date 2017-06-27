using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Gk3Main.Graphics.OpenGl
{
    public class GlException : Exception
    {
        public GlException(int error)
            : base(convertError(error))
        {
           
        }

        public static void ThrowExceptionIfErrorExists()
        {
            int error = (int)GL.GetError();
            if (error != 0)
                throw new GlException(error);
        }

        private static string convertError(int error)
        {
            if (error == (int)ErrorCode.InvalidOperation)
                return "Invalid Operation";
            else
                return error.ToString();
        }
    }
}
