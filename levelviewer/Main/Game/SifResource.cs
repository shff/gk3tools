using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace gk3levelviewer.Game
{
    public class SifResource : Resource.InfoResource
    {
        public SifResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
        }

        public override void Dispose()
        {
            // nothing
        }
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
