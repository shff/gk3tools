using System;
using System.Collections.Generic;
using System.Text;

namespace gk3levelviewer.Resource
{
    class TextResource : Resource
    {
        public TextResource(string name, System.IO.Stream stream)
            : base(name)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            System.IO.StreamReader reader = new System.IO.StreamReader(stream);
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

    class TextResourceLoader : IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "TXT", "HTM", "HTML", "SCN"}; }
        }

        public Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            TextResource resource = new TextResource(name, stream);

            stream.Close();

            return resource;
        }
    }
}
