namespace Viewer2
{
    partial class SearchPathEditor
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAddBarn = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnAddPath = new System.Windows.Forms.Button();
            this.lstSearchPaths = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnRemove);
            this.panel1.Controls.Add(this.btnAddBarn);
            this.panel1.Controls.Add(this.btnMoveDown);
            this.panel1.Controls.Add(this.btnMoveUp);
            this.panel1.Controls.Add(this.btnAddPath);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(471, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(96, 291);
            this.panel1.TabIndex = 0;
            // 
            // btnRemove
            // 
            this.btnRemove.Enabled = false;
            this.btnRemove.Location = new System.Drawing.Point(6, 61);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 4;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAddBarn
            // 
            this.btnAddBarn.Location = new System.Drawing.Point(6, 32);
            this.btnAddBarn.Name = "btnAddBarn";
            this.btnAddBarn.Size = new System.Drawing.Size(75, 23);
            this.btnAddBarn.TabIndex = 3;
            this.btnAddBarn.Text = "Add barn...";
            this.btnAddBarn.UseVisualStyleBackColor = true;
            this.btnAddBarn.Click += new System.EventHandler(this.btnAddBarn_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Enabled = false;
            this.btnMoveDown.Location = new System.Drawing.Point(6, 119);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(75, 23);
            this.btnMoveDown.TabIndex = 2;
            this.btnMoveDown.Text = "Move down";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Enabled = false;
            this.btnMoveUp.Location = new System.Drawing.Point(6, 90);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(75, 23);
            this.btnMoveUp.TabIndex = 1;
            this.btnMoveUp.Text = "Move up";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            // 
            // btnAddPath
            // 
            this.btnAddPath.Location = new System.Drawing.Point(6, 3);
            this.btnAddPath.Name = "btnAddPath";
            this.btnAddPath.Size = new System.Drawing.Size(75, 23);
            this.btnAddPath.TabIndex = 0;
            this.btnAddPath.Text = "Add path...";
            this.btnAddPath.UseVisualStyleBackColor = true;
            this.btnAddPath.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // lstSearchPaths
            // 
            this.lstSearchPaths.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSearchPaths.FormattingEnabled = true;
            this.lstSearchPaths.HorizontalScrollbar = true;
            this.lstSearchPaths.Location = new System.Drawing.Point(0, 0);
            this.lstSearchPaths.Name = "lstSearchPaths";
            this.lstSearchPaths.Size = new System.Drawing.Size(471, 290);
            this.lstSearchPaths.TabIndex = 1;
            // 
            // SearchPathEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 291);
            this.Controls.Add(this.lstSearchPaths);
            this.Controls.Add(this.panel1);
            this.Name = "SearchPathEditor";
            this.Text = "Edit search paths";
            this.Load += new System.EventHandler(this.SearchPathEditor_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnAddPath;
        private System.Windows.Forms.ListBox lstSearchPaths;
        private System.Windows.Forms.Button btnAddBarn;
        private System.Windows.Forms.Button btnRemove;
    }
}