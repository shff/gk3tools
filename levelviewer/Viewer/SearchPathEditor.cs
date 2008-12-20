using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Viewer
{
    public partial class SearchPathEditor : Form
    {
        public SearchPathEditor()
        {
            InitializeComponent();
        }

        public new void Refresh()
        {
            lstSearchPaths.Items.Clear();

            StringBuilder searchPathString = new StringBuilder();
            foreach (Gk3Main.FileSystem.PathInfo info in Gk3Main.FileSystem.SearchPath)
            {
                if (info.Barn == null)
                    lstSearchPaths.Items.Add(info.Name);
                else
                    lstSearchPaths.Items.Add(info.Barn.Name);

                searchPathString.Append(info.Name);
                searchPathString.Append(';');
            }

            Settings.Default.SearchPath = searchPathString.ToString();
            Settings.Default.Save();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void SearchPathEditor_Load(object sender, EventArgs e)
        {
            Refresh();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Gk3Main.FileSystem.AddPathToSearchPath(dialog.SelectedPath);
                Refresh();
            }
        }

        private void btnAddBarn_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Barn files (*.brn)|*.brn|All files (*.*)|*.*";
                dialog.Title = "Open barn file";
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;

                    Gk3Main.FileSystem.AddBarnToSearchPath(dialog.FileName);
                    Refresh();

                    Cursor.Current = Cursors.Default;
                }
            }
            catch (BarnLib.BarnException)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("You must provide a valid barn file.", "Unable to add barn to search path",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            string path = lstSearchPaths.SelectedItem as string;

            if (path != null)
            {
                Gk3Main.FileSystem.RemoveFromSearchPath(path);
            }

            Refresh();
        }
    }
}