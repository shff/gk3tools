using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public class Actor : IDisposable
    {
        private string _noun;
        private string _modelName;
        private Math.Vector3 _position;
        private float _facingAngle;
        private Graphics.ModelResource _model;
        private bool _isEgo;

        public Actor(string modelName, string noun, bool isEgo)
        {
            _noun = noun;
            _modelName = modelName;
            _isEgo = isEgo;

            _model = (Graphics.ModelResource)Resource.ResourceManager.Load(Utils.MakeEndsWith(modelName, ".MOD"));
        }

        public void Dispose()
        {
            if (_model != null)
            {
                Resource.ResourceManager.Unload(_model);
                _model = null;
            }
        }

        public void Render(Graphics.Camera camera)
        {
            if (_model != null)
                _model.RenderAt(_position, _facingAngle, camera);
        }

        public bool CollideRay(Math.Vector3 origin, Math.Vector3 direction, float length, out float distance)
        {
            if (_model != null && _model.Loaded)
                return _model.CollideRay(_position, origin, direction, length, out distance);

            distance = float.MaxValue;
            return false;
        }

        public string ModelName
        {
            get { return _modelName; }
        }

        public string Noun
        {
            get { return _noun; }
        }

        public Math.Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float FacingAngle
        {
            get { return _facingAngle; }
            set { _facingAngle = value; }
        }

        public bool IsEgo
        {
            get { return _isEgo; }
        }
    }
}
