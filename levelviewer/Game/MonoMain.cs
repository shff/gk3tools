using System;
using System.Collections.Generic;
using Tao.Sdl;

class MyConsole : Gk3Main.Console
{
    public override void Write(Gk3Main.ConsoleVerbosity verbosity, string text, params object[] arg)
    {
        if (verbosity >= Verbosity)
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

    const float DefaultScreenWidth = 640.0f;
    const float DefaultScreenHeight = 480.0f;

    private static float _screenWidth = DefaultScreenWidth;
    private static float _screenHeight = DefaultScreenHeight;
    private static Game.TimeBlockSplash _timeBlockSplash;
    private static Gk3Main.Gui.CursorResource _pointCursor;
    private static Gk3Main.Gui.CursorResource _zoom1Cursor;
    private static MainMenu _menu;
    private static int _timeAtLastStateChange;
    private static GameState _state;
    private static bool _isDemo;
    private static Gk3Main.Graphics.SpriteBatch _spriteBatch;

	public static void Main(string[] args)
	{
        Gk3Main.Graphics.RenderWindow renderWindow = init(args);
        if (renderWindow == null) return; // something bad happened

        int dummy1, dummy2;
		Sdl.SDL_GetMouseState(out dummy1, out dummy2);

		while(MainLoop())
		{
            refreshInput();

            update();

            //if (Gk3Main.DebugFlagManager.GetDebugFlag(Gk3Main.DebugFlag.ShowStats))
            //   renderStats();

            
            Gk3Main.Graphics.Camera camera = Gk3Main.SceneManager.CurrentCamera;

            render(camera, Game.Input.MouseX, Game.Input.MouseY);

            renderWindow.Present();
		}

        shutdown();
	}
	
	public static Gk3Main.Graphics.RenderWindow SetupGraphics(int width, int height, int depth, bool fullscreen)
	{
        Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO | Sdl.SDL_INIT_NOPARACHUTE);

        //Gk3Main.Graphics.RenderWindow window = setupOpenGL(width, height, depth, fullscreen);
        Gk3Main.Graphics.RenderWindow window = setupDirect3D9(width, height, depth, fullscreen);
        Gk3Main.Graphics.RendererManager.CurrentRenderer = window.CreateRenderer();
        

        Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport = new Gk3Main.Graphics.Viewport(0, 0, width, height);

        Gk3Main.Graphics.RendererManager.CurrentRenderer.DepthTestEnabled = true;
        Gk3Main.Graphics.RendererManager.CurrentRenderer.AlphaTestEnabled = true;
        Gk3Main.Graphics.RendererManager.CurrentRenderer.AlphaTestFunction = Gk3Main.Graphics.CompareFunction.GreaterOrEqual;
        Gk3Main.Graphics.RendererManager.CurrentRenderer.AlphaTestReference = 0.5f;
        Gk3Main.Graphics.RendererManager.CurrentRenderer.BlendState = Gk3Main.Graphics.BlendState.AlphaBlend;

        Gk3Main.Graphics.RendererManager.CurrentRenderer.CullMode = Gk3Main.Graphics.CullMode.CounterClockwise;

        return window;
    }
	
	public static bool MainLoop()
	{
		Sdl.SDL_Event e;
		while(Sdl.SDL_PollEvent(out e) != 0)
		{
			switch(e.type)
			{
				case Sdl.SDL_QUIT:
					return false;
			}
		}
		
		return true;
	}

    private static Gk3Main.Graphics.RenderWindow init(string[] args)
    {
        Gk3Main.Console.CurrentConsole = new MyConsole();

        //Gk3Main.DebugFlagManager.SetDebugFlag(Gk3Main.DebugFlag.ShowStats, true);

        Gk3Main.FileSystem.AddPathToSearchPath(System.IO.Directory.GetCurrentDirectory());
        Gk3Main.FileSystem.AddPathToSearchPath("Shaders");
        Gk3Main.FileSystem.AddPathToSearchPath("Icons");

        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.ScnResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.SifResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.NvcResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.BspResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.TextureResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.LightmapResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ModelResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ActResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.EffectLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Gui.FontResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Gui.CursorResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundTrackLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.YakLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.MomLoader());
        //Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.AnmLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.GasResourceLoader());

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

        Gk3Main.Sound.SoundManager.Init();

        Gk3Main.SceneManager.LightmapsEnabled = true;
        Gk3Main.SceneManager.CurrentShadeMode = Gk3Main.ShadeMode.Textured;
        Gk3Main.SceneManager.DoubleLightmapValues = true;

        parseArgs(args);
        Gk3Main.Game.GameManager.CurrentTime = Gk3Main.Game.Timeblock.Day2_12PM;

        Gk3Main.Graphics.RenderWindow renderWindow;
        try
        {
            renderWindow = SetupGraphics((int)_screenWidth, (int)_screenHeight, 16, false);
        }
        catch (DllNotFoundException ex)
        {
            Gk3Main.Console.CurrentConsole.ReportError(ex.Message);
            return null;
        }

        _spriteBatch = new Gk3Main.Graphics.SpriteBatch();

        Sdl.SDL_ShowCursor(0);

        _state = GameState.MainMenu;

        Gk3Main.Game.GameManager.Load();
        Gk3Main.Game.HelperIcons.Load();
        Gk3Main.Gui.CursorResource waitCursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_WAIT.CUR");
        _pointCursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_POINT.CUR");
        _zoom1Cursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_ZOOM.CUR");
        Gk3Main.Gui.CursorResource zoom2Cursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_ZOOM_2.CUR");

        //Gk3Main.Graphics.Camera camera = new Gk3Main.Graphics.Camera(1.04719755f, _screenWidth / _screenHeight, 1.0f, 10000.0f);
        // Gk3Main.SceneManager.CurrentCamera = camera;

        _menu = null;
        if (_state == GameState.MainMenu)
        {
            _menu = new MainMenu();
            _menu.OnPlayClicked += new EventHandler(menu_OnPlayClicked);
            _menu.OnQuitClicked += new EventHandler(menu_OnQuitClicked);
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
            if (_timeBlockSplash != null)
            {
                _timeBlockSplash.Render(_spriteBatch);
            }
        }
        else if (_state == GameState.MainMenu)
        {
            if (_menu != null)
            {
                _menu.Render(_spriteBatch, Gk3Main.Game.GameManager.TickCount);
            }
        }

        Game.VerbPickerManager.Render(_spriteBatch, Gk3Main.Game.GameManager.TickCount);
        Game.VerbPickerManager.RenderProperCursor(_spriteBatch, camera, mouseX, mouseY, _pointCursor, _zoom1Cursor);
        _spriteBatch.End();

        Gk3Main.Graphics.RendererManager.CurrentRenderer.EndScene();
    }

    private static void update()
    {
        // TODO: this still needs cleaning up!

        Gk3Main.Graphics.Camera camera = Gk3Main.SceneManager.CurrentCamera;

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
                        Game.VerbPickerManager.MouseDown(0, Game.Input.MouseX, Game.Input.MouseY);
                }
            }
            else if (Game.Input.LeftMouseReleasedFirstTime && camera != null)
                Game.VerbPickerManager.MouseUp(camera, 0, Game.Input.MouseX, Game.Input.MouseY);
        }
       
        if (Game.Input.RelMouseX != 0 || Game.Input.RelMouseY != 0)
            Game.VerbPickerManager.MouseMove(Game.Input.MouseX, Game.Input.MouseY);

        Gk3Main.Game.GameManager.InjectTickCount(Sdl.SDL_GetTicks());


        if (camera != null)
            Gk3Main.Sound.SoundManager.UpdateListener(camera);

        Gk3Main.Game.Animator.Advance(Gk3Main.Game.GameManager.ElapsedTickCount);
        Gk3Main.Game.DialogManager.Step();
        Gk3Main.Sheep.SheepMachine.ResumeIfNoMoreBlockingWaits();


        if (_state == GameState.TimeBlockSplash)
        {
            updateTimeBlockSplash();
        }
        else if (_state == GameState.MainMenu)
        {
            updateMainMenu();
        }

        Game.VerbPickerManager.Process();

        Gk3Main.Game.GameTimer? timer;
        while ((timer = Gk3Main.Game.GameManager.GetNextExpiredGameTimer()).HasValue)
        {
            Gk3Main.Console.CurrentConsole.WriteLine(Gk3Main.ConsoleVerbosity.Extreme,
                "Timer expired- noun: {0} verb: {1}", timer.Value.Noun, timer.Value.Verb);

            Gk3Main.Game.NounVerbCase? nvc = Gk3Main.SceneManager.GetNounVerbCase(timer.Value.Noun, timer.Value.Verb, true);

            if (nvc.HasValue)
            {
                Gk3Main.Console.CurrentConsole.WriteLine(Gk3Main.ConsoleVerbosity.Extreme,
                    "Executing timer NVC: {0}", nvc.Value.Script);

                Gk3Main.Sheep.SheepMachine.RunCommand(nvc.Value.Script);
            }
        }
    }

    private static void updateMainMenu()
    {
        if (Game.Input.LeftMousePressedFirstTime)
        {
            _menu.OnMouseDown(0);
        }
        else if (Game.Input.LeftMouseReleasedFirstTime)
        {
            _menu.OnMouseUp(0);
        }

        if (Game.Input.RelMouseX != 0 || Game.Input.RelMouseY != 0)
            _menu.OnMouseMove(Gk3Main.Game.GameManager.TickCount,
                Game.Input.MouseX, Game.Input.MouseY);
    }

    private static void updateTimeBlockSplash()
    {
        if (_timeAtLastStateChange + 4000 < Gk3Main.Game.GameManager.TickCount)
        {
            _state = GameState.Game;
            _timeAtLastStateChange = Gk3Main.Game.GameManager.TickCount;

            if (_timeBlockSplash != null)
            {
                _timeBlockSplash.Dispose();
                _timeBlockSplash = null;
            }

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
    }

    private static void refreshInput()
    {
        int mx, my;
        byte buttons = Sdl.SDL_GetMouseState(out mx, out my);

        int numkeys;
        byte[] keys = Sdl.SDL_GetKeyState(out numkeys);

        bool lmb = ((buttons & Sdl.SDL_BUTTON_LMASK) != 0);
        bool rmb = ((buttons & Sdl.SDL_BUTTON_RMASK) != 0);

        Game.Input.Refresh(mx, my, lmb, false, rmb, keys);
    }

    static void menu_OnPlayClicked(object sender, EventArgs e)
    {
        if (_timeBlockSplash == null)
        {
            if (_isDemo)
                _timeBlockSplash = new Game.TimeBlockSplash(Gk3Main.Game.Timeblock.Day2_12PM);
            else
                _timeBlockSplash = new Game.TimeBlockSplash(Gk3Main.Game.Timeblock.Day1_10AM);

            _state = GameState.TimeBlockSplash;
            _timeAtLastStateChange = Gk3Main.Game.GameManager.TickCount;
        }
    }

    static void menu_OnQuitClicked(object sender, EventArgs e)
    {
        Sdl.SDL_Event quitEvent = new Sdl.SDL_Event();
        quitEvent.type = Sdl.SDL_QUIT;

        Sdl.SDL_PushEvent(out quitEvent);
    }

    private static Gk3Main.Graphics.RenderWindow setupOpenGL(int width, int height, int depth, bool fullscreen)
    {
        return new Game.OpenGLRenderWindow(width, height, depth, fullscreen);
    }

    private static Gk3Main.Graphics.RenderWindow setupDirect3D9(int width, int height, int depth, bool fullscreen)
    {
        return new Game.Direct3D9RenderWindow(width, height, depth, fullscreen);
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
                float.TryParse(width, out _screenWidth);
            }
            else if (args[i] == "-height")
            {
                string height = args[++i];
                float.TryParse(height, out _screenHeight);
            }
			
			i++;
		}
	}

    private static void renderStats()
    {
        Gk3Main.Gui.Font font = (Gk3Main.Gui.Font)Gk3Main.Resource.ResourceManager.Load("F_CONSOLE_DISPLAY.FON");

        _spriteBatch.Begin();

        font.Print(_spriteBatch, 0, 0, "FPS: " + (1.0f / Gk3Main.Game.GameManager.SecsPerFrame));

        _spriteBatch.End();
    }
}
