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

        private Math.Vector3 _skyColor;
        private Math.Vector3 _sunDirection;
        private Math.Vector3 _sunColor;
        private List<SurfaceLightmap> _surfaces = new List<SurfaceLightmap>();

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

        public List<SurfaceLightmap> Surfaces
        {
            get { return _surfaces; }
        }
    }
}
