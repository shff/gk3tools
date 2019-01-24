#if !D3D_DISABLED

using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.Direct3D9;

namespace Gk3Main.Graphics.Direct3D9
{
    class Direct3D9Effect : Effect
    {
        private SharpDX.Direct3D9.Effect _effect;
        private Dictionary<string, EffectHandle> _parameters;

        public Direct3D9Effect(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            Direct3D9Renderer renderer = (Direct3D9Renderer)RendererManager.CurrentRenderer;
            string errors;
            _effect = SharpDX.Direct3D9.Effect.FromString(renderer.Direct3D9Device, Text, null, null, null, ShaderFlags.None, null);

            _effect.Technique = _effect.GetTechnique(0);
            _parameters = new Dictionary<string, EffectHandle>();
        }

        public override void Begin()
        {
            _effect.Begin();
            _effect.BeginPass(0);
        }

        public override void End()
        {
            _effect.EndPass();
            _effect.End();
        }

        public override void CommitParams()
        {
            _effect.CommitChanges();
        }

        #region Parameters
        public override void SetParameter(string name, float parameter)
        {
            EffectHandle param = getParameter(name);
            _effect.SetValue(param, parameter);
        }

        public override void SetParameter(string name, Math.Vector4 parameter)
        {
            EffectHandle param = getParameter(name);
            _effect.SetValue(param, parameter);
        }

        public override void SetParameter(string name, Gk3Main.Math.Matrix parameter)
        {
            EffectHandle param = getParameter(name);

            // convert the matrix, transposing as we go
            SharpDX.Mathematics.Interop.RawMatrix m;
            m.M11 = parameter.M11;
            m.M12 = parameter.M21;
            m.M13 = parameter.M31;
            m.M14 = parameter.M41;
            m.M21 = parameter.M12;
            m.M22 = parameter.M22;
            m.M23 = parameter.M32;
            m.M24 = parameter.M42;
            m.M31 = parameter.M13;
            m.M32 = parameter.M23;
            m.M33 = parameter.M33;
            m.M34 = parameter.M43;
            m.M41 = parameter.M14;
            m.M42 = parameter.M24;
            m.M43 = parameter.M34;
            m.M44 = parameter.M44;

            _effect.SetValue(param, m);
        }

        public override void SetParameter(string name, Color parameter)
        {
            Math.Vector4 color = new Math.Vector4(parameter.R / 255.0f, 
                parameter.G / 255.0f, parameter.B / 255.0f, parameter.A / 255.0f);

            EffectHandle param = getParameter(name);
            _effect.SetValue(param, color);
        }

        public override void SetParameter(string name, TextureResource parameter, int index)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            // TODO: support updatable textures
            Texture tex;
            if (parameter is Direct3D9UpdatableTexture)
                tex = ((Direct3D9UpdatableTexture)parameter).InternalTexture;
            else
                tex = ((Direct3D9Texture)parameter).InternalTexture;

            EffectHandle param = getParameter(name);

            _effect.SetTexture(param, tex);
        }

        public override void SetParameter(string name, CubeMapResource parameter, int index)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            Direct3D9CubeMap d3dCubeMap = (Direct3D9CubeMap)parameter;
            EffectHandle param = getParameter(name);

            _effect.SetTexture(param, d3dCubeMap.CubeMap);
        }

        public EffectHandle getParameter(string name)
        {
            EffectHandle param;
            if (_parameters.TryGetValue(name, out param) == false)
            {
                param = _effect.GetParameter(null, name);
                _parameters.Add(name, param);
            }

            return param;
        }

        #endregion
    }
}

#endif