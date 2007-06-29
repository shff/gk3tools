/*
 * Created by SharpDevelop.
 * User: Brad Farris
 * Date: 9/18/2006
 * Time: 8:22 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace GK3BB
{
	partial class MainForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openBarnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setExtractToPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previewFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.convertBitmapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decompressFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAllBitmapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAllWavsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAllDocsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAllHtmlFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainListView = new System.Windows.Forms.ListView();
            this.filenameColumn = new System.Windows.Forms.ColumnHeader();
            this.sizeColumn = new System.Windows.Forms.ColumnHeader();
            this.typeColumn = new System.Windows.Forms.ColumnHeader();
            this.barnColumn = new System.Windows.Forms.ColumnHeader();
            this.compressionColumn = new System.Windows.Forms.ColumnHeader();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(560, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openBarnToolStripMenuItem,
            this.extractSelectedFilesToolStripMenuItem,
            this.setExtractToPathToolStripMenuItem,
            this.previewFileToolStripMenuItem,
            this.toolStripMenuItem2,
            this.convertBitmapsToolStripMenuItem,
            this.decompressFilesToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openBarnToolStripMenuItem
            // 
            this.openBarnToolStripMenuItem.Name = "openBarnToolStripMenuItem";
            this.openBarnToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.openBarnToolStripMenuItem.Text = "Open barn...";
            this.openBarnToolStripMenuItem.Click += new System.EventHandler(this.OpenBarnToolStripMenuItemClick);
            // 
            // extractSelectedFilesToolStripMenuItem
            // 
            this.extractSelectedFilesToolStripMenuItem.Enabled = false;
            this.extractSelectedFilesToolStripMenuItem.Name = "extractSelectedFilesToolStripMenuItem";
            this.extractSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.extractSelectedFilesToolStripMenuItem.Text = "Extract selected files";
            this.extractSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.extractSelectedFilesToolStripMenuItem_Click);
            // 
            // setExtractToPathToolStripMenuItem
            // 
            this.setExtractToPathToolStripMenuItem.Name = "setExtractToPathToolStripMenuItem";
            this.setExtractToPathToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.setExtractToPathToolStripMenuItem.Text = "Set extract to path...";
            this.setExtractToPathToolStripMenuItem.Click += new System.EventHandler(this.setExtractToPathToolStripMenuItem_Click);
            // 
            // previewFileToolStripMenuItem
            // 
            this.previewFileToolStripMenuItem.Enabled = false;
            this.previewFileToolStripMenuItem.Name = "previewFileToolStripMenuItem";
            this.previewFileToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.previewFileToolStripMenuItem.Text = "Preview file";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(186, 6);
            // 
            // convertBitmapsToolStripMenuItem
            // 
            this.convertBitmapsToolStripMenuItem.Checked = true;
            this.convertBitmapsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.convertBitmapsToolStripMenuItem.Name = "convertBitmapsToolStripMenuItem";
            this.convertBitmapsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.convertBitmapsToolStripMenuItem.Text = "Convert bitmaps";
            // 
            // decompressFilesToolStripMenuItem
            // 
            this.decompressFilesToolStripMenuItem.Checked = true;
            this.decompressFilesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.decompressFilesToolStripMenuItem.Name = "decompressFilesToolStripMenuItem";
            this.decompressFilesToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.decompressFilesToolStripMenuItem.Text = "Decompress files";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(186, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractAllBitmapsToolStripMenuItem,
            this.extractAllWavsToolStripMenuItem,
            this.extractAllDocsToolStripMenuItem,
            this.extractAllHtmlFilesToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // extractAllBitmapsToolStripMenuItem
            // 
            this.extractAllBitmapsToolStripMenuItem.Enabled = false;
            this.extractAllBitmapsToolStripMenuItem.Name = "extractAllBitmapsToolStripMenuItem";
            this.extractAllBitmapsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.extractAllBitmapsToolStripMenuItem.Text = "Extract all bitmaps";
            // 
            // extractAllWavsToolStripMenuItem
            // 
            this.extractAllWavsToolStripMenuItem.Enabled = false;
            this.extractAllWavsToolStripMenuItem.Name = "extractAllWavsToolStripMenuItem";
            this.extractAllWavsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.extractAllWavsToolStripMenuItem.Text = "Extract all wavs";
            // 
            // extractAllDocsToolStripMenuItem
            // 
            this.extractAllDocsToolStripMenuItem.Enabled = false;
            this.extractAllDocsToolStripMenuItem.Name = "extractAllDocsToolStripMenuItem";
            this.extractAllDocsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.extractAllDocsToolStripMenuItem.Text = "Extract all docs";
            // 
            // extractAllHtmlFilesToolStripMenuItem
            // 
            this.extractAllHtmlFilesToolStripMenuItem.Enabled = false;
            this.extractAllHtmlFilesToolStripMenuItem.Name = "extractAllHtmlFilesToolStripMenuItem";
            this.extractAllHtmlFilesToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.extractAllHtmlFilesToolStripMenuItem.Text = "Extract all html files";
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
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
            // 
            // mainListView
            // 
            this.mainListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.filenameColumn,
            this.sizeColumn,
            this.typeColumn,
            this.barnColumn,
            this.compressionColumn});
            this.mainListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainListView.Location = new System.Drawing.Point(0, 24);
            this.mainListView.Name = "mainListView";
            this.mainListView.Size = new System.Drawing.Size(560, 349);
            this.mainListView.TabIndex = 1;
            this.mainListView.UseCompatibleStateImageBehavior = false;
            this.mainListView.View = System.Windows.Forms.View.Details;
            this.mainListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mainListView_ColumnClick);
            // 
            // filenameColumn
            // 
            this.filenameColumn.Text = "File name";
            this.filenameColumn.Width = 218;
            // 
            // sizeColumn
            // 
            this.sizeColumn.Text = "Size";
            // 
            // typeColumn
            // 
            this.typeColumn.Text = "Type";
            // 
            // barnColumn
            // 
            this.barnColumn.Text = "Barn";
            // 
            // compressionColumn
            // 
            this.compressionColumn.Text = "Compression";
            this.compressionColumn.Width = 91;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 373);
            this.Controls.Add(this.mainListView);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Gabriel Knight 3 Browser";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.ListView mainListView;
		private System.Windows.Forms.ColumnHeader compressionColumn;
		private System.Windows.Forms.ColumnHeader barnColumn;
		private System.Windows.Forms.ColumnHeader typeColumn;
		private System.Windows.Forms.ColumnHeader sizeColumn;
		private System.Windows.Forms.ColumnHeader filenameColumn;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem extractAllHtmlFilesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem extractAllDocsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem extractAllWavsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem extractAllBitmapsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem decompressFilesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem convertBitmapsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem previewFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem setExtractToPathToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem extractSelectedFilesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openBarnToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip1;
	}
}
