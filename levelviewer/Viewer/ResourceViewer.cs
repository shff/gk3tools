using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Viewer
{
    public partial class ResourceViewer : Form
    {
        public ResourceViewer()
        {
            InitializeComponent();

            createEmptyTree();
        }

        public void UpdateResources(IList<string> resources)
        {
            createEmptyTree();

            foreach (string resource in resources)
            {
                if (resource.ToUpper().EndsWith(".BMP"))
                    resourceTree.Nodes["textures"].Nodes.Add(resource);
                else
                    resourceTree.Nodes["other"].Nodes.Add(resource);
            }
        }

        private void createEmptyTree()
        {
            resourceTree.Nodes.Clear();

            resourceTree.Nodes.Add("textures", "Textures");
            resourceTree.Nodes.Add("other", "Other");
        }
    }
}