using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    class VerbButton : Button
    {
        private Game.Nouns _noun;
        private Game.Verbs _verb;
        private Game.NvcApproachType _approach;
        private string _approachTarget;
        private string _script;

        public VerbButton(Resource.ResourceManager content, Game.Nouns noun, Game.Verbs verb, string script,
            Game.NvcApproachType approach, string approachTarget,
            string downImage, string hoverImage, string upImage, string disabledImage,
            string clickedSound, string tooltip)
            : base(content, downImage, hoverImage, upImage, disabledImage, clickedSound)
        {
            _noun = noun;
            _verb = verb;
            _script = script;
            _approach = approach;
            _approachTarget = approachTarget;
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

        public Game.NvcApproachType Approach
        {
            get { return _approach; }
        }

        public string ApproachTarget
        {
            get { return _approachTarget; }
        }
    }
}
