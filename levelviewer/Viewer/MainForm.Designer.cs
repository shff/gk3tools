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
            this.simpleOpenGlControl1 = new Tao.Platform.Windows.SimpleOpenGlControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSCNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMODToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.editSearchPathsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lightmappingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // simpleOpenGlControl1
            // 
            this.simpleOpenGlControl1.AccumBits = ((byte)(0));
            this.simpleOpenGlControl1.AutoCheckErrors = false;
            this.simpleOpenGlControl1.AutoFinish = false;
            this.simpleOpenGlControl1.AutoMakeCurrent = true;
            this.simpleOpenGlControl1.AutoSwapBuffers = true;
            this.simpleOpenGlControl1.BackColor = System.Drawing.Color.Black;
            this.simpleOpenGlControl1.ColorBits = ((byte)(32));
            this.simpleOpenGlControl1.DepthBits = ((byte)(16));
            this.simpleOpenGlControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleOpenGlControl1.Location = new System.Drawing.Point(0, 24);
            this.simpleOpenGlControl1.Name = "simpleOpenGlControl1";
            this.simpleOpenGlControl1.Size = new System.Drawing.Size(547, 402);
            this.simpleOpenGlControl1.StencilBits = ((byte)(0));
            this.simpleOpenGlControl1.TabIndex = 0;
            this.simpleOpenGlControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl1_MouseDown);
            this.simpleOpenGlControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl1_MouseMove);
            this.simpleOpenGlControl1.Resize += new System.EventHandler(this.simpleOpenGlControl1_Resize);
            this.simpleOpenGlControl1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl1_KeyUp);
            this.simpleOpenGlControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.simpleOpenGlControl1_Paint);
            this.simpleOpenGlControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.simpleOpenGlControl1_MouseUp);
            this.simpleOpenGlControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.simpleOpenGlControl1_KeyDown);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
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
            this.openMODToolStripMenuItem,
            this.toolStripMenuItem2,
            this.editSearchPathsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openSCNToolStripMenuItem
            // 
            this.openSCNToolStripMenuItem.Name = "openSCNToolStripMenuItem";
            this.openSCNToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openSCNToolStripMenuItem.Text = "Open .SCN...";
            this.openSCNToolStripMenuItem.Click += new System.EventHandler(this.openSCNToolStripMenuItem_Click);
            // 
            // openMODToolStripMenuItem
            // 
            this.openMODToolStripMenuItem.Enabled = false;
            this.openMODToolStripMenuItem.Name = "openMODToolStripMenuItem";
            this.openMODToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openMODToolStripMenuItem.Text = "Open .MOD...";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(177, 6);
            // 
            // editSearchPathsToolStripMenuItem
            // 
            this.editSearchPathsToolStripMenuItem.Name = "editSearchPathsToolStripMenuItem";
            this.editSearchPathsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.editSearchPathsToolStripMenuItem.Text = "Edit search paths...";
            this.editSearchPathsToolStripMenuItem.Click += new System.EventHandler(this.editSearchPathsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lightmappingToolStripMenuItem,
            this.texturingToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // lightmappingToolStripMenuItem
            // 
            this.lightmappingToolStripMenuItem.Checked = true;
            this.lightmappingToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.lightmappingToolStripMenuItem.Name = "lightmappingToolStripMenuItem";
            this.lightmappingToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.lightmappingToolStripMenuItem.Text = "Lightmapping";
            this.lightmappingToolStripMenuItem.Click += new System.EventHandler(this.lightmappingToolStripMenuItem_Click);
            // 
            // texturingToolStripMenuItem
            // 
            this.texturingToolStripMenuItem.Checked = true;
            this.texturingToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.texturingToolStripMenuItem.Name = "texturingToolStripMenuItem";
            this.texturingToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.texturingToolStripMenuItem.Text = "Texturing";
            this.texturingToolStripMenuItem.Click += new System.EventHandler(this.texturingToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 426);
            this.Controls.Add(this.simpleOpenGlControl1);
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

        private Tao.Platform.Windows.SimpleOpenGlControl simpleOpenGlControl1;
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
    }
}

