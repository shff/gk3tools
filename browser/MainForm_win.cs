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
            mainListView.SmallImageList = _imageList;

            _sorter = new ListViewColumnSorter();

            // build the preview extensions map
            _previewExtensionsMap = new Dictionary<string, string>();
            _previewExtensionsMap.Add("TXT", "TXT");
            _previewExtensionsMap.Add("ANM", "TXT");
            _previewExtensionsMap.Add("GAS", "TXT");
            _previewExtensionsMap.Add("NVC", "TXT");
            _previewExtensionsMap.Add("SCN", "TXT");
            _previewExtensionsMap.Add("SIF", "TXT");
            _previewExtensionsMap.Add("STK", "TXT");
            _previewExtensionsMap.Add("YAK", "TXT");
            _previewExtensionsMap.Add("BMP", "BMP");
            _previewExtensionsMap.Add("HTML", "HTML");
            _previewExtensionsMap.Add("HTM", "HTML");
            _previewExtensionsMap.Add("WAV", "WAV");
        }

        #region Event handlers

        void AboutToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			MessageBox.Show(UiUtils.GetAboutDialogText(), "About GK3BB", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                    BarnManager.OpenBarn(dialog.FileName);

                    List<BarnFile> files = BarnManager.GetFiles();

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
                            file.Extension == "BSP")
                            iconKey = "binary";
                        else if (file.Extension == "YAK" || file.Extension == "ANM" ||
                            file.Extension == "NVC" || file.Extension == "SIF" ||
                            file.Extension == "STK" || file.Extension == "GAS" ||
                            file.Extension == "SCN")
                            iconKey = "script";
                        else if (file.Extension == "FON")
                            iconKey = "font";

                        ListViewItem item = new ListViewItem(new string[] {file.Name,
                        file.InternalSize.ToString(), BarnManager.MapExtensionToType(file.Extension),
                        file.Barn, file.Compression.ToString()}, iconKey);

                        item.Tag = file;

                        mainListView.Items.Add(item);
                    }
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

                // enable the menu items
                extractSelectedFilesToolStripMenuItem.Enabled = true;
                // TODO: uncomment the following lines once previewing and stuff is implemented
                previewFileToolStripMenuItem.Enabled = true;
                //extractAllBitmapsToolStripMenuItem.Enabled = true;
                //extractAllDocsToolStripMenuItem.Enabled = true;
                //extractAllHtmlFilesToolStripMenuItem.Enabled = true;
                //extractAllWavsToolStripMenuItem.Enabled = true;
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

                    uint index = bf.Index;

                    if (bf.Extension == "BMP" && convertBitmapsToolStripMenuItem.Checked)
                    {
                        byte[] data = BarnManager.ExtractData(bf.Name);
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

                    string filename = BarnManager.GetFileName((uint)item.Tag);

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
                if (Settings.Default.ExtractPath == String.Empty)
                    dialog.SelectedPath = Environment.CurrentDirectory;
               else
                    dialog.SelectedPath = Settings.Default.ExtractPath;
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
                        System.IO.FileInfo newFileInfo = new System.IO.FileInfo(System.IO.Path.GetTempFileName());
                        newFileInfo.MoveTo(newFileInfo.FullName + "." + getPreviewExtension(bf.Extension));

                        byte[] data = BarnManager.ExtractData(bf.Name);

                        // if it's a bitmap then convert it
                        if (bf.Extension == "BMP")
                        {
                            GK3Bitmap bmp = new GK3Bitmap(data);
                            bmp.Save(newFileInfo.FullName);
                        }
                        else
                        {
                            System.IO.FileStream fs = new System.IO.FileStream(newFileInfo.FullName, System.IO.FileMode.Create);
                            fs.Write(data, 0, data.Length);
                            fs.Close();
                        }

                        System.Diagnostics.Process.Start(newFileInfo.FullName);

                        // TODO: delete the file when the process is done!
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

        #endregion

        private bool isPreviewSupported(string extension)
        {
            return _previewExtensionsMap.ContainsKey(extension.ToUpper());
        }

        private string getPreviewExtension(string extension)
        {
            return _previewExtensionsMap[extension.ToUpper()];
        }

        private ImageList _imageList;
        private ListViewColumnSorter _sorter;
        private Dictionary<string, string> _previewExtensionsMap;

        
        
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
