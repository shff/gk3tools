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

        public VerbButton(string noun, string verb, string script,
            string downImage, string hoverImage, string upImage, string disabledImage,
            string clickedSound, string tooltip)
            : base(downImage, hoverImage, upImage, disabledImage, clickedSound)
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
