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

namespace Gk3Main
{
    public enum ShadeMode
    {
        Wireframe,
        Flat,
        Colored,
        Textured
    }

    public class SceneManagerException : Exception
    {
        public SceneManagerException(string message)
            : base(message)
        {

        }

        public SceneManagerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public static class SceneManager
    {
        public static void Initialize()
        {
            _verbs = new Gk3Main.Game.Verbs("verbs.txt", FileSystem.Open("verbs.txt"));
        }

        public static void LoadSif(string sif)
        {
            Gk3Main.Game.SifResource sifResource = (Gk3Main.Game.SifResource)Gk3Main.Resource.ResourceManager.Load(sif);

            LoadScene(sifResource.Scene);

            // load the models
            foreach (Game.SifModel model in sifResource.Models)
            {
                if (model.Type == Gk3Main.Game.SifModelType.Prop && model.Hidden == false)
                {
                    AddModel(model.Name + ".MOD");
                }
            }

            // load the NVCs
            foreach (string nvcFile in sifResource.Actions)
            {
                _nvcs.Add((Game.NvcResource)Resource.ResourceManager.Load(nvcFile));
            }
        }

        public static void LoadScene(string scn)
        {
            try
            {
                Game.ScnResource scnFile = (Game.ScnResource)Resource.ResourceManager.Load(scn);

                string bspFile = scnFile.BspFile.ToUpper() + ".BSP";

                // load the BSP
                if (_currentRoom != null)
                    Resource.ResourceManager.Unload(_currentRoom);

                _currentRoom = (Graphics.BspResource)Resource.ResourceManager.Load(bspFile);

                // load the lightmaps
                if (_currentLightmaps != null)
                    Resource.ResourceManager.Unload(_currentLightmaps);

                string lightmapFile = Utils.GetFilenameWithoutExtension(scn) + ".MUL";
                _currentLightmaps = (Graphics.LightmapResource)Resource.ResourceManager.Load(lightmapFile);

                unloadModels();
            }
            catch (System.IO.FileNotFoundException ex)
            {
                throw new SceneManagerException("Unable to load the scene", ex);
            }
        }

        public static void AddModel(string modelname)
        {
            Graphics.ModelResource model = 
                (Graphics.ModelResource)Resource.ResourceManager.Load(modelname);

            _models.Add(model);
        }

        public static void Render(Graphics.Camera camera)
        {
            if (camera != null)
                camera.Update();

            // render the room
            if (camera != null && _currentRoom != null)
                _currentRoom.Render(_currentLightmaps);

            // render the models
            foreach (Graphics.ModelResource model in _models)
                model.Render();
        }

        public static bool LightmapsEnabled
        {
            get { return _lightmapsEnabled; }
            set { _lightmapsEnabled = value; }
        }

        public static bool DoubleLightmapValues
        {
            get { return _doubleLightmapValues; }
            set { _doubleLightmapValues = value; }
        }

        public static ShadeMode CurrentShadeMode
        {
            get { return _shadeMode; }
            set { _shadeMode = value; }
        }

        /// <summary>
        /// Collides a ray against the world and returns the name of the
        /// first model the ray collided with.
        /// </summary>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="length">The length of the ray.</param>
        /// <returns>The name of the model, or null if no collision occured.</returns>
        public static string GetCollisionModel(Math.Vector origin, Math.Vector direction, float length)
        {
            Graphics.BspSurface surface;
            if (_currentRoom != null && _currentRoom.CollideRayWithSurfaces(origin, direction, length, out surface) == true)
                return _currentRoom.GetModelName(surface.modelIndex);

            return null;
        }

        public static int GetNounVerbCaseCountForTarget(string target)
        {
            int count = 0;
            foreach (Game.NvcResource nvcResource in _nvcs)
            {
                foreach (Game.NounVerbCase nvc in nvcResource.NounVerbCases)
                {
                    if (nvc.Target == target)
                        count++;
                }
            }

            return count;
        }

        private static void unloadModels()
        {
            foreach (Graphics.ModelResource model in _models)
            {
                Resource.ResourceManager.Unload(model);
            }

            _models.Clear();
        }

        private static Graphics.BspResource _currentRoom;
        private static Graphics.LightmapResource _currentLightmaps;
        private static List<Graphics.ModelResource> _models = new List<Gk3Main.Graphics.ModelResource>();
        private static List<Game.NvcResource> _nvcs = new List<Gk3Main.Game.NvcResource>();

        private static ShadeMode _shadeMode = ShadeMode.Textured;
        private static bool _lightmapsEnabled = false;
        private static bool _doubleLightmapValues = false;

        private static Game.Verbs _verbs;
    }
}
