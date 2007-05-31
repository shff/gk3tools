// Copyright (c) 2007 Brad Farris
// This file is part of the GK3 Scene Viewer.

// The GK3 Scene Viewer is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// The GK3 Scene Viewer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Foobar; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace gk3levelviewer
{
    public static class FileSystem
    {
        public static void AddPathToSearchPath(string path)
        {
            // make sure the path is valid
            if (Directory.Exists(path))
            {
                PathInfo pathinfo = new PathInfo();
                pathinfo.Name = path;
                _searchPath.Add(pathinfo);
            }
            else
            {
                throw new DirectoryNotFoundException("Cannot add " + path 
                    + " to the search path because it does not exist");
            }
        }

        public static BarnLib.Barn AddBarnToSearchPath(string barn)
        {
            PathInfo path = new PathInfo();
            path.Barn = new BarnLib.Barn(barn);
            _searchPath.Add(path);

            return path.Barn;
        }

        public static Stream Open(string filename)
        {
            foreach (PathInfo path in _searchPath)
            {
                if (path.Barn != null)
                {
                    Stream file = path.Barn.ReadFile(filename, true);
                    if (file == null) continue;

                    return file;
                }
                else
                {
                    string filepath = path.Name + Path.DirectorySeparatorChar + filename;
                    if (File.Exists(filepath))
                    {
                        return new System.IO.FileStream(filepath, FileMode.Open, FileAccess.Read);
                    }
                }
            }

            // apparently we couldn't find the file
            throw new FileNotFoundException("Unable to find file " + filename);
        }

        private struct PathInfo
        {
            public string Name;
            public BarnLib.Barn Barn;
        }

        private static List<PathInfo> _searchPath = new List<PathInfo>();
    }
}
