using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GK3BB
{
    public partial class SetViewerPath : Form
    {
        public SetViewerPath()
        {
            InitializeComponent();
        }

        public string ViewerPath
        {
            get { return txtPath.Text; }
            set { txtPath.Text = value; }
        }

        private void SetViewerPath_Load(object sender, EventArgs e)
        {
            txtPath.Text = Settings.Default.PathToViewer;
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Executables (*.exe)|*.exe";
            if (string.IsNullOrEmpty(txtPath.Text) == false)
            {
                open.FileName = txtPath.Text;
                open.InitialDirectory = System.IO.Path.GetDirectoryName(txtPath.Text);
            }

            if (open.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = open.FileName;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
