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

        public static void Load(Resource.ResourceManager content)
        {
            _camera = content.Load<Graphics.TextureResource>("Icons/camera.bmp");
            _flag = content.Load<Graphics.TextureResource>("Icons/flag.bmp");
            _bulb = content.Load<Graphics.TextureResource>("Icons/bulb.bmp");
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
