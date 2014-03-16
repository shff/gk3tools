/*
 * Created by SharpDevelop.
 * User: Brad Farris
 * Date: 9/18/2006
 * Time: 8:22 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GK3BB
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

            // setup localization stuff
            setUILabels();

            // load the icons
            _imageList = new ImageList();
            _imageList.Images.Add("audio", Image.FromFile("icons/audio.png"));
            _imageList.Images.Add("executable", Image.FromFile("icons/executable.png"));
            _imageList.Images.Add("image", Image.FromFile("icons/image.png"));
            _imageList.Images.Add("html", Image.FromFile("icons/html.png"));
            _imageList.Images.Add("text", Image.FromFile("icons/text.png"));
            _imageList.Images.Add("binary", Image.FromFile("icons/binary.png"));
            _imageList.Images.Add("script", Image.FromFile("icons/script.png"));
            _imageList.Images.Add("font", Image.FromFile("icons/font.png"));
            _imageList.Images.Add("cursor", Image.FromFile("icons/cursor.png"));
            mainListView.SmallImageList = _imageList;

            _sorter = new ListViewColumnSorter();

            // build the preview extensions map
            _previewExtensionsMap = new Dictionary<string, string>();
            _previewExtensionsMap.Add("BSP", "BSP");
            _previewExtensionsMap.Add("CUR", "TXT");
            _previewExtensionsMap.Add("FON", "TXT");
            _previewExtensionsMap.Add("TXT", "TXT");
            _previewExtensionsMap.Add("ANM", "TXT");
            _previewExtensionsMap.Add("GAS", "TXT");
            _previewExtensionsMap.Add("MOD", "MOD");
            _previewExtensionsMap.Add("NVC", "TXT");
            _previewExtensionsMap.Add("SCN", "TXT");
            _previewExtensionsMap.Add("SIF", "TXT");
            _previewExtensionsMap.Add("SHP", "TXT");
            _previewExtensionsMap.Add("STK", "TXT");
            _previewExtensionsMap.Add("YAK", "TXT");
            _previewExtensionsMap.Add("BMP", "BMP");
            _previewExtensionsMap.Add("HTML", "HTML");
            _previewExtensionsMap.Add("HTM", "HTML");
            _previewExtensionsMap.Add("WAV", "WAV");

            BarnManager.ExtractPath = Settings.Default.ExtractPath;

            _temporaryFiles = new List<string>();
        }

        #region Event handlers

        void AboutToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			//MessageBox.Show(UiUtils.GetAboutDialogText(), "About GK3BB", MessageBoxButtons.OK, MessageBoxIcon.Information);
            About about = new About();
            about.ShowDialog();
		}
		
		void ExitToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			Application.Exit();
		}
		
		void OpenBarnToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			
			dialog.Filter = "Barn files (*.brn)|*.brn";
			
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    openFile(dialog.FileName);
                }
                catch (BarnLib.BarnException)
                {
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show("Unable to open " + dialog.FileName + "." + Environment.NewLine
                        + "Please make sure it is a valid GK3 .brn file and try again.",
                        "Unable to open barn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
			}
		}

        private void extractSelectedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            foreach (ListViewItem item in mainListView.SelectedItems)
            {
                try
                {
                    BarnFile bf = item.Tag as BarnFile;

                    int index = bf.Index;

                    if (bf.Extension == "BMP" && convertBitmapsToolStripMenuItem.Checked)
                    {
                        byte[] data = BarnManager.ExtractData(bf.Index);
                        GK3Bitmap bmp = new GK3Bitmap(data);

                        bmp.Save(BarnManager.ExtractPath + bf.Name);
                        bmp.Dispose();
                    }
                    else
                    {
                        BarnManager.Extract(index);
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    // FileNotFoundException most likely means the barn the file
                    // is in could not be found.

                    string filename = BarnManager.GetFileName((int)item.Tag);

                    DialogResult result = MessageBox.Show("Unable to extract " + filename + Environment.NewLine
                         + "Continue extracting the rest of the files?", "Error extracting file",
                         MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    if (result == DialogResult.No)
                        break;
                }
            }

            Cursor.Current = Cursors.Default;
        }

        private void setExtractToPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select the directory where files will be extracted";
            if (BarnManager.ExtractPath == string.Empty)
            {
                dialog.SelectedPath = Environment.CurrentDirectory;
            }
            else
            {
                dialog.SelectedPath = BarnManager.ExtractPath;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                BarnManager.ExtractPath = dialog.SelectedPath + System.IO.Path.DirectorySeparatorChar;
                Settings.Default.ExtractPath = BarnManager.ExtractPath;
                Settings.Default.Save();
            }
        }

        private void mnuSetViewerPath_Click(object sender, EventArgs e)
        {
            SetViewerPath viewerPath = new SetViewerPath();

            if (string.IsNullOrEmpty(Settings.Default.PathToViewer))
            {
                viewerPath.ViewerPath = Environment.CurrentDirectory;
            }
            else
            {
                viewerPath.ViewerPath = Settings.Default.PathToViewer;
            }

            if (viewerPath.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.PathToViewer = viewerPath.ViewerPath;
                Settings.Default.Save();
            }
        }
        
        private void previewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in mainListView.SelectedItems)
            {
                try
                {
                    BarnFile bf = item.Tag as BarnFile;

                    if (isPreviewSupported(bf.Extension))
                    {
                        // get the name of a temporary file we can use
                        string filename = System.IO.Path.GetTempPath() +
                            System.IO.Path.DirectorySeparatorChar +
                            bf.Name + "." + getPreviewExtension(bf.Extension);

                        byte[] data = BarnManager.ExtractData(bf.Index);
                        bool success = true;

                        // if it's a bitmap then convert it
                        if (bf.Extension == "BMP")
                        {
                            GK3Bitmap bmp = new GK3Bitmap(data);
                            bmp.Save(filename);
                        }
                        else if (bf.Extension == "SHP")
                        {
                            success = writeSheepPreview(filename, data);
                        }
                        else if (bf.Extension == "MOD" ||
                            bf.Extension == "BSP")
                        {
                            // we're using the viewer, which can look directly inside
                            // this barn, so no need to extract anything
                        }
                        else
                        {
                            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                            fs.Write(data, 0, data.Length);
                            fs.Close();
                        }

                        if (success)
                        {
                            if (bf.Extension == "MOD" ||
                                bf.Extension == "BSP")
                            {
                                // BSP and MOD are special cases. Windows most likely won't have
                                // a file type association with the viewer, so we
                                // have to crank it up manually
                                string viewerPath;
                                if (string.IsNullOrEmpty(Settings.Default.PathToViewer))
                                    viewerPath = "GK3Viewer.exe";
                                else if (Settings.Default.PathToViewer.EndsWith(".EXE", StringComparison.OrdinalIgnoreCase))
                                    viewerPath = Settings.Default.PathToViewer;
                                else
                                    viewerPath = Settings.Default.PathToViewer + System.IO.Path.DirectorySeparatorChar + "GK3Viewer.exe";

                                string args = string.Empty;
                                if (bf.Extension == "MOD")
                                    args = "-b " + _currentBarnName + " -mod " + bf.Name;
                                else if (bf.Extension == "BSP")
                                    args = "-b " + _currentBarnName + " -bsp " + bf.Name;

                                try
                                {
                                    System.Diagnostics.Process.Start(viewerPath, args);
                                }
                                catch
                                {
                                    MessageBox.Show("Unable to start the Viewer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                System.Diagnostics.Process.Start(filename);

                                // add the file to the list of files to delete when the browser closes
                                _temporaryFiles.Add(filename);
                            }

                            
                        }
                    }
                }
                catch(System.IO.FileNotFoundException)
                {
                }
            }
        }

        private void convertBitmapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            convertBitmapsToolStripMenuItem.Checked = !convertBitmapsToolStripMenuItem.Checked;
        }

        private void decompressFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            decompressFilesToolStripMenuItem.Checked = !decompressFilesToolStripMenuItem.Checked;

            BarnManager.Decompress = decompressFilesToolStripMenuItem.Checked;
        }

        private void mainListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // default to not enabled
            previewFileToolStripMenuItem.Enabled = false;

            // if only 1 item is selected and it's a supported type then enable previewing
            if (mainListView.SelectedItems.Count == 1)
            {
                BarnFile bf = mainListView.SelectedItems[0].Tag as BarnFile;

                if (isPreviewSupported(bf.Extension))
                    previewFileToolStripMenuItem.Enabled = true;
            }
            
        }

        private void mainListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            mainListView.ListViewItemSorter = _sorter;

            if (_sorter.SortColumn == e.Column)
            {
                if (_sorter.Order == SortOrder.Ascending)
                    _sorter.Order = SortOrder.Descending;
                else
                    _sorter.Order = SortOrder.Ascending;
            }

            _sorter.SortColumn = e.Column;
            mainListView.Sort();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // delete all the temporary files
            foreach(string file in _temporaryFiles)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch
                {
                    // meh, no big deal, just ignore it
                }
            }
        }

        private void extractFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            extractSelectedFilesToolStripMenuItem_Click(sender, e);
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            previewFileToolStripMenuItem_Click(sender, e);
        }

        private void mainContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // count # selected files
            int numSelected = mainListView.SelectedItems.Count;

            previewToolStripMenuItem.Enabled = false;
            if (mainListView.SelectedItems.Count == 1)
            {
                BarnFile bf = mainListView.SelectedItems[0].Tag as BarnFile;

                if (isPreviewSupported(bf.Extension))
                    previewToolStripMenuItem.Enabled = true;
            }

            extractFilesToolStripMenuItem.Enabled = numSelected > 0;
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                // open the first file
                openFile(files[0]);
            }
        }

        #endregion

        private void openFile(string filename)
        {
            _currentBarnName = filename;

            BarnManager.OpenBarn(filename);

            List<BarnFile> files = BarnManager.GetFiles();

            mainListView.SuspendLayout();
            foreach (BarnFile file in files)
            {
                string iconKey = "";
                if (file.Extension == "WAV")
                    iconKey = "audio";
                else if (file.Extension == "EXE")
                    iconKey = "executable";
                else if (file.Extension == "BMP")
                    iconKey = "image";
                else if (file.Extension == "TXT")
                    iconKey = "text";
                else if (file.Extension == "HTML" || file.Extension == "HTM")
                    iconKey = "html";
                else if (file.Extension == "MUL" || file.Extension == "MOD" ||
                    file.Extension == "BSP" || file.Extension == "ACT")
                    iconKey = "binary";
                else if (file.Extension == "YAK" || file.Extension == "ANM" ||
                    file.Extension == "NVC" || file.Extension == "SIF" ||
                    file.Extension == "STK" || file.Extension == "GAS" ||
                    file.Extension == "SCN")
                    iconKey = "script";
                else if (file.Extension == "FON")
                    iconKey = "font";
                else if (file.Extension == "CUR")
                    iconKey = "cursor";

                string compression;
                if (file.Compression == BarnLib.Compression.None)
                    compression = Strings.CompressionNone;
                else
                    compression = file.Compression.ToString();

                ListViewItem item = new ListViewItem(new string[] {file.Name,
                            file.InternalSize.ToString(), BarnManager.MapExtensionToType(file.Extension),
                            file.Barn, compression}, iconKey);

                item.Tag = file;

                mainListView.Items.Add(item);
            }
            mainListView.ResumeLayout();

            // enable the menu items
            extractSelectedFilesToolStripMenuItem.Enabled = true;
            // TODO: uncomment the following lines once previewing and stuff is implemented
            previewFileToolStripMenuItem.Enabled = true;
            //extractAllBitmapsToolStripMenuItem.Enabled = true;
            //extractAllDocsToolStripMenuItem.Enabled = true;
            //extractAllHtmlFilesToolStripMenuItem.Enabled = true;
            //extractAllWavsToolStripMenuItem.Enabled = true;
        }

        private bool isPreviewSupported(string extension)
        {
            return _previewExtensionsMap.ContainsKey(extension.ToUpper());
        }

        private string getPreviewExtension(string extension)
        {
            return _previewExtensionsMap[extension.ToUpper()];
        }

        private void setUILabels()
        {
            // main menu stuff
            fileToolStripMenuItem.Text = Strings.MainMenuFile;
            toolsToolStripMenuItem.Text = Strings.MainMenuTools;
            helpToolStripMenuItem.Text = Strings.MainMenuHelp;

            // file menu stuff
            openBarnToolStripMenuItem.Text = Strings.MainMenuOpenBarn;
            extractSelectedFilesToolStripMenuItem.Text = Strings.MainMenuExtractSelected;
            setExtractToPathToolStripMenuItem.Text = Strings.MainMenuSetExtractionPath;
            previewFileToolStripMenuItem.Text = Strings.MainMenuPreviewFile;
            convertBitmapsToolStripMenuItem.Text = Strings.MainMenuConvertBitmaps;
            decompressFilesToolStripMenuItem.Text = Strings.MainMenuDecompress;
            exitToolStripMenuItem.Text = Strings.MainMenuExit;

            // tool menu stuff
            extractAllBitmapsToolStripMenuItem.Text = Strings.MainMenuExtractAllBitmaps;
            extractAllWavsToolStripMenuItem.Text = Strings.MainMenuExtractAllWavs;
            extractAllDocsToolStripMenuItem.Text = Strings.MainMenuExtractAllDocs;
            extractAllHtmlFilesToolStripMenuItem.Text = Strings.MainMenuExtractAllHtml;

            // help menu stuff
            aboutToolStripMenuItem.Text = Strings.MainMenuAbout;

            // list columns
            mainListView.Columns[0].Text = Strings.ListFilename;
            mainListView.Columns[1].Text = Strings.ListSize;
            mainListView.Columns[2].Text = Strings.ListType;
            mainListView.Columns[3].Text = Strings.ListBarn;
            mainListView.Columns[4].Text = Strings.ListCompression;

            // list context menu
            extractFilesToolStripMenuItem.Text = Strings.MainMenuExtractSelected;
            previewFileToolStripMenuItem.Text = Strings.MainMenuPreviewFile;
        }

        private bool writeSheepPreview(string filename, byte[] data)
        {
            if (data[0] == 'G')
            {
                // this is a compiled sheep, so disassemble it
                string text = Sheep.GetDisassembly(data);

                if (text != null)
                {
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, false);
                    writer.WriteLine(text);
                    writer.Close();

                    return true;
                }
            }
            else
            {
                // this is just a normal sheep script, so write it out
                System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                fs.Write(data, 0, data.Length);
                fs.Close();

                return true;
            }

            return false;
        }

        private string _currentBarnName;
        private ImageList _imageList;
        private ListViewColumnSorter _sorter;
        private Dictionary<string, string> _previewExtensionsMap;
        private List<string> _temporaryFiles;
    }

    class ListViewColumnSorter : System.Collections.IComparer
    {
        public int Compare(object item1, object item2)
        {
            ListViewItem listItem1 = item1 as ListViewItem;
            ListViewItem listItem2 = item2 as ListViewItem;

            int result;

            if (_columnToSort == 1)
            {
                uint i1 = uint.Parse(listItem1.SubItems[_columnToSort].Text);
                uint i2 = uint.Parse(listItem2.SubItems[_columnToSort].Text);
                result = Comparer<uint>.Default.Compare(i1, i2);
            }
            else
                result = string.Compare(listItem1.SubItems[_columnToSort].Text, listItem2.SubItems[_columnToSort].Text);

            if (_sortOrder == SortOrder.Ascending)
                return result;

            return -result;
        }

        public int SortColumn
        {
            get { return _columnToSort; }
            set { _columnToSort = value; }
        }

        public SortOrder Order
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }

        private SortOrder _sortOrder = SortOrder.Ascending;
        private int _columnToSort;
    }
}
