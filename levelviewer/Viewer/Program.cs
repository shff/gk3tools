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

using Tao.Sdl;

namespace gk3levelviewer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // TODO: this whole method is rather messy. Clean it up!

            string coreBarn;

            if (args.Length < 2)
                coreBarn = getCoreBarnPath();
            else
                coreBarn = args[1];

            BarnLib.Barn barn;
            try
            {
                barn = FileSystem.AddBarnToSearchPath(coreBarn);
            }
            catch (BarnLib.BarnException)
            {
                System.Windows.Forms.MessageBox.Show("You must provide a valid barn file",
                    "Unable to open barn", System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            Resource.ResourceManager.AddResourceLoader(new Resource.TextResourceLoader());
            Resource.ResourceManager.AddResourceLoader(new Graphics.TextureResourceLoader());
            Resource.ResourceManager.AddResourceLoader(new Graphics.BspResourceLoader());
            Resource.ResourceManager.AddResourceLoader(new Graphics.LightmapResourceLoader());
            Resource.ResourceManager.AddResourceLoader(new Game.SifResourceLoader());

            string sceneToLoad = getSceneToLoad(barn);

            if (sceneToLoad == "") return;

            Graphics.Video.SetScreenMode(800, 600, 32, false);

            try
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                System.Windows.Forms.MessageBox.Show("Unable to load "
                    + sceneToLoad + ", possibly because the required files "
                    + "are inside barn files that the viewer couldn't find.",
                    "Unable to load scene!", System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);

                return;
            }
       
            SceneManager.CurrentCamera = new Camera();

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

                if (Input.IsFreshKeyPressed(Sdl.SDLK_l))
                {
                    if (SceneManager.LightmapsEnabled)
                        SceneManager.LightmapsEnabled = false;
                    else
                        SceneManager.LightmapsEnabled = true;
                }

                if (Input.IsKeyPressed(Sdl.SDLK_t))
                {
                    SceneManager.CurrentShadeMode = ShadeMode.Textured;
                }
                else if (Input.IsKeyPressed(Sdl.SDLK_f))
                {
                    SceneManager.CurrentShadeMode = ShadeMode.Flat;
                }
                else if (Input.IsKeyPressed(Sdl.SDLK_c))
                {
                    SceneManager.CurrentShadeMode = ShadeMode.Colored;
                }

                SceneManager.CurrentCamera.AddRelativePositionOffset(new Math.Vector(x, y, z));

                int mx, my;
                Input.GetRelMouseCoords(out mx, out my);
                SceneManager.CurrentCamera.AdjustYaw(mx * -0.01f);
                SceneManager.CurrentCamera.AdjustPitch(my * -0.01f);

                SceneManager.Render();
            }

            Logger.Close();
        }

        private static string getSceneToLoad(BarnLib.Barn barn)
        {
            SceneChooser chooser = new SceneChooser();

            for (uint i = 0; i < barn.NumberOfFiles; i++)
            {
                string name = barn.GetFileName(i);

                if (name.EndsWith(".SCN"))
                    chooser.AddFile(name);
            }

            System.Windows.Forms.DialogResult result = chooser.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                return chooser.SelectedScene;

            return "";
        }

        private static string getCoreBarnPath()
        {
//#if !DEBUG
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Barn files (*.brn)|*.brn|All files (*.*)|*.*";
            dialog.Title = "Open barn file";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                return dialog.FileName;

            return "";
//#else
          // return @"E:\gk3backup\Data\core.brn";
//#endif
        }
    }
}
