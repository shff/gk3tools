using System;
using System.Collections.Generic;
using System.Text;

class MainMenu
{
    public MainMenu()
    {
        _background = (Gk3Main.Graphics.TextureResource)Gk3Main.Resource.ResourceManager.Load("TITLE.BMP");
        _introButton = new Gk3Main.Gui.Button("TITLE_INTRO_D.BMP", "TITLE_INTRO_H.BMP", "TITLE_INTRO_U.BMP", "TITLE_INTRO_X.BMP");
        _playButton = new Gk3Main.Gui.Button("TITLE_PLAY_D.BMP", "TITLE_PLAY_H.BMP", "TITLE_PLAY_U.BMP", "TITLE_PLAY_X.BMP");
        _restoreButton = new Gk3Main.Gui.Button("TITLE_RESTORE_D.BMP", "TITLE_RESTORE_H.BMP", "TITLE_RESTORE_U.BMP", "TITLE_RESTORE_X.BMP");
        _quitButton = new Gk3Main.Gui.Button("TITLE_QUIT_D.BMP", "TITLE_QUIT_H.BMP", "TITLE_QUIT_U.BMP", "TITLE_QUIT_X.BMP");

        _introButton.X = new Gk3Main.Gui.Unit(0.35f, 0);
        _introButton.Y = new Gk3Main.Gui.Unit(1.0f, -50);

        _playButton.X = new Gk3Main.Gui.Unit(0.35f, 100);
        _playButton.Y = new Gk3Main.Gui.Unit(1.0f, -50);

        _restoreButton.X = new Gk3Main.Gui.Unit(0.35f, 200);
        _restoreButton.Y = new Gk3Main.Gui.Unit(1.0f, -50);

        _quitButton.X = new Gk3Main.Gui.Unit(0.35f, 300);
        _quitButton.Y = new Gk3Main.Gui.Unit(1.0f, -50);
    }

    public void SetMouseCoords(int x, int y)
    {
        _mouseX = x;
        _mouseY = y;

        _introButton.SetMousePosition(x, y);
        _playButton.SetMousePosition(x, y);
        _restoreButton.SetMousePosition(x, y);
        _quitButton.SetMousePosition(x, y);
    }

    public void OnMouseDown(int button)
    {
        _introButton.OnMouseDown(button);
        _playButton.OnMouseDown(button);
        _restoreButton.OnMouseDown(button);
        _quitButton.OnMouseDown(button);
    }

    public void OnMouseUp(int button)
    {
        _introButton.OnMouseUp(button);
        _playButton.OnMouseUp(button);
        _restoreButton.OnMouseUp(button);
        _quitButton.OnMouseUp(button);
    }

    public void Render()
    {
        Gk3Main.Graphics.Rect dest;
        dest.X = 0;
        dest.Y = 0;
        dest.Width = 1.0f;
        dest.Height = 1.0f;

        Gk3Main.Graphics.Rect src;
        src.X = 0;
        src.Y = 0;
        src.Width = 1.0f;
        src.Height = 1.0f;

        Gk3Main.Graphics.Utils.ScaleBlit(dest, _background, src);

        _introButton.Render();
        _playButton.Render();
        _restoreButton.Render();
        _quitButton.Render();
    }

    public event EventHandler OnQuitClicked
    {
        add { _quitButton.OnClick += value; }
        remove { _quitButton.OnClick -= value; }
    }

    int _mouseX, _mouseY;
    private Gk3Main.Graphics.TextureResource _background;
    private Gk3Main.Gui.Button _introButton;
    private Gk3Main.Gui.Button _playButton;
    private Gk3Main.Gui.Button _restoreButton;
    private Gk3Main.Gui.Button _quitButton;
}
