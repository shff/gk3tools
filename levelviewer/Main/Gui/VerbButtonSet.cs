using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class VerbButtonSet : IButtonContainer
    {
        const int ButtonWidth = 32;
        private List<Button> _buttons = new List<Button>();
        private bool _active = true;
        private Button _tooltipButton = null;
        private int _screenX, _screenY;
        private int _tooltipX, _tooltipY;

        public VerbButtonSet(Resource.ResourceManager content, 
            int screenX, int screenY, List<Game.NounVerbCase> nvcs, bool cancel)
        {
            _screenX = screenX;
            _screenY = screenY;
            int buttonOffsetX = 0;

            foreach (Game.NounVerbCase nvc in nvcs)
            {
                Game.VerbInfo info = Game.GameManager.Verbs[nvc.Verb];

                VerbButton b = new VerbButton(this, content, nvc.Noun, nvc.Verb, nvc.Script,
                    nvc.Approach, nvc.Target,
                    string.Format("{0}.BMP", info.DownButton),
                    string.Format("{0}.BMP", info.HoverButton),
                    string.Format("{0}.BMP", info.UpButton),
                    info.DisableButton != null ? string.Format("{0}.BMP", info.DisableButton) : null,
                    null,
                    info.Verb != Game.Verbs.V_NONE ? Game.GameManager.Strings.GetVerbTooltip(info.Verb) : null);
                b.X = new Unit(0, buttonOffsetX);
                b.OnClick += new EventHandler(buttonClicked);

                _buttons.Add(b);

                buttonOffsetX += ButtonWidth;
            }

            if (cancel)
            {
                Game.VerbInfo info = Game.GameManager.Verbs["t_cancel"];

                Button b = new Button(this, content, string.Format("{0}.BMP", info.DownButton),
                    string.Format("{0}.BMP", info.HoverButton),
                    string.Format("{0}.BMP", info.UpButton),
                    info.DisableButton != null ? string.Format("{0}.BMP", info.DisableButton) : null,
                    null,
                    Game.GameManager.Strings.GetVerbTooltip(info.Verb));
                b.X = new Unit(0, buttonOffsetX);
                b.OnClick += delegate { cancelClicked(); };

                _buttons.Add(b);

                buttonOffsetX += ButtonWidth;
            }
        }

        public void KeepInsideViewport(Graphics.Viewport viewport)
        {
            if (_buttons.Count > 0)
            {
                int totalWidth = _buttons.Count * ButtonWidth;

                int overflowX = (_screenX + totalWidth) - viewport.Width;

                if (overflowX > 0)
                    _screenX -= overflowX;

                foreach (Button button in _buttons)
                    button.CalculateScreenCoordinates();
            }
        }

        public void Dismiss()
        {
            _active = false;
        }

        public void Render(Graphics.SpriteBatch sb, int tickCount)
        {
            foreach(Button b in _buttons)
            {
                b.Render(sb, tickCount);
            }
        }

        public bool IsMouseInside(int mouseX, int mouseY)
        {
            foreach (Button b in _buttons)
            {
                if (b.IsMouseOverButton(mouseX, mouseY))
                    return true;
            }

            return false;
        }

        public void OnMouseMove(int mouseX, int mouseY)
        {
            foreach (Button b in _buttons)
            {
                b.OnMouseMove(Gk3Main.Game.GameManager.TickCount, mouseX, mouseY);
            }
        }

        public void OnMouseDown(int mouseX, int mouseY)
        {
            foreach (Button b in _buttons)
            {
                if (b.IsMouseOverButton(mouseX, mouseY))
                    b.OnMouseDown(0);
            }
        }

        public void OnMouseUp(int mouseX, int mouseY)
        {
            foreach (Button b in _buttons)
            {
                b.OnMouseUp(0);
            }
        }

        public Unit X
        {
            get { return new Unit(0, _screenX); }
        }

        public Unit Y
        {
            get { return new Unit(0, _screenY); }
        }

        public bool Active
        {
            get { return _active; }
        }

        public bool TooltipVisible
        {
            get { return _tooltipButton != null; }
        }

        private void buttonClicked(object sender, EventArgs e)
        {
            VerbButton button = (VerbButton)sender;

            Console.CurrentConsole.WriteLine(ConsoleVerbosity.Extreme, "Clicked verb: {0}", button.Verb);
            
            // TODO: this is temporary. Eventually we have to handle the approach
            // correctly. But for now we'll just teleport the actor to the location.
            if (button.Approach == Gk3Main.Game.NvcApproachType.WalkTo)
            {
                SceneManager.SetEgoToSifPosition(button.ApproachTarget);
            }
            
            Sheep.SheepMachine.RunCommand(button.Script);

            if (button.Verb == Game.Verbs.V_Z_CHAT)
            {
                Game.GameManager.IncrementChatCount(button.Noun);
            }
            else if (Game.VerbsUtils.IsTopicVerb(button.Verb))
            {
                Game.GameManager.IncrementTopicCount(button.Noun, button.Verb);
            }
            
            Game.GameManager.IncrementNounVerbCount(button.Noun, button.Verb);
        }

        private void cancelClicked()
        {
            _active = false;
        }
    }
}
