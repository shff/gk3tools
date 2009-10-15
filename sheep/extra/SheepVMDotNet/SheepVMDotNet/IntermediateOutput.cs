using System;
using System.Collections.Generic;

namespace SheepVMDotNet
{
    public class IntermediateOutput
    {
        private List<SheepSymbol> _symbols = new List<SheepSymbol>();
        private List<SheepStringConstant> _constants = new List<SheepStringConstant>();
        private List<SheepImport> _imports = new List<SheepImport>();
        private List<SheepFunction> _functions = new List<SheepFunction>();

        public List<SheepSymbol> Symbols
        {
            get { return _symbols; }
        }

        public List<SheepStringConstant> Constants
        {
            get { return _constants; }
        }

        public List<SheepImport> Imports
        {
            get { return _imports; }
        }

        public List<SheepFunction> Functions
        {
            get { return _functions; }
        }
    }
}
