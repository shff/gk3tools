using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gk3Main.Game
{
    public struct SifActor
    {
        public string Model;
        public string Noun;
        public string Pos;
        public string Idle;
        public string Talk;
        public bool IsEgo;
    }

    public enum SifModelType
    {
        Scene,
        Prop,
        HitTest
    }

    public struct SifModel
    {
        public string Name;
        public string Noun;
        public SifModelType Type;
        public bool Hidden;
    }

    public struct SifRoomCamera
    {
        public string Name;
        public float PitchDegrees, YawDegrees;
        public float X, Y, Z;
    }

    public struct SifPosition
    {
        public string Name;
        public float X, Y, Z;
        public float HeadingDegrees;
        public string CameraName;
    }

    public class SifResource : Resource.InfoResource
    {
        public SifResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            //_scene = Utils.GetFilenameWithoutExtension(name).ToUpper() + ".SCN";

            foreach (Resource.InfoLine line in GlobalSection.Lines)
            {
                if (line.Value == "" && line.Attributes[0].Key == "scene")
                    _scene = line.Attributes[0].Value.ToUpper() + ".SCN";
                else if (line.Value == "" && line.Attributes[0].Key == "cameraBounds")
                    _cameraBoundsModel = line.Attributes[0].Value.ToUpper() + ".MOD";
            }

            foreach (Resource.InfoSection section in Sections)
            {
                if (section.Name == "GENERAL" &&
                    (section.Condition == "" || Sheep.SheepMachine.RunSnippet(section.Condition) != 0))
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        if (line.Value == null && line.Attributes[0].Key == "scene")
                            _scene = line.Attributes[0].Value.ToUpper() + ".SCN";
                    }
                }
                else if (section.Name == "ACTORS" &&
                    (section.Condition == "" || Sheep.SheepMachine.RunSnippet(section.Condition) != 0))
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        SifActor actor;

                        line.TryGetAttribute("model", out actor.Model);
                        line.TryGetAttribute("noun", out actor.Noun);
                        line.TryGetAttribute("pos", out actor.Pos);
                        line.TryGetAttribute("idle", out actor.Idle);
                        line.TryGetAttribute("talk", out actor.Talk);

                        string dummy;
                        actor.IsEgo = line.TryGetAttribute("ego", out dummy);

                        _actors.Add(actor);
                    }
                }
                else if (section.Name == "MODELS" &&
                    (section.Condition == "" || Sheep.SheepMachine.RunSnippet(section.Condition) != 0))
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        SifModel model;
                        string modelType;

                        line.TryGetAttribute("model", out model.Name);
                        line.TryGetAttribute("noun", out model.Noun);
                        line.TryGetAttribute("type", out modelType);

                        if (modelType != null)
                        {
                            if (modelType.ToUpper() == "SCENE")
                                model.Type = SifModelType.Scene;
                            else if (modelType.ToUpper() == "PROP")
                                model.Type = SifModelType.Prop;
                            else
                                model.Type = SifModelType.HitTest;
                        }
                        else
                        {
                            model.Type = SifModelType.Scene;
                        }

                        string dummy;
                        model.Hidden = line.TryGetAttribute("hidden", out dummy);

                        _models.Add(model);
                    }
                }
                else if (section.Name == "ROOM_CAMERAS")
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        SifRoomCamera camera;
                        camera.Name = line.Value;

                        string angle, pos;
                        line.TryGetAttribute("angle", out angle);
                        line.TryGetAttribute("pos", out pos);

                        TryParse2f(angle, out camera.YawDegrees, out camera.PitchDegrees);
                        TryParse3f(pos, out camera.Z, out camera.Y, out camera.X);

                        camera.YawDegrees += 180.0f;

                        _roomCameras.Add(camera);
                    }
                }
                else if (section.Name == "CINEMATIC_CAMERAS")
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        SifRoomCamera camera;
                        camera.Name = line.Value;

                        string angle, pos;
                        line.TryGetAttribute("angle", out angle);
                        line.TryGetAttribute("pos", out pos);

                        TryParse2f(angle, out camera.YawDegrees, out camera.PitchDegrees);
                        TryParse3f(pos, out camera.Z, out camera.Y, out camera.X);

                        camera.YawDegrees += 180.0f;

                        _cinematicCameras.Add(camera);
                    }
                }
                else if (section.Name == "POSITIONS")
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        SifPosition position;
                        position.Name = line.Value;

                        string pos, heading;
                        line.TryGetAttribute("pos", out pos);
                        line.TryGetAttribute("heading", out heading);
                        line.TryGetAttribute("camera", out position.CameraName);

                        TryParse3f(pos, out position.Z, out position.Y, out position.X);
                        float.TryParse(heading, out position.HeadingDegrees);

                        _positions.Add(position);
                    }
                }
                else if (section.Name.Equals("Ambient", StringComparison.OrdinalIgnoreCase) &&
                    (section.Condition == "" || Sheep.SheepMachine.RunSnippet(section.Condition) != 0))
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        // TODO: get this working
                        _soundTracks.Add(line.Value);
                    }
                }
                else if (section.Name.Equals("Actions", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        _actions.Add(line.Value);
                    }
                }
            }
        }

        public override void Dispose()
        {
            // do nothing
        }

        public string Scene
        {
            get { return _scene; }
        }

        public string CameraBoundsModel
        {
            get { return _cameraBoundsModel; }
        }

        public List<SifActor> Actors
        {
            get { return _actors; }
        }

        public List<SifModel> Models
        {
            get { return _models; }
        }

        public List<SifRoomCamera> RoomCameras
        {
            get { return _roomCameras; }
        }

        public List<SifRoomCamera> CinematicCameras
        {
            get { return _cinematicCameras; }
        }

        public List<SifPosition> Positions
        {
            get { return _positions; }
        }

        public List<string> SoundTracks
        {
            get { return _soundTracks; }
        }

        public List<string> Actions
        {
            get { return _actions; }
        }

        private string _scene;
        private string _cameraBoundsModel;
        private List<SifActor> _actors = new List<SifActor>();
        private List<SifModel> _models = new List<SifModel>();
        private List<SifRoomCamera> _roomCameras = new List<SifRoomCamera>();
        private List<SifRoomCamera> _cinematicCameras = new List<SifRoomCamera>();
        private List<SifPosition> _positions = new List<SifPosition>();
        private List<string> _actions = new List<string>();
        private List<string> _soundTracks = new List<string>();
    }

    public class SifResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            return new SifResource(name, stream);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[] { "SIF" };
    }
}
