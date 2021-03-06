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

namespace Gk3Main
{
    public static class FileSystem
    {
        public static void AddPathToSearchPath(string path)
        {
            AddPathToSearchPath(path, false);
        }

        public static void AddPathToSearchPath(string path, bool addAsFirst)
        {
            // make sure the path is valid
            if (Directory.Exists(path))
            {
                PathInfo pathinfo = new PathInfo();
                pathinfo.Name = Path.GetFullPath(path);

                if (addAsFirst)
                    _searchPath.Insert(0, pathinfo);
                else
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
            AddBarnToSearchPath(barn, false);
        }

        public static void AddBarnToSearchPath(string barn, bool addAsFirst)
        {
            PathInfo path = new PathInfo();
            path.Name = barn;
            path.Barn = new BetterBarn(new BarnLib.Barn(barn), barn);

            if (addAsFirst)
                _searchPath.Insert(0, path);
            else
                _searchPath.Add(path);
        }

        public static void RemoveFromSearchPath(PathInfo path)
        {
            _searchPath.Remove(path);
        }

        public static void IncreasePathPriority(string path)
        {
            // find the path
            int index = 0;
            for (; index < _searchPath.Count; index++)
            {
                if (_searchPath[index].Name == path)
                {
                    break;
                }
            }

            if (index == 0 || index == _searchPath.Count) return;

            // remove the path and insert it "higher"
            PathInfo info = _searchPath[index];
            _searchPath.RemoveAt(index);
            _searchPath.Insert(index-1, info);
        }

        public static void DecreasePathPriority(string path)
        {
            // find the path
            int index = 0;
            for (; index < _searchPath.Count; index++)
            {
                if (_searchPath[index].Name == path)
                {
                    break;
                }
            }

            if (index >= _searchPath.Count-1) return;

            // remove the path and insert it "higher"
            PathInfo info = _searchPath[index];
            _searchPath.RemoveAt(index);
            _searchPath.Insert(index + 1, info);
        }

        private struct SearchPathPredicate
        {
            public string Path;

            public bool Where(PathInfo info)
            {
                if (info.Name == Path || (info.Barn != null && info.Barn.InternalBarn.Path == Path))
                    return true;
                
                return false;
            }
        }

        public static void RemoveFromSearchPath(string path)
        {
            SearchPathPredicate p;
            p.Path = path;

            _searchPath.RemoveAll(p.Where);
        }

        public static PathInfo[] SearchPath { get { return _searchPath.ToArray(); } }

        /// <summary>
        /// Returns the absolute filename of the file, based on the search path, or null if the file isn't found
        /// </summary>
        public static BarnLib.Barn FindBarn(string name)
        {
            foreach (PathInfo path in _searchPath)
            {
                if (path.Barn != null && path.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return path.Barn.InternalBarn;
            }

            return null;
        }

        public static Stream Open(string filename)
        {
            foreach (PathInfo path in _searchPath)
            {
                if (path.Barn != null)
                {
                    Stream file = path.Barn.ReadFile(filename);
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

        public static string[] GetFilesWithExtension(string extension)
        {
            string dotExtension = "." + extension;
            Dictionary<string, string> files = new Dictionary<string, string>();

            foreach (PathInfo path in _searchPath)
            {
                if (path.Barn != null)
                {
                    for (int i = 0; i < path.Barn.InternalBarn.NumberOfFiles; i++)
                    {
                        try
                        {
                            string name = path.Barn.InternalBarn.GetFileName(i);

                            if (name.EndsWith(dotExtension))
                                files.Add(name, name);
                        }
                        catch (ArgumentException)
                        {
                            // ignore
                        }
                    }
                }
                else
                {
                    string[] dirFiles = Directory.GetFiles(path.Name, "*." + extension);

                    foreach (string file in dirFiles)
                    {
                        try { files.Add(file, file); }
                        catch (ArgumentException)
                        {
                            // ignore }
                        }
                    }
                }
            }

            string[] fileArray = new string[files.Count];
            files.Values.CopyTo(fileArray, 0);
            return fileArray;
        }

        public struct PathInfo
        {
            public string Name;
            public BetterBarn Barn;
        }

        private static List<PathInfo> _searchPath = new List<PathInfo>();
    }
}
