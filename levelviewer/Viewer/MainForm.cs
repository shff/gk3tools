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
        public MainForm()
        {
            InitializeComponent();
            simpleOpenGlControl1.InitializeContexts();

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
            }
            else
            {
                string[] paths = Settings.Default.SearchPath.Split(';');

                foreach(string path in paths)
                {
                    if (path != string.Empty)
                    {
                        if (System.IO.Directory.Exists(path))
                            Gk3Main.FileSystem.AddPathToSearchPath(path);
                        else
                            Gk3Main.FileSystem.AddBarnToSearchPath(path);
                    }
                }
            }

            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.ScnResourceLoader());
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Game.SifResourceLoader());
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.BspResourceLoader());
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.TextureResourceLoader());
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.LightmapResourceLoader());
            Gk3Main.Resource.ResourceManager.AddResourceLoader(new Gk3Main.Graphics.ModelResourceLoader());

            try
            {
                Gk3Main.Sheep.SheepMachine.Initialize();
            }
            catch (Gk3Main.Sheep.SheepException ex)
            {
                Gk3Main.Console.CurrentConsole.ReportError(ex.Message);
            }

            _camera = new Gk3Main.Graphics.Camera();

            Gk3Main.SceneManager.LightmapsEnabled = true;
            Gk3Main.SceneManager.CurrentShadeMode = Gk3Main.ShadeMode.Textured;

            Video.Init(simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
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
            Gk3Main.Game.SifResource sif = (Gk3Main.Game.SifResource)Gk3Main.Resource.ResourceManager.Load("R25.SIF");

            string[] scnFiles = Gk3Main.FileSystem.GetFilesWithExtension("SCN");

            SceneChooser dialog = new SceneChooser();

            foreach (string scene in scnFiles)
                dialog.AddFile(scene);

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                Gk3Main.SceneManager.LoadScene(dialog.SelectedScene);

                IList<string> resources = Gk3Main.Resource.ResourceManager.GetLoadedResourceNames();
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
                Gk3Main.Game.SifResource sif = (Gk3Main.Game.SifResource)Gk3Main.Resource.ResourceManager.Load(dialog.SelectedScene);

                Gk3Main.SceneManager.LoadScene(sif.Scene);

                // load the models
                foreach (Gk3Main.Game.SifModel model in sif.Models)
                {
                    if (model.Type == Gk3Main.Game.SifModelType.Prop && model.Hidden == false)
                    {
                        Gk3Main.SceneManager.AddModel(model.Name + ".MOD");
                    }
                }

                IList<string> resources = Gk3Main.Resource.ResourceManager.GetLoadedResourceNames();
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
                Gk3Main.SceneManager.AddModel(dialog.SelectedScene);

                IList<string> resources = Gk3Main.Resource.ResourceManager.GetLoadedResourceNames();
                _resourceViewerForm.UpdateResources(resources);
            }
        }

        private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Video.SaveScreenshot("screenshot.bmp");
        }

        private void simpleOpenGlControl1_Paint(object sender, PaintEventArgs e)
        {
            Tao.OpenGl.Gl.glClear(Tao.OpenGl.Gl.GL_COLOR_BUFFER_BIT | Tao.OpenGl.Gl.GL_DEPTH_BUFFER_BIT);
            Gk3Main.SceneManager.Render(_camera);

            Tao.OpenGl.Gl.glFlush();
        }
        
        private void simpleOpenGlControl1_Resize(object sender, EventArgs e)
        {
            Video.Init(simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
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
                    _camera.AddRelativePositionOffset(new Gk3Main.Math.Vector(relx, 0, 0));
                    _camera.AddPositionOffset(0, -rely, 0);
                }
                else
                {

                    if (_keys[(int)Keys.ShiftKey])
                    {
                        _camera.AdjustYaw(-relx * 0.01f);
                        _camera.AdjustPitch(-rely * 0.01f);
                    }
                    else
                    {
                        _camera.AdjustYaw(-relx * 0.01f);
                        _camera.AddRelativePositionOffset(new Gk3Main.Math.Vector(0, 0, rely));
                    }
                }

                simpleOpenGlControl1.Draw();
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

            simpleOpenGlControl1.Draw();
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

            simpleOpenGlControl1.Draw();
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

            simpleOpenGlControl1.Draw();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            MessageBox.Show(string.Format("GK3 Level Viewer\nVersion {0}\n\nhttp://gk3tools.sourceforge.net", version), "About GK3 Level Viewer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        private Gk3Main.Graphics.Camera _camera;
        private SearchPathEditor _pathEditor;
        private ConsoleForm _consoleForm;
        private ResourceViewer _resourceViewerForm;
        private bool _leftMouseButton, _rightMouseButton;
        private bool[] _keys = new bool[MaxKeyValue+1];

        private int _oldMouseX, _oldMouseY;

        private const int MaxKeyValue = 163;

        
    }
}