// Copyright (c) 2009 Brad Farris
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
// along with the GK3 Scene Viewer; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;

using Gk3Main.Game;

namespace Gk3Main
{
    public enum ShadeMode
    {
        Wireframe,
        Flat,
        Colored,
        Textured
    }

    public enum TextureFilterMode
    {
        None,
        Linear,
        Anisotropic2X,
        Anisotropic4X
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

    struct SceneModel
    {
        public string Name;
        public Graphics.ModelResource Model;
        public bool Visible;
        public Math.Matrix Transform;
    }

    public static class SceneManager
    {
        public static ISceneCustomizer SceneCustomizer
        {
            get { return _sceneCustomizer; }
        }

        public static void Initialize(Resource.ResourceManager globalContent)
        {
            ///_verbs = new Gk3Main.Game.Verbs("verbs.txt", FileSystem.Open("verbs.txt"));
            loadGlobalNvc(globalContent);

            _sceneContentManager = new Resource.ResourceManager();
        }

        public static void Reset()
        {
            _sceneContentManager.UnloadAll();
            _sceneCustomizer = null;
        }

        public static void LoadSif(string location, string timeblock)
        {
            try
            {
                LoadSif(location + timeblock);
            }
            catch (System.IO.FileNotFoundException)
            {
                // try something else...
                LoadSif(location);
            }

            setupCustomScenes(location);
        }

        public static void LoadSif(string sif)
        {
            Logger.WriteInfo("Loading SIF: " + sif, LoggerStream.Normal);

            Animator.StopAll();
            Sheep.SheepMachine.CancelAllWaits();

            SifResource sifResource = _sceneContentManager.Load<SifResource>(sif);

            _currentRoom = null;
            _currentLightmaps = null;
            _roomPositions.Clear();
            _cameras.Clear();
            _modelNounMap.Clear();
            _stks.Clear();
            NvcManager.Reset();

            // attempt to load a "parent" sif
            Gk3Main.Game.SifResource parentSif = null;
            if (sif.IndexOf('.') != 3 && sif.Length > 3) //if we're loading "XXX.SIF" then we're already loading the parent sif
            {
                string parentSifName = sif.Substring(0, 3);
                if (parentSifName.Equals(sif, StringComparison.OrdinalIgnoreCase) == false)
                {
                    try
                    {
                        parentSif = _sceneContentManager.Load<SifResource>(parentSifName);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            if (string.IsNullOrEmpty(sifResource.Scene) == false)
                LoadScene(sifResource.Scene);
            else if (parentSif != null && string.IsNullOrEmpty(parentSif.Scene) == false)
                LoadScene(parentSif.Scene);

            // load the pathing info
            if (string.IsNullOrEmpty(sifResource.Boundary) == false)
                _currentPathMap = new ActorPathfinder(sifResource.Boundary, sifResource.BoundarySize, sifResource.BoundaryOffset);
            else if (parentSif != null && string.IsNullOrEmpty(parentSif.Boundary) == false)
                _currentPathMap = new ActorPathfinder(parentSif.Boundary, parentSif.BoundarySize, parentSif.BoundaryOffset);

            // temp
           /* Math.Vector2 start = new Math.Vector2(17,41);
            Math.Vector2 end = new Math.Vector2(35, 45);
            Math.Vector2[] path = _currentPathMap.CalculatePath(start, end);
            Logger.WriteInfo("path from " + start.ToString() + " to " + end.ToString());
            if (path == null)
                Logger.WriteInfo("NO PATH FOUND!");
            else
               _currentPathMap.PrintPathToLogger(path);
           */
            // load the models
            _modelNounMap.Clear();
            loadSifModels(sifResource);
            if (parentSif != null) loadSifModels(parentSif);

            // load the NVCs
            loadSifNvcs(sifResource);
            if (parentSif != null) loadSifNvcs(parentSif);
            NvcManager.Compile();

            // load the STKs
            Sound.SoundManager.StopChannel(Sound.SoundTrackChannel.Music);
            loadSifStks(sifResource);
            if (parentSif != null) loadSifStks(parentSif);

            // load positions and room cameras
            foreach (SifRoomCamera camera in sifResource.RoomCameras)
                _cameras.Add(camera.Name, camera);
            foreach (SifPosition position in sifResource.Positions)
                _roomPositions.Add(position.Name, position);

            if (parentSif != null)
            {
                foreach (SifRoomCamera camera in parentSif.RoomCameras)
                {
                    // only add if it doesn't exist yet
                    if (_cameras.ContainsKey(camera.Name) == false)
                        _cameras.Add(camera.Name, camera);
                }
                foreach (SifPosition position in parentSif.Positions)
                {
                    // only add if it doesn't exist yet
                    if (_roomPositions.ContainsKey(position.Name) == false)
                        _roomPositions.Add(position.Name, position);
                }
            }

            loadSifActorModels(sifResource);
            if (parentSif != null) loadSifActorModels(parentSif);

            Sound.SoundManager.StopChannel(Gk3Main.Sound.SoundTrackChannel.Ambient);

            setupCustomScenes(GameManager.CurrentLocation);
        }

        public static void LoadScene(string scn)
        {
            try
            {
                ScnResource scnFile = _sceneContentManager.Load<ScnResource>(scn);
                string scnWithoutExtension = Utils.GetFilenameWithoutExtension(scn);

                string bspFile = scnFile.BspFile;

                _currentRoom = _sceneContentManager.Load<Graphics.BspResource>(bspFile);
                _currentSkybox = loadSkybox(scnFile);

                // load the lightmaps
                _currentLightmaps = _sceneContentManager.Load<Graphics.LightmapResource>(scnWithoutExtension);

                _currentRoom.FinalizeVertices(_currentLightmaps, false);

                _models.Clear();
                unloadActors();
            }
            catch (System.IO.FileNotFoundException ex)
            {
                throw new SceneManagerException("Unable to load the scene", ex);
            }
        }

        /// <summary>
        /// Loads the specified BSP. This should only be used by the viewer!
        /// Otherwise you should be loading via LoadSif() or LoadScene().
        /// </summary>
        public static void LoadBsp(string bsp)
        {
            _currentRoom = _sceneContentManager.Load<Graphics.BspResource>(bsp);

            // load the lightmaps
            string bspWithoutExtension = Utils.GetFilenameWithoutExtension(bsp);
            try
            {
                _currentLightmaps = _sceneContentManager.Load<Graphics.LightmapResource>(bspWithoutExtension);
            }
            catch (System.IO.FileNotFoundException)
            {
                _currentLightmaps = null;
            }

            _currentRoom.FinalizeVertices(_currentLightmaps, false);
        }

        public static void AddModel(string modelname, bool visible)
        {
            SceneModel sceneModel;
            sceneModel.Name = modelname;
            sceneModel.Model = _sceneContentManager.Load<Graphics.ModelResource>(modelname);
            sceneModel.Visible = visible;
            sceneModel.Transform = Math.Matrix.Identity;

            _models.Add(sceneModel);
        }

        public static void AddGas(string gasFileName)
        {
            GasResource gas = _sceneContentManager.Load<GasResource>(gasFileName);

            _modelGases.Add(gas);
            gas.Play();
        }

        public static void AddActor(string modelName, string noun, Math.Vector3 position, float heading, bool isEgo)
        {
            Game.Actor actor = new Game.Actor(_sceneContentManager, modelName, noun, isEgo);
            actor.Position = position;
            actor.FacingAngle = heading;

            _actors.Add(actor);

            actor.LoadClothing(_sceneContentManager);
        }

        public static Actor GetActor(string actorNoun)
        {
            for (int i = 0; i < _actors.Count; i++)
                if (_actors[i].Noun.Equals(actorNoun))
                    return _actors[i];

            return null;
        }

        public static Graphics.ModelResource GetSceneModel(string model)
        {
            for (int i = 0; i < _models.Count; i++)
            {
                if (_models[i].Name.Equals(model, StringComparison.OrdinalIgnoreCase))
                    return _models[i].Model;
            }

            // couldn't find it? maybe it's an actor...
            for (int i = 0; i < _actors.Count; i++)
                if (_actors[i].ModelName.Equals(model, StringComparison.OrdinalIgnoreCase))
                    return _actors[i].Model;

            return null;
        }

        public static void SetSceneModelVisibility(string name, bool visible)
        {
            for (int i = 0; i < _models.Count; i++)
            {
                if (_models[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    SceneModel model = _models[i];
                    model.Visible = visible;
                    _models[i] = model;
                }
            }
        }

        public static void SetModelTexture(string model, int meshIndex, int groupIndex, string texture)
        {
            // find the model
            Graphics.ModelResource m = GetSceneModel(model);

            if (m != null)
                m.ReplaceTexture(meshIndex, groupIndex, texture + ".BMP");
        }

        public static void Render()
        {
            Render(_currentCamera);
        }

        public static void Render(Graphics.Camera camera)
        {
            if (camera != null)
            {
                camera.Update();

                // render the skybox
                if (_currentSkybox != null)
                    _currentSkybox.Render(camera);

                // render the room
                if (camera != null && _currentRoom != null)
                    _currentRoom.Render(camera, _currentLightmaps, false);

                // render the models
                Graphics.ModelResource.BeginBatchRender();
                foreach (SceneModel model in _models)
                {
                    if (model.Visible)
                        model.Model.RenderBatch(camera, model.Model.TempTransform);
                }

                // render the actors
                foreach (Game.Actor actor in _actors)
                    actor.RenderBatch(camera);

                Graphics.ModelResource.EndBatchRender();

                // render model AABBs (if needed)
                if (Settings.ShowBoundingBoxes)
                {
                    foreach (SceneModel model in _models)
                    {
                        if (model.Visible)
                            model.Model.RenderAABB(camera);
                    }

                    foreach (Game.Actor actor in _actors)
                        actor.RenderAABB(camera);
                }

                // add helpers to the billboard list
                foreach (KeyValuePair<string, SifPosition> position in _roomPositions)
                {
                    //Graphics.BillboardManager.AddBillboard(new Math.Vector3(position.Value.X, position.Value.Y + 30.0f, position.Value.Z),
                    //    50.0f, 50.0f, HelperIcons.Flag);
                }

                foreach (KeyValuePair<string, SifRoomCamera> rcamera in _cameras)
                {
                    //Graphics.BillboardManager.AddBillboard(new Math.Vector3(rcamera.Value.X, rcamera.Value.Y + 30.0f, rcamera.Value.Z),
                    //    50.0f, 50.0f, HelperIcons.Camera);
                }

                // render any billboards
                Graphics.BillboardManager.RenderBillboards(camera);
            }

            foreach (Sound.SoundTrackResource stk in _stks)
                stk.Step(Game.GameManager.TickCount);
            foreach (Sound.SoundTrackResource stk in _playingSoundTracks)
                stk.Step(Game.GameManager.TickCount);

            // remove stale STKs
            for (LinkedListNode<Sound.SoundTrackResource> node = _playingSoundTracks.First;
                node != null;  )
            {
                LinkedListNode<Sound.SoundTrackResource> next = node.Next;

                if (node.Value.Playing == false)
                {
                    _playingSoundTracks.Remove(node);
                }

                node = next;
            }

            // run any GASes
            for (int i = 0; i < _modelGases.Count; i++)
                _modelGases[i].Continue();
        }

        public static Resource.ResourceManager SceneContentManager
        {
            get { return _sceneContentManager; }
        }

        public static Graphics.Camera CurrentCamera
        {
            get { return _currentCamera; }
            set { _currentCamera = value; }
        }

        public static Graphics.BspResource CurrentRoom
        {
            get { return _currentRoom; }
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

        public static TextureFilterMode CurrentFilterMode
        {
            get { return _currentFilterMode; }
            set { _currentFilterMode = value; }
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
            float distance;
            foreach (SceneModel model in _models)
            {
                if (model.Visible && model.Model.CollideRay(Math.Vector3.Zero, origin, direction, length, out distance))
                {
                    Console.CurrentConsole.WriteLine(model.Name);
                    return model.Model.NameWithoutExtension;
                }
            }

            foreach (Game.Actor actor in _actors)
            {
                if (actor.CollideRay(origin, direction, length, out distance))
                {
                    Console.CurrentConsole.WriteLine(actor.ModelName);
                    return actor.ModelName;
                }
            }

            Graphics.BspSurface surface;
            if (_currentRoom != null && _currentRoom.CollideRayWithSurfaces(origin, direction, length, out surface) == true)
                return _currentRoom.GetModelName(surface.modelIndex);

            

            return null;
        }

        public static Nouns GetModelNoun(string model)
        {
            Nouns noun = Nouns.N_NONE;
            _modelNounMap.TryGetValue(model, out noun);

            return noun;
        }

        public static List<string> GetAllModels()
        {
            if (_currentRoom != null)
                return _currentRoom.GetAllModels();

            // just return an empty list
            return new List<string>();
        }

        /// <summary>
        /// Gets a model by its name, or null if it doesn't exist
        /// </summary>
        public static Graphics.ModelResource GetModelByName(string name, bool includeActorModels)
        {
            for (int i = 0; i < _models.Count; i++)
                if (_models[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return _models[i].Model;

            if (includeActorModels)
            {
                for (int i = 0; i < _actors.Count; i++)
                    if (_actors[i].Model != null && _actors[i].Model.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        return _actors[i].Model;
            }

            return null;
        }

        public static void SetEgoToSifPosition(string name)
        {
            SifPosition position = _roomPositions[name];

            foreach (Actor a in _actors)
            {
                if (a.IsEgo)
                {
                    a.Position = new Math.Vector3(position.X, position.Y, position.Z);
                    a.FacingAngle = Utils.DegreesToRadians(position.HeadingDegrees);
                    return;
                }
            }
        }

        public static void SetCameraToSifPosition(string name)
        {
            /*if (_currentCamera != null)
            {
                SifPosition position = _roomPositions[name];
                SifRoomCamera camera = _cameras[position.CameraName];

                _currentCamera.SetPitchYaw(Utils.DegreesToRadians(camera.PitchDegrees), Utils.DegreesToRadians(camera.YawDegrees));
                //_currentCamera.AdjustPitch(Utils.DegreesToRadians(camera.PitchDegrees));
                //_currentCamera.AdjustYaw(Utils.DegreesToRadians(camera.YawDegrees));
                _currentCamera.Position = new Math.Vector3(camera.X, camera.Y, camera.Z);
            }*/

            SifPosition position = _roomPositions[name];
            SifRoomCamera camera = _cameras[position.CameraName];

            _currentCamera = GameManager.CreateCameraWithDefaults();
            _currentCamera.SetPitchYaw(Utils.DegreesToRadians(camera.PitchDegrees), Utils.DegreesToRadians(camera.YawDegrees));
            _currentCamera.Position = new Math.Vector3(camera.X, camera.Y, camera.Z);
        }

        

        public static void SetCameraToCinematicCamera(string name)
        {
            /*if (_currentCamera != null)
            {
                SifRoomCamera camera = _cameras[name];

                //_currentCamera.SetPitchYaw(Utils.DegreesToRadians(camera.PitchDegrees), Utils.DegreesToRadians(camera.YawDegrees));
               // _currentCamera.AdjustPitch(Utils.DegreesToRadians(camera.PitchDegrees));
                //_currentCamera.AdjustYaw(Utils.DegreesToRadians(camera.YawDegrees));
                //_currentCamera.Position = new Math.Vector3(camera.X, camera.Y, camera.Z);

                _currentCamera = camera.Camera;
            }*/

            SifRoomCamera camera = _cameras[name];
            _currentCamera = camera.Camera;
        }

        public static void PlaySoundTrack(string name)
        {
            try
            {
                Sound.SoundTrackResource stk = _sceneContentManager.Load<Sound.SoundTrackResource>(name);
                stk.Start(GameManager.TickCount);

                _playingSoundTracks.AddFirst(stk);
            }
            catch(System.IO.FileNotFoundException)
            {
                // apparently this can happen sometimes, but let's at least warn
                Logger.WriteError("Unable to play {0} because it couldn't be found.", name);
            }
        }

        public static void StopSoundTrack(string name)
        {
            for (LinkedListNode<Sound.SoundTrackResource> node = _playingSoundTracks.First;
                node != null; node = node.Next)
            {
                if (node.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    _playingSoundTracks.Remove(node);
                    break;
                }
            }
        }

        public static void SetActorPosition(string noun, Math.Vector3 position, float heading)
        {
            foreach (Actor actor in _actors)
            {
                if (actor.Noun.Equals(noun, StringComparison.OrdinalIgnoreCase))
                {
                    actor.Position = position;
                    actor.FacingAngle = heading;
                    actor.Model.ClearAnimatedTransforms();
                    break;
                }
            }
        }

        public static void SetActorPosition(string noun, string position)
        {
            SifPosition pos;
            if (_roomPositions.TryGetValue(position, out pos))
            {
                SetActorPosition(noun, new Math.Vector3(pos.X, pos.Y, pos.Z), Utils.DegreesToRadians(pos.HeadingDegrees));
            }
        }

        public static void CallSceneFunction(string function)
        {
            if (_sceneCustomizer != null)
               _sceneCustomizer.OnCustomFunction(function);
        }

        public static void CalculateLightmaps()
        {
            if (_currentRoom != null && _currentLightmaps != null)
            {
                Radiosity.Init(new Radiosity.RenderDelegate(renderRadiosityCallback));

                CurrentFilterMode = TextureFilterMode.None;
                LightmapsEnabled = true;
                CurrentShadeMode = ShadeMode.Flat;
                DoubleLightmapValues = false;
                Graphics.Camera originalCamera = CurrentCamera;
                Graphics.Viewport originalViewport = Graphics.RendererManager.CurrentRenderer.Viewport;

                RadiosityMaps radiosityMaps = _currentRoom.GenerateMemoryTextures(_currentLightmaps);

                Graphics.RendererManager.CurrentRenderer.CullMode = Graphics.CullMode.None;

                Graphics.TextureResource skyboxTop = Radiosity.GenerateMemoryTexture(1, 1, 200.0f, 200.0f, 200.0f);
                Graphics.TextureResource skyboxElse =  Radiosity.GenerateMemoryTexture(1, 1, 0, 0, 0);
                Graphics.BitmapSurface skyboxTopPixels = new Graphics.BitmapSurface(skyboxTop);
                Graphics.BitmapSurface skyboxElsePixels = new Graphics.BitmapSurface(skyboxElse);

                Graphics.SkyBox originalSkybox = _currentSkybox;
                _currentSkybox = new Graphics.SkyBox("box", skyboxElsePixels, skyboxElsePixels, skyboxElsePixels, skyboxElsePixels, skyboxTopPixels, skyboxElsePixels, 0);

                Graphics.LightmapResource oldLightmaps = _currentLightmaps;
                _currentLightmaps = radiosityMaps.CreateBigMemoryTexture();
                _currentRoom.CalcRadiosityPass(_currentLightmaps, radiosityMaps);

                Radiosity.Shutdown();

                _currentRoom.FinalizeVertices(_currentLightmaps, true);
                _currentLightmaps = radiosityMaps.ConvertToLightmap(0.02f);

                Graphics.RendererManager.CurrentRenderer.CullMode = Graphics.CullMode.CounterClockwise;
                Graphics.RendererManager.CurrentRenderer.Viewport = originalViewport;
                CurrentCamera = originalCamera;
                _currentSkybox = originalSkybox;
            }
        }

        public static void renderRadiosityCallback(Radiosity.HemicubeRenderType type,
           int viewportX, int viewportY, int viewportWidth, int viewportHeight,
            float eyeX, float eyeY, float eyeZ,
            float directionX, float directionY, float directionZ,
            float upX, float upY, float upZ)
        {
            Graphics.RendererManager.CurrentRenderer.Viewport = new Graphics.Viewport(viewportX, viewportY, viewportWidth, viewportHeight);

            Graphics.Camera c = null;
           bool zNegOne = (Graphics.RendererManager.CurrentRenderer.ZClipMode == Gk3Main.Graphics.ZClipMode.NegativeOne);

           const float near = 2.0f;

            Math.Vector3 position = new Math.Vector3(eyeX, eyeY, eyeZ);
            Math.Vector3 direction = new Math.Vector3(directionX, directionY, directionZ);

            if (type == Radiosity.HemicubeRenderType.Front)
            {
                c = new Graphics.Camera(90.0f * 0.0174532925f, 1.0f, near, 1000.0f, zNegOne);
            }
            else if (type == Radiosity.HemicubeRenderType.Top)
            {
                Math.Matrix projection = Math.Matrix.PerspectiveOffCenter(-near, near, -near, 0, near, 1000.0f, zNegOne);
                c = new Graphics.Camera(projection);
            }
            else if (type == Radiosity.HemicubeRenderType.Bottom)
            {
                Math.Matrix projection = Math.Matrix.PerspectiveOffCenter(-near, near, 0, near, near, 1000.0f, zNegOne);
                c = new Graphics.Camera(projection);
            }
            else if (type == Radiosity.HemicubeRenderType.Left)
            {
                Math.Matrix projection = Math.Matrix.PerspectiveOffCenter(-near, 0, -near, near, near, 1000.0f, zNegOne);
                c = new Graphics.Camera(projection);
            }
            else if (type == Radiosity.HemicubeRenderType.Right)
            {
                Math.Matrix projection = Math.Matrix.PerspectiveOffCenter(0, near, -near, near, near, 1000.0f, zNegOne);
                c = new Graphics.Camera(projection);
            }

            c.LookAt(new Math.Vector3(eyeX, eyeY, eyeZ), new Math.Vector3(directionX, directionY, directionZ), new Math.Vector3(upX, upY, upZ));

            _currentSkybox.Render(c);
            _currentRoom.Render(c, _currentLightmaps, true);
        }

        private static void setupCustomScenes(string location)
        {
           // not sure if hardcoding is the best way to do this...
           if (location.Equals(Game.LocationCodes.ChateauDeSerrasLibrary, StringComparison.OrdinalIgnoreCase))
              _sceneCustomizer = new LaserSceneCustomizer();
           else
              _sceneCustomizer = null;

           if (_sceneCustomizer != null) _sceneCustomizer.OnLoad();
        }

        private static void loadSifModels(Game.SifResource sif)
        {
            foreach (Game.SifModel model in sif.Models)
            {
                if (model.Type == Gk3Main.Game.SifModelType.Prop ||
                    model.Type == SifModelType.GasProp)
                {
                    AddModel(model.Name, !model.Hidden && 
                        GameManager.IsInInventory(model.Noun, true) == false && 
                        GameManager.IsInInventory(model.Noun, false) == false);

                    if (model.Type == SifModelType.GasProp)
                        AddGas(model.Gas);
                }

                if (_currentRoom != null)
                {
                    // hide the surface if it shouldn't be visible
                    if (model.Type == SifModelType.HitTest || model.Hidden)
                        _currentRoom.SetSurfaceVisibility(model.Name, false);
                }

                if (string.IsNullOrEmpty(model.Noun) == false)
                {
                    Nouns n = NounUtils.ConvertStringToNoun(model.Noun);
                    _modelNounMap.Add(model.Name, n);
                }

                // play the first frame of the init animation (if it exists)
                if (model.InitAnim != null)
                {
                    _sceneContentManager.Load<MomResource>(model.InitAnim + ".ANM").Play(true);
                }
            }
        }

        private static void loadSifActorModels(Game.SifResource sif)
        {
            foreach (Game.SifActor actor in sif.Actors)
            {
                AddActor(actor.Model, actor.Noun, Math.Vector3.Zero, 0, actor.IsEgo);

                if (string.IsNullOrEmpty(actor.Pos) == false)
                    SetActorPosition(actor.Noun, actor.Pos);

                if (string.IsNullOrEmpty(actor.Noun) == false)
                {
                    Nouns n = NounUtils.ConvertStringToNoun(actor.Noun);
                    _modelNounMap.Add(actor.Model, n);
                }

                // play the first frame of the init animation (if it exists)
                if (actor.InitAnim != null)
                {
                   try
                   {
                      _sceneContentManager.Load<MomResource>(actor.InitAnim + ".ANM").Play(true);
                   }
                   catch (System.IO.FileNotFoundException)
                   {
                      // apparently, especially when playing the GK3 demo,
                      // some SIF files can refer to actors that don't actually
                      // exist in the demo, so we need to ignore these errors
                   }
                }
            }
        }

        private static void loadGlobalNvc(Resource.ResourceManager content)
        {
            NvcManager.AddNvc(content.Load<NvcResource>("GLB_ALL.NVC"), true);
        }

        private static void loadSifNvcs(Game.SifResource sif)
        {
            foreach (string nvcFile in sif.Actions)
            {
                try
                {
                    // just because an NVC is loaded doesn't automatically mean we should load and use it!
                    int underscore = nvcFile.IndexOf("_");
                    if (underscore >= 0)
                    {
                        int all = nvcFile.IndexOf("all", StringComparison.OrdinalIgnoreCase);
                        if (all != underscore + 1) // check for day limitations
                        {
                            // I think looking for underscore should be enough to determine which NVCs we need
                            // to examine closer... maybe...
                            int currentDay = GameManager.CurrentDay;
                            if (nvcFile.Substring(underscore + 1).Contains(currentDay.ToString()) == false)
                                continue;
                        }
                    }

                    NvcResource nvc = _sceneContentManager.Load<NvcResource>(nvcFile);
                    NvcManager.AddNvc(nvc, false);
                }
                catch (System.IO.FileNotFoundException)
                {
                    // do nothing, sometimes NVCs just don't exist
                }
            }
        }

        private static void loadSifStks(Game.SifResource sif)
        {
            foreach (string stkFile in sif.SoundTracks)
            {
                Sound.SoundTrackResource stk = _sceneContentManager.Load<Sound.SoundTrackResource>(stkFile);
                stk.Start(Game.GameManager.TickCount);

                _stks.Add(stk);
            }
        }
        
        private static void unloadActors()
        {
            _actors.Clear();
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
                System.IO.Stream frontStream = FileSystem.Open(scn.SkyboxFront + ".BMP");
                System.IO.Stream backStream = FileSystem.Open(scn.SkyboxBack + ".BMP");
                System.IO.Stream leftStream = FileSystem.Open(scn.SkyboxLeft + ".BMP");
                System.IO.Stream rightStream = FileSystem.Open(scn.SkyboxRight + ".BMP");
                System.IO.Stream upStream = FileSystem.Open(scn.SkyboxUp + ".BMP");

                Graphics.BitmapSurface fronts = new Graphics.BitmapSurface(frontStream);
                Graphics.BitmapSurface backs = new Graphics.BitmapSurface(backStream);
                Graphics.BitmapSurface lefts = new Graphics.BitmapSurface(leftStream);
                Graphics.BitmapSurface rights = new Graphics.BitmapSurface(rightStream);
                Graphics.BitmapSurface ups = new Graphics.BitmapSurface(upStream);
                Graphics.BitmapSurface downs = null;

                try
                {
                    System.IO.Stream downStream = FileSystem.Open(scn.SkyboxDown + ".BMP");
                    downs = new Graphics.BitmapSurface(downStream);
                    downStream.Close();
                }
                catch (System.IO.FileNotFoundException)
                {
                    downs = lefts;
                }

                frontStream.Close();
                backStream.Close();
                leftStream.Close();
                rightStream.Close();
                upStream.Close();

                return new Gk3Main.Graphics.SkyBox(scn.Name + "_skybox", fronts, backs,
                    lefts, rights, ups, downs, scn.SkyboxAzimuth);
            }

            return null;
        }

        private static Graphics.Camera _currentCamera;
        private static Graphics.SkyBox _currentSkybox;
        private static Graphics.BspResource _currentRoom;
        private static Graphics.LightmapResource _currentLightmaps;
        private static ActorPathfinder _currentPathMap;
        private static List<Game.Actor> _actors = new List<Actor>();
        private static List<SceneModel> _models = new List<SceneModel>();
        private static List<Game.GasResource> _modelGases = new List<GasResource>();
        private static Dictionary<string, Nouns> _modelNounMap = new Dictionary<string, Nouns>(StringComparer.OrdinalIgnoreCase);
        private static List<Sound.SoundTrackResource> _stks = new List<Gk3Main.Sound.SoundTrackResource>();
        private static Dictionary<string, SifRoomCamera> _cameras = new Dictionary<string, SifRoomCamera>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, SifPosition> _roomPositions = new Dictionary<string, SifPosition>(StringComparer.OrdinalIgnoreCase);
        private static LinkedList<Sound.SoundTrackResource> _playingSoundTracks = new LinkedList<Sound.SoundTrackResource>();
        private static Resource.ResourceManager _sceneContentManager;
        private static ISceneCustomizer _sceneCustomizer;

        private static ShadeMode _shadeMode = ShadeMode.Textured;
        private static TextureFilterMode _currentFilterMode = TextureFilterMode.Linear;
        private static bool _lightmapsEnabled = false;
        private static bool _doubleLightmapValues = false;
        private static bool _renderHelperIcons = false;
    }
}
