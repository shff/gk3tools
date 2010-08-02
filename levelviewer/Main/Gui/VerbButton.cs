using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    class VerbButton : Button
    {
        private Game.Nouns _noun;
        private Game.Verbs _verb;
        private string _script;

        public VerbButton(Resource.ResourceManager content, Game.Nouns noun, Game.Verbs verb, string script,
            string downImage, string hoverImage, string upImage, string disabledImage,
            string clickedSound, string tooltip)
            : base(content, downImage, hoverImage, upImage, disabledImage, clickedSound)
        {
            _noun = noun;
            _verb = verb;
            _script = script;
        }

        public Game.Nouns Noun
        {
            get { return _noun; }
        }

        public Game.Verbs Verb
        {
            get { return _verb; }
        }

        public string Script
        {
            get { return _script; }
        }
    }
}
