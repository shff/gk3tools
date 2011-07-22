using System;
using System.Collections.Generic;
using System.Text;
using Tao.OpenGl;

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
            int error = Gl.glGetError();
            if (error != 0)
                throw new GlException(error);
        }

        private static string convertError(int error)
        {
            if (error == Gl.GL_INVALID_OPERATION)
                return "Invalid Operation";
            else
                return error.ToString();
        }
    }
}
