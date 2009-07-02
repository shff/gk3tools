using System;
using System.Collections.Generic;
using Tao.Sdl;

class MyConsole : Gk3Main.Console
{
    public override void Write(Gk3Main.ConsoleVerbosity verbosity, string text, params object[] arg)
    {
        if (verbosity >= Verbosity)
            Console.WriteLine(text, arg);
    }
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
    private static int _timeAtLastStateChange;
    private static GameState _state;
    private static bool _isDemo;

	public static void Main(string[] args)
	{
        Gk3Main.Console.CurrentConsole = new MyConsole();

		Gk3Main.FileSystem.AddPathToSearchPath(System.IO.Directory.GetCurrentDirectory());
        Gk3Main.FileSystem.AddPathToSearchPath("Shaders");
		
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.ScnResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.SifResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.NvcResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.BspResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.TextureResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.LightmapResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ModelResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.EffectLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Gui.FontResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Gui.CursorResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundTrackLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.YakLoader());
        
		Gk3Main.Sheep.SheepMachine.Initialize();
        Gk3Main.Sound.SoundManager.Init();
		
		Gk3Main.SceneManager.LightmapsEnabled = true;
		Gk3Main.SceneManager.CurrentShadeMode = Gk3Main.ShadeMode.Textured;
        Gk3Main.SceneManager.DoubleLightmapValues = true;

        parseArgs(args);

		SetupGraphics((int)_screenWidth, (int)_screenHeight, 32, false);
        Sdl.SDL_ShowCursor(0);

        _state = GameState.MainMenu;
		

        Gk3Main.Game.GameManager.Load();
        Gk3Main.Gui.CursorResource waitCursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_WAIT.CUR");
        Gk3Main.Gui.CursorResource pointCursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_POINT.CUR");
        Gk3Main.Gui.CursorResource zoom1Cursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_ZOOM.CUR");
        Gk3Main.Gui.CursorResource zoom2Cursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_ZOOM_2.CUR");

        Gk3Main.Graphics.Camera camera = new Gk3Main.Graphics.Camera(1.04719755f, _screenWidth / _screenHeight, 1.0f, 1000.0f);
        Gk3Main.SceneManager.CurrentCamera = camera;

        MainMenu menu = null;
        if (_state == GameState.MainMenu)
        {
            menu = new MainMenu();
            menu.OnPlayClicked += new EventHandler(menu_OnPlayClicked);
            menu.OnQuitClicked += new EventHandler(menu_OnQuitClicked);
        }


		int mx, my, rmx, rmy;
		Sdl.SDL_GetMouseState(out mx, out my);
        byte buttons = 0, oldButtons = 0;



		while(MainLoop())
		{
            int oldmx = mx, oldmy = my;
            oldButtons = buttons;
			buttons = Sdl.SDL_GetMouseState(out mx, out my);
            rmx = mx - oldmx; rmy = my - oldmy;
			
			int numkeys;
			byte[] keys = Sdl.SDL_GetKeyState(out numkeys);

            bool lmb = ((buttons & Sdl.SDL_BUTTON_LMASK) != 0);
            bool rmb = ((buttons & Sdl.SDL_BUTTON_RMASK) != 0);

            Game.Input.Refresh(lmb, false, rmb);


            if (Game.Input.LeftMousePressed)
            {
                if (Game.Input.RightMousePressed)
                {
                    if (Game.VerbPickerManager.VerbButtonsVisible == false)
                    {
                        camera.AddRelativePositionOffset(Gk3Main.Math.Vector3.Right * rmx);
                        camera.AddPositionOffset(0, -rmy, 0);
                    }
                }
                else
                {
                    if (Game.VerbPickerManager.VerbButtonsVisible == false)
                    {
                        if (keys[Sdl.SDLK_LSHIFT] != 0 ||
                            keys[Sdl.SDLK_RSHIFT] != 0)
                        {
                            camera.AdjustYaw(-rmx * 0.01f);
                            camera.AdjustPitch(-rmy * 0.01f);
                        }
                        else
                        {
                            camera.AdjustYaw(-rmx * 0.01f);
                            camera.AddRelativePositionOffset(Gk3Main.Math.Vector3.Forward * -rmy);
                        }
                    }

                    if (Game.Input.LeftMousePressedFirstTime)
                        Game.VerbPickerManager.MouseDown(0, mx, my);
                }

                if (_state == GameState.MainMenu && menu != null && Game.Input.LeftMousePressedFirstTime)
                    menu.OnMouseDown(0);
            }
            else if (_state == GameState.MainMenu && menu != null && Game.Input.LeftMouseReleasedFirstTime)
                menu.OnMouseUp(0);
            else if (Game.Input.LeftMouseReleasedFirstTime)
                Game.VerbPickerManager.MouseUp(camera, 0, mx, my);


            if (rmx != 0 || rmy != 0)
                Game.VerbPickerManager.MouseMove(mx, my);

            Gk3Main.Game.GameManager.InjectTickCount(Sdl.SDL_GetTicks());
			
            Gk3Main.Graphics.RendererManager.CurrentRenderer.Clear();
			Gk3Main.SceneManager.Render(camera);


            if (_state == GameState.TimeBlockSplash)
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
                        Gk3Main.SceneManager.LoadSif("CSE212P.SIF");
                        Gk3Main.Sheep.SheepMachine.RunSheep("CSE_ALL.SHP", "PlaceEgo$");
                    }
                    else
                    {
                        // TODO: do what the game would do
                    }

                    Gk3Main.Sound.SoundManager.StopChannel(Gk3Main.Sound.SoundTrackChannel.Music);
                }

                if (_timeBlockSplash != null)
                {
                    _timeBlockSplash.Render();
                }
            }
            else if (_state == GameState.MainMenu)
            {
                if (menu != null)
                {
                    if (rmx != 0 || rmy != 0)
                        menu.OnMouseMove(Gk3Main.Game.GameManager.TickCount, mx, my);

                    menu.Render(Gk3Main.Game.GameManager.TickCount);
                }
            }


            Game.VerbPickerManager.Render(Gk3Main.Game.GameManager.TickCount);
            Game.VerbPickerManager.RenderProperCursor(camera, mx, my, pointCursor, zoom1Cursor);

            Game.VerbPickerManager.Process();


			Sdl.SDL_GL_SwapBuffers();
		}

        Gk3Main.Sound.SoundManager.Shutdown();
		Gk3Main.Sheep.SheepMachine.Shutdown();
	}
	
	public static void SetupGraphics(int width, int height, int depth, bool fullscreen)
	{
		Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO | Sdl.SDL_INIT_NOPARACHUTE);

        Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 8);
        Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 8);
        Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 8);
        Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, 24);
        Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);

		Sdl.SDL_SetVideoMode(width, height, depth, Sdl.SDL_OPENGL | (fullscreen ? Sdl.SDL_FULLSCREEN : 0));
        Sdl.SDL_WM_SetCaption("FreeGeeKayThree", "FreeGK3");
		
        Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport = new Gk3Main.Graphics.Viewport(0, 0, width, height);

        Gk3Main.Graphics.RendererManager.CurrentRenderer.DepthTestEnabled = true;
        Gk3Main.Graphics.RendererManager.CurrentRenderer.AlphaTestEnabled = true;
        Gk3Main.Graphics.RendererManager.CurrentRenderer.AlphaTestFunction = Gk3Main.Graphics.CompareFunction.Greater;
        Gk3Main.Graphics.RendererManager.CurrentRenderer.AlphaTestReference = 0.9f;

        Gk3Main.Graphics.RendererManager.CurrentRenderer.CullMode = Gk3Main.Graphics.CullMode.CounterClockwise;
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

                Gk3Main.Game.GameManager.CurrentTime = Gk3Main.Game.Timeblock.Day2_12PM;
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
}
