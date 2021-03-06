using System;
using System.Collections.Generic;
//using Tao.Sdl;

class MyConsole : Gk3Main.Console
{
    public override void Write(Gk3Main.ConsoleSeverity severity, string text, params object[] arg)
    {
        if (severity >= MinSeverity)
        {
            Console.WriteLine(text, arg);
            System.Diagnostics.Trace.WriteLine(string.Format(text, arg));
        }
    }

    public override void ReportError(string error)
    {
        try
        {
            if (_messageBoxAvailable)
                MessageBox(IntPtr.Zero, error, "Error", MB_ICONERROR);
        }
        catch (DllNotFoundException)
        {
            _messageBoxAvailable = false;
        }
        base.ReportError(error);
    }

    public Gk3Main.ConsoleSeverity MinSeverity
    {
        get; set;
    }


    private const int MB_OK = 0x0;
    private const int MB_ICONERROR = 0x10;

    [System.Runtime.InteropServices.DllImport("user32")]
    private static extern int MessageBox(IntPtr hwnd, string text, string caption, uint type);

    private bool _messageBoxAvailable = true;
}


class MonoMain
{
    enum GameState
    {
        MainMenu,
        TimeBlockSplash,
        Game
    }

    const string DefaultRenderer = "d3d9";

    private static Gk3Main.Graphics.RenderWindow _window;
    private static Gk3Main.Gui.CursorResource _pointCursor;
    private static Gk3Main.Gui.CursorResource _zoom1Cursor;
    private static int _timeAtLastStateChange;
    private static bool _leftDownWhileRightDown;
    private static bool _rightDownWhileLeftDown;
    private static GameState _state;
    private static bool _isDemo;
    private static Gk3Main.Graphics.SpriteBatch _spriteBatch;
    private static System.Diagnostics.Stopwatch _clock = new System.Diagnostics.Stopwatch();

	public static void Main(string[] args)
	{
        Gk3Main.Settings.Renderer = DefaultRenderer;
        Gk3Main.Settings.Load("settings.txt");

        _window = init(args);
        if (_window == null) return; // something bad happened

       // int dummy1, dummy2;
		//Sdl.SDL_GetMouseState(out dummy1, out dummy2);

        _clock.Start();

		while(MainLoop())
		{
            refreshInput();

            update();

            Gk3Main.Graphics.Camera camera = Gk3Main.SceneManager.CurrentCamera;

            render(camera, Game.Input.MouseX, Game.Input.MouseY);

            _window.Present();
		}

        shutdown();
	}
	
	public static Gk3Main.Graphics.RenderWindow SetupGraphics(int width, int height, int depth, bool fullscreen)
	{
        //Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO | Sdl.SDL_INIT_NOPARACHUTE);

        Gk3Main.Graphics.RenderWindow window;
        try
        {
#if D3D_DISABLED
            Gk3Main.Logger.WriteInfo("Using OpenGL renderer");
            window = setupOpenGL(width, height, depth, fullscreen);
#else
            if (string.IsNullOrEmpty(Gk3Main.Settings.Renderer))
            {
                Gk3Main.Logger.WriteInfo("No renderer specified. Defaulting to Direct3D 9.");
                window = setupDirect3D9(width, height, depth, fullscreen);
            }
            else if (Gk3Main.Settings.Renderer.Equals("gl30", StringComparison.OrdinalIgnoreCase))
                window = setupOpenGL(width, height, depth, fullscreen);
            else if (Gk3Main.Settings.Renderer.Equals("d3d9", StringComparison.OrdinalIgnoreCase))
                window = setupDirect3D9(width, height, depth, fullscreen);
            else
            {
                Gk3Main.Logger.WriteError("Unknown renderer specified! Defaulting to Direct3D 9.");
                window = setupDirect3D9(width, height, depth, fullscreen);
            }
#endif

            Gk3Main.Graphics.RendererManager.CurrentRenderer = window.CreateRenderer();
        }
        catch(Exception e)
        {
            showMessageBox("Error while trying to setup the renderer: " + e.Message, true);
            return null;
        }
        
        Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport = new Gk3Main.Graphics.Viewport(0, 0, width, height);

        Gk3Main.Graphics.RendererManager.CurrentRenderer.DepthTestEnabled = true;
        Gk3Main.Graphics.RendererManager.CurrentRenderer.BlendState = Gk3Main.Graphics.BlendState.AlphaBlend;

        Gk3Main.Graphics.RendererManager.CurrentRenderer.CullMode = Gk3Main.Graphics.CullMode.CounterClockwise;

        return window;
    }
	
	public static bool MainLoop()
	{
        return _window.ProcessEvents();
	}

    private static Gk3Main.Graphics.RenderWindow init(string[] args)
    {
        Gk3Main.Console.CurrentConsole = Game.Console.Instance;

        //Gk3Main.DebugFlagManager.SetDebugFlag(Gk3Main.DebugFlag.ShowStats, true);

        Gk3Main.FileSystem.AddPathToSearchPath(System.IO.Directory.GetCurrentDirectory());
        Gk3Main.FileSystem.AddPathToSearchPath("Shaders");
        Gk3Main.FileSystem.AddPathToSearchPath("Icons");

        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.ScnResourceLoader(), typeof(Gk3Main.Game.ScnResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.SifResourceLoader(), typeof(Gk3Main.Game.SifResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.NvcResourceLoader(), typeof(Gk3Main.Game.NvcResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.BspResourceLoader(), typeof(Gk3Main.Graphics.BspResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.TextureResourceLoader(), typeof(Gk3Main.Graphics.TextureResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.LightmapResourceLoader(), typeof(Gk3Main.Graphics.LightmapResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ModelResourceLoader(), typeof(Gk3Main.Graphics.ModelResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ActResourceLoader(), typeof(Gk3Main.Graphics.ActResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.EffectLoader(), typeof(Gk3Main.Graphics.Effect));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Gui.FontResourceLoader(), typeof(Gk3Main.Gui.FontSpec));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Gui.CursorResourceLoader(), typeof(Gk3Main.Gui.CursorResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundLoader(), typeof(Gk3Main.Sound.AudioEngine.SoundEffect));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundTrackLoader(), typeof(Gk3Main.Sound.SoundTrackResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.YakLoader(), typeof(Gk3Main.Game.YakResource));
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.MomLoader(), typeof(Gk3Main.Game.MomResource));
        //Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.AnmLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.GasResourceLoader(), typeof(Gk3Main.Game.GasResource));

        try
        {
            Gk3Main.Sheep.SheepMachine.Initialize();

            int major, minor, rev;
            Gk3Main.Sheep.SheepMachine.GetVersion(out major, out minor, out rev);
            Gk3Main.Console.CurrentConsole.WriteLine("Using Sheep v{0}.{1}.{2}", major, minor, rev);

            string test = "symbols { int result$; } code { snippet$() { result$ = (result$ == 4); } }";
            Gk3Main.Sheep.SheepMachine.RunScript(test, "snippet$");
        }
        catch (DllNotFoundException)
        {
            Gk3Main.Console.CurrentConsole.ReportError("Unable to find Sheep library");
            return null;
        }
        catch (Exception ex)
        {
            Gk3Main.Console.CurrentConsole.ReportError("Unable to initialize Sheep VM: " + ex.Message);
            return null;
        }

        try
        {
            Gk3Main.Sound.SoundManager.Init();
        }
        catch (DllNotFoundException)
        {
            Gk3Main.Console.CurrentConsole.ReportError("Unable to find OpenAL library");
            return null;
        }
        
        Gk3Main.SceneManager.LightmapsEnabled = true;
        Gk3Main.SceneManager.CurrentShadeMode = Gk3Main.ShadeMode.Textured;
        Gk3Main.SceneManager.DoubleLightmapValues = true;
        Gk3Main.SceneManager.RenderHelperIcons = true;

        parseArgs(args);
        Gk3Main.Game.GameManager.CurrentTime = Gk3Main.Game.Timeblock.Day2_12PM;

        Gk3Main.Graphics.RenderWindow renderWindow;
        try
        {
            renderWindow = SetupGraphics(Gk3Main.Settings.ScreenWidth, Gk3Main.Settings.ScreenHeight,
                16, false);
            if (renderWindow == null)
                return null;
        }
        catch (DllNotFoundException ex)
        {
            Gk3Main.Console.CurrentConsole.ReportError(ex.Message);
            return null;
        }

        Game.Console.Load();
        Game.Console.Wrap = true;
        Game.Console.WrapWidth = Gk3Main.Settings.ScreenWidth;

        Gk3Main.Graphics.BspResource.Init();
        Gk3Main.Graphics.SpriteBatch.Init();
        Gk3Main.Graphics.SkyBox.Init();
        Gk3Main.Graphics.BillboardManager.Init();
        Gk3Main.Graphics.AxisAlignedBoundingBox.Init();
        Gk3Main.Graphics.ModelResource.LoadGlobalContent();

        _spriteBatch = new Gk3Main.Graphics.SpriteBatch();

        // TODO: hide cursor
        //Sdl.SDL_ShowCursor(0);

        _state = GameState.MainMenu;

        try
        {
            Gk3Main.Game.GameManager.Load();
            Gk3Main.Game.HelperIcons.Load();
            Gk3Main.Gui.CursorResource waitCursor = Gk3Main.Resource.ResourceManager.Global.Load<Gk3Main.Gui.CursorResource>("C_WAIT.CUR");
            _pointCursor = Gk3Main.Resource.ResourceManager.Global.Load<Gk3Main.Gui.CursorResource>("C_POINT.CUR");
            _zoom1Cursor = Gk3Main.Resource.ResourceManager.Global.Load<Gk3Main.Gui.CursorResource>("C_ZOOM.CUR");
            Gk3Main.Gui.CursorResource zoom2Cursor = Gk3Main.Resource.ResourceManager.Global.Load<Gk3Main.Gui.CursorResource>("C_ZOOM_2.CUR");
        }
        catch(System.IO.FileNotFoundException ex)
        {
            Gk3Main.Console.CurrentConsole.ReportError("Unable to find a required resource file. " + ex.Message);
            return null;
        }

        //Gk3Main.Graphics.Camera camera = new Gk3Main.Graphics.Camera(1.04719755f, _screenWidth / _screenHeight, 1.0f, 10000.0f);
        // Gk3Main.SceneManager.CurrentCamera = camera;

        Gk3Main.Gui.GuiMaster.AddLayer(new Game.VerbPickerManager());

        if (_state == GameState.MainMenu)
        {
            Gk3Main.Gui.MainMenu menu = Gk3Main.Gui.GuiMaster.ShowMainMenu();
            menu.OnPlayClicked += new EventHandler(menu_OnPlayClicked);
            menu.OnQuitClicked += new EventHandler(menu_OnQuitClicked);
        }

        return renderWindow;
    }

    private static void shutdown()
    {
        Gk3Main.Sound.SoundManager.Shutdown();
        Gk3Main.Sheep.SheepMachine.Shutdown();
        Gk3Main.Logger.Close();
    }

    private static void render(Gk3Main.Graphics.Camera camera, int mouseX, int mouseY)
    {
        Gk3Main.Graphics.RendererManager.CurrentRenderer.Clear();
        Gk3Main.Graphics.RendererManager.CurrentRenderer.BeginScene();

        Gk3Main.SceneManager.Render();

        _spriteBatch.Begin();

        if (_state == GameState.TimeBlockSplash)
        {
        }
        else if (_state == GameState.MainMenu)
        {
        }

        bool mouseIntercepted = Gk3Main.Gui.GuiMaster.Render(_spriteBatch, Gk3Main.Game.GameManager.TickCount);

        if (mouseIntercepted)
            _pointCursor.Render(_spriteBatch, mouseX, mouseY);
        else
            Game.VerbPickerManager.RenderProperCursor(_spriteBatch, camera, mouseX, mouseY, _pointCursor, _zoom1Cursor);

        
        
        _spriteBatch.End();

        //if (Gk3Main.DebugFlagManager.GetDebugFlag(Gk3Main.DebugFlag.ShowStats))
            renderStats();

        Game.Console.Render(_spriteBatch);

        Gk3Main.Graphics.RendererManager.CurrentRenderer.EndScene();
    }

    private static int _lastTickCount = 0;
    private static void update()
    {
        // TODO: this still needs cleaning up!


        if (Game.Input.LeftMousePressedFirstTime)
            onMouseLeftDown(Game.Input.MouseX, Game.Input.MouseY);
        else if (Game.Input.LeftMouseReleasedFirstTime)
            onMouseLeftUp(Game.Input.MouseX, Game.Input.MouseY);
        if (Game.Input.RightMousePressedFirstTime)
            onMouseRightDown(Game.Input.MouseX, Game.Input.MouseY);
        else if (Game.Input.RightMouseReleasedFirstTime)
            onMouseRightUp(Game.Input.MouseX, Game.Input.MouseY);

        if (Game.Input.RelMouseX != 0 || Game.Input.RelMouseY != 0)
            onMouseMove(Game.Input.MouseX, Game.Input.MouseY);

        if (Game.Input.KeyboardButtonPressedFirstTime(Game.Keys.OemTilde))
            Game.Console.Visible = !Game.Console.Visible;

        var pressed = Game.Input.CurrentKeys.GetPressedKeys();
        for (int i = 0; i < pressed.Length; i++)
        {
            if (Game.Input.PreviousKeys.IsKeyUp(pressed[i]))
                onKeyPress(pressed[i]);
        }


        Gk3Main.Graphics.Camera camera = Gk3Main.SceneManager.CurrentCamera;
        /*
        if (camera != null)
        {
            if (Game.Input.LeftMousePressed)
            {
                if (Game.Input.RightMousePressed)
                {
                    if (Game.VerbPickerManager.VerbButtonsVisible == false)
                    {
                        camera.AddRelativePositionOffset(Gk3Main.Math.Vector3.Right * Game.Input.RelMouseX);
                        camera.AddPositionOffset(0, -Game.Input.RelMouseY, 0);
                    }
                }
                else
                {
                    if (Game.VerbPickerManager.VerbButtonsVisible == false)
                    {
                        if (Game.Input.Keys[Sdl.SDLK_LSHIFT] != 0 ||
                            Game.Input.Keys[Sdl.SDLK_RSHIFT] != 0)
                        {
                            camera.AdjustYaw(Game.Input.RelMouseX * 0.01f);
                            camera.AdjustPitch(Game.Input.RelMouseY * 0.01f);
                        }
                        else
                        {
                            camera.AdjustYaw(Game.Input.RelMouseX * 0.01f);
                            camera.AddRelativePositionOffset(Gk3Main.Math.Vector3.Forward * Game.Input.RelMouseY);
                        }
                    }

                    if (Game.Input.LeftMousePressedFirstTime)
                    {
                        Game.VerbPickerManager.MouseDown(0, Game.Input.MouseX, Game.Input.MouseY);
                        if (_optionsMenu != null)
                            _optionsMenu.OnMouseDown(Game.Input.MouseX, Game.Input.MouseY);
                    }
                }
            }
            else if (Game.Input.LeftMouseReleasedFirstTime && camera != null)
            {
                Game.VerbPickerManager.MouseUp(camera, 0, Game.Input.MouseX, Game.Input.MouseY);

                if (_optionsMenu != null)
                    _optionsMenu.OnMouseUp(Game.Input.MouseX, Game.Input.MouseY);
            }
            else if (Game.Input.RightMousePressedFirstTime && camera != null)
            {
                if (_optionsMenu == null)
                    _optionsMenu = new Gk3Main.Gui.OptionsMenu(_globalContent, Game.Input.MouseX, Game.Input.MouseY);
                else
                    _optionsMenu = null;
            }
                
        }*/


        int ticks = (int)_clock.ElapsedMilliseconds;

       // int ticks = Sdl.SDL_GetTicks();
        int elapsed = Math.Min(ticks - _lastTickCount, 1000);
        _lastTickCount = ticks;
        Gk3Main.Game.GameManager.InjectTickCount(elapsed);


        Gk3Main.Sound.SoundManager.Update(camera);

        Gk3Main.Game.Animator.Advance(Gk3Main.Game.GameManager.ElapsedTickCount);
        Gk3Main.Game.DialogManager.Step();
        Gk3Main.Sheep.SheepMachine.ResumeIfNoMoreBlockingWaits();

        Gk3Main.Game.GameTimer? timer;
        while ((timer = Gk3Main.Game.GameManager.GetNextExpiredGameTimer()).HasValue)
        {
            Gk3Main.Console.CurrentConsole.WriteLine(Gk3Main.ConsoleSeverity.Debug,
                "Timer expired- noun: {0} verb: {1}", timer.Value.Noun, timer.Value.Verb);

            List<Gk3Main.Game.NounVerbCase> nvcs = Gk3Main.Game.NvcManager.GetNounVerbCases(timer.Value.Noun, timer.Value.Verb, true);

            foreach(Gk3Main.Game.NounVerbCase nvc in nvcs)
            {
                Gk3Main.Console.CurrentConsole.WriteLine(Gk3Main.ConsoleSeverity.Debug,
                    "Executing timer NVC: {0}", nvc.Script);

                Gk3Main.Sheep.SheepMachine.RunCommand(nvc.Script);
            }
        }
    }

    public static void onTimeBlockSplashFinished(object sender, EventArgs e)
    {
        ((Gk3Main.Gui.TimeBlockSplash)sender).Dismiss();

        _state = GameState.Game;
        _timeAtLastStateChange = Gk3Main.Game.GameManager.TickCount;

        Gk3Main.SceneManager.Initialize();
        if (_isDemo)
        {

            Gk3Main.Game.GameManager.CurrentTime = Gk3Main.Game.Timeblock.Day2_12PM;
            Gk3Main.Game.GameManager.CurrentEgo = Gk3Main.Game.Ego.Grace;
            Gk3Main.Game.GameManager.SetLocation("CSE");
        }
        else
        {
            Gk3Main.Game.GameManager.CurrentTime = Gk3Main.Game.Timeblock.Day1_10AM;
            Gk3Main.Game.GameManager.CurrentEgo = Gk3Main.Game.Ego.Gabriel;
            Gk3Main.Game.GameManager.SetLocation("R25");
        }

        Gk3Main.Sound.SoundManager.StopChannel(Gk3Main.Sound.SoundTrackChannel.Music);
    }

    private static void refreshInput()
    {
        var mouse = _window.GetMouseState();
        var keyboard = OpenTK.Input.Keyboard.GetState();
        Game.Input.Refresh(mouse.X, mouse.Y, mouse.LeftButton, false, mouse.RightButton, keyboard);
    }

    static void menu_OnPlayClicked(object sender, EventArgs e)
    {
        Gk3Main.Gui.TimeBlockSplash splash;

        if (_isDemo)
            splash = Gk3Main.Gui.GuiMaster.ShowTimeBlockSplash(Gk3Main.Resource.ResourceManager.Global, Gk3Main.Game.Timeblock.Day2_12PM);
        else
            splash = Gk3Main.Gui.GuiMaster.ShowTimeBlockSplash(Gk3Main.Resource.ResourceManager.Global, Gk3Main.Game.Timeblock.Day1_10AM);

        _state = GameState.TimeBlockSplash;
        _timeAtLastStateChange = Gk3Main.Game.GameManager.TickCount;
        splash.OnFinished += new EventHandler(onTimeBlockSplashFinished);
    }

    static void menu_OnQuitClicked(object sender, EventArgs e)
    {
        _window.Close();
    }

    private static Gk3Main.Graphics.RenderWindow setupOpenGL(int width, int height, int depth, bool fullscreen)
    {
        return new Game.OpenGLRenderWindow(width, height, depth, fullscreen);
    }

#if !D3D_DISABLED
    private static Gk3Main.Graphics.RenderWindow setupDirect3D9(int width, int height, int depth, bool fullscreen)
    {
        return new Game.Direct3D9RenderWindow(width, height, depth, fullscreen);
    }
#endif

    private static void onMouseLeftDown(int mx, int my)
    {
        if (Game.Input.RightMousePressed)
            _leftDownWhileRightDown = true;

        Gk3Main.Gui.GuiMaster.OnMouseDown(0, mx, my);
    }

    private static void onMouseLeftUp(int mx, int my)
    {
        Gk3Main.Gui.GuiMaster.OnMouseUp(0, mx, my);

        _rightDownWhileLeftDown = false;
    }

    private static void onMouseRightDown(int mx, int my)
    {
        Gk3Main.Gui.GuiMaster.OnMouseDown(1, mx, my);

        if (Game.Input.LeftMousePressed)
        {
            _rightDownWhileLeftDown = true;
            _leftDownWhileRightDown = true;
        }
    }

    private static void onMouseRightUp(int mx, int my)
    {
        Gk3Main.Gui.GuiMaster.OnMouseUp(1, mx, my);

        if (Game.VerbPickerManager.VerbButtonsVisible)
            Game.VerbPickerManager.Dismiss();
        else if (_leftDownWhileRightDown == false && Game.Input.LeftMousePressed == false)
        {
            Gk3Main.Graphics.Camera camera = Gk3Main.SceneManager.CurrentCamera;
            if (camera != null)
            {
                Gk3Main.Gui.GuiMaster.ToggleOptionsMenu(Gk3Main.Resource.ResourceManager.Global, mx, my);
            }
        }

        _leftDownWhileRightDown = false;
    }

    private static void onMouseMove(int mx, int my)
    {
        Gk3Main.Graphics.Camera camera = Gk3Main.SceneManager.CurrentCamera;
        if (camera != null)
        {
            if (Game.Input.LeftMousePressed)
            {
                if (Game.Input.RightMousePressed)
                {
                    camera.AddRelativePositionOffset(Gk3Main.Math.Vector3.Right * Game.Input.RelMouseX);
                    camera.AddPositionOffset(0, -Game.Input.RelMouseY, 0);
                }
                else
                {
                    if (Game.VerbPickerManager.VerbButtonsVisible == false)
                    {
                        if (Game.Input.CurrentKeys.IsKeyDown(Game.Keys.LeftShift) ||
                            Game.Input.CurrentKeys.IsKeyDown(Game.Keys.RightShift))
                        {
                            camera.AdjustYaw(Game.Input.RelMouseX * 0.01f);
                            camera.AdjustPitch(Game.Input.RelMouseY * 0.01f);
                        }
                        else
                        {
                            camera.AdjustYaw(Game.Input.RelMouseX * 0.01f);
                            camera.AddRelativePositionOffset(Gk3Main.Math.Vector3.Forward * -Game.Input.RelMouseY);
                        }
                    }
                }
            }
        }

        Gk3Main.Gui.GuiMaster.OnMouseMove(Gk3Main.Game.GameManager.TickCount, mx, my);
    }

    private static void onKeyPress(Game.Keys key)
    {
        Game.Console.KeyPress(key);
    }

	private static void parseArgs(string[] args)
	{
		int i = 0;
		while (i < args.Length)
		{
			if (args[i] == "-b")
			{
				Gk3Main.FileSystem.AddBarnToSearchPath(args[++i]);
			}
			else if (args[i] == "-scn")
			{
                _state = GameState.Game;
                Gk3Main.SceneManager.Initialize();
				Gk3Main.SceneManager.LoadScene(args[++i]);
			}
			else if (args[i] == "-sif")
			{
                _state = GameState.Game;
                Gk3Main.SceneManager.Initialize();
                Gk3Main.SceneManager.LoadSif(args[++i]);
			}
			else if (args[i] == "-mod")
			{
				throw new NotSupportedException();
			}
            else if (args[i] == "-demo")
            {
                _isDemo = true;
            }
            else if (args[i] == "-width")
            {
                string width = args[++i];
                int screenWidth;
                if (int.TryParse(width, out screenWidth))
                    Gk3Main.Settings.ScreenWidth = screenWidth;
            }
            else if (args[i] == "-height")
            {
                string height = args[++i];
                int screenHeight;
                if (int.TryParse(height, out screenHeight))
                    Gk3Main.Settings.ScreenHeight = screenHeight;
            }
            else if (args[i] == "-renderer")
            {
                string renderer = args[++i];
                Gk3Main.Settings.Renderer = renderer;
            }
			
			i++;
		}
	}

    private static void renderStats()
    {
        var font = Gk3Main.Gui.Font.Load(Gk3Main.Resource.ResourceManager.Global.Load<Gk3Main.Gui.FontSpec>("F_CONSOLE_DISPLAY.FON"));

        _spriteBatch.Begin();

        Gk3Main.Gui.Font.Print(_spriteBatch, font, 0, 0, "FPS: " + (1.0f / Gk3Main.Game.GameManager.SecsPerFrame));

        _spriteBatch.End();
    }

    private static void showMessageBox(string message, bool isError)
    {
        if (isError)
            Gk3Main.Logger.WriteError(message);
        else
            Gk3Main.Logger.WriteInfo(message);

        try
        {
            const uint MB_ICONINFORMATION = 0x40;
            const uint MB_ICONERROR = 0x10;
            const uint MB_OK = 0x00;

            MessageBox(IntPtr.Zero, message, (isError ? "Error!" : "FYI..."), MB_OK | (isError ? MB_ICONERROR : MB_ICONINFORMATION));
        }
        catch (DllNotFoundException)
        {
            // hmm, this isn't windows, apparently...
        }
    }

    [System.Runtime.InteropServices.DllImport("User32")]
    private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

}
