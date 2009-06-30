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

namespace Gk3Main.Graphics
{
    public class Camera
    {
        public Camera(Math.Matrix projection)
        {
            _orientation = new Gk3Main.Math.Quaternion();
            _position = new Gk3Main.Math.Vector3();

            _projection = projection;
        }

        public Camera(float fov, float aspect, float near, float far)
        {
            _orientation = new Gk3Main.Math.Quaternion();
            _position = new Gk3Main.Math.Vector3();

            _projection = Math.Matrix.Perspective(fov, aspect, near, far);

            // HACK: remove this when we can use our own matrix stuff for UnProject()!
            Gl.glMatrixMode(Gl.GL_PROJECTION_MATRIX);
            Gl.glLoadIdentity();
            Glu.gluPerspective(fov * 57.2957795, aspect, near, far);
            Gl.glMatrixMode(Gl.GL_MODELVIEW_MATRIX);
            Gl.glLoadIdentity();
        }

        public void AddRelativePositionOffset(Math.Vector3 offset)
        {
            offset = _orientation * offset;

            _position += offset;
        }

        public void AddPositionOffset(Math.Vector3 offset)
        {
            _position += offset;
        }

        public void AddPositionOffset(float x, float y, float z)
        {
            _position.X += x;
            _position.Y += y;
            _position.Z += z;
        }

        public Math.Vector3 Position
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
            Math.Quaternion rotation = Math.Quaternion.FromAxis(Math.Vector3.Up, radians);

            _orientation = rotation * _orientation;
        }

        public void AdjustPitch(float radians)
        {
            const float maxPitch = (float)System.Math.PI * 0.49f;
            const float minPitch = (float)System.Math.PI * -0.49f;

            //Math.Vector3 right = _orientation * Math.Vector3.Right;

            Math.Quaternion rotation = Math.Quaternion.FromAxis(Math.Vector3.Right, radians);

            _orientation = _orientation * rotation;
        }

        public void SetPitchYaw(float pitch, float yaw)
        {
            Math.Quaternion yawq = Math.Quaternion.FromAxis(Math.Vector3.Up, yaw);
            Math.Vector3 right = yawq * Math.Vector3.Right;

            Math.Quaternion pitchq = Math.Quaternion.FromAxis(Math.Vector3.Right, pitch);
            _orientation = yawq * pitchq;
        }

        public void Update()
        {
            Math.Vector3 forward = Math.Vector3.Forward;
            Math.Vector3 up = Math.Vector3.Up;

            forward = _orientation * forward;
            up = _orientation * up;

            Gl.glLoadIdentity();
            Glu.gluLookAt(_position.X, _position.Y, _position.Z,
                _position.X + forward.X, _position.Y + forward.Y, _position.Z + forward.Z,
                up.X, up.Y, up.Z);

            // calculate the ModelViewProjection matrix
            _modelView = Math.Matrix.LookAt(_position, forward, up);
            _modelViewProjection = _modelView * _projection;

            _frustum = new Frustum(_modelViewProjection);
        }

        public Math.Vector3 Unproject(Math.Vector3 v)
        {
            // TODO: use our own matrix stuff instead of this OpenGL stuff

            double[] modelMatrix = new double[16];
            double[] projectionMatrix = new double[16];
            int[] viewport = new int[4];

            Gl.glGetDoublev(Gl.GL_MODELVIEW_MATRIX, modelMatrix);
            Gl.glGetDoublev(Gl.GL_PROJECTION_MATRIX, projectionMatrix);
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            double x, y, z;
            Glu.gluUnProject(v.X, viewport[3] - v.Y, v.Z, modelMatrix, projectionMatrix, viewport, out x, out y, out z);

            return new Math.Vector3((float)x, (float)y, (float)z);
        }

        public Math.Matrix ModelViewProjection
        {
            get { return _modelViewProjection; }
        }

        public Math.Matrix Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        public Math.Matrix ModelView
        {
            get { return _modelView; }
            set { _modelView = value; }
        }

        public Frustum Frustum
        {
            get { return _frustum; }
        }

        private Math.Quaternion _orientation;
        private Math.Vector3 _position;
        private Math.Matrix _projection;
        private Math.Matrix _modelView;
        private Math.Matrix _modelViewProjection;
        private Frustum _frustum;
    }
}
