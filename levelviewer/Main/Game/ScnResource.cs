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
                    if (attribute.Key == "BSP")
                        _bsp = attribute.Value;
                }
            }
        }

        public override void Dispose()
        {
            // nothing
        }

        public string BspFile { get { return _bsp; } }

        private string _bsp;
    }

    public class ScnResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            return new ScnResource(name, stream);
        }

        public string[] SupportedExtensions { get { return _supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return false; } }

        private static string[] _supportedExtensions = new string[] { "SCN" };
    }
}
