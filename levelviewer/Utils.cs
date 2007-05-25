using System;
using System.Collections.Generic;
using System.Text;

namespace gk3levelviewer
{
    static class Utils
    {
        public static string GetFilenameWithoutExtension(string filename)
        {
            int dot = filename.LastIndexOf('.');
            if (dot == -1 || dot == 0)
                return filename;

            return filename.Substring(0, dot);
        }
    }
}
