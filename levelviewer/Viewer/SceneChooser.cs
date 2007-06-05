using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Viewer2
{
    public partial class SceneChooser : Form
    {
        public SceneChooser()
        {
            InitializeComponent();
        }

        public void AddFile(string filename)
        {
            listBox1.Items.Add(filename);
        }

        public string SelectedScene
        {
            get { return listBox1.SelectedItem.ToString(); }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}