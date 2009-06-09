using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class VerbButtonSet : IDisposable
    {
        const int ButtonWidth = 32;
        private List<Button> _buttons = new List<Button>();

        public VerbButtonSet(int screenX, int screenY, List<Game.NounVerbCase> nvcs, bool cancel)
        {
            int buttonOffsetX = screenX;

            foreach (Game.NounVerbCase nvc in nvcs)
            {
                Game.VerbInfo info = Game.GameManager.Verbs[nvc.Verb];

                Button b = new Button(string.Format("{0}.BMP", info.DownButton),
                    string.Format("{0}.BMP", info.HoverButton),
                    string.Format("{0}.BMP", info.UpButton),
                    string.Format("{0}.BMP", info.DisableButton), null,
                    Game.GameManager.Strings.GetVerbTooltip(info.Verb));
                b.X = new Unit(0, buttonOffsetX);
                b.Y = new Unit(0, screenY);
                b.OnClick += delegate { buttonClicked(nvc.Verb, nvc.Script); };

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

        public void Render(int mouseX, int mouseY)
        {
            foreach(Button b in _buttons)
            {
                b.SetMousePosition(mouseX, mouseY);
                b.Render(true);
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

        private void buttonClicked(string verb, string script)
        {
            Sheep.SheepMachine.RunCommand(script);
        }

        private void cancelClicked()
        {
            // TODO
        }
    }
}
