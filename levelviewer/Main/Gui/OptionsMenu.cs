using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Gui
{
    /// <summary>
    /// GUI menu for the options menu that shows up when the user right-clicks in the game
    /// </summary>
    public class OptionsMenu : IButtonContainer, IGuiLayer
    {
        private const float _leftGutter = 5.0f;
        private const float _upperButtonOffsetY = 39.0f;
        private const float _upperButtonWidth = 35.0f;

        private Resource.ResourceManager _content;
        private int _screenX, _screenY;
        private Graphics.TextureResource _upperBackground;
        private Graphics.TextureResource _optionsBackground;
        private Graphics.TextureResource _advancedBackground;
        private Graphics.TextureResource _graphicsBackground;
        private Button[] _upperButtons;
        private Button[] _optionsButtons;
        private Button[] _advancedOptionsButtons;
        private Dropdown _3dDriverDropdown;
        private Dropdown _resolutionDropdown;
        private MsgBox _restartNotice;
        private bool _active;

        enum OptionsMenuState
        {
            Initial,
            Options,
            AdvancedOption,
            GameOptions,
            GraphicsOptions,
            SoundOptions,
            AdvancedGraphicsOptions
        }
        private OptionsMenuState _state;

        public OptionsMenu(Resource.ResourceManager globalContent)
        {
            _upperButtons = new Button[7];
            _optionsButtons = new Button[4];
            _content = globalContent;

            // we need to load all the positioning data and whatnot from a file...
            Resource.TextResource layout = new Gk3Main.Resource.TextResource("RC_LAYOUT.TXT", FileSystem.Open("RC_LAYOUT.TXT"));
            string[] lines = layout.Text.Split('\n');

            Dictionary<string, string> layoutInfo = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in lines)
            {
                if (line.StartsWith(";") == false)
                {
                    int equal = line.IndexOf('=');
                    if (equal > 0)
                    {
                        layoutInfo.Add(line.Substring(0, equal).Trim(), line.Substring(equal + 1).Trim());
                    }
                }
            }

            _upperBackground = globalContent.Load<Graphics.TextureResource>(layoutInfo["backSprite"]);
            _optionsBackground = globalContent.Load<Graphics.TextureResource>(layoutInfo["optBackSprite"]);
            _advancedBackground = globalContent.Load<Graphics.TextureResource>(layoutInfo["advOptBackSprite"]);
            _graphicsBackground = globalContent.Load<Graphics.TextureResource>(layoutInfo["graphicsOptBackSprite"]);

            setupUpperButtons(globalContent, layoutInfo);
            setupOptionsButtons(globalContent, layoutInfo);
            setupAdvancedOptionsButtons(globalContent, layoutInfo);
            setupGraphicsOptionsButtons(globalContent, layoutInfo);
        }

        private void setupUpperButtons(Resource.ResourceManager globalContent, Dictionary<string, string> layoutInfo)
        {            
            // load all the buttons
            _upperButtons[0] = new Button(this, globalContent, layoutInfo["closedSpriteDown"], layoutInfo["closedSpriteHov"], layoutInfo["closedSpriteUp"], layoutInfo["closedSpriteDis"], null);
            _upperButtons[1] = new Button(this, globalContent, "RC_HINTS_DWN", "RC_HINTS_HOV", "RC_HINTS_STD", "RC_HINTS_DIS", null);
            _upperButtons[2] = new Button(this, globalContent, "RC_CAMERAS_DWN", "RC_CAMERAS_HOV", "RC_CAMERAS_STD", "RC_CAMERAS_DIS", null);
            _upperButtons[3] = new Button(this, globalContent, "RC_CINEMATIC_DWN", "RC_CINEMATIC_HOV", "RC_CINEMATIC_STD", "RC_CINEMATIC_DIS", null);
            _upperButtons[4] = new Button(this, globalContent, "RC_HELP_DWN", "RC_HELP_HOV", "RC_HELP_STD", "RC_HELP_DIS", null);
            _upperButtons[5] = new Button(this, globalContent, "RC_OPTIONS_DWN", "RC_OPTIONS_HOV", "RC_OPTIONS_STD", "RC_OPTIONS_DIS", null);
            _upperButtons[6] = new Button(this, globalContent, "RC_EXIT_DWN", "RC_EXIT_HOV", "RC_EXIT_STD", "RC_EXIT_DIS", null);

            // position the buttons
            positionButton(_upperButtons[0], layoutInfo, "closedPos");
            positionButton(_upperButtons[1], layoutInfo, "hintPos");
            positionButton(_upperButtons[2], layoutInfo, "cameraPos");
            positionButton(_upperButtons[3], layoutInfo, "cinePos");
            positionButton(_upperButtons[4], layoutInfo, "helpPos");
            positionButton(_upperButtons[5], layoutInfo, "optionsPos");
            positionButton(_upperButtons[6], layoutInfo, "exitPos");

            // TODO: implement each of these buttons
            _upperButtons[0].Enabled = false;
            _upperButtons[1].Enabled = false;
            _upperButtons[2].Enabled = false;
            _upperButtons[3].Enabled = false;
            _upperButtons[4].Enabled = false;

            _upperButtons[5].OnClick += new EventHandler(onOptionsClicked);
            _upperButtons[6].OnClick += new EventHandler(onCancelClicked);
        }

        private void setupOptionsButtons(Resource.ResourceManager globalContent, Dictionary<string, string> layoutInfo)
        {
            _optionsButtons[0] = new Button(this, globalContent, "RC_SO_SAVE_DWN", "RC_SO_SAVE_HOV", "RC_SO_SAVE_STD", "RC_SO_SAVE_DIS", null);
            _optionsButtons[1] = new Button(this, globalContent, "RC_SO_RESTORE_DWN", "RC_SO_RESTORE_HOV", "RC_SO_RESTORE_STD", "RC_SO_RESTORE_DIS", null);
            _optionsButtons[2] = new Button(this, globalContent, "RC_SO_ADVANCED_DWN", "RC_SO_ADVANCED_HOV", "RC_SO_ADVANCED_STD", "RC_SO_ADVANCED_DIS", null);
            _optionsButtons[3] = new Button(this, globalContent, layoutInfo["optQuitSpriteDown"], layoutInfo["optQuitSpriteHov"], layoutInfo["optQuitSpriteUp"], null, null); 

            positionButton(_optionsButtons[0], layoutInfo, "optSavePos", 0, _upperBackground.Height);
            positionButton(_optionsButtons[1], layoutInfo, "optRestorePos", 0, _upperBackground.Height);
            positionButton(_optionsButtons[2], layoutInfo, "optAdvancedPos", 0, _upperBackground.Height);
            positionButton(_optionsButtons[3], layoutInfo, "optQuitPos", 0, _upperBackground.Height);

            _optionsButtons[0].Enabled = false;
            _optionsButtons[1].Enabled = false;

            _optionsButtons[2].OnClick += new EventHandler(onAdvancedOptionsClicked);
        }

        private void setupAdvancedOptionsButtons(Resource.ResourceManager globalContent, Dictionary<string, string> layoutInfo)
        {
            _advancedOptionsButtons = new Button[3];

            _advancedOptionsButtons[0] = new Button(this, globalContent, layoutInfo["advOptSoundSpriteDown"], layoutInfo["advOptSoundSpriteHov"], layoutInfo["advOptSoundSpriteUp"], null, null);
            _advancedOptionsButtons[1] = new Button(this, globalContent, layoutInfo["advOptGraphicsSpriteDown"], layoutInfo["advOptGraphicsSpriteHov"], layoutInfo["advOptGraphicsSpriteUp"], null, null);
            _advancedOptionsButtons[2] = new Button(this, globalContent, layoutInfo["advOptGameSpriteDown"], layoutInfo["advOptGameSpriteHov"], layoutInfo["advOptGameSpriteUp"], null, null);

            positionButton(_advancedOptionsButtons[0], layoutInfo, "advOptSoundPos", 0, _upperBackground.Height + _optionsBackground.Height);
            positionButton(_advancedOptionsButtons[1], layoutInfo, "advOptGraphicsPos", 0, _upperBackground.Height + _optionsBackground.Height);
            positionButton(_advancedOptionsButtons[2], layoutInfo, "advOptGamePos", 0, _upperBackground.Height + _optionsBackground.Height);

            _advancedOptionsButtons[1].OnClick += new EventHandler(onGraphicsOptionsClicked);
        }

        private void setupGraphicsOptionsButtons(Resource.ResourceManager globalContent, Dictionary<string, string> layoutInfo)
        {
            float width, height;
            tryParse2f(layoutInfo["graphOptDriverBoxSize"], out width, out height);
            _3dDriverDropdown = new Dropdown(this, globalContent, (int)width, layoutInfo["graphOptDriverSpriteDown"], layoutInfo["graphOptDriverSpriteHov"], layoutInfo["graphOptDriverSpriteUp"]);
            _3dDriverDropdown.OnSelectedItemChanged += new EventHandler(onGraphicsDriverChanged);

            float x, y;
            tryParse2f(layoutInfo["graphOptDriverPos"], out x, out y);
            _3dDriverDropdown.X = new Unit(0, (int)x);
            _3dDriverDropdown.Y = new Unit(0, _upperBackground.Height + _optionsBackground.Height + _advancedBackground.Height + (int)y);

            _3dDriverDropdown.Items.Add(new KeyValuePair<string, string>("d3d9", "Direct3D 9"));
            _3dDriverDropdown.Items.Add(new KeyValuePair<string, string>("gl30", "OpenGL 3.0"));

            tryParse2f(layoutInfo["graphOptResolutionBoxSize"], out width, out height);
            _resolutionDropdown = new Dropdown(this, globalContent, (int)width, layoutInfo["graphOptResolutionSpriteDown"], layoutInfo["graphOptDriverSpriteHov"], layoutInfo["graphOptDriverSpriteUp"]);
            _resolutionDropdown.OnSelectedItemChanged += new EventHandler(onGraphicsResChanged);

            tryParse2f(layoutInfo["graphOptResolutionPos"], out x, out y);
            _resolutionDropdown.X = new Unit(0, (int)x);
            _resolutionDropdown.Y = new Unit(0, _upperBackground.Height + _optionsBackground.Height + _advancedBackground.Height + (int)y);

            List<Graphics.DisplayMode> modes = Graphics.RendererManager.CurrentRenderer.ParentWindow.GetSupportedDisplayModes();
            foreach (Graphics.DisplayMode mode in modes)
            {
                string keyStr = mode.Width.ToString() + "," + mode.Height.ToString();
                string modeStr = mode.Width.ToString() + " x " + mode.Height.ToString();
                _resolutionDropdown.Items.Add(new KeyValuePair<string, string>(keyStr, modeStr));
            }
        }

        private void positionButton(Button button, Dictionary<string, string> layoutInfo, string positionKey)
        {
            positionButton(button, layoutInfo, positionKey, 0, 0);
        }

        private void positionButton(Button button, Dictionary<string, string> layoutInfo, string positionKeyName, int offsetX, int offsetY)
        {
            float x, y;
            tryParse2f(layoutInfo[positionKeyName], out x, out y);
            button.X = new Unit(0, (int)(x + offsetX));
            button.Y = new Unit(0, (int)(y + offsetY));
        }

        private bool tryParse2f(string str, out float f1, out float f2)
        {
            int comma = str.IndexOf(',');

            f1 = f2 = 0;

            return float.TryParse(str.Substring(0, comma), out f1) &&
                float.TryParse(str.Substring(comma + 1), out f2);
        }

        public void KeepInsideViewport(Graphics.Viewport viewport)
        {
        }

        public void Show(int screenX, int screenY)
        {
            _active = true;
            _screenX = screenX - _upperBackground.Width / 2;
            _screenY = screenY - _upperBackground.Height / 2;

            foreach (Button btn in _upperButtons)
            {
                btn.CalculateScreenCoordinates();
            }

            foreach (Button btn in _optionsButtons)
            {
                btn.CalculateScreenCoordinates();
            }

            foreach (Button btn in _advancedOptionsButtons)
            {
                btn.CalculateScreenCoordinates();
            }

            _3dDriverDropdown.CalculateScreenCoordinates();
            _resolutionDropdown.CalculateScreenCoordinates();

            _state = OptionsMenuState.Initial;
        }

        public void Dismiss()
        {
            _active = false;
        }

        public bool IsActive
        {
            get { return _active; }
        }

        public void Render(Graphics.SpriteBatch sb, int tickCount)
        {
            if (_active)
            {
                sb.Draw(_upperBackground, new Math.Vector2(_screenX, _screenY));

                foreach (Button b in _upperButtons)
                {
                    b.Render(sb, tickCount);
                }

                if (_state != OptionsMenuState.Initial)
                {
                    // all states other than Initial render the options dropdown
                    sb.Draw(_optionsBackground, new Math.Vector2(_screenX, _screenY + _upperBackground.Height));

                    foreach (Button b in _optionsButtons)
                    {
                        b.Render(sb, tickCount);
                    }

                    if (_state == OptionsMenuState.AdvancedOption ||
                        _state == OptionsMenuState.GraphicsOptions)
                    {
                        sb.Draw(_advancedBackground, new Math.Vector2(_screenX, _screenY + _upperBackground.Height + _optionsBackground.Height));

                        foreach (Button b in _advancedOptionsButtons)
                        {
                            b.Render(sb, tickCount);
                        }

                        if (_state == OptionsMenuState.GraphicsOptions)
                        {
                            sb.Draw(_graphicsBackground, new Gk3Main.Math.Vector2(_screenX, _screenY + _upperBackground.Height + _optionsBackground.Height + _advancedBackground.Height));

                            _resolutionDropdown.Render(sb, tickCount);
                            _3dDriverDropdown.Render(sb, tickCount);
                        }
                    }
                }
            }
        }

        public void OnMouseMove(int tickCount, int mouseX, int mouseY)
        {
            if (_active)
            {
                foreach (Button b in _upperButtons)
                {
                    b.OnMouseMove(tickCount, mouseX, mouseY);
                }

                if (_state != OptionsMenuState.Initial)
                {
                    foreach (Button b in _optionsButtons)
                    {
                        b.OnMouseMove(tickCount, mouseX, mouseY);
                    }

                    if (_state == OptionsMenuState.AdvancedOption ||
                        _state == OptionsMenuState.GraphicsOptions)
                    {
                        foreach (Button b in _advancedOptionsButtons)
                        {
                            b.OnMouseMove(tickCount, mouseX, mouseY);
                        }

                        if (_state == OptionsMenuState.GraphicsOptions)
                        {
                            _3dDriverDropdown.OnMouseMove(tickCount, mouseX, mouseY);
                            _resolutionDropdown.OnMouseMove(tickCount, mouseX, mouseY);
                        }
                    }
                }
            }
        }

        public void OnMouseDown(int button, int mouseX, int mouseY)
        {
            if (_active)
            {
                foreach (Button b in _upperButtons)
                {
                    if (b.IsMouseOverButton(mouseX, mouseY))
                        b.OnMouseDown(0);
                }

                if (_state != OptionsMenuState.Initial)
                {
                    foreach (Button b in _optionsButtons)
                    {
                        b.OnMouseDown(0);
                    }

                    if (_state == OptionsMenuState.AdvancedOption ||
                        _state == OptionsMenuState.GraphicsOptions)
                    {
                        foreach (Button b in _advancedOptionsButtons)
                        {
                            b.OnMouseDown(0);
                        }

                        if (_state == OptionsMenuState.GraphicsOptions)
                        {
                            _3dDriverDropdown.OnMouseDown(0);
                            _resolutionDropdown.OnMouseDown(0);
                        }
                    }
                }
            }
        }

        public void OnMouseUp(int button, int mouseX, int mouseY)
        {
            if (_active)
            {
                foreach (Button b in _upperButtons)
                {
                    b.OnMouseUp(0);
                }

                if (_state != OptionsMenuState.Initial)
                {
                    foreach (Button b in _optionsButtons)
                    {
                        b.OnMouseUp(0);
                    }

                    if (_state == OptionsMenuState.AdvancedOption ||
                        _state == OptionsMenuState.GraphicsOptions)
                    {
                        foreach (Button b in _advancedOptionsButtons)
                        {
                            b.OnMouseUp(0);
                        }

                        if (_state == OptionsMenuState.GraphicsOptions)
                        {
                            _3dDriverDropdown.OnMouseUp(0, mouseX, mouseY);
                            _resolutionDropdown.OnMouseUp(0, mouseX, mouseY);
                        }
                    }
                }
            }
        }

        public bool IsPopup { get { return true; } }

        public Unit X
        {
            get { return new Unit(0, _screenX); }
        }

        public Unit Y
        {
            get { return new Unit(0, _screenY); }
        }

        public int ScreenX { get { return _screenX; } }
        public int ScreenY { get { return _screenY; } }

        private void onOptionsClicked(object sender, EventArgs e)
        {
            if (_state == OptionsMenuState.Initial)
                _state = OptionsMenuState.Options;
            else
                _state = OptionsMenuState.Initial;
        }

        private void onCancelClicked(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void onAdvancedOptionsClicked(object sender, EventArgs e)
        {
            if (_state == OptionsMenuState.Options)
                _state = OptionsMenuState.AdvancedOption;
            else
                _state = OptionsMenuState.Options;
        }

        private void onGraphicsOptionsClicked(object sender, EventArgs e)
        {
            if (_state == OptionsMenuState.AdvancedOption)
                _state = OptionsMenuState.GraphicsOptions;
            else
                _state = OptionsMenuState.AdvancedOption;
        }

        private void onGraphicsDriverChanged(object sender, EventArgs e)
        {
            _restartNotice = GuiMaster.ShowMessageBox(_content, "Changing renderers on-the-fly isn't supported (yet). You'll have to restart the game before settings take effect.", MsgBoxType.OK);
            _restartNotice.OnResult += new EventHandler<MsgBoxResultEventArgs>(onRestartNoticeResult);

            Settings.Renderer = _3dDriverDropdown.Items[_3dDriverDropdown.SelectedIndex].Key;
            Settings.Save();
        }

        private void onGraphicsResChanged(object sender, EventArgs e)
        {
            _restartNotice = GuiMaster.ShowMessageBox(_content, "Changing resolution on-the-fly isn't supported (yet). You'll have to restart the game before settings take effect.", MsgBoxType.OK);
            _restartNotice.OnResult += new EventHandler<MsgBoxResultEventArgs>(onRestartNoticeResult);

            string res = _resolutionDropdown.Items[_resolutionDropdown.SelectedIndex].Key;
            float w, h;
            if (tryParse2f(res, out w, out h))
            {
                Settings.ScreenWidth = (int)w;
                Settings.ScreenHeight = (int)h;
                Settings.Save();
            }
        }

        private void onRestartNoticeResult(object sender, MsgBoxResultEventArgs e)
        {
            _restartNotice.Dismiss();
        }
    }
}
