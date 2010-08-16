using System;
using System.Collections.Generic;
using System.Text;

namespace Viewer
{
    class ProgramArguments
    {
        private List<string> _searchPaths = new List<string>();
        private List<string> _searchBarns = new List<string>();
        private List<string> _modelsToLoad = new List<string>();
        private string _bspToLoad;
        private string _sifToLoad;
        private string _scnToLoad;

        public ProgramArguments(string[] args)
        {
            int currentIndex = 0;
            while (currentIndex < args.Length)
            {
                currentIndex = parseArg(args, currentIndex);
            }
        }

        public List<string> SearchPaths
        {
            get { return _searchPaths; }
        }

        public List<string> SearchBarns
        {
            get { return _searchBarns; }
        }

        public List<string> ModelsToLoad
        {
            get { return _modelsToLoad; }
        }

        private int parseArg(string[] args, int currentIndex)
        {
            if (args[currentIndex] == "-b")
            {
                requireFollowingParameter(args, currentIndex, "-b requires a following barn name");

                _searchBarns.Add(args[currentIndex + 1]);
                currentIndex += 2;
            }
            else if (args[currentIndex] == "-p")
            {
                requireFollowingParameter(args, currentIndex, "-s requires a following path name");

                _searchPaths.Add(args[currentIndex + 1]);
                currentIndex += 2;
            }
            else if (args[currentIndex] == "-mod")
            {
                requireFollowingParameter(args, currentIndex, "-mod requires a model name");

                _modelsToLoad.Add(args[currentIndex + 1]);
                currentIndex += 2;
            }

            return currentIndex;
        }

        private void requireFollowingParameter(string[] args, int currentIndex, string errorMsg)
        {
            if (currentIndex + 1 >= args.Length)
                throw new InvalidOperationException(errorMsg);
        }
    }
}
