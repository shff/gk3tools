namespace Viewer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSCNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSifMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMODToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.editSearchPathsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.takeScreenshotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lightmappingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xLightmapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shadingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.linearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.anisotropicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.anisotropic4XToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calculateLightmapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rendererToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.direct3D9ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openGLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pbRenderWindow = new Viewer.Direct3D9RenderControl();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(547, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSCNToolStripMenuItem,
            this.openSifMenuItem,
            this.openMODToolStripMenuItem,
            this.toolStripMenuItem2,
            this.editSearchPathsToolStripMenuItem,
            this.toolStripSeparator1,
            this.takeScreenshotToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openSCNToolStripMenuItem
            // 
            this.openSCNToolStripMenuItem.Name = "openSCNToolStripMenuItem";
            this.openSCNToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.openSCNToolStripMenuItem.Text = "Open .SCN...";
            this.openSCNToolStripMenuItem.Click += new System.EventHandler(this.openSCNToolStripMenuItem_Click);
            // 
            // openSifMenuItem
            // 
            this.openSifMenuItem.Name = "openSifMenuItem";
            this.openSifMenuItem.Size = new System.Drawing.Size(172, 22);
            this.openSifMenuItem.Text = "Open .SIF...";
            this.openSifMenuItem.Click += new System.EventHandler(this.openSifMenuItem_Click);
            // 
            // openMODToolStripMenuItem
            // 
            this.openMODToolStripMenuItem.Name = "openMODToolStripMenuItem";
            this.openMODToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.openMODToolStripMenuItem.Text = "Open .MOD...";
            this.openMODToolStripMenuItem.Click += new System.EventHandler(this.openMODToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(169, 6);
            // 
            // editSearchPathsToolStripMenuItem
            // 
            this.editSearchPathsToolStripMenuItem.Name = "editSearchPathsToolStripMenuItem";
            this.editSearchPathsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.editSearchPathsToolStripMenuItem.Text = "Edit search paths...";
            this.editSearchPathsToolStripMenuItem.Click += new System.EventHandler(this.editSearchPathsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(169, 6);
            // 
            // takeScreenshotToolStripMenuItem
            // 
            this.takeScreenshotToolStripMenuItem.Name = "takeScreenshotToolStripMenuItem";
            this.takeScreenshotToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.takeScreenshotToolStripMenuItem.Text = "Take screenshot";
            this.takeScreenshotToolStripMenuItem.Click += new System.EventHandler(this.takeScreenshotToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rendererToolStripMenuItem,
            this.lightmappingToolStripMenuItem,
            this.texturingToolStripMenuItem,
            this.xLightmapsToolStripMenuItem,
            this.shadingToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // lightmappingToolStripMenuItem
            // 
            this.lightmappingToolStripMenuItem.Checked = true;
            this.lightmappingToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.lightmappingToolStripMenuItem.Name = "lightmappingToolStripMenuItem";
            this.lightmappingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.lightmappingToolStripMenuItem.Text = "Lightmapping";
            this.lightmappingToolStripMenuItem.Click += new System.EventHandler(this.lightmappingToolStripMenuItem_Click);
            // 
            // texturingToolStripMenuItem
            // 
            this.texturingToolStripMenuItem.Checked = true;
            this.texturingToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.texturingToolStripMenuItem.Name = "texturingToolStripMenuItem";
            this.texturingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.texturingToolStripMenuItem.Text = "Texturing";
            this.texturingToolStripMenuItem.Click += new System.EventHandler(this.texturingToolStripMenuItem_Click);
            // 
            // xLightmapsToolStripMenuItem
            // 
            this.xLightmapsToolStripMenuItem.Checked = true;
            this.xLightmapsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.xLightmapsToolStripMenuItem.Name = "xLightmapsToolStripMenuItem";
            this.xLightmapsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.xLightmapsToolStripMenuItem.Text = "2X Lightmaps";
            this.xLightmapsToolStripMenuItem.Click += new System.EventHandler(this.xLightmapsToolStripMenuItem_Click);
            // 
            // shadingToolStripMenuItem
            // 
            this.shadingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noneToolStripMenuItem,
            this.linearToolStripMenuItem,
            this.anisotropicToolStripMenuItem,
            this.anisotropic4XToolStripMenuItem});
            this.shadingToolStripMenuItem.Name = "shadingToolStripMenuItem";
            this.shadingToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.shadingToolStripMenuItem.Text = "Smoothing";
            // 
            // noneToolStripMenuItem
            // 
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            this.noneToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.noneToolStripMenuItem.Text = "None";
            this.noneToolStripMenuItem.Click += new System.EventHandler(this.noneToolStripMenuItem_Click);
            // 
            // linearToolStripMenuItem
            // 
            this.linearToolStripMenuItem.Checked = true;
            this.linearToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.linearToolStripMenuItem.Name = "linearToolStripMenuItem";
            this.linearToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.linearToolStripMenuItem.Text = "Linear";
            this.linearToolStripMenuItem.Click += new System.EventHandler(this.linearToolStripMenuItem_Click);
            // 
            // anisotropicToolStripMenuItem
            // 
            this.anisotropicToolStripMenuItem.Name = "anisotropicToolStripMenuItem";
            this.anisotropicToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.anisotropicToolStripMenuItem.Text = "Anisotropic (2X)";
            this.anisotropicToolStripMenuItem.Click += new System.EventHandler(this.anisotropicToolStripMenuItem_Click);
            // 
            // anisotropic4XToolStripMenuItem
            // 
            this.anisotropic4XToolStripMenuItem.Name = "anisotropic4XToolStripMenuItem";
            this.anisotropic4XToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.anisotropic4XToolStripMenuItem.Text = "Anisotropic (4X)";
            this.anisotropic4XToolStripMenuItem.Click += new System.EventHandler(this.anisotropic4XToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.calculateLightmapsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // calculateLightmapsToolStripMenuItem
            // 
            this.calculateLightmapsToolStripMenuItem.Name = "calculateLightmapsToolStripMenuItem";
            this.calculateLightmapsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.calculateLightmapsToolStripMenuItem.Text = "Calculate lightmaps";
            this.calculateLightmapsToolStripMenuItem.Click += new System.EventHandler(this.calculateLightmapsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // rendererToolStripMenuItem
            // 
            this.rendererToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openGLToolStripMenuItem,
            this.direct3D9ToolStripMenuItem});
            this.rendererToolStripMenuItem.Name = "rendererToolStripMenuItem";
            this.rendererToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rendererToolStripMenuItem.Text = "Renderer";
            // 
            // direct3D9ToolStripMenuItem
            // 
            this.direct3D9ToolStripMenuItem.Name = "direct3D9ToolStripMenuItem";
            this.direct3D9ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.direct3D9ToolStripMenuItem.Text = "Direct3D 9";
            this.direct3D9ToolStripMenuItem.Click += new System.EventHandler(this.direct3D9ToolStripMenuItem_Click);
            // 
            // openGLToolStripMenuItem
            // 
            this.openGLToolStripMenuItem.Checked = true;
            this.openGLToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.openGLToolStripMenuItem.Name = "openGLToolStripMenuItem";
            this.openGLToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openGLToolStripMenuItem.Text = "OpenGL";
            this.openGLToolStripMenuItem.Click += new System.EventHandler(this.openGLToolStripMenuItem_Click);
            // 
            // pbRenderWindow
            // 
            this.pbRenderWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbRenderWindow.Location = new System.Drawing.Point(0, 24);
            this.pbRenderWindow.Name = "pbRenderWindow";
            this.pbRenderWindow.Size = new System.Drawing.Size(547, 402);
            this.pbRenderWindow.TabIndex = 2;
            this.pbRenderWindow.Text = "direct3D9RenderControl1";
            this.pbRenderWindow.Paint += new System.Windows.Forms.PaintEventHandler(this.simpleOpenGlControl1_Paint);
            this.pbRenderWindow.KeyDown += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl1_KeyDown);
            this.pbRenderWindow.KeyUp += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl1_KeyUp);
            this.pbRenderWindow.MouseDown += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl1_MouseDown);
            this.pbRenderWindow.MouseMove += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl1_MouseMove);
            this.pbRenderWindow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl1_MouseUp);
            this.pbRenderWindow.Resize += new System.EventHandler(this.pbRenderWindow_Resize);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 426);
            this.Controls.Add(this.pbRenderWindow);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "GK3 Viewer";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSCNToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMODToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem editSearchPathsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightmappingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSifMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xLightmapsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem takeScreenshotToolStripMenuItem;
        private Direct3D9RenderControl pbRenderWindow;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calculateLightmapsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shadingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem linearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem anisotropicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem anisotropic4XToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rendererToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem direct3D9ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openGLToolStripMenuItem;
    }
}

