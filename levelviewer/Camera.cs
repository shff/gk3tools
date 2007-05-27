// Copyright (c) 2007 Brad Farris
// This file is part of the GK3 Scene Viewer.

// The GK3 Scene Viewer is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// The GK3 Scene Viewer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Foobar; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

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
