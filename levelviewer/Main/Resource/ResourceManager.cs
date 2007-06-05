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

        Resource Load(string filename);
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

        public bool Loaded { get { return _loaded; } }

        protected bool _loaded;
        private int _referenceCount = 0;
        private string _name;
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

    public class ResourceManager
    {
        public static Resource Load(string filename)
        {
            Resource resource;
            if (_resources.TryGetValue(filename, out resource) == false)
            {
                // find the resource loader for this extension
                IResourceLoader loader = getLoaderForFile(filename);

                if (loader == null)
                    throw new CannotFindResourceLoaderException(
                        "Cannot find loader for " + filename);

                resource = loader.Load(filename);
                resource.ReferenceCount++;
                _resources.Add(filename, resource);
            }
            
            return resource;
        }

        public static void Unload(Resource resource)
        {
            resource.ReferenceCount--;

            if (resource.ReferenceCount < 1)
            {
                _resources.Remove(resource.Name);
                resource.Dispose();               
            }
        }

        public static Resource Get(string name)
        {
            Resource resource;
            if (_resources.TryGetValue(name, out resource) == false)
                return null;

            return resource;
        }


        public static void AddResourceLoader(IResourceLoader loader)
        {
            string[] supportedExtensions = loader.SupportedExtensions;

            foreach (string ext in supportedExtensions)
            {
                _loaders.Add(ext.ToUpper(), loader);
            }
        }

        private static IResourceLoader getLoaderForFile(string filename)
        {
            int dot = filename.LastIndexOf('.');
            if (dot == -1 || dot == filename.Length-1) return null;

            string extension = filename.Substring(dot + 1).ToUpper();

            IResourceLoader loader;
            if (_loaders.TryGetValue(extension, out loader) == true)
                return loader;

            return null;
        }

        private static Dictionary<string, IResourceLoader> _loaders = new Dictionary<string,IResourceLoader>();
        private static Dictionary<string, Resource> _resources = new Dictionary<string, Resource>();
    }
}
