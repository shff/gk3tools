using System;
using System.Collections.Generic;
using System.Text;

using Tao.Sdl;

namespace gk3levelviewer
{
    class Program
    {
        static void Main(string[] args)
        {
            FileSystem.AddBarnToSearchPath("C:/Sierra/Gabriel Knight 3/Data/core.brn");
            Resource.ResourceManager.AddResourceLoader(new Resource.TextResourceLoader());
            Resource.ResourceManager.AddResourceLoader(new Graphics.TextureResourceLoader());
            Resource.ResourceManager.AddResourceLoader(new Graphics.BspResourceLoader());
            Resource.ResourceManager.AddResourceLoader(new Graphics.LightmapResourceLoader());

            Graphics.Video.SetScreenMode(800, 600, 16, false);
            //SceneManager.LoadScene("CEM_N.SCN");
            SceneManager.LoadScene("R25_M.SCN");

            Camera camera = new Camera();

            while (Input.Tick())
            {
                float x = 0, y = 0, z = 0;
                if (Input.IsKeyPressed(Sdl.SDLK_UP))
                    z -= 1.0f;
                if (Input.IsKeyPressed(Sdl.SDLK_DOWN))
                    z += 1.0f;
                if (Input.IsKeyPressed(Sdl.SDLK_LEFT))
                    x -= 1.0f;
                if (Input.IsKeyPressed(Sdl.SDLK_RIGHT))
                    x += 1.0f;

                camera.AddRelativePositionOffset(new Math.Vector(x, y, z));

                int mx, my;
                Input.GetRelMouseCoords(out mx, out my);
                camera.AdjustYaw(mx * -0.01f);
                camera.AdjustPitch(my * -0.01f);

                camera.Update();
                SceneManager.Render();
            }
        }
    }
}
