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

namespace Gk3Main.Resource
{
    public interface IResourceLoader
    {
        string[] SupportedExtensions { get; }
        bool EmptyResourceIfNotFound { get; }

        Resource Load(string filename, ResourceManager content);
    }

    /// <summary>
    /// Resource is the base class for all resources, like textures, text files, etc.
    /// </summary>
    /// <remarks>Resources can be either "loaded" or "unloaded." A loaded resource
    /// has valid data, an unloaded resource doesn't. Only certain Resources are
    /// allowed to be in an unloaded state.</remarks>
    public abstract class Resource : IDisposable
    {
        /// <summary>
        /// Base constructor for a Resource
        /// </summary>
        /// <param name="name">The name of the resource</param>
        /// <param name="loaded">Whether or not the resource is considered "loaded"</param>
        public Resource(string name, bool loaded)
        {
            _name = name;
            _loaded = loaded;

            _nameWithoutExtension = Utils.GetFilenameWithoutExtension(name);
        }

        public abstract void Dispose();

        /// <summary>
        /// Gets the reference count.
        /// </summary>
        public int ReferenceCount
        {
            get { return _referenceCount; }
            set { _referenceCount = value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string NameWithoutExtension
        {
            get { return _nameWithoutExtension; }
        }

        public bool Loaded { get { return _loaded; } }

        protected bool _loaded;
        private int _referenceCount = 0;
        private string _name;
        private string _nameWithoutExtension;
    }

    public class CannotFindResourceLoaderException : Exception
    {
        public CannotFindResourceLoaderException(string message)
            : base(message) { }
    }

    public class InvalidResourceFileFormat : Exception
    {
        public InvalidResourceFileFormat(string message)
            : base(message) { }
    }

    public class ResourceManager : IDisposable
    {

        #region Instance members

        public void Dispose()
        {
            UnloadAll();
        }

        public T Load<T>(string filename) where T : Resource
        {
            return Load<T>(filename, false);
        }

        public T Load<T>(string filename, bool keepExtension) where T : Resource
        {
            Type t = typeof(T);

            // strip off the extension if it exists
            string filenameWithoutExtension;
            if (keepExtension == false)
                filenameWithoutExtension = Utils.GetFilenameWithoutExtension(filename);
            else
                filenameWithoutExtension = filename;

            // is the resource already loaded?
            Dictionary<Type, Resource> rs;
            Resource r;
            if (_loadedContent.TryGetValue(filenameWithoutExtension, out rs))
                if (rs.TryGetValue(t, out r))
                    return (T)r;

            // we need to load it
            IResourceLoader loader = _loadersByType[t];
            r = loader.Load(filename, this);

            // cache the new resource.
            // even though we just checked we need to check again
            // since the resource we loaded could have loaded other
            // stuff, so the state of the resource collection is unknown at this point
            if (_loadedContent.TryGetValue(filenameWithoutExtension, out rs))
            {
                Resource dummy;
                if (rs.TryGetValue(t, out dummy))
                {
                    // nothing to do!
                }
                else
                {
                    rs.Add(t, r);
                }
            }
            else
            {
                rs = new Dictionary<Type,Resource>();
                rs.Add(t, r);
                _loadedContent.Add(filenameWithoutExtension, rs);
            }

            return (T)r;
        }

        public void UnloadAll()
        {
            foreach (Dictionary<Type, Resource> d in _loadedContent.Values)
                foreach (Resource r in d.Values)
                    r.Dispose();

            _loadedContent.Clear();
        }

        /// <summary>
        /// Returns a list of the names of loaded resources
        /// </summary>
        public IList<string> GetLoadedResourceNames()
        {
            IList<string> list = new List<string>();

            foreach (string key in _loadedContent.Keys)
            {
                list.Add(key);
            }

            return list;
        }

        public IList<T> GetLoadedResources<T>() where T : Resource
        {
            List<T> list = new List<T>();
            foreach (Dictionary<Type, Resource> d in _loadedContent.Values)
            {
                Resource r;
                if (d.TryGetValue(typeof(T), out r))
                    list.Add((T)r);
            }

            return list;
        }

        private Dictionary<string, Dictionary<Type, Resource>> _loadedContent 
            = new Dictionary<string, Dictionary<Type, Resource>>(StringComparer.OrdinalIgnoreCase);

        #endregion

        public static void AddResourceLoader(IResourceLoader loader, Type type)
        {
            string[] supportedExtensions = loader.SupportedExtensions;

            foreach (string ext in supportedExtensions)
            {
                _loaders.Add(ext.ToUpper(), loader);
            }

            _loadersByType.Add(type, loader);
        }

        private static IResourceLoader getLoaderForFile(string filename)
        {
            int dot = filename.LastIndexOf('.');
            if (dot == -1 || dot == filename.Length-1) return null;

            string extension = filename.Substring(dot + 1).ToUpper();

            return getLoaderForExtension(extension);
        }

        private static IResourceLoader getLoaderForExtension(string extension)
        {
             IResourceLoader loader;
            if (_loaders.TryGetValue(extension, out loader) == true)
                return loader;

            return null;
        }

        private static Dictionary<string, IResourceLoader> _loaders = new Dictionary<string,IResourceLoader>();
        private static Dictionary<Type, IResourceLoader> _loadersByType = new Dictionary<Type, IResourceLoader>();
        //private static Dictionary<string, Resource> _resources = new Dictionary<string, Resource>();
    }
}
