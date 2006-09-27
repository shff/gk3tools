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
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
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
				BarnManager.OpenBarn(dialog.FileName);
				
				List<BarnFile> files = BarnManager.GetFiles();
				
				foreach(BarnFile file in files)
				{
					mainListView.Items.Add(file.Index, file.Name, file.InternalSize,
						BarnManager.MapExtensionToType(file.Extension),
						file.Barn, file.Compression.ToString());
				}
			}
		}
	}
}
