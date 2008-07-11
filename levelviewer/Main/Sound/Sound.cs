using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Sound
{
#if !SOUND_DISABLED
    public class Sound : Resource.Resource
    {
        public Sound(string name, System.IO.Stream stream)
            : base(name, true)
        {
            _sound = SoundManager.Engine.AddSoundSourceFromFile(name);
        }

        public override void Dispose()
        {
            SoundManager.Engine.RemoveSoundSource(_sound.Name);
            _sound = null;
        }

        public void Play2D()
        {
            SoundManager.Engine.Play2D(_sound, false, false, false);
        }

        private IrrKlang.ISoundSource _sound;
       
    }

    public class SoundLoader : Resource.IResourceLoader
    {
        public string[]  SupportedExtensions
        {
	        get { return new string[] { "WAV" }; }
        }

        public bool EmptyResourceIfNotFound
        {
            get { return false; }
        }

        public Resource.Resource Load(string filename)
        {
            System.IO.Stream stream = FileSystem.Open(filename);

            Sound sound = new Sound(filename, stream);

            stream.Close();

            return sound;
        }

    }
#else
    public class Sound : Resource.Resource
    {
        public Sound(string name, System.IO.Stream stream)
            : base(name, true)
        {
            // nothing
        }

        public override void Dispose()
        {
            // nothing
        }

        public void Play2D()
        {
            // nothing
        }
    }

    public class SoundLoader : Resource.IResourceLoader
    {
        public string[] SupportedExtensions
        {
            get { return new string[] { "WAV" }; }
        }

        public bool EmptyResourceIfNotFound
        {
            get { return false; }
        }

        public Resource.Resource Load(string filename)
        {
            return new Sound(filename, null);
        }

    }
#endif
}
