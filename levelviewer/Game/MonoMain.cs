using System;
using Tao.Sdl;
using Tao.OpenGl;

class MonoMain
{
	public static void Main(string[] args)
	{
		Gk3Main.FileSystem.AddPathToSearchPath(System.IO.Directory.GetCurrentDirectory());
		
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.ScnResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.SifResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.NvcResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.BspResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.TextureResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.LightmapResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ModelResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Gui.FontResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Gui.CursorResourceLoader());
        Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundLoader());
        
		Gk3Main.Sheep.SheepMachine.Initialize();
        Gk3Main.Sound.SoundManager.Init();
		
		Gk3Main.SceneManager.LightmapsEnabled = true;
		Gk3Main.SceneManager.CurrentShadeMode = Gk3Main.ShadeMode.Textured;
        Gk3Main.SceneManager.DoubleLightmapValues = true;
		
		SetupGraphics(640, 480, 32, false);
        Sdl.SDL_ShowCursor(0);
		

		parseArgs(args);

        Gk3Main.Gui.CursorResource waitCursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_WAIT.CUR");
        Gk3Main.Gui.CursorResource pointCursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_POINT.CUR");
        Gk3Main.Gui.CursorResource zoom1Cursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_ZOOM.CUR");
        Gk3Main.Gui.CursorResource zoom2Cursor = (Gk3Main.Gui.CursorResource)Gk3Main.Resource.ResourceManager.Load("C_ZOOM_2.CUR");

        Gk3Main.Graphics.Camera camera = new Gk3Main.Graphics.Camera();

        MainMenu menu = null;
        if (Gk3Main.SceneManager.IsSceneLoaded == false)
        {
            menu = new MainMenu();
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

            if ((buttons & Sdl.SDL_BUTTON_LMASK) != 0)
            {
                if ((buttons & Sdl.SDL_BUTTON_RMASK) != 0)
                {
                    camera.AddRelativePositionOffset(new Gk3Main.Math.Vector(rmx, 0, 0));
                    camera.AddPositionOffset(0, -rmy, 0);
                }
                else
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
                        camera.AddRelativePositionOffset(new Gk3Main.Math.Vector(0, 0, rmy));
                    }
                }

                if (menu != null && (oldButtons & Sdl.SDL_BUTTON_LMASK) == 0)
                    menu.OnMouseDown(0);
            }
            else if (menu != null && (oldButtons & Sdl.SDL_BUTTON_LMASK) != 0)
                menu.OnMouseUp(0);

            Gk3Main.Game.GameManager.InjectTickCount(Sdl.SDL_GetTicks());
			
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gk3Main.SceneManager.Render(camera);

            if (menu != null)
            {
                menu.SetMouseCoords(mx, my);
                menu.Render();
            }

           
            
            //f.Print(0, 16, "h");
           // f.Print(0, 24, " 3");
            //f.Print(0, 32, "!");
            //f.Print(0, 48, "oo");
            renderProperCursor(camera, mx, my, pointCursor, zoom1Cursor);
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
		
		#region Perspective view setup
		float ratio = (float)width / height;
		Gl.glViewport(0, 0, width, height);

		Gl.glMatrixMode(Gl.GL_PROJECTION);
		Gl.glLoadIdentity();

		Glu.gluPerspective(60.0f, ratio, 10.0f, 5000.0f);

		Gl.glMatrixMode(Gl.GL_MODELVIEW);
		Glu.gluLookAt(0, 0, 0, 0, 0, 1.0f, 0, 1.0f, 0);
		#endregion

		Gl.glEnable(Gl.GL_DEPTH_TEST);
		Gl.glEnable(Gl.GL_ALPHA_TEST);
		Gl.glAlphaFunc(Gl.GL_GREATER, 0.9f);

		Gl.glEnable(Gl.GL_CULL_FACE);
		Gl.glFrontFace(Gl.GL_CW);
		Gl.glCullFace(Gl.GL_BACK);
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

    static void menu_OnQuitClicked(object sender, EventArgs e)
    {
        Sdl.SDL_Event quitEvent = new Sdl.SDL_Event();
        quitEvent.type = Sdl.SDL_QUIT;

        Sdl.SDL_PushEvent(out quitEvent);
    }

    private static void renderProperCursor(Gk3Main.Graphics.Camera camera, int mx, int my, Gk3Main.Gui.CursorResource point, Gk3Main.Gui.CursorResource zoom)
    {
        double[] modelMatrix = new double[16];
        double[] projectionMatrix = new double[16];
        int[] viewport = new int[4];

        Gl.glGetDoublev(Gl.GL_MODELVIEW_MATRIX, modelMatrix);
        Gl.glGetDoublev(Gl.GL_PROJECTION_MATRIX, projectionMatrix);
        Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

        double x, y, z;
        Glu.gluUnProject(mx, 480 - my, 0, modelMatrix, projectionMatrix, viewport, out x, out y, out z);

        string model = Gk3Main.SceneManager.GetCollisionModel(camera.Position, new Gk3Main.Math.Vector((float)x, (float)y, (float)z) - camera.Position, 1000.0f);

        if (model == null || Gk3Main.SceneManager.GetNounVerbCaseCountForTarget(model) == 0)
        {
            point.Render(mx, my);
        }
        else
        {
            Console.WriteLine(model);
            zoom.Render(mx, my);
        }
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
                Gk3Main.SceneManager.Initialize();
				Gk3Main.SceneManager.LoadScene(args[++i]);
			}
			else if (args[i] == "-sif")
			{
                Gk3Main.SceneManager.Initialize();
                Gk3Main.SceneManager.LoadSif(args[++i]);
			}
			else if (args[i] == "-mod")
			{
				throw new NotSupportedException();
			}
			
			i++;
		}
	}
}