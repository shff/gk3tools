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

namespace Gk3Main.Graphics
{
    public class LightmapResource : Resource.Resource
    {
        public LightmapResource(string name, System.IO.Stream stream)
            : base(name, true)
        {
            System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);

            uint header = reader.ReadUInt32();
            uint numMaps = reader.ReadUInt32();

            _maps = new BitmapSurface[numMaps];

            for (int i = 0; i < numMaps; i++)
                _maps[i] = new BitmapSurface(stream, false);

            GenTextureAtlas();
        }

        internal LightmapResource(string name, int numMaps)
            : base(name, true)
        {
            _maps = new BitmapSurface[numMaps];
        }

        internal void GenTextureAtlas()
        {
            _packedMaps = new TextureAtlas(_maps);

            _packedMapsTexture = RendererManager.CurrentRenderer.CreateTexture("blah", _packedMaps.Surface, false);
        }

        public override void Dispose()
        {
            // nothing
        }

        [Obsolete("Use the Maps property instead")]
        public BitmapSurface this[int index]
        {
            get { return _maps[index]; }
        }

        public BitmapSurface[] Maps
        {
            get { return _maps; }
        }

        public TextureResource PackedLightmapTexture
        {
            get { return _packedMapsTexture; }
        }

        public TextureAtlas PackedLightmaps
        {
            get { return _packedMaps; }
        }

        private TextureAtlas _packedMaps;
        private TextureResource _packedMapsTexture;
        private BitmapSurface[] _maps;
    }

    public class LightmapResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name, Resource.ResourceManager content)
        {
            if (name.IndexOf('.') < 0)
                name += ".MUL";

            System.IO.Stream stream = FileSystem.Open(name);

            LightmapResource resource = new LightmapResource(name, stream);

            stream.Close();

            return resource;
        }

        public string[] SupportedExtensions { get { return new string[] { "MUL" }; } }
        public bool EmptyResourceIfNotFound { get { return true; } }
    }
}
