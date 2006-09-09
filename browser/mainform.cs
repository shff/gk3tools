using System;
using System.Collections.Generic;

using Gtk;
using GtkSharp;

namespace GK3BB
{
	public class MainForm : Window
	{
		private const string defaultTitle = "GK3 Barn Browser";
		
		public static void Main()
		{
			Application.Init();

			new MainForm(defaultTitle);
			
			Application.Run ();
		}
		
		public MainForm(string title) : base(title)
		{
			DeleteEvent += new DeleteEventHandler(exit_Clicked);
			SetDefaultSize(640, 480);
			
			mainBox = new VBox (false, 2);
			
			createMenu();
			createMainList();
			
			mainBox.PackStart(mainMenu, false, false, 0);
			
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(mainListBox);
			mainListBox.HeadersVisible = true;
			mainBox.PackStart(sw, true, true, 0);
			
			Add(mainBox);
			ShowAll();
		}
		
		private void createMenu()
		{
			mainMenu = new MenuBar ();
			
			// create the top menu buttons
			mnuFile = new MenuItem("File");
			mnuTools = new MenuItem("Tools");
			mnuHelp = new MenuItem("Help");
			
			mainMenu.Append(mnuFile);
			mainMenu.Append(mnuTools);
			mainMenu.Append(mnuHelp);
			
			// create the File menu items
			mnuFile_Open = new MenuItem("Open barn...");
			mnuFile_Extract = new MenuItem("Extract selected files");
			mnuFile_SetExtractPath = new MenuItem("Set extract to path...");
			mnuFile_Preview = new MenuItem("Preview file");
			mnuFile_ConvertBitmaps = new CheckMenuItem("Convert bitmaps");
			mnuFile_DecompressFiles = new CheckMenuItem("Decompress files");
			mnuFile_Exit = new MenuItem("Exit");
			
			// set the default checks
			mnuFile_ConvertBitmaps.Active = BarnManager.ConvertBitmaps;
			mnuFile_DecompressFiles.Active = BarnManager.Decompress;
			
			mnuTools_ExtractBitmaps = new MenuItem("Extract all bitmaps");
			mnuTools_ExtractWavs = new MenuItem("Extract all wavs");
			mnuTools_ExtractDocs = new MenuItem("Extract all docs");
			mnuTools_ExtractHTML = new MenuItem("Extract all html files");
			
			// create the Help menu items
			mnuHelp_About = new MenuItem("About");
			
			// insert the File menu items into the File menu
			Menu fm = new Menu();
			fm.Append(mnuFile_Open);
			fm.Append(mnuFile_Extract);
			fm.Append(mnuFile_SetExtractPath);
			fm.Append(mnuFile_Preview);
			fm.Append(new SeparatorMenuItem());
			fm.Append(mnuFile_ConvertBitmaps);
			fm.Append(mnuFile_DecompressFiles);
			fm.Append(new SeparatorMenuItem());
			fm.Append(mnuFile_Exit);
			mnuFile.Submenu = fm;
			
			// insert the Layer menu items into the Layer menu
			Menu tm = new Menu();
			tm.Append(mnuTools_ExtractBitmaps);
			tm.Append(mnuTools_ExtractWavs);
			tm.Append(mnuTools_ExtractDocs);
			tm.Append(mnuTools_ExtractHTML);
			mnuTools.Submenu = tm;
			
			// insert the Help menu items into the File menu
			Menu hm = new Menu();
			hm.Append(mnuHelp_About);
			mnuHelp.Submenu = hm;
			
			// assign File menu item event handlers
			mnuFile_Open.Activated += new EventHandler(mnuFile_Open_Clicked);
			mnuFile_Extract.Activated += new EventHandler(mnuFile_Extract_Clicked);
			mnuFile_SetExtractPath.Activated += new EventHandler(mnuFile_SetExtractPath_Clicked);
			mnuFile_Preview.Activated += new EventHandler(mnuFile_Preview_Clicked);
			mnuFile_ConvertBitmaps.Toggled += new EventHandler(mnuFile_ConvertBitmaps_Toggled);
			mnuFile_DecompressFiles.Toggled += new EventHandler(mnuFile_DecompressFiles_Toggled);
			mnuFile_Exit.Activated += new EventHandler(exit_Clicked);
			
			// assign Help menu item event handlers
			mnuHelp_About.Activated += new EventHandler(mnuHelp_About_Clicked);
		}
		
		
		private void createMainList()
		{
			mainListBox = new TreeView();
			mainListStore = new ListStore(typeof(uint), typeof(string), typeof(uint), typeof(string), typeof(string), typeof(string));
			mainListBox.Model = mainListStore;
			mainListBox.HeadersVisible = true;
			mainListBox.RulesHint = true;
			mainListBox.Selection.Mode = SelectionMode.Multiple;
			
			TreeViewColumn index = new TreeViewColumn();
			index.Visible = false;
			mainListBox.AppendColumn(index);
			filenameColumn = mainListBox.AppendColumn("File name", new CellRendererText(), "text", 1);
			sizeColumn = mainListBox.AppendColumn("Size", new CellRendererText(), "text", 2);
			TreeViewColumn type = mainListBox.AppendColumn("Type", new CellRendererText(), "text", 3);
			TreeViewColumn barn = mainListBox.AppendColumn("Barn", new CellRendererText(), "text", 4);
			TreeViewColumn compression = mainListBox.AppendColumn("Compression", new CellRendererText(), "text", 5);
			
			filenameColumn.Clickable = true;
			filenameColumn.Resizable = true;
			filenameColumn.Clicked += new EventHandler(filename_column_clicked);
			
			sizeColumn.Clickable = true;
			sizeColumn.Resizable = true;
			sizeColumn.Clicked += new EventHandler(size_column_clicked);
			
			type.Clickable = true;
			type.Resizable = true;
			type.Clicked += new EventHandler(type_column_clicked);
			
			barn.Clickable = true;
			barn.Resizable = true;
			
			compression.Clickable = true;
			compression.Resizable = true;
			
			mainListStore.SetSortFunc(0, stringCompareFunc);
		}
		
		#region Events
		
		void exit_Clicked(object o, EventArgs args)
		{
			Application.Quit();
		}
		
		void mnuFile_Open_Clicked(object o, EventArgs args)
		{
			FileChooserDialog chooser = new FileChooserDialog ("Open",
									   this,
									   FileChooserAction.Open);
			
			chooser.LocalOnly = true;

			chooser.AddButton (Stock.Cancel, ResponseType.Cancel);
			chooser.AddButton (Stock.Open, ResponseType.Ok);

			FileFilter brnfilter = new FileFilter();
			FileFilter allfilter = new FileFilter();
			brnfilter.Name = "Barn files";
			brnfilter.AddPattern("*.brn");
			allfilter.Name = "All files";
			allfilter.AddPattern("*.*");
			
			chooser.AddFilter(brnfilter);
			chooser.AddFilter(allfilter);
			
			int response = chooser.Run();

			if ((ResponseType) response == ResponseType.Ok)
			{
				try
				{
					BarnManager.OpenBarn(chooser.Uri.Replace("file://", ""));
					
					List<BarnFile> files = BarnManager.GetFiles();
					
					Console.WriteLine("Starting...");

					mainListStore.Clear();
					foreach(BarnFile file in files)
					{
						mainListStore.AppendValues(file.Index, file.Name, file.InternalSize,
							BarnManager.MapExtensionToType(file.Extension),
							file.Barn, file.Compression.ToString());
					}
					
					Console.WriteLine("There are " + files.Count + " files!");
				}
				catch(System.IO.FileNotFoundException)
				{
					displayOkErrorMessageBox("Unable to open " + chooser.Uri
						+ " because it could not be found.");
				}
				catch(System.IO.DirectoryNotFoundException)
				{
					displayOkErrorMessageBox("Unable to open " + chooser.Uri
						+ " because part of the path could not be found.");
				}
				catch(BarnLib.BarnException)
				{
					displayOkErrorMessageBox("Unable to open " + chooser.Uri
						+ " because it is not a valid Barn file.");
				}
			}
			
			chooser.Destroy();
		}
		
		void mnuFile_Extract_Clicked(object o, EventArgs args)
		{
			TreePath[] paths;
			TreeModel model;
			
			paths = mainListBox.Selection.GetSelectedRows(out model);
			
			foreach(TreePath path in paths)
			{
				TreeIter itr;
				if (model.GetIter(out itr, path) == true)
				{
					uint index = (uint)model.GetValue(itr, 0);
					
					Console.Write("Extracting... ");
					BarnManager.Extract(index);
					Console.WriteLine("Done!");
				}
			}
		}
		
		void mnuFile_SetExtractPath_Clicked(object o, EventArgs args)
		{
			FileChooserDialog chooser = new FileChooserDialog ("Select Folder",
									   this,
									   FileChooserAction.SelectFolder);
			
			chooser.LocalOnly = true;

			chooser.AddButton(Stock.Cancel, ResponseType.Cancel);
			chooser.AddButton(Stock.Open, ResponseType.Ok);
			
			chooser.SetCurrentFolder(BarnManager.ExtractPath);
			
			int response = chooser.Run();
			
			if ((ResponseType)response == ResponseType.Ok)
			{
				BarnManager.ExtractPath = chooser.CurrentFolder;
			}
			
			chooser.Destroy();
		}
		
		void mnuFile_Preview_Clicked(object o, EventArgs args)
		{
		}
		
		void mnuFile_ConvertBitmaps_Toggled(object o, EventArgs args)
		{
			BarnManager.ConvertBitmaps = mnuFile_ConvertBitmaps.Active;
		}
		
		void mnuFile_DecompressFiles_Toggled(object o, EventArgs args)
		{
			BarnManager.Decompress = mnuFile_DecompressFiles.Active;
		}
		
		void mnuHelp_About_Clicked(object o, EventArgs args)
		{
			MessageDialog md = new MessageDialog(this,
				DialogFlags.DestroyWithParent,
				MessageType.Info,
				ButtonsType.Ok,
				"Gabriel Knight 3 Barn Browser" + Environment.NewLine
				+ Environment.NewLine
				+ "Copyright 2006 Brad Farris" + Environment.NewLine
				+ "http://www.fwheel.net" + Environment.NewLine
				+ "Licensed under the GNU GPL" + Environment.NewLine
				+ Environment.NewLine
				+ "Version " + Info.Version + Environment.NewLine
				+ Environment.NewLine
				+ "Using " + BarnLib.Barn.GetLibBarnInfo());
			
			md.Run();
			md.Destroy();
		}
		
		private void displayOkInfoMessageBox(string message)
		{
			MessageDialog md = new MessageDialog(this,
				DialogFlags.DestroyWithParent,
				MessageType.Info,
				ButtonsType.Ok,
				message);
			
			md.Run();
			md.Destroy();
		}
		
		private void displayOkErrorMessageBox(string message)
		{
			MessageDialog md = new MessageDialog(this,
				DialogFlags.DestroyWithParent,
				MessageType.Error,
				ButtonsType.Ok,
				message);
			
			md.Run();
			md.Destroy();
		}
		
		private void filename_column_clicked(object sender, EventArgs args)
		{
			TreeViewColumn col = (TreeViewColumn)sender;
			
			setSortColumn(col, 1);
		}
		
		private void size_column_clicked(object sender, EventArgs args)
		{
			TreeViewColumn col = (TreeViewColumn)sender;
			
			setSortColumn(col, 2);
		}
		
		private void type_column_clicked(object sender, EventArgs args)
		{
			TreeViewColumn col = (TreeViewColumn)sender;
			setSortColumn(col, 3);
		}
		
		private void setSortColumn(TreeViewColumn col, int id)
		{
			if (col.SortIndicator)
			{
				if (col.SortOrder == SortType.Ascending)
					col.SortOrder = SortType.Descending;
				else
					col.SortOrder = SortType.Ascending;
			}
			else
				col.SortOrder = SortType.Ascending;
				
			// disable all the columns' sort indicator
			filenameColumn.SortIndicator = false;
			sizeColumn.SortIndicator = false;
			
			col.SortIndicator = true;
			mainListStore.SetSortColumnId(id, col.SortOrder);
		}
		
		private int stringCompareFunc(TreeModel model, TreeIter a, TreeIter b)
		{
			return String.Compare((string)model.GetValue(a, 1), (string)model.GetValue(b, 1));
		}
		
		#endregion Events
		
		#region Private members
		private VBox mainBox;
		
		private MenuBar mainMenu;
		private MenuItem mnuFile;
		private MenuItem mnuTools;
		private MenuItem mnuHelp;
		
		private MenuItem mnuFile_Open;
		private MenuItem mnuFile_Extract;
		private MenuItem mnuFile_SetExtractPath;
		private MenuItem mnuFile_Preview;
		private CheckMenuItem mnuFile_ConvertBitmaps;
		private CheckMenuItem mnuFile_DecompressFiles;
		private MenuItem mnuFile_Exit;
		
		private MenuItem mnuTools_ExtractBitmaps;
		private MenuItem mnuTools_ExtractWavs;
		private MenuItem mnuTools_ExtractDocs;
		private MenuItem mnuTools_ExtractHTML;
		
		private MenuItem mnuHelp_About;
			
		private TreeView mainListBox;
		private ListStore mainListStore;
		private TreeViewColumn filenameColumn;
		private TreeViewColumn sizeColumn;
		
		#endregion Private members
	}
}