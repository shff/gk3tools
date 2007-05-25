using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace gk3levelviewer
{
    static class FileSystem
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

        public static void AddBarnToSearchPath(string barn)
        {
            PathInfo path = new PathInfo();
            path.Barn = new BarnLib.Barn(barn);
            _searchPath.Add(path);
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
