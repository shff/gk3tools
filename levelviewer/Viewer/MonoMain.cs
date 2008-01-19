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
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.BspResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.TextureResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.LightmapResourceLoader());
		Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ModelResourceLoader());
		
		Gk3Main.Sheep.SheepMachine.Initialize();
		
		Gk3Main.SceneManager.LightmapsEnabled = true;
		Gk3Main.SceneManager.CurrentShadeMode = Gk3Main.ShadeMode.Textured;
		
		SetupGraphics(800, 600, 32, false);
		

		parseArgs(args);
		
		Gk3Main.Graphics.Camera camera = new Gk3Main.Graphics.Camera();
		
		int mx, my;
		Sdl.SDL_GetRelativeMouseState(out mx, out my);
		while(MainLoop())
		{
			byte buttons = Sdl.SDL_GetRelativeMouseState(out mx, out my);
			
			int numkeys;
			byte[] keys = Sdl.SDL_GetKeyState(out numkeys);
			
			if ((buttons & Sdl.SDL_BUTTON_LMASK) != 0)
			{
				if ((buttons & Sdl.SDL_BUTTON_RMASK) != 0)
				{
					camera.AddRelativePositionOffset(new Gk3Main.Math.Vector(mx, 0, 0));
					camera.AddPositionOffset(0, -my, 0);
				}
				else
				{
					if (keys[Sdl.SDLK_LSHIFT] != 0 ||
						keys[Sdl.SDLK_RSHIFT] != 0)
					{
						camera.AdjustYaw(-mx * 0.01f);
						camera.AdjustPitch(-my * 0.01f);
					}
					else
					{
						camera.AdjustYaw(-mx * 0.01f);
						camera.AddRelativePositionOffset(new Gk3Main.Math.Vector(0, 0, my));
					}
				}
			}

			
			
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gk3Main.SceneManager.Render(camera);
			
			Sdl.SDL_GL_SwapBuffers();
		}
		
		Gk3Main.Sheep.SheepMachine.Shutdown();
	}
	
	public static void SetupGraphics(int width, int height, int depth, bool fullscreen)
	{
		Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO | Sdl.SDL_INIT_NOPARACHUTE);
		
		Sdl.SDL_SetVideoMode(width, height, depth, Sdl.SDL_OPENGL | (fullscreen ? Sdl.SDL_FULLSCREEN : 0));
		
		#region Perspective view setup
		float ratio = (float)width / height;
		Gl.glViewport(0, 0, width, height);

		Gl.glMatrixMode(Gl.GL_PROJECTION);
		Gl.glLoadIdentity();

		Glu.gluPerspective(60.0f, ratio, 1.0f, 5000.0f);

		Gl.glMatrixMode(Gl.GL_MODELVIEW);
		Glu.gluLookAt(0, 0, 0, 0, 0, 1.0f, 0, 1.0f, 0);
		#endregion

		Gl.glEnable(Gl.GL_DEPTH_TEST);
		Gl.glEnable(Gl.GL_ALPHA_TEST);
		Gl.glAlphaFunc(Gl.GL_LESS, 0.1f);

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
				Gk3Main.SceneManager.LoadScene(args[++i]);
			}
			else if (args[i] == "-sif")
			{
				throw new NotSupportedException();
			}
			else if (args[i] == "-mod")
			{
				throw new NotSupportedException();
			}
			
			i++;
		}
	}
}