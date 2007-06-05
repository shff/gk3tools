using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Viewer2
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

            foreach (Gk3Main.FileSystem.PathInfo info in Gk3Main.FileSystem.SearchPath)
            {
                if (info.Barn == null)
                    lstSearchPaths.Items.Add(info.Name);
                else
                    lstSearchPaths.Items.Add(info.Barn.Name);
            }
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

        private void btnRemove_Click(object sender, EventArgs e)
        {
            // TODO
        }
    }
}