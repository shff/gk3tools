using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    class VerbButton : Button
    {
        private string _noun;
        private string _verb;
        private string _script;

        public VerbButton(Resource.ResourceManager content, string noun, string verb, string script,
            string downImage, string hoverImage, string upImage, string disabledImage,
            string clickedSound, string tooltip)
            : base(content, downImage, hoverImage, upImage, disabledImage, clickedSound)
        {
            _noun = noun;
            _verb = verb;
            _script = script;
        }

        public string Noun
        {
            get { return _noun; }
        }

        public string Verb
        {
            get { return _verb; }
        }

        public string Script
        {
            get { return _script; }
        }
    }
}
