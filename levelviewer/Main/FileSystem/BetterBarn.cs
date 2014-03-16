using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main
{
    public class BetterBarn : IDisposable
    {
        private string _path;
        private BarnLib.Barn _root;

        private struct ChildBarn
        {
            public bool OwnedByManager;
            public BarnLib.Barn Barn;
        }

        private List<ChildBarn> _children = new List<ChildBarn>();

        public BetterBarn(BarnLib.Barn barn, string absolutePathOfBarn)
        {
            _root = barn;
            _path = absolutePathOfBarn;
        }

        public void Dispose()
        {
            foreach (var barn in _children)
            {
                if (barn.OwnedByManager)
                    barn.Barn.Dispose();
            }
        }

        public BarnLib.Barn InternalBarn { get { return _root; } }

        public System.IO.Stream ReadFile(string file)
        {
            // attempt to load from the "root" barn
            int index = _root.GetFileIndex(file);
            if (index < 0)
                return null;

            string barn = _root.GetBarnName(index);

            if (barn == string.Empty)
            {
                // read!
                return new System.IO.MemoryStream(_root.ReadFile(index, true));
            }
            else
            {
                // it's in a child barn. Go get it.
                BarnLib.Barn child = FindOrAddChildBarn(barn);

                index = child.GetFileIndex(file);
                if (index < 0)
                    return null;

                return new System.IO.MemoryStream(child.ReadFile(index, true));
            }
        }

        private BarnLib.Barn FindOrAddChildBarn(string name)
        {
            foreach (var child in _children)
            {
                if (child.Barn.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return child.Barn;
            }

            BarnLib.Barn barn = FileSystem.FindBarn(name);

            if (barn != null)
            {
                ChildBarn child;
                child.OwnedByManager = false;
                child.Barn = barn;

                _children.Add(child);
                return barn;
            }

            // guess we need to open the barn ourselves
            string path = System.IO.Path.GetDirectoryName(_root.Path) + "/" + name;

            barn = new BarnLib.Barn(path);
            ChildBarn child2;
            child2.OwnedByManager = true;
            child2.Barn = barn;
            _children.Add(child2);

            return barn;
        }
    }
}
