using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public class LightmapSpecs
    {
        private Math.Vector3 _skyColor;
        private Math.Vector3 _sunDirection;

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
    }
}
