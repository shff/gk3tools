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

namespace Gk3Main.Graphics
{
    public class Camera
    {
        public Camera(float fov, float aspect, float near, float far, bool zNegOne)
        {
            _orientation = new Gk3Main.Math.Quaternion();
            _position = new Gk3Main.Math.Vector3();

            _projection = Math.Matrix.PerspectiveLH(fov, aspect, near, far, zNegOne);

            _near = near;
            _far = far;
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

        public float CalcYaw()
        {
            return Math.Quaternion.CalcYaw(ref _orientation);
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

        // TODO: get this to save the orientation! Right now has the same effect as Update(),
        // but the orientation quaternion is out of date!
        internal void LookAt(Math.Vector3 position, Math.Vector3 direction, Math.Vector3 up)
        {
            _position = position;
            _modelView = Math.Matrix.LookAt(position, direction, up);

            Math.Matrix.Multiply(ref _modelView, ref _projection, out _modelViewProjection);

            _frustum = new Frustum(_modelViewProjection);
        }

        public void Update()
        {
            Math.Vector3 forward = Math.Vector3.Forward;
            Math.Vector3 up = Math.Vector3.Up;

            forward = _orientation * forward;
            up = _orientation * up;

            // calculate the ModelViewProjection matrix
            _modelView = Math.Matrix.LookAt(_position, forward, up);
            //_modelViewProjection = _modelView * _projection;
            Math.Matrix.Multiply(ref _modelView, ref _projection, out _modelViewProjection);

            _frustum = new Frustum(_modelViewProjection);
        }

        public Math.Vector3 Unproject(Math.Vector3 v)
        {
            Math.Matrix world = Math.Matrix.Identity;
            return RendererManager.CurrentRenderer.Viewport.Unproject(v, ref _projection, ref _modelView, ref world);
        }

        public Math.Matrix ViewProjection
        {
            get { return _modelViewProjection; }
        }

        public Math.Matrix Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        public Math.Matrix View
        {
            get { return _modelView; }
            set { _modelView = value; }
        }

        public Frustum Frustum
        {
            get { return _frustum; }
        }

        public float Near
        {
            get { return _near; }
        }

        public float Far
        {
            get { return _far; }
        }

        public void CreateBillboardMatrix(Math.Vector3 position, bool includePosition, out Math.Matrix matrix)
        {
            matrix = Math.Matrix.Translate(-position) *
                Math.Matrix.RotateY(CalcYaw()) *
                Math.Matrix.Translate(position);
        }

        private Math.Quaternion _orientation;
        private Math.Vector3 _position;
        private Math.Matrix _projection;
        private Math.Matrix _modelView;
        private Math.Matrix _modelViewProjection;
        private Frustum _frustum;
        private float _near, _far;
    }
}
