using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public static class HelperIcons
    {
        private static Graphics.TextureResource _camera;

        public static void Load()
        {
            _camera = (Graphics.TextureResource)Resource.ResourceManager.Load("Icons/camera.bmp");
        }

        public static Graphics.TextureResource Camera
        {
            get { return _camera; }
        }
    }
}
