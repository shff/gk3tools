using System;
using System.Collections.Generic;

using Tao.OpenGl;
using Tao.Cg;

namespace Gk3Main.Graphics
{
    public static class RendererManager
    {
        private static OpenGLRenderer _glRenderer = new OpenGLRenderer();

        public static IRenderer CurrentRenderer
        {
            get { return _glRenderer; }
        }
    }

    public class CgEffect : Effect
    {
        private IntPtr _effect;
        private IntPtr _technique;
        private List<IntPtr> _passes;
        private IntPtr _currentPass;

        internal CgEffect(string name, System.IO.Stream stream, IntPtr cgContext)
            : base(name, stream)
        {
            IntPtr cgEffect = Cg.cgCreateEffect(cgContext, Text, null);

            if (cgEffect == IntPtr.Zero)
                throw new Resource.InvalidResourceFileFormat("Unable to create effect: " + Cg.cgGetLastListing(cgContext));

            _effect = cgEffect;
            _technique = Cg.cgGetNamedTechnique(_effect, "GL");

            if (_technique == null || Cg.cgValidateTechnique(_technique) == 0)
                throw new Resource.InvalidResourceFileFormat("Unable to validate technique");

            _passes = new List<IntPtr>();
            IntPtr pass = Cg.cgGetFirstPass(_technique);
            while(pass != IntPtr.Zero)
            {
                _passes.Add(pass);
                pass = Cg.cgGetNextPass(pass);
            }
        }

        public override void Dispose()
        {
            if (_effect != IntPtr.Zero)
            {
                Cg.cgDestroyEffect(_effect);
                _effect = IntPtr.Zero;
            }
        }

        public override void Begin()
        {
            // nothing
        }

        public override void End()
        {
            // nothing
        }

        public override void BeginPass(int index)
        {
            _currentPass = _passes[index];
            Cg.cgSetPassState(_currentPass);
        }

        public override void EndPass()
        {
            Cg.cgResetPassState(_currentPass);
            _currentPass = IntPtr.Zero;
        }

        public override void SetParameter(string name, float parameter)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            Cg.cgSetParameter1f(param, parameter);
        }

        public override void SetParameter(string name, Math.Vector4 parameter)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            Cg.cgSetParameter4f(param, parameter.X, parameter.Y, parameter.Z, parameter.W);
        }

        public override void SetParameter(string name, Gk3Main.Math.Matrix parameter)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            Cg.cgSetMatrixParameterfc(param, out parameter.M11);
        }

        public override void SetParameter(string name, TextureResource parameter)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            CgGl.cgGLSetTextureParameter(param, parameter.OpenGlTexture);
        }

        public override void DisableTextureParameter(string name)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            CgGl.cgGLDisableTextureParameter(param);
        }

        public override void EnableTextureParameter(string name)
        {
            IntPtr param = Cg.cgGetNamedEffectParameter(_effect, name);
            CgGl.cgGLEnableTextureParameter(param);
        }

        public override void UpdatePassParameters()
        {
            Cg.cgGetError();
            cgUpdatePassParameters(_currentPass);
            if (Cg.cgGetError() != Cg.CG_NO_ERROR)
                throw new Exception("OH NO!");
        }


        // TODO: hopefully Tao will expose this method someday.
        // When it does we can remove this.
        [System.Runtime.InteropServices.DllImport("cg")]
        private static extern void cgUpdatePassParameters(IntPtr pass);

    }

    public class OpenGLRenderer : IRenderer
    {
        private IntPtr _cgContext;

        public OpenGLRenderer()
        {
            _cgContext = Cg.cgCreateContext();
            CgGl.cgGLEnableProfile(Cg.CG_PROFILE_ARBVP1);

            CgGl.cgGLRegisterStates(_cgContext);

            Cg.cgGetError();
            cgSetParameterSettingMode(_cgContext, CG_DEFERRED_PARAMETER_SETTING);
            if (Cg.cgGetError() != Cg.CG_NO_ERROR)
                throw new Exception("Oh no!");
        }

        public IntPtr CgContext { get { return _cgContext; } }
        public int DefaultCgProfile { get { return Cg.CG_PROFILE_ARBVP1; } }

        public bool BlendEnabled
        {
            get { return Gl.glIsEnabled(Gl.GL_BLEND) == Gl.GL_TRUE; }
            set { if (value) Gl.glEnable(Gl.GL_BLEND); else Gl.glDisable(Gl.GL_BLEND); }
        }

        public bool AlphaTestEnabled
        {
            get { return Gl.glIsEnabled(Gl.GL_ALPHA_TEST) == Gl.GL_TRUE; }
            set { if (value) Gl.glEnable(Gl.GL_ALPHA_TEST); else Gl.glDisable(Gl.GL_ALPHA_TEST); }
        }

        public bool DepthTestEnabled
        {
            get { return Gl.glIsEnabled(Gl.GL_DEPTH_TEST) == Gl.GL_TRUE; }
            set { if (value) Gl.glEnable(Gl.GL_DEPTH_TEST); else Gl.glDisable(Gl.GL_DEPTH_TEST); }
        }

        public CullMode CullMode
        {
            get
            {
                if (Gl.glIsEnabled(Gl.GL_CULL_FACE) == Gl.GL_FALSE)
                    return CullMode.None;

                int param;
                Gl.glGetIntegerv(Gl.GL_FRONT_FACE, out param);

                // OpenGL stores what NOT to cull, so we need to reverse it
                if (param == Gl.GL_CW)
                    return CullMode.CounterClockwise;
                else
                    return CullMode.Clockwise;
            }
            set
            {
                if (value == CullMode.None)
                    Gl.glDisable(Gl.GL_CULL_FACE);
                else
                {
                    Gl.glEnable(Gl.GL_CULL_FACE);

                    if (value == CullMode.Clockwise)
                        Gl.glFrontFace(Gl.GL_CCW);
                    else
                        Gl.glFrontFace(Gl.GL_CW);
                }
            }
        }

        public CompareFunction AlphaTestFunction
        {
            get
            {
                int func;
                Gl.glGetIntegerv(Gl.GL_ALPHA_TEST_FUNC, out func);

                if (func == Gl.GL_ALWAYS)
                    return CompareFunction.Always;
                else if (func == Gl.GL_NEVER)
                    return CompareFunction.Never;
                else if (func == Gl.GL_EQUAL)
                    return CompareFunction.Equal;
                else if (func == Gl.GL_NOTEQUAL)
                    return CompareFunction.NotEqual;
                else if (func == Gl.GL_GREATER)
                    return CompareFunction.Greater;
                else if (func == Gl.GL_LESS)
                    return CompareFunction.Less;
                else if (func == Gl.GL_LEQUAL)
                    return CompareFunction.LessOrEqual;
                else if (func == Gl.GL_GEQUAL)
                    return CompareFunction.GreaterOrEqual;

                throw new NotImplementedException("Unknown OpenGL alpha test function");
            }
            set
            {
                if (value == CompareFunction.Always)
                    Gl.glAlphaFunc(Gl.GL_ALWAYS, AlphaTestReference);
                else if (value == CompareFunction.Greater)
                    Gl.glAlphaFunc(Gl.GL_GREATER, AlphaTestReference);
            }
        }

        public float AlphaTestReference
        {
            get
            {
                float alphaRef;
                Gl.glGetFloatv(Gl.GL_ALPHA_TEST_REF, out alphaRef);

                return alphaRef;
            }
            set
            {
                int func;
                Gl.glGetIntegerv(Gl.GL_ALPHA_TEST_FUNC, out func);

                Gl.glAlphaFunc(func, value);
            }
        }

        public Viewport Viewport
        {
            get
            {
                int[] viewport = new int[4];
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

                Viewport v = new Viewport();
                v.X = viewport[0];
                v.Y = viewport[1];
                v.Width = viewport[2];
                v.Height = viewport[3];

                return v;
            }
            set
            {
                Gl.glViewport(value.X, value.Y, value.Width, value.Height);
            }
        }

        public Effect CreateEffect(string name, System.IO.Stream stream)
        {
            return new CgEffect(name, stream, _cgContext);
        }

        public VertexBuffer CreateVertexBuffer(float[] data, int stride)
        {
            return new GlVertexBuffer(data, stride);
        }

        public IndexBuffer CreateIndexBuffer(uint[] data)
        {
            return new GlIndexBuffer(data);
        }

        public void RenderBuffers(VertexBuffer vertices, IndexBuffer indices)
        {
            GlVertexBuffer glVertices = (GlVertexBuffer)vertices;
            GlIndexBuffer glIndices = (GlIndexBuffer)indices;

            glVertices.Bind();
            glIndices.Bind();

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glVertexPointer(3, Gl.GL_FLOAT, vertices.Stride, null);

            Gl.glDrawElements(Gl.GL_TRIANGLES, indices.Length, Gl.GL_UNSIGNED_INT, null);

            glVertices.Unbind();
            glIndices.Unbind();
        }

        public void RenderPrimitives(PrimitiveType type, int startIndex, int count, float[] vertices)
        {
            int glType;

            if (type == PrimitiveType.LineStrip)
            {
                glType = Gl.GL_LINE_STRIP;
            }
            else
            {
                glType = Gl.GL_TRIANGLES;
            }

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, vertices);

            Gl.glDrawArrays(glType, startIndex, count);
        }

        public void Clear()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
        }

        const int CG_IMMEDIATE_PARAMETER_SETTING = 4132;
        const int CG_DEFERRED_PARAMETER_SETTING = 4133;

        // TODO: hopefully Tao will expose this method someday.
        // When it does we can remove this.
        [System.Runtime.InteropServices.DllImport("cg")]
        private static extern void cgSetParameterSettingMode(IntPtr context, int value);
    }

    public class GlVertexBuffer : VertexBuffer
    {
        private int _buffer;
        private int _length;

        public GlVertexBuffer(float[] data, int stride)
        {
            _stride = stride;
            _length = data.Length;

            Gl.glGenBuffers(1, out _buffer);
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _buffer);
            Gl.glBufferData(Gl.GL_ARRAY_BUFFER, (IntPtr)(data.Length * sizeof(float)), data, Gl.GL_STATIC_DRAW);

            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);
        }

        public override void Dispose()
        {
            Gl.glDeleteBuffers(1, ref _buffer);
        }

        public void Bind()
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _buffer);
        }

        public void Unbind()
        {
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);
        }

        public override int Length
        {
            get { return _length; }
        }
    }

    public class GlIndexBuffer : IndexBuffer
    {
        private int _buffer;
        private int _length;

        public GlIndexBuffer(uint[] data)
        {
            _length = data.Length;

            Gl.glGenBuffers(1, out _buffer);
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, _buffer);
            Gl.glBufferData(Gl.GL_ELEMENT_ARRAY_BUFFER, (IntPtr)(data.Length * sizeof(uint)), data, Gl.GL_STATIC_DRAW);

            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public override void Dispose()
        {
            Gl.glDeleteBuffers(1, ref _buffer);
        }

        public void Bind()
        {
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, _buffer);
        }

        public void Unbind()
        {
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public override int Length
        {
            get { return _length; }
        }
    }
}
