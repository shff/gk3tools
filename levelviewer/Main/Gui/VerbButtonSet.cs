using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class VerbButtonSet : IDisposable
    {
        const int ButtonWidth = 32;
        private List<Button> _buttons = new List<Button>();
        private bool _active = true;
        private Button _tooltipButton = null;
        private int _tooltipX, _tooltipY;

        public VerbButtonSet(int screenX, int screenY, List<Game.NounVerbCase> nvcs, bool cancel)
        {
            int buttonOffsetX = screenX;

            foreach (Game.NounVerbCase nvc in nvcs)
            {
                Game.VerbInfo info = Game.GameManager.Verbs[nvc.Verb];

                VerbButton b = new VerbButton(nvc.Verb, nvc.Script,
                    string.Format("{0}.BMP", info.DownButton),
                    string.Format("{0}.BMP", info.HoverButton),
                    string.Format("{0}.BMP", info.UpButton),
                    string.Format("{0}.BMP", info.DisableButton), null,
                    Game.GameManager.Strings.GetVerbTooltip(info.Verb));
                b.X = new Unit(0, buttonOffsetX);
                b.Y = new Unit(0, screenY);
                b.OnClick += new EventHandler(buttonClicked);

                _buttons.Add(b);

                buttonOffsetX += ButtonWidth;
            }

            if (cancel)
            {
                Game.VerbInfo info = Game.GameManager.Verbs["t_cancel"];

                Button b = new Button(string.Format("{0}.BMP", info.DownButton),
                    string.Format("{0}.BMP", info.HoverButton),
                    string.Format("{0}.BMP", info.UpButton),
                    string.Format("{0}.BMP", info.DisableButton), null,
                    Game.GameManager.Strings.GetVerbTooltip(info.Verb));
                b.X = new Unit(0, buttonOffsetX);
                b.Y = new Unit(0, screenY);
                b.OnClick += delegate { cancelClicked(); };

                _buttons.Add(b);

                buttonOffsetX += ButtonWidth;
            }
        }

        public void Dispose()
        {
            foreach (Button b in _buttons)
            {
                b.Dispose();
            }

            _buttons = null;
        }

        public void Render(int tickCount)
        {
            foreach(Button b in _buttons)
            {
                b.Render(tickCount);
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
            Sheep.SheepMachine.RunCommand(button.Script);
        }

        private void cancelClicked()
        {
            _active = false;
        }
    }
}
