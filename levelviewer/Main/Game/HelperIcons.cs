using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public static class HelperIcons
    {
        private static Graphics.TextureResource _camera;
        private static Graphics.TextureResource _flag;
        private static Graphics.TextureResource _bulb;

        public static void Load()
        {
            var global = Gk3Main.Resource.ResourceManager.Global;
            _camera = global.Load<Graphics.TextureResource>("Icons/camera.png");
            _flag = global.Load<Graphics.TextureResource>("Icons/flag.png");
            _bulb = global.Load<Graphics.TextureResource>("Icons/bulb.png");
        }

        public static Graphics.TextureResource Camera
        {
            get { return _camera; }
        }

        public static Graphics.TextureResource Flag
        {
            get { return _flag; }
        }

        public static Graphics.TextureResource Bulb
        {
            get { return _bulb; }
        }
    }
}
