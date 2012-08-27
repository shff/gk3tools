using System;
using System.Collections.Generic;
using Tao.OpenGl;
using System.Text;

namespace Gk3Main.Graphics.OpenGl
{
    class GlslEffect : Effect
    {
        private int _program;
        internal struct Attribute
        {
            public string Name;
            public VertexElementUsage Usage;
            public int Index;
            public int GlHandle;

            public static bool IsValidAttribute(Attribute a)
            {
                return a.Index >= 0 && a.GlHandle >= 0;
            }
        }
        private List<Attribute> _attributes = new List<Attribute>();

        private struct Uniform
        {
            public string Name;
            public int GlHandle;
        }
        private Dictionary<string, Uniform> _uniforms = new Dictionary<string, Uniform>();

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

            ((OpenGLRenderer)RendererManager.CurrentRenderer).VertexPointersNeedSetup = true;
            ((OpenGLRenderer)RendererManager.CurrentRenderer).CurrentEffect = this;
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

            Uniform u = getUniform(name);
            if (u.GlHandle == -1) return;

            Gl.glUniform1f(u.GlHandle, parameter);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, Gk3Main.Math.Vector4 parameter)
        {
            Gl.glGetError();

            Uniform u = getUniform(name);
            if (u.GlHandle == -1) return;

            Gl.glUniform4f(u.GlHandle, parameter.X, parameter.Y, parameter.Z, parameter.W);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, Gk3Main.Math.Matrix parameter)
        {
            Gl.glGetError();

            Uniform u = getUniform(name);
            if (u.GlHandle == -1) return;

            Gl.glUniformMatrix4fv(u.GlHandle, 1, 0, ref parameter.M11);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, Color parameter)
        {
            Gl.glGetError();

            Uniform u = getUniform(name);
            if (u.GlHandle == -1) return;

            Gl.glUniform4f(u.GlHandle, parameter.R / 255.0f, parameter.G / 255.0f, parameter.B / 255.0f, parameter.A / 255.0f);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, TextureResource parameter, int index)
        {
            Gl.glGetError();

            Gl.glActiveTexture(Gl.GL_TEXTURE0 + index);

            if (parameter is GlUpdatableTexture)
                ((GlUpdatableTexture)parameter).Bind(index);
            else
                ((GlTexture)parameter).Bind(index);

            Uniform u = getUniform(name);
            if (u.GlHandle == -1) return;

            Gl.glUniform1i(u.GlHandle, index);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        public override void SetParameter(string name, CubeMapResource parameter, int index)
        {
            Gl.glGetError();

            GlCubeMap texture = (GlCubeMap)parameter;

            Uniform u = getUniform(name);
            if (u.GlHandle == -1) return;

            Gl.glActiveTexture(Gl.GL_TEXTURE0 + index);
            Gl.glBindTexture(Gl.GL_TEXTURE_CUBE_MAP, texture.OpenGlTexture);
            Gl.glUniform1i(u.GlHandle, index);

            int r = Gl.glGetError();
            if (r != Gl.GL_NO_ERROR)
                throw new Exception("Unable to set shader parameter: " + name);
        }

        internal Attribute GetAttribute(VertexElementUsage usage, int index)
        {
            foreach (Attribute a in _attributes)
            {
                if (a.Index == index &&
                    a.Usage == usage)
                    return a;
            }

            Attribute invalid = new Attribute();
            invalid.Index = -1;
            invalid.GlHandle = -1;
            return invalid;
        }

        private Uniform getUniform(string name)
        {
            Uniform u;
            if (_uniforms.TryGetValue(name, out u))
                return u;

            throw new Exception("Unable to find uniform: " + name);
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

            // now load the attribute info
            int numAttribs;
            Gl.glGetProgramiv(_program, Gl.GL_ACTIVE_ATTRIBUTES, out numAttribs);

            int attribMaxLength;
            Gl.glGetProgramiv(_program, Gl.GL_ACTIVE_ATTRIBUTE_MAX_LENGTH, out attribMaxLength);

            System.Text.StringBuilder buffer = new System.Text.StringBuilder(attribMaxLength);

            for (int i = 0; i < numAttribs; i++)
            {
                int length, size, type;
                Gl.glGetActiveAttrib(_program, i, attribMaxLength, out length, out size, out type, buffer);
                string name = buffer.ToString();

                for (int j = 0; j < _attributes.Count; j++)
                {
                    if (_attributes[j].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        Attribute a = _attributes[j];
                        a.GlHandle = i;
                        _attributes[j] = a;

                        break;
                    }
                }

                buffer.Length = 0;
            }

            extractUniformInfo(vertexSource);
            if (string.IsNullOrEmpty(fragSource) == false)
                extractUniformInfo(fragSource);
        }

        private int compileShader(string source, bool vertex)
        {
            int shader;
            if (vertex)
            {
                shader = Gl.glCreateShader(Gl.GL_VERTEX_SHADER);
                
                if (vertex)
                    source = extractAttribInfo(source);
            }
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

        private void extractUniformInfo(string shaderSource)
        {
            // read the code and look for uniforms
            for (int i = 0; i < shaderSource.Length; )
            {
                int nextSemicolon = shaderSource.IndexOf(';', i);
                if (nextSemicolon == -1) break;

                int uniform = shaderSource.IndexOf("uniform ", i);
                if (uniform > -1 && uniform < nextSemicolon)
                {
                    int nameEnd = lastIndexNotSpace(shaderSource, nextSemicolon - 1);
                    int nameStart = shaderSource.LastIndexOf(' ', nameEnd) + 1;

                    Uniform u;
                    u.Name = shaderSource.Substring(nameStart, nameEnd - nameStart + 1);
                    u.GlHandle = Gl.glGetUniformLocation(_program, u.Name);

                    _uniforms.Add(u.Name, u);
                }

                i = nextSemicolon + 1;
            }
        }

        private string extractAttribInfo(string vertexShaderSource)
        {
            StringBuilder sb = new StringBuilder();
            char[] number = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            int currentIndex = 0;
            while (true)
            {
                int colon = vertexShaderSource.IndexOf(':', currentIndex);
                if (colon == -1)
                {
                    sb.Append(vertexShaderSource, currentIndex, vertexShaderSource.Length - currentIndex);
                    break;
                }

                int nameEnd = lastIndexNotSpace(vertexShaderSource, colon - 1);
                int nameStart = vertexShaderSource.LastIndexOf(' ', nameEnd) + 1;

                Attribute attrib;
                attrib.Name = vertexShaderSource.Substring(nameStart, nameEnd - nameStart + 1);
                attrib.Usage = VertexElementUsage.Position;
                attrib.Index = 0;

                int semicolon = vertexShaderSource.IndexOf(';', colon);
                int usageStart = firstIndexNotSpace(vertexShaderSource, colon + 1);
                string usage = vertexShaderSource.Substring(usageStart, semicolon - usageStart);

                int firstNumber = usage.IndexOfAny(number);
                if (firstNumber > 0)
                {
                    attrib.Index = int.Parse(usage.Substring(firstNumber));
                    usage = usage.Substring(0, firstNumber);
                }

                if (usage.Equals("position", StringComparison.OrdinalIgnoreCase))
                    attrib.Usage = VertexElementUsage.Position;
                else if (usage.Equals("texcoord", StringComparison.OrdinalIgnoreCase))
                    attrib.Usage = VertexElementUsage.TexCoord;
                else if (usage.Equals("color", StringComparison.OrdinalIgnoreCase))
                    attrib.Usage = VertexElementUsage.Color;
                else if (usage.Equals("normal", StringComparison.OrdinalIgnoreCase))
                    attrib.Usage = VertexElementUsage.Normal;

                sb.Append(vertexShaderSource, currentIndex, colon - currentIndex);
                
                currentIndex = semicolon;

                attrib.GlHandle = 0; // will be set later
                _attributes.Add(attrib);
            }

            return sb.ToString();
        }

        private static int lastIndexNotSpace(string str, int startIndex)
        {
            for (int i = startIndex; i >= 0; i--)
            {
                if (str[i] != ' ')
                    return i;
            }

            // not found :(
            return -1;
        }

        private static int firstIndexNotSpace(string str, int startIndex)
        {
            for (int i = startIndex; i < str.Length; i++)
            {
                if (str[i] != ' ')
                    return i;
            }

            // not found :(
            return -1;
        }
    }
}
