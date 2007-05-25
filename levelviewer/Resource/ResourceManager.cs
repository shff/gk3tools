using System;
using System.Collections.Generic;
using System.Text;

namespace gk3levelviewer.Resource
{
    interface IResourceLoader
    {
        string[] SupportedExtensions { get; }

        Resource Load(string filename);
    }

    abstract class Resource : IDisposable
    {
        public Resource(string name)
        {
            _name = name;
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

        private int _referenceCount = 0;
        private string _name;
    }

    class CannotFindResourceLoaderException : Exception
    {
        public CannotFindResourceLoaderException(string message)
            : base(message) { }
    }

    class InvalidResourceFileFormat : Exception
    {
        public InvalidResourceFileFormat(string message)
            : base(message) { }
    }

    class ResourceManager
    {
        public static Resource Load(string filename)
        {
            Console.WriteLine("Loading " + filename);

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
