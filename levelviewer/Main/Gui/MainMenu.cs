using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    public class MainMenu : IGuiLayer
    {
        public MainMenu(Resource.ResourceManager globalContent)
        {
            _isActive = true;
            _theme = globalContent.Load<Gk3Main.Sound.AudioEngine.SoundEffect>("THEME.WAV");
            _background = globalContent.Load<Gk3Main.Graphics.TextureResource>("TITLE.BMP");
            _introButton = new Gk3Main.Gui.Button(null, globalContent, "TITLE_INTRO_D.BMP", "TITLE_INTRO_H.BMP", "TITLE_INTRO_U.BMP", "TITLE_INTRO_X.BMP", "SIDBUTN-1.WAV");
            _playButton = new Gk3Main.Gui.Button(null, globalContent, "TITLE_PLAY_D.BMP", "TITLE_PLAY_H.BMP", "TITLE_PLAY_U.BMP", "TITLE_PLAY_X.BMP", "SIDBUTN-1.WAV");
            _restoreButton = new Gk3Main.Gui.Button(null, globalContent, "TITLE_RESTORE_D.BMP", "TITLE_RESTORE_H.BMP", "TITLE_RESTORE_U.BMP", "TITLE_RESTORE_X.BMP", "SIDBUTN-1.WAV");
            _quitButton = new Gk3Main.Gui.Button(null, globalContent, "TITLE_QUIT_D.BMP", "TITLE_QUIT_H.BMP", "TITLE_QUIT_U.BMP", "TITLE_QUIT_X.BMP", "SIDBUTN-1.WAV");

            _introButton.X = new Gk3Main.Gui.Unit(0.35f, 0);
            _introButton.Y = new Gk3Main.Gui.Unit(1.0f, -50);

            _playButton.X = new Gk3Main.Gui.Unit(0.35f, 100);
            _playButton.Y = new Gk3Main.Gui.Unit(1.0f, -50);

            _restoreButton.X = new Gk3Main.Gui.Unit(0.35f, 200);
            _restoreButton.Y = new Gk3Main.Gui.Unit(1.0f, -50);

            _quitButton.X = new Gk3Main.Gui.Unit(0.35f, 300);
            _quitButton.Y = new Gk3Main.Gui.Unit(1.0f, -50);

            _introButton.Enabled = false;
            _restoreButton.Enabled = false;

            Sound.SoundManager.PlaySound2DToChannel(_theme, Gk3Main.Sound.SoundTrackChannel.Music);
        }

        public void Dismiss()
        {
            _isActive = false;
        }

        public void OnMouseDown(int button, int mx, int my)
        {
            _introButton.OnMouseDown(button);
            _playButton.OnMouseDown(button);
            _restoreButton.OnMouseDown(button);
            _quitButton.OnMouseDown(button);
        }

        public void OnMouseUp(int button, int mx, int my)
        {
            _introButton.OnMouseUp(button);
            _playButton.OnMouseUp(button);
            _restoreButton.OnMouseUp(button);
            _quitButton.OnMouseUp(button);
        }

        public void OnMouseMove(int tickCount, int x, int y)
        {
            _mouseX = x;
            _mouseY = y;

            _introButton.OnMouseMove(tickCount, x, y);
            _playButton.OnMouseMove(tickCount, x, y);
            _restoreButton.OnMouseMove(tickCount, x, y);
            _quitButton.OnMouseMove(tickCount, x, y);
        }

        public void Render(Gk3Main.Graphics.SpriteBatch sb, int tickCount)
        {
            Gk3Main.Graphics.Viewport vp = Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport;

            Gk3Main.Graphics.Rect src;
            src.X = 0;
            src.Y = 0;
            src.Width = 1.0f;
            src.Height = 1.0f;

            // this keeps everything at a 4:3 ratio, even if it isn't IRL
            float screenWidth = (vp.Height * 4) / 3;
            float widescreenOffset = (vp.Width - screenWidth) / 2;

            Gk3Main.Graphics.Rect dest;
            dest.X = widescreenOffset + vp.X;
            dest.Y = vp.Y;
            dest.Width = screenWidth;
            dest.Height = vp.Height;


            sb.Draw(_background, dest, null, 0);

            _introButton.Render(sb, tickCount);
            _playButton.Render(sb, tickCount);
            _restoreButton.Render(sb, tickCount);
            _quitButton.Render(sb, tickCount);
        }

        public bool IsActive { get { return _isActive; } }
        public bool IsPopup { get { return false; } }
        public bool InterceptMouse { get { return true; } }

        public event EventHandler OnPlayClicked
        {
            add { _playButton.OnClick += value; }
            remove { _playButton.OnClick -= value; }
        }

        public event EventHandler OnQuitClicked
        {
            add { _quitButton.OnClick += value; }
            remove { _quitButton.OnClick -= value; }
        }

        private bool _isActive;
        int _mouseX, _mouseY;
        private Gk3Main.Sound.AudioEngine.SoundEffect _theme;
        private Gk3Main.Graphics.TextureResource _background;
        private Gk3Main.Gui.Button _introButton;
        private Gk3Main.Gui.Button _playButton;
        private Gk3Main.Gui.Button _restoreButton;
        private Gk3Main.Gui.Button _quitButton;
    }
}