using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public class LightmapSpecs
    {
        public struct SurfaceLightmap
        {
            public int Width, Height;
        }

        public struct OmniLight
        {
            public float Radius;
            public Math.Vector3 Position;
            public Math.Vector3 Color;
        }

        private Math.Vector3 _skyColor;
        private Math.Vector3 _sunDirection;
        private Math.Vector3 _sunColor;
        private List<SurfaceLightmap> _surfaces = new List<SurfaceLightmap>();
        private List<OmniLight> _lights = new List<OmniLight>();

        public Math.Vector3 SkyColor
        {
            get { return _skyColor; }
            set { _skyColor = value; }
        }

        public Math.Vector3 SunDirection
        {
            get { return _sunDirection; }
            set { _sunDirection = value; }
        }

        public Math.Vector3 SunColor
        {
            get { return _sunColor; }
            set { _sunColor = value; }
        }

        public List<OmniLight> OmniLights
        {
            get { return _lights; }
        }

        public List<SurfaceLightmap> Surfaces
        {
            get { return _surfaces; }
        }
    }
}
