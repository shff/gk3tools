using System;
using System.Collections.Generic;
using System.Text;

namespace gk3levelviewer.Graphics
{
    class LightmapResource : Resource.Resource
    {
        public LightmapResource(string name, System.IO.Stream stream)
            : base(name)
        {
            System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);

            uint header = reader.ReadUInt32();
            uint numMaps = reader.ReadUInt32();

            _maps = new TextureResource[numMaps];

            for (int i = 0; i < numMaps; i++)
                _maps[i] = new TextureResource(name + "_map_" + i.ToString(), stream);
        }

        public override void Dispose()
        {
            // nothing
        }

        public TextureResource this[int index]
        {
            get { return _maps[index]; }
        }

        private TextureResource[] _maps;
    }

    class LightmapResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            LightmapResource resource = new LightmapResource(name, stream);

            stream.Close();

            return resource;
        }

        public string[] SupportedExtensions { get { return new string[] { "MUL" }; } }
    }
}
