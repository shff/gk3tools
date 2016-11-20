using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Viewer
{
    public partial class MainForm : Form
    {
        public MainForm(string[] args)
        {
            ProgramArguments arguments = null;
            try
            {
                 arguments = new ProgramArguments(args);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to parse arguments: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            InitializeComponent();

#if !D3D_DISABLED
            if (Settings.Default.Renderer.Equals("Direct3D 9", StringComparison.OrdinalIgnoreCase))
            {
                direct3D9ToolStripMenuItem.Checked = true;
                openGLToolStripMenuItem.Checked = false;
                _window = new Direct3D9RenderWindow(pbRenderWindow);
            }
            else
#endif
            {
                direct3D9ToolStripMenuItem.Checked = false;
                openGLToolStripMenuItem.Checked = true;
                _window = new OpenGLRenderWindow(pbRenderWindow);
            }
            Gk3Main.Graphics.RendererManager.CurrentRenderer = _window.CreateRenderer();

            _pathEditor = new SearchPathEditor();
            _pathEditor.Hide();

            _consoleForm = new ConsoleForm();
            _consoleForm.Show();

            _resourceViewerForm = new ResourceViewer();
            _resourceViewerForm.Show();

            Gk3Main.Console.CurrentConsole = new FormConsole(_consoleForm);
            Gk3Main.Console.CurrentConsole.AddCommand("run", new Gk3Main.ConsoleCommand(run));
            Gk3Main.Console.CurrentConsole.AddCommand("look", new Gk3Main.ConsoleCommand(look));
            Gk3Main.Console.CurrentConsole.AddCommand("viewSurfaceLightmap", new Gk3Main.ConsoleCommand(viewSurfaceLightmap));

            if (Settings.Default.SearchPath == String.Empty)
            {
                Gk3Main.FileSystem.AddPathToSearchPath(System.IO.Directory.GetCurrentDirectory());
                Gk3Main.FileSystem.AddPathToSearchPath("Shaders");
            }
            else
            {
                string[] paths = Settings.Default.SearchPath.Split(';');
                foreach(string path in paths)
                {
                    if (path != string.Empty)
                    {
                        try
                        {
                            if (System.IO.Directory.Exists(path))
                                Gk3Main.FileSystem.AddPathToSearchPath(path);
                            else
                            {
                                Gk3Main.FileSystem.AddBarnToSearchPath(path);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error when attempting to add to the search path: " + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

            // handle any search paths passed in as arguments
            if (arguments != null)
            {
                foreach (string path in arguments.SearchPaths)
                {
                    Gk3Main.FileSystem.AddPathToSearchPath(path, true);
                }

                foreach (string barn in arguments.SearchBarns)
                {
                    Gk3Main.FileSystem.AddBarnToSearchPath(barn, true);
                }
            }

            _globalContent = new Gk3Main.Resource.ResourceManager();

            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.NvcResourceLoader(), typeof(Gk3Main.Game.NvcResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.ScnResourceLoader(), typeof(Gk3Main.Game.ScnResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.SifResourceLoader(), typeof(Gk3Main.Game.SifResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.MomLoader(), typeof(Gk3Main.Game.MomResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.GasResourceLoader(), typeof(Gk3Main.Game.GasResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.BspResourceLoader(), typeof(Gk3Main.Graphics.BspResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.TextureResourceLoader(), typeof(Gk3Main.Graphics.TextureResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.LightmapResourceLoader(), typeof(Gk3Main.Graphics.LightmapResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ModelResourceLoader(), typeof(Gk3Main.Graphics.ModelResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ActResourceLoader(), typeof(Gk3Main.Graphics.ActResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.EffectLoader(), typeof(Gk3Main.Graphics.Effect));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundTrackLoader(), typeof(Gk3Main.Sound.SoundTrackResource));
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundLoader(), typeof(Gk3Main.Sound.AudioEngine.SoundEffect));

            try
            {
                Gk3Main.Sheep.SheepMachine.Initialize();
            }
            catch (Gk3Main.Sheep.SheepException ex)
            {
                Gk3Main.Console.CurrentConsole.ReportError(ex.Message);
            }

            



            bool zNegOne = (Gk3Main.Graphics.RendererManager.CurrentRenderer.ZClipMode == Gk3Main.Graphics.ZClipMode.NegativeOne);
            _camera = new Gk3Main.Graphics.Camera(1.04719755f, (float)pbRenderWindow.Width / pbRenderWindow.Height, 1.0f, 5000.0f, zNegOne);

            Gk3Main.SceneManager.LightmapsEnabled = true;
            Gk3Main.SceneManager.DoubleLightmapValues = true;
            Gk3Main.SceneManager.CurrentShadeMode = Gk3Main.ShadeMode.Textured;
            Gk3Main.Graphics.RendererManager.CurrentRenderer.CullMode = Gk3Main.Graphics.CullMode.CounterClockwise;
            Gk3Main.Graphics.RendererManager.CurrentRenderer.DepthTestEnabled = true;

            // check the arguments to see if we need to load anything
            if (arguments != null)
            {
                foreach (string model in arguments.ModelsToLoad)
                {
                    loadInitialData();
                    Gk3Main.SceneManager.AddModel(model, true);
                }

                if (string.IsNullOrEmpty(arguments.BspToLoad) == false)
                {
                    loadInitialData();
                    Gk3Main.SceneManager.LoadBsp(arguments.BspToLoad);
                }
            }
        }

        private void loadInitialData()
        {
            if (_initialDataLoaded == false)
            {
                // BUG: this code runs the first time the user tries
                // to open a SCN or SIF or MOD, and never runs again.
                // But meanwhile the search paths might change.
                Gk3Main.Graphics.BspResource.Init(_globalContent);
                Gk3Main.Graphics.SpriteBatch.Init(_globalContent);
                Gk3Main.Graphics.SkyBox.Init(_globalContent);
                Gk3Main.Graphics.BillboardManager.Init(_globalContent);
                Gk3Main.Graphics.AxisAlignedBoundingBox.Init(_globalContent);
                Gk3Main.Sound.SoundManager.Init();
                Gk3Main.Graphics.ModelResource.LoadGlobalContent(_globalContent);
                Gk3Main.SceneManager.Initialize(_globalContent);
                Gk3Main.Game.GameManager.Load();
                Gk3Main.Game.HelperIcons.Load(_globalContent);
            }

            _initialDataLoaded = true;
        }

        bool run(string[] args, Gk3Main.Console console)
        {
            if (args.Length != 3)
                return false;

          //  Gk3Main.Sheep.SheepMachine.RunSheep(args[1], args[2]);

            return true;
        }

        bool look(string[] args, Gk3Main.Console console)
        {
            Gk3Main.Math.Vector3 forward = Gk3Main.Math.Vector3.Forward;
            forward = _camera.Orientation * forward;

            console.WriteLine("Looking at {0}", forward);

            return true;
        }

        bool viewSurfaceLightmap(string[] args, Gk3Main.Console console)
        {
            if (args.Length != 2)
                return false;

            int surface;
            if (int.TryParse(args[1], out surface) == false)
                return false;

            TextureViewer viewer = new TextureViewer();
            viewer.DisplaySurfaceLightmap(Gk3Main.SceneManager.CurrentRoom, Gk3Main.SceneManager.CurrentLightmaps, surface);
            viewer.Show();

            return true;
        }

#region Event handlers
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Gk3Main.Sheep.SheepMachine.Shutdown();
            Application.Exit();
        }

        private void editSearchPathsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _pathEditor.Refresh();
            _pathEditor.Show();
        }

        private void openSCNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] scnFiles = Gk3Main.FileSystem.GetFilesWithExtension("SCN");

            SceneChooser dialog = new SceneChooser();

            foreach (string scene in scnFiles)
                dialog.AddFile(scene);

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedScene) == false)
                {
                    loadInitialData();
                    Gk3Main.SceneManager.LoadScene(dialog.SelectedScene);
                }

                IList<string> resources = Gk3Main.SceneManager.SceneContentManager.GetLoadedResourceNames();
                _resourceViewerForm.UpdateResources(resources);
            }
        }

        private void openSifMenuItem_Click(object sender, EventArgs e)
        {
            string[] sifFiles = Gk3Main.FileSystem.GetFilesWithExtension("SIF");

            SceneChooser dialog = new SceneChooser();

            foreach (string sif in sifFiles)
                dialog.AddFile(sif);

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                //Gk3Main.Game.SifResource sif = null; // TODO: (Gk3Main.Game.SifResource)Gk3Main.Resource.ResourceManager.Load(dialog.SelectedScene);

                if (string.IsNullOrEmpty(dialog.SelectedScene) == false)
                {
                    loadInitialData();
                    Gk3Main.SceneManager.LoadSif(dialog.SelectedScene);

                    IList<string> resources = Gk3Main.SceneManager.SceneContentManager.GetLoadedResourceNames();
                    _resourceViewerForm.UpdateResources(resources);
                }

                //Gk3Main.SceneManager.LoadScene(sif.Scene);

                /*// load the models
                foreach (Gk3Main.Game.SifModel model in sif.Models)
                {
                    if (model.Type == Gk3Main.Game.SifModelType.Prop)
                    {
                        Gk3Main.SceneManager.AddModel(model.Name, !model.Hidden);
                    }
                }*/

                
            }
        }

        private void openMODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] modFiles = Gk3Main.FileSystem.GetFilesWithExtension("MOD");

            SceneChooser dialog = new SceneChooser();

            foreach (string model in modFiles)
                dialog.AddFile(model);

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedScene) == false)
                {
                    loadInitialData();
                    Gk3Main.SceneManager.AddModel(dialog.SelectedScene, true);
                }

                // TODO: fix this!
                //IList<string> resources = Gk3Main.Resource.ResourceManager.GetLoadedResourceNames();
                //_resourceViewerForm.UpdateResources(resources);
            }
        }

        private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Video.SaveScreenshot("screenshot.bmp");
        }

        private void simpleOpenGlControl1_Paint(object sender, PaintEventArgs e)
        {
                _window.Renderer.Clear();

                if (_initialDataLoaded)
                {
                    _window.Renderer.BeginScene();
                    if (_renderHemicube)
                    {
                        Gk3Main.Math.Vector3 forward = Gk3Main.Math.Vector3.Forward;
                        Gk3Main.Math.Vector3 up = _camera.Orientation * Gk3Main.Math.Vector3.Up;
                        forward = _camera.Orientation * forward;

                        Gk3Main.Game.Radiosity.RenderHemicube(_camera.Position, forward, up);
                    }
                    else
                        Gk3Main.SceneManager.Render(_camera);

                    

                    _window.Renderer.EndScene();
                }

                _window.Present();
        }
        
        private void pbRenderWindow_Resize(object sender, EventArgs e)
        {
            _window.Resize(pbRenderWindow.Width, pbRenderWindow.Height);
            _camera.Projection = Gk3Main.Math.Matrix.Perspective(1.04719755f,
                (float)pbRenderWindow.Width / pbRenderWindow.Height, 1.0f, 5000.0f);

            Gk3Main.Graphics.RendererManager.CurrentRenderer.Viewport = new Gk3Main.Graphics.Viewport(0, 0, pbRenderWindow.Width, pbRenderWindow.Height);
        }

        private void simpleOpenGlControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _leftMouseButton = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _rightMouseButton = true;
            }
        }

        private void simpleOpenGlControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _leftMouseButton = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _rightMouseButton = false;
            }
        }

        private void simpleOpenGlControl1_MouseMove(object sender, MouseEventArgs e)
        {
            int relx = e.X - _oldMouseX;
            int rely = e.Y - _oldMouseY;

            if (_leftMouseButton)
            {
                if (_rightMouseButton)
                {
                    _camera.AddRelativePositionOffset(new Gk3Main.Math.Vector3(relx, 0, 0));
                    _camera.AddPositionOffset(0, -rely, 0);
                }
                else
                {

                    if (_keys[(int)Keys.ShiftKey])
                    {
                        _camera.AdjustYaw(relx * 0.01f);
                        _camera.AdjustPitch(rely * 0.01f);
                    }
                    else
                    {
                        _camera.AdjustYaw(relx * 0.01f);
                        _camera.AddRelativePositionOffset(new Gk3Main.Math.Vector3(0, 0, -rely));
                    }
                }

                pbRenderWindow.Refresh();
            }
            else
            {
                if (Gk3Main.SceneManager.CurrentRoom != null)
                {
                    Gk3Main.Graphics.BspSurface surface;
                    Gk3Main.Math.Vector3 unprojected = _camera.Unproject(new Gk3Main.Math.Vector3(e.X, e.Y, 0));
                    Gk3Main.SceneManager.CurrentRoom.CollideRayWithSurfaces(_camera.Position, (unprojected - _camera.Position).Normalize(), 10000.0f, out surface);

                    if (surface != null)
                    {
                        lblStatusSurface.Visible = true;
                        lblSurfaceIndexValue.Visible = true;
                        lblSurfaceIndexValue.Text = surface.index.ToString();
                    }
                    else
                    {
                        lblStatusSurface.Visible = false;
                        lblSurfaceIndexValue.Visible = false;
                    }
                }
            }
            
            _oldMouseX = e.X;
            _oldMouseY = e.Y;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;

            if (msg.Msg == WM_KEYDOWN)
            {
                if (keyData == Keys.Up || keyData == Keys.Down
                    || keyData == Keys.Left || keyData == Keys.Right)
                {
                    _keys[(int)keyData] = true;

                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void simpleOpenGlControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((int)e.KeyCode > MaxKeyValue)
            {
                return;
            }

            _keys[(int)e.KeyCode] = true;
        }

        private void openGLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.Renderer = "OpenGL";
            Settings.Default.Save();

            MessageBox.Show("Please restart the Viewer to apply your changes.", "Restart required", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void direct3D9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.Renderer = "Direct3D 9";
            Settings.Default.Save();

            MessageBox.Show("Please restart the Viewer to apply your changes.", "Restart required", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void simpleOpenGlControl1_KeyUp(object sender, KeyEventArgs e)
        {
            if ((int)e.KeyCode > MaxKeyValue)
                return;

            _keys[(int)e.KeyCode] = false;
        }
        
        private void lightmappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lightmappingToolStripMenuItem.Checked)
            {
                lightmappingToolStripMenuItem.Checked = false;
                Gk3Main.SceneManager.LightmapsEnabled = false;
            }
            else
            {
                lightmappingToolStripMenuItem.Checked = true;
                Gk3Main.SceneManager.LightmapsEnabled = true;
            }

            pbRenderWindow.Refresh();
        }

        private void texturingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (texturingToolStripMenuItem.Checked)
            {
                texturingToolStripMenuItem.Checked = false;
                Gk3Main.SceneManager.CurrentShadeMode = Gk3Main.ShadeMode.Flat;
            }
            else
            {
                texturingToolStripMenuItem.Checked = true;
                Gk3Main.SceneManager.CurrentShadeMode = Gk3Main.ShadeMode.Textured;
            }

            pbRenderWindow.Refresh();
        }

        private void xLightmapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (xLightmapsToolStripMenuItem.Checked)
            {
                xLightmapsToolStripMenuItem.Checked = false;
                Gk3Main.SceneManager.DoubleLightmapValues = false;
            }
            else
            {
                xLightmapsToolStripMenuItem.Checked = true;
                Gk3Main.SceneManager.DoubleLightmapValues = true;
            }

            pbRenderWindow.Refresh();
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noneToolStripMenuItem.Checked = true;
            linearToolStripMenuItem.Checked = false;
            anisotropic4XToolStripMenuItem.Checked = false;

            Gk3Main.SceneManager.CurrentFilterMode = Gk3Main.TextureFilterMode.None;
            pbRenderWindow.Refresh();
        }

        private void linearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noneToolStripMenuItem.Checked = false;
            linearToolStripMenuItem.Checked = true;
            anisotropic4XToolStripMenuItem.Checked = false;

            Gk3Main.SceneManager.CurrentFilterMode = Gk3Main.TextureFilterMode.Linear;
            pbRenderWindow.Refresh();
        }

        private void anisotropic4XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noneToolStripMenuItem.Checked = false;
            linearToolStripMenuItem.Checked = false;
            anisotropic4XToolStripMenuItem.Checked = true;

            Gk3Main.SceneManager.CurrentFilterMode = Gk3Main.TextureFilterMode.Anisotropic4X;
            pbRenderWindow.Refresh();
        }

        private void showHelpersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showHelpersToolStripMenuItem.Checked)
            {
                showHelpersToolStripMenuItem.Checked = false;
                Gk3Main.SceneManager.RenderHelperIcons = false;
            }
            else
            {
                showHelpersToolStripMenuItem.Checked = true;
                Gk3Main.SceneManager.RenderHelperIcons = true;
            }
        }

        private bool _renderHemicube;
        private void calculateLightmapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult r = MessageBox.Show("Are you sure you want to generate lightmaps for this scene? It might take a while...", "Really?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r == System.Windows.Forms.DialogResult.Yes)
                {
                    string lightingInfo = Gk3Main.SceneManager.CurrentRoom.NameWithoutExtension + "_lighting.xml";
                    Gk3Main.Game.LightmapSpecs specs = null;

                    if (System.IO.File.Exists(lightingInfo) == false)
                    {
                        LightingXml lighting = new LightingXml();

                        foreach (Gk3Main.Graphics.BspSurface surface in Gk3Main.SceneManager.CurrentRoom.Surfaces)
                        {
                            lighting.Surfaces.Add(new SurfaceXml((int)surface.index,
                                System.Math.Max(Gk3Main.SceneManager.CurrentLightmaps.Maps[surface.index].Width, 4),
                                System.Math.Max(Gk3Main.SceneManager.CurrentLightmaps.Maps[surface.index].Height, 4)));
                        }

                        lighting.Skylight = new SkylightXml(new ColorXml(100.0f, 100.0f, 100.0f));

                        lighting.Write(lightingInfo);

                        specs = LightingXml.GenerateSpecs(lighting);
                    }
                    else
                    {
                        LightingXml lighting = LightingXml.Load(lightingInfo);
                        specs = LightingXml.GenerateSpecs(lighting);

                        // hide all hidden surfaces
                        foreach (SurfaceXml surface in lighting.Surfaces)
                        {
                            if (surface.Visible == false)
                                Gk3Main.SceneManager.CurrentRoom.SetSurfaceVisibility(surface.Index, false);
                        }
                    }

                    

                    Gk3Main.SceneManager.CalculateLightmaps(specs);

                    MessageBox.Show("All done!");

                   // Gk3Main.Game.Radiosity.Init(Gk3Main.SceneManager.renderRadiosityCallback);
                   // _renderHemicube = true;
                }
            }
            catch(DllNotFoundException)
            {
                MessageBox.Show("Unable to find the radiosity calculator library.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            MessageBox.Show(string.Format("GK3 Level Viewer\nVersion {0}\n\nhttp://gk3tools.sourceforge.net", version), "About GK3 Level Viewer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

#endregion

        private Gk3Main.Graphics.RenderWindow _window;
        private Gk3Main.Resource.ResourceManager _globalContent;
        private Gk3Main.Graphics.Camera _camera;
        private SearchPathEditor _pathEditor;
        private ConsoleForm _consoleForm;
        private ResourceViewer _resourceViewerForm;
        private bool _initialDataLoaded;
        private bool _leftMouseButton, _rightMouseButton;
        private bool[] _keys = new bool[MaxKeyValue+1];

        private int _oldMouseX, _oldMouseY;

        private const int MaxKeyValue = 163;
    }
}
