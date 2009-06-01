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
            ///_verbs = new Gk3Main.Game.Verbs("verbs.txt", FileSystem.Open("verbs.txt"));
        }

        public static void LoadSif(string sif)
        {
            Gk3Main.Game.SifResource sifResource = (Gk3Main.Game.SifResource)Gk3Main.Resource.ResourceManager.Load(sif);

            // attempt to load a "parent" sif
            Gk3Main.Game.SifResource parentSif = null;
            string parentSifName = sif.Substring(0, 3) + ".SIF";
            if (parentSifName.Equals(sif, StringComparison.OrdinalIgnoreCase) == false)
            {
                try
                {
                    parentSif = (Gk3Main.Game.SifResource)Gk3Main.Resource.ResourceManager.Load(parentSifName);
                }
                catch
                {
                    // ignore
                }
            }

            if (string.IsNullOrEmpty(sifResource.Scene) == false)
                LoadScene(sifResource.Scene);
            else if (parentSif != null && string.IsNullOrEmpty(parentSif.Scene) == false)
                LoadScene(parentSif.Scene);

            // load the models
            _modelNounMap.Clear();
            loadSifModels(sifResource);
            if (parentSif != null) loadSifModels(parentSif);

            // load the NVCs
            loadSifNvcs(sifResource);
            if (parentSif != null) loadSifNvcs(parentSif);
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
                _currentSkybox = loadSkybox(scnFile);

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

            // render the skybox
            if (_currentSkybox != null)
                _currentSkybox.Render(camera);

            // render the room
            if (camera != null && _currentRoom != null)
                _currentRoom.Render(camera, _currentLightmaps);

            // render the models
            foreach (Graphics.ModelResource model in _models)
                model.Render(camera);
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

        public static bool IsSceneLoaded
        {
            get { return _currentRoom != null; }
        }

        /// <summary>
        /// Collides a ray against the world and returns the name of the
        /// first model the ray collided with.
        /// </summary>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="length">The length of the ray.</param>
        /// <returns>The name of the model, or null if no collision occured.</returns>
        public static string GetCollisionModel(Math.Vector3 origin, Math.Vector3 direction, float length)
        {
            Graphics.BspSurface surface;
            if (_currentRoom != null && _currentRoom.CollideRayWithSurfaces(origin, direction, length, out surface) == true)
                return _currentRoom.GetModelName(surface.modelIndex);

            return null;
        }

        public static string GetModelNoun(string model)
        {
            string noun = null;
            _modelNounMap.TryGetValue(model, out noun);

            return noun;
        }

        public static int GetNounVerbCaseCountForTarget(string target)
        {
            int count = 0;
            foreach (Game.NvcResource nvcResource in _nvcs)
            {
                foreach (Game.NounVerbCase nvc in nvcResource.NounVerbCases)
                {
                    if (nvc.Target != null && nvc.Target.Equals(target, StringComparison.OrdinalIgnoreCase))
                        count++;
                }
            }

            return count;
        }

        public static int GetNounVerbCaseCountForNoun(string noun)
        {
            int count = 0;
            foreach (Game.NvcResource nvcResource in _nvcs)
            {
                foreach (Game.NounVerbCase nvc in nvcResource.NounVerbCases)
                {
                    if (nvc.Target != null && nvc.Noun.Equals(noun, StringComparison.OrdinalIgnoreCase))
                        count++;
                }
            }

            return count;
        }

        public static List<string> GetAllModels()
        {
            if (_currentRoom != null)
                return _currentRoom.GetAllModels();

            // just return an empty list
            return new List<string>();
        }

        private static void loadSifModels(Game.SifResource sif)
        {
            foreach (Game.SifModel model in sif.Models)
            {
                if (model.Type == Gk3Main.Game.SifModelType.Prop && model.Hidden == false)
                {
                    AddModel(model.Name + ".MOD");
                }

                if (string.IsNullOrEmpty(model.Noun) == false)
                    _modelNounMap.Add(model.Name, model.Noun);
            }
        }

        private static void loadSifNvcs(Game.SifResource sif)
        {
            foreach (string nvcFile in sif.Actions)
            {
                try
                {
                    _nvcs.Add((Game.NvcResource)Resource.ResourceManager.Load(nvcFile));
                }
                catch (System.IO.FileNotFoundException)
                {
                    // do nothing, sometimes NVCs just don't exist
                }
            }
        }

        private static void unloadModels()
        {
            foreach (Graphics.ModelResource model in _models)
            {
                Resource.ResourceManager.Unload(model);
            }

            _models.Clear();
        }

        private static Graphics.SkyBox loadSkybox(Game.ScnResource scn)
        {
            if (string.IsNullOrEmpty(scn.SkyboxLeft) == false &&
                string.IsNullOrEmpty(scn.SkyboxRight) == false &&
                string.IsNullOrEmpty(scn.SkyboxFront) == false &&
                string.IsNullOrEmpty(scn.SkyboxBack) == false &&
                string.IsNullOrEmpty(scn.SkyboxUp) == false &&
                string.IsNullOrEmpty(scn.SkyboxDown) == false)
            {
                return new Gk3Main.Graphics.SkyBox(scn.Name + "_skybox", scn.SkyboxFront + ".BMP", scn.SkyboxBack + ".BMP",
                    scn.SkyboxLeft + ".BMP", scn.SkyboxRight + ".BMP", scn.SkyboxUp + ".BMP", scn.SkyboxDown + ".BMP", scn.SkyboxAzimuth);
            }

            return null;
        }

        private static Graphics.SkyBox _currentSkybox;
        private static Graphics.BspResource _currentRoom;
        private static Graphics.LightmapResource _currentLightmaps;
        private static List<Graphics.ModelResource> _models = new List<Gk3Main.Graphics.ModelResource>();
        private static Dictionary<string, string> _modelNounMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static List<Game.NvcResource> _nvcs = new List<Gk3Main.Game.NvcResource>();

        private static ShadeMode _shadeMode = ShadeMode.Textured;
        private static bool _lightmapsEnabled = false;
        private static bool _doubleLightmapValues = false;
    }
}
