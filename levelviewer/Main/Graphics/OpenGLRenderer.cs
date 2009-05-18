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


        const int CG_IMMEDIATE_PARAMETER_SETTING = 4132;
        const int CG_DEFERRED_PARAMETER_SETTING = 4133;

        // TODO: hopefully Tao will expose this method someday.
        // When it does we can remove this.
        [System.Runtime.InteropServices.DllImport("cg")]
        private static extern void cgSetParameterSettingMode(IntPtr context, int value);
    }
}
