using System;
using Tao.OpenGl;

namespace Gk3Main.Graphics.OpenGl
{
    class GlslEffect : Effect
    {
        private int _program;

        public GlslEffect(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            int indexOfVertex = Text.IndexOf("#vertex");
            int indexOfFrag = Text.IndexOf("#fragment");

            if (indexOfVertex == -1 && indexOfFrag == -1)
                load(Text, null);
            else if (indexOfVertex != -1 && indexOfFrag == -1)
                load(Text.Substring(indexOfVertex + 7), null);
            else
                load(Text.Substring(indexOfVertex + 7, indexOfFrag - indexOfVertex - 7),
                    Text.Substring(indexOfFrag + 9));
        }

        public override void Bind()
        {
            Gl.glGetError();
            Gl.glUseProgram(_program);
            if (Gl.glGetError() != Gl.GL_NO_ERROR)
                throw new Exception("Unable to bind GLSL shader");
        }

        public override void Begin()
        {
            // nothing
        }

        public override void End()
        {
            Gl.glUseProgram(0);
        }

        public override void CommitParams()
        {
            // nothing
        }

        public override void SetParameter(string name, float parameter)
        {
            Gl.glGetError();

            int uniform = Gl.glGetUniformLocation(_program, name);
            if (uniform == -1)
                throw new Exception("Unable to find uniform: " + name);

            Gl.glUniform1f(uniform, parameter);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, Gk3Main.Math.Vector4 parameter)
        {
            Gl.glGetError();

            int uniform = Gl.glGetUniformLocation(_program, name);
            if (uniform == -1)
                throw new Exception("Unable to find uniform: " + name);

            Gl.glUniform4f(uniform, parameter.X, parameter.Y, parameter.Z, parameter.W);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, Gk3Main.Math.Matrix parameter)
        {
            Gl.glGetError();

            int uniform = Gl.glGetUniformLocation(_program, name);
            if (uniform == -1)
                throw new Exception("Unable to find uniform: " + name);

            Gl.glUniformMatrix4fv(uniform, 1, 0, ref parameter.M11);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, Color parameter)
        {
            Gl.glGetError();

            int uniform = Gl.glGetUniformLocation(_program, name);
            if (uniform == -1)
                throw new Exception("Unable to find uniform: " + name);

            Gl.glUniform4f(uniform, parameter.R / 255.0f, parameter.G / 255.0f, parameter.B / 255.0f, parameter.A / 255.0f);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, TextureResource parameter, int index)
        {
            Gl.glGetError();

            Gl.glActiveTexture(Gl.GL_TEXTURE0 + index);
            ((GlTexture)parameter).Bind();

            int uniform = Gl.glGetUniformLocation(_program, name);
            if (uniform == -1)
                throw new Exception("Unable to find uniform: " + name);

            Gl.glUniform1i(uniform, index);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, CubeMapResource parameter, int index)
        {
            Gl.glGetError();

            GlCubeMap texture = (GlCubeMap)parameter;

            int uniform = Gl.glGetUniformLocation(_program, name);
            if (uniform == -1)
                throw new Exception("Unable to find uniform: " + name);

            Gl.glActiveTexture(Gl.GL_TEXTURE0 + index);
            Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, texture.OpenGlTexture);
            Gl.glUniform1i(uniform, index);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        private void load(string vertexSource, string fragSource)
        {
            int vertexShader = compileShader(vertexSource, true);
            int fragShader = (fragSource == null ? 0 : compileShader(fragSource, false));

            _program = Gl.glCreateProgram();

            Gl.glAttachShader(_program, vertexShader);
            if (fragSource != null) Gl.glAttachShader(_program, fragShader);

            Gl.glLinkProgram(_program);

            int result;
            Gl.glGetProgramiv(_program, Gl.GL_LINK_STATUS, out result);
            if (result != Gl.GL_TRUE)
                throw new Exception("Unable to link GLSL program");
        }

        private int compileShader(string source, bool vertex)
        {
            int shader;
            if (vertex)
                shader = Gl.glCreateShader(Gl.GL_VERTEX_SHADER);
            else
                shader = Gl.glCreateShader(Gl.GL_FRAGMENT_SHADER);

            Gl.glShaderSource(shader, 1, new string[] { source }, new int[] { source.Length });

            Gl.glCompileShader(shader);

            int logLength;
            Gl.glGetShaderiv(shader, Gl.GL_INFO_LOG_LENGTH, out logLength);
            System.Text.StringBuilder log = new System.Text.StringBuilder(logLength);
            Gl.glGetShaderInfoLog(shader, logLength, out logLength, log);

            if (log.Length > 0)
                Console.CurrentConsole.WriteLine(log.ToString());

            int result;
            Gl.glGetShaderiv(shader, Gl.GL_COMPILE_STATUS, out result);
            if (result != Gl.GL_TRUE)
                throw new Exception("Unable to compile shader");

            return shader;
        }
    }
}
