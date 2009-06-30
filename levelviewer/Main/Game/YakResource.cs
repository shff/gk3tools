using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Game
{
    class YakResource : Resource.TextResource
    {
        private List<Sound.Sound> _sounds = new List<Gk3Main.Sound.Sound>();

        public YakResource(string name, System.IO.Stream stream)
            : base(name, stream)
        {
            string[] lines = Text.Split('\n');

            bool inSoundSection = false;
            bool expectingLineCount = false;
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("[SOUNDS]", StringComparison.OrdinalIgnoreCase))
                {
                    inSoundSection = true;
                    expectingLineCount = true;
                }
                else if (line.StartsWith("[GK3]", StringComparison.OrdinalIgnoreCase))
                {
                    inSoundSection = false;
                    expectingLineCount = true;
                }
                else
                {
                    if (expectingLineCount)
                    {
                        expectingLineCount = false;
                        continue;
                    }
                    else if (inSoundSection)
                    {
                        // the format seems to be:
                        // FRAME, SOUND FILE, #

                        // TODO: load everything, not just the sound file!
                        int firstCommaPos = line.IndexOf(",");
                        int secondCommaPos = line.IndexOf(",", firstCommaPos + 1);

                        if (firstCommaPos >= 0 && secondCommaPos > firstCommaPos)
                        {
                            string soundName = line.Substring(firstCommaPos + 1, secondCommaPos - firstCommaPos - 1);

                            _sounds.Add(new Sound.Sound(soundName, FileSystem.Open(soundName)));
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            foreach (Sound.Sound sound in _sounds)
            {
                sound.Dispose();
            }

            _sounds = null;

            base.Dispose();
        }

        public void Play()
        {
            if (_sounds.Count > 0)
                _sounds[0].Play2D();
        }
    }

    public class YakLoader : Resource.IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "YAK" }; }
        }

        public bool EmptyResourceIfNotFound { get { return false; } }

        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            YakResource resource = new YakResource(name, stream);

            stream.Close();

            return resource;
        }
    }
}
