using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public static class HelperIcons
    {
        private static Graphics.TextureResource _camera;
        private static Graphics.TextureResource _flag;

        public static void Load()
        {
            _camera = (Graphics.TextureResource)Resource.ResourceManager.Load("Icons/camera.bmp");
            _flag = (Graphics.TextureResource)Resource.ResourceManager.Load("Icons/flag.bmp");
        }

        public static Graphics.TextureResource Camera
        {
            get { return _camera; }
        }

        public static Graphics.TextureResource Flag
        {
            get { return _flag; }
        }
    }
}
