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
            _window = new Direct3D9RenderWindow(pbRenderWindow);
            Gk3Main.Graphics.RendererManager.CurrentRenderer = _window.CreateRenderer();

            _pathEditor = new SearchPathEditor();
            _pathEditor.Hide();

            _consoleForm = new ConsoleForm();
            _consoleForm.Show();

            _resourceViewerForm = new ResourceViewer();
            _resourceViewerForm.Show();

            Gk3Main.Console.CurrentConsole = new FormConsole(_consoleForm);
            Gk3Main.Console.CurrentConsole.AddCommand("run", new Gk3Main.ConsoleCommand(run));

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
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Sound.SoundLoader(), typeof(Gk3Main.Sound.Sound));

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
            }

            _initialDataLoaded = true;
        }

        bool run(string[] args, Gk3Main.Console console)
        {
            if (args.Length != 3)
                return false;

            Gk3Main.Sheep.SheepMachine.RunSheep(args[1], args[2]);

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
                loadInitialData();
                Gk3Main.SceneManager.LoadScene(dialog.SelectedScene);

                // TODO: fix this!
                //IList<string> resources = Gk3Main.Resource.ResourceManager.GetLoadedResourceNames();
                //_resourceViewerForm.UpdateResources(resources);
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
                
                loadInitialData();
                Gk3Main.SceneManager.LoadSif(dialog.SelectedScene);

                //Gk3Main.SceneManager.LoadScene(sif.Scene);

                /*// load the models
                foreach (Gk3Main.Game.SifModel model in sif.Models)
                {
                    if (model.Type == Gk3Main.Game.SifModelType.Prop)
                    {
                        Gk3Main.SceneManager.AddModel(model.Name, !model.Hidden);
                    }
                }*/

                IList<string> resources = Gk3Main.SceneManager.SceneContentManager.GetLoadedResourceNames();
                _resourceViewerForm.UpdateResources(resources);
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
                loadInitialData();
                Gk3Main.SceneManager.AddModel(dialog.SelectedScene, true);

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

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            MessageBox.Show(string.Format("GK3 Level Viewer\nVersion {0}\n\nhttp://gk3tools.sourceforge.net", version), "About GK3 Level Viewer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        private Direct3D9RenderWindow _window;
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
