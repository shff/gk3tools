using System;
using System.Collections.Generic;
using System.Text;

using Tao.OpenGl;

namespace gk3levelviewer
{
    class Camera
    {
        public Camera()
        {
            _orientation = new gk3levelviewer.Math.Quaternion();
            _position = new gk3levelviewer.Math.Vector();
        }

        public void AddRelativePositionOffset(Math.Vector offset)
        {
            offset = _orientation * offset;

            _position += offset;
        }

        public Math.Vector Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Math.Quaternion Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        public void AdjustYaw(float radians)
        {
            Math.Quaternion rotation = Math.Quaternion.FromAxis(new Math.Vector(0, 1.0f, 0), radians);

            _orientation = rotation * _orientation;
        }

        public void AdjustPitch(float radians)
        {
            const float maxPitch = (float)System.Math.PI * 0.49f;
            const float minPitch = (float)System.Math.PI * -0.49f;

            Math.Vector right = _orientation * new Math.Vector(1.0f, 0, 0);

            Math.Quaternion rotation = Math.Quaternion.FromAxis(new Math.Vector(1.0f, 0, 0), radians);

            _orientation = _orientation * rotation;
        }

        public void Update()
        {
            Math.Vector forward = new Math.Vector(0, 0, -1.0f);
            Math.Vector up = new Math.Vector(0, 1.0f, 0);

            forward = _orientation * forward;
            up = _orientation * up;

            Gl.glLoadIdentity();
            Glu.gluLookAt(_position.X, _position.Y, _position.Z,
                _position.X + forward.X, _position.Y + forward.Y, _position.Z + forward.Z,
                up.X, up.Y, up.Z);
        }

        private Math.Quaternion _orientation;
        private Math.Vector _position;
    }
}
