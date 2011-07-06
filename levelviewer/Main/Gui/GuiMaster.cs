using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public static class GuiMaster
    {
        private static List<IGuiLayer> _layers = new List<IGuiLayer>();
        private static MainMenu _mainMenu;
        private static OptionsMenu _optionsMenu;

        public static void Render(Graphics.SpriteBatch sb, int tickCount)
        {
            // remove inactive layers
            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                if (_layers[i].IsActive == false)
                    _layers.RemoveAt(i);
                else
                    break;
            }

            // look for the first layer that needs rendering...
            int firstNonPopupLayer = _layers.Count - 1;
            while (firstNonPopupLayer >= 0 && _layers[firstNonPopupLayer].IsPopup)
            {
                firstNonPopupLayer--;
            }

            if (firstNonPopupLayer < 0) firstNonPopupLayer = 0;

            // now start rendering layers
            for (int i = firstNonPopupLayer; i < _layers.Count; i++)
            {
                _layers[i].Render(sb, tickCount);
            }
        }

        public static MainMenu ShowMainMenu(Resource.ResourceManager globalContent)
        {
            _mainMenu = new MainMenu(globalContent);

            _layers.Add(_mainMenu);

            return _mainMenu;
        }

        public static TimeBlockSplash ShowTimeBlockSplash(Resource.ResourceManager globalContent, Game.Timeblock timeblock)
        {
            if (_mainMenu != null)
            {
                _mainMenu.Dismiss();
                _layers.Remove(_mainMenu);
            }

            TimeBlockSplash splash = new TimeBlockSplash(globalContent, timeblock, Game.GameManager.TickCount);

            _layers.Add(splash);

            return splash;
        }

        public static MsgBox ShowMessageBox(Resource.ResourceManager globalContent, string text, MsgBoxType type)
        {
            MsgBox box = new MsgBox(globalContent, text, type);

            _layers.Add(box);

            return box;
        }

        public static void ToggleOptionsMenu(Resource.ResourceManager globalContent, int x, int y)
        {
            if (_optionsMenu == null)
                _optionsMenu = new OptionsMenu(globalContent);

            if (_optionsMenu.IsActive)
            {
                _optionsMenu.Dismiss();
                _layers.Remove(_optionsMenu);
            }
            else
            {
               _optionsMenu.Show(x, y, Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport);
                _layers.Add(_optionsMenu);
            }
        }

        public static void OnMouseUp(int button, int mx, int my)
        {
            // only the first layer gets events
            if (_layers.Count > 0)
                _layers[_layers.Count - 1].OnMouseUp(button, mx, my);
        }

        public static void OnMouseDown(int button, int mx, int my)
        {
            // only the first layer gets events
            if (_layers.Count > 0)
                _layers[_layers.Count - 1].OnMouseDown(button, mx, my);
        }

        public static void OnMouseMove(int tickCount, int mx, int my)
        {
            // only the first layer gets events
            if (_layers.Count > 0)
                _layers[_layers.Count - 1].OnMouseMove(tickCount, mx, my);
        }
    }

    public interface IGuiLayer
    {
        void Render(Graphics.SpriteBatch sb, int tickCount);
        void OnMouseUp(int button, int mx, int my);
        void OnMouseDown(int button, int mx, int my);
        void OnMouseMove(int tickCount, int mx, int my);

        bool IsPopup { get; }
        bool IsActive { get; }
    }
}
