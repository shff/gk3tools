using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    public class ScnResource : Resource.InfoResource
    {
        public ScnResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            _bsp = Utils.GetFilenameWithoutExtension(name);

            foreach (Resource.InfoLine line in GlobalSection.Lines)
            {
                foreach (KeyValuePair<string, string> attribute in line.Attributes)
                {
                    if (attribute.Key.Equals("BSP", StringComparison.InvariantCultureIgnoreCase))
                        _bsp = attribute.Value;
                }
            }

            foreach (Resource.InfoSection section in Sections)
            {
                // load the skybox
                if (section.Name.Equals("SKYBOX", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (Resource.InfoLine line in section.Lines)
                    {
                        foreach (KeyValuePair<string, string> attribute in line.Attributes)
                        {
                            if (attribute.Key.Equals("FRONT", StringComparison.InvariantCultureIgnoreCase))
                                _skyboxFront = attribute.Value;
                            else if (attribute.Key.Equals("BACK", StringComparison.InvariantCultureIgnoreCase))
                                _skyboxBack = attribute.Value;
                            else if (attribute.Key.Equals("LEFT", StringComparison.InvariantCultureIgnoreCase))
                                _skyboxLeft = attribute.Value;
                            else if (attribute.Key.Equals("RIGHT", StringComparison.InvariantCultureIgnoreCase))
                                _skyboxRight = attribute.Value;
                            else if (attribute.Key.Equals("UP", StringComparison.InvariantCultureIgnoreCase))
                                _skyboxUp = attribute.Value;
                            else if (attribute.Key.Equals("DOWN", StringComparison.InvariantCultureIgnoreCase))
                                _skyboxDown = attribute.Value;
                            else if (attribute.Key.Equals("AZIMUTH", StringComparison.InvariantCultureIgnoreCase))
                                _skyboxAzimuth = float.Parse(attribute.Value);
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            // nothing
        }

        public string BspFile { get { return _bsp; } }

        public string SkyboxFront { get { return _skyboxFront; } }
        public string SkyboxBack { get { return _skyboxBack; } }
        public string SkyboxLeft { get { return _skyboxLeft; } }
        public string SkyboxRight { get { return _skyboxRight; } }
        public string SkyboxUp { get { return _skyboxUp; } }
        public string SkyboxDown { get { return _skyboxDown; } }
        public float SkyboxAzimuth { get { return _skyboxAzimuth; } }

        private string _bsp;

        private string _skyboxFront;
        private string _skyboxBack;
        private string _skyboxLeft;
        private string _skyboxRight;
        private string _skyboxUp;
        private string _skyboxDown;
        private float _skyboxAzimuth;
    }

    public class ScnResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            if (name.IndexOf('.') < 0)
                name += ".SCN";

            System.IO.Stream stream = FileSystem.Open(name);

            return new ScnResource(name, stream);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[] { "SCN" };
    }
}
