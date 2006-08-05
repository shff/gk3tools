
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
	}
	
}
