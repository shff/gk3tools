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

namespace Gk3Main.Resource
{
    public class TextResource : Resource
    {
        public TextResource(string name, System.IO.Stream stream)
            : base(name, true)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            // TODO: Encoding.Default works to load extended ASCII characters on my machine, but it won't always work!
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.Default);
            _text = reader.ReadToEnd();
            reader.Close();
        }

        public override void Dispose()
        {
            // nothing to do
        }

        public string Text { get { return _text; } }

        private string _text;
    }

    public class TextResourceLoader : IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "TXT", "HTM", "HTML" }; }
        }

        public bool EmptyResourceIfNotFound { get { return false; } }

        public Resource Load(string name, ResourceManager content)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            TextResource resource = new TextResource(name, stream);

            stream.Close();

            return resource;
        }
    }
}
