using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gk3Main.Game
{
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

        private string _scene;
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
