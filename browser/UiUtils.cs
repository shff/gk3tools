
using System;

namespace GK3BB
{
	/// <summary>
	/// A class full of handy methods for the UI to use
	/// </summary>
	public static class UiUtils
	{
		public static string FormatFileSize(uint size)
		{
			float fsize = size;
			
			if (fsize > 1024)
			{
				fsize /= 1024;
				
				return String.Format("{0:0.00} KB", fsize);
			}
			
			return fsize + " bytes";
		}
		
		public static string GetAboutDialogText()
		{
			return "Gabriel Knight 3 Barn Browser" + Environment.NewLine
				+ Environment.NewLine
				+ "Copyright 2006 Brad Farris" + Environment.NewLine
				+ "http://www.fwheel.net" + Environment.NewLine
				+ "Licensed under the GNU GPL" + Environment.NewLine
				+ Environment.NewLine
				+ "Version " + Info.Version + Environment.NewLine
				+ Environment.NewLine
				+ "Using " + BarnLib.Barn.GetLibBarnInfo();
		}
	}
	
}
