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
            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                if (_layers[i].IsActive == false)
                {
                    _layers.RemoveAt(i);
                    continue;
                }

                _layers[i].Render(sb, tickCount);

                if (_layers[i].IsPopup == false)
                    break;
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
                _optionsMenu.Show(x, y);
                _layers.Add(_optionsMenu);
            }
        }

        public static void OnMouseUp(int button, int mx, int my)
        {
            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                _layers[i].OnMouseUp(button, mx, my);

                if (_layers[i].IsPopup == false)
                    break;
            }
        }

        public static void OnMouseDown(int button, int mx, int my)
        {
            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                _layers[i].OnMouseDown(button, mx, my);

                if (_layers[i].IsPopup == false)
                    break;
            }
        }

        public static void OnMouseMove(int tickCount, int mx, int my)
        {
            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                _layers[i].OnMouseMove(tickCount, mx, my);

                if (_layers[i].IsPopup == false)
                    break;
            }
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
