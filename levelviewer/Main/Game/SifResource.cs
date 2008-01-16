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

    public struct SifModel
    {
        public string Name;
        public string Noun;
        public string Type;
        public bool Hidden;
    }

    public class SifResource : Resource.InfoResource
    {
        public SifResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            _scene = Utils.GetFilenameWithoutExtension(name).ToUpper() + ".SCN";

            foreach (Resource.InfoLine line in GlobalSection.Lines)
            {
                if (line.Value == "" && line.Attributes[0].Key == "scene")
                    _scene = line.Attributes[0].Value.ToUpper() + ".SCN";
            }

            foreach (Resource.InfoSection section in Sections)
            {
                if (section.Name == "GENERAL" && section.Condition == "")
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

                        line.TryGetAttribute("model", out model.Name);
                        line.TryGetAttribute("noun", out model.Noun);
                        line.TryGetAttribute("type", out model.Type);

                        string dummy;
                        model.Hidden = line.TryGetAttribute("hidden", out dummy);

                        _models.Add(model);
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

        public List<SifActor> Actors
        {
            get { return _actors; }
        }

        public List<SifModel> Models
        {
            get { return _models; }
        }

        private string _scene;
        private List<SifActor> _actors = new List<SifActor>();
        private List<SifModel> _models = new List<SifModel>();
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
