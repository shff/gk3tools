using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SheepVMDotNet
{
    enum SheepTreeNodeType
    {
        SymbolsSection,
        CodeSection,

        IntDeclaration,
        FloatDeclaration,
        StringDeclaration,

        FunctionDeclaration,

        LocalReference,
        Assignment,

        IntegerLiteral,
        FloatLiteral,
        StringLiteral
    }

    struct SheepTreeNode
    {
        public int ParentIndex;
        public SheepTreeNodeType Type;
        
        public int FirstChildIndex;
        public int NumChildren;

        public int ExtrasIndex;
    }

    struct SheepTreeNodeExtras
    {
        public int IntegerData;
        public float FloatData;
        public BetterString Name;
        public SheepScannerPosition FunctionStart;
    }

    class SheepCompilerException : Exception
    {
    }

    class BetterList<T>
    {
        private const int INITIAL_SIZE = 8;
        private T[] _ts;
        private int _nextFree = 0;

        public void Add(T t)
        {
            if (_ts == null)
                _ts = new T[INITIAL_SIZE];
            else if (_nextFree >= _ts.Length)
                resize();

            _ts[_nextFree++] = t;
        }

        public T[] InternalArray
        {
            get { return _ts; }
        }

        public int Count
        {
            get { return _nextFree; }
        }

        private void resize()
        {
            T[] newArray = new T[_ts.Length * 2];
            for (int i = 0; i < _ts.Length; i++)
                newArray[i] = _ts[i];

            _ts = newArray;
        }
    }

    class SheepCodeTree
    {
        private SheepScanner _scanner = new SheepScanner();
        private BetterList<SheepTreeNode> _codeTree;
        private BetterList<SheepTreeNodeExtras> _extras;
        private Dictionary<string, int> _stringConstants;

        public SheepCodeTree(string code)
        {
            _stringConstants = new Dictionary<string, int>();
            _codeTree = new BetterList<SheepTreeNode>();
            _extras = new BetterList<SheepTreeNodeExtras>();
            _scanner = new SheepScanner();
            _scanner.Begin(code);

            parseRoot(_codeTree);

            // now go back and parse the function contents
            for (int i = 0; i < _codeTree.Count; i++)
            {
                if (_codeTree.InternalArray[i].Type == SheepTreeNodeType.FunctionDeclaration)
                {
                    SheepTreeNodeExtras extras = _extras.InternalArray[_codeTree.InternalArray[i].ExtrasIndex];
                    _scanner.Seek(extras.FunctionStart);

                    parseFunctionContents(i);
                }
            }
        }

        public void Print()
        {
            for (int i = 0; i < _codeTree.InternalArray.Length; i++)
            {
                if (_codeTree.InternalArray[i].ParentIndex == -1)
                    printNodes(0, i);
            }
        }

        private void printNodes(int indention, int index)
        {
            SheepTreeNode node = _codeTree.InternalArray[index];

            for (int i = 0; i < indention; i++)
                Console.Write('\t');

            if (node.Type == SheepTreeNodeType.IntDeclaration ||
                node.Type == SheepTreeNodeType.FloatDeclaration ||
                node.Type == SheepTreeNodeType.StringDeclaration)
            {
                SheepTreeNodeExtras extras = _extras.InternalArray[node.ExtrasIndex];

                Console.Write(node.Type);
                Console.Write(": ");
                Console.Write(extras.Name.ToString());
                Console.Write(" = ");

                if (node.Type == SheepTreeNodeType.FloatDeclaration)
                    Console.WriteLine(extras.FloatData);
                else
                    Console.WriteLine(extras.IntegerData);
            }
            else if (node.Type == SheepTreeNodeType.FunctionDeclaration)
            {
                SheepTreeNodeExtras extras = _extras.InternalArray[node.ExtrasIndex];

                Console.Write(node.Type);
                Console.Write(": ");
                Console.WriteLine(extras.Name.ToString());
            }
            else
            {
                Console.WriteLine(node.Type);
            }

            for (int i = node.FirstChildIndex; i < node.FirstChildIndex + node.NumChildren; i++)
                printNodes(indention + 1, i);
        }

        private void parseRoot(BetterList<SheepTreeNode> tree)
        {
            int currentIndex = -1;

            ScannedToken token = _scanner.GetNextToken();
            while (token.Type != SheepTokenType.None)
            {
                if (token.Type == SheepTokenType.Symbols)
                    parseSymbolsSection(currentIndex, tree);
                else if (token.Type == SheepTokenType.Code)
                    parseCodeSection(currentIndex, tree);
                else
                    throw new SheepCompilerException();

                token = _scanner.GetNextToken();
            }
        }

        private void parseSymbolsSection(int parentIndex, BetterList<SheepTreeNode> tree)
        {
            int currentIndex = getNewNodeIndex(tree);
            tree.InternalArray[currentIndex].ParentIndex = parentIndex;
            tree.InternalArray[currentIndex].FirstChildIndex = currentIndex + 1;

            if (_scanner.GetNextToken().Type != SheepTokenType.LBrace)
                throw new SheepCompilerException();

            ScannedToken t = _scanner.GetNextToken();
            while (t.Type != SheepTokenType.RBrace)
            {
                if (t.Type == SheepTokenType.Int)
                    parseSymbolDeclaration(SymbolType.Int, currentIndex, tree);
                else if (t.Type == SheepTokenType.Float)
                    parseSymbolDeclaration(SymbolType.Float, currentIndex, tree);
                else if (t.Type == SheepTokenType.String)
                    parseSymbolDeclaration(SymbolType.String, currentIndex, tree);
                else
                    throw new SheepCompilerException();

                tree.InternalArray[currentIndex].NumChildren++;

                t = _scanner.GetNextToken();
            }
        }

        private enum SymbolType { Int, Float, String };
        private void parseSymbolDeclaration(SymbolType type, int parentIndex, BetterList<SheepTreeNode> tree)
        {
            ScannedToken t = _scanner.GetNextToken();
            if (t.Type != SheepTokenType.LocalIdentifier)
                throw new SheepCompilerException();

            int idNode = getNewNodeIndex(tree);
            int extras = getNewNodeIndex(_extras);
            tree.InternalArray[idNode].ParentIndex = parentIndex;
            tree.InternalArray[idNode].ExtrasIndex = extras;
            _extras.InternalArray[extras].Name = t.Text;
            if (type == SymbolType.Int)
                tree.InternalArray[idNode].Type = SheepTreeNodeType.IntDeclaration;
            else if (type == SymbolType.Float)
                tree.InternalArray[idNode].Type = SheepTreeNodeType.FloatDeclaration;
            else if (type == SymbolType.String)
                tree.InternalArray[idNode].Type = SheepTreeNodeType.StringDeclaration;

            ScannedToken next = _scanner.GetNextToken();
            if (next.Type == SheepTokenType.Equal)
            {
                next = _scanner.GetNextToken();
                if (type == SymbolType.Int)
                    _extras.InternalArray[extras].IntegerData = getIntegerFromLiteralToken(next.Type, next.Text);
                else if (type == SymbolType.Float)
                    _extras.InternalArray[extras].FloatData = getFloatFromLiteralToken(next.Type, next.Text);
                else if (type == SymbolType.String)
                    _extras.InternalArray[extras].IntegerData = lookupStringConstant(next.Text, true);


                if (_scanner.GetNextToken().Type != SheepTokenType.Semicolon)
                    throw new SheepCompilerException();
            }
            else if (next.Type != SheepTokenType.Semicolon)
                throw new SheepCompilerException();
        }

        private void parseCodeSection(int parentIndex, BetterList<SheepTreeNode> tree)
        {
            int currentIndex = getNewNodeIndex(tree);
            tree.InternalArray[currentIndex].Type = SheepTreeNodeType.CodeSection;
            tree.InternalArray[currentIndex].ParentIndex = parentIndex;
            tree.InternalArray[currentIndex].FirstChildIndex = currentIndex + 1;

            if (_scanner.GetNextToken().Type != SheepTokenType.LBrace)
                throw new SheepCompilerException();

            ScannedToken t = _scanner.GetNextToken();
            while (t.Type != SheepTokenType.RBrace)
            {
                if (t.Type == SheepTokenType.LocalIdentifier)
                {
                    if (_scanner.GetNextToken().Type != SheepTokenType.LParen ||
                        _scanner.GetNextToken().Type != SheepTokenType.RParen)
                        throw new SheepCompilerException();
                    if (_scanner.GetNextToken().Type != SheepTokenType.LBrace)
                        throw new SheepCompilerException();

                    int functionIndex = getNewNodeIndex(tree);
                    int extrasIndex = getNewNodeIndex(_extras);
                    tree.InternalArray[functionIndex].ParentIndex = currentIndex;
                    tree.InternalArray[functionIndex].Type = SheepTreeNodeType.FunctionDeclaration;
                    tree.InternalArray[functionIndex].ExtrasIndex = extrasIndex;
                    _extras.InternalArray[extrasIndex].Name = t.Text;
                    _extras.InternalArray[extrasIndex].FunctionStart = _scanner.CurrentPosition;

                    tree.InternalArray[currentIndex].NumChildren++;

                    // skip to the end of the function
                    int lbraceCount = 0;
                    while(true)
                    {
                        t = _scanner.GetNextToken();
                        if (t.Type == SheepTokenType.LBrace)
                            lbraceCount++;
                        else if (t.Type == SheepTokenType.RBrace)
                        {
                            lbraceCount--;
                            if (lbraceCount < 0)
                                break;
                        }
                    }
                }
                else
                    throw new SheepCompilerException();

                t = _scanner.GetNextToken();
            }
        }

        private void parseFunctionContents(int parentIndex)
        {
            ScannedToken token = _scanner.GetNextToken();
            while (token.Type != SheepTokenType.None)
            {
                if (token.Type == SheepTokenType.RBrace)
                    break; // done with this function

                // TODO

                token = _scanner.GetNextToken();
            }
        }

        private int getIntegerFromLiteralToken(SheepTokenType type, BetterString text)
        {
            if (type == SheepTokenType.LiteralInteger)
            {
                int data;
                if (text.TryParseInt(out data))
                    return data;
            }
            else if (type == SheepTokenType.LiteralFloat)
            {
                float data;
                if (text.TryParseFloat(out data))
                    return (int)data;
            }

            throw new SheepCompilerException();
        }

        private float getFloatFromLiteralToken(SheepTokenType type, BetterString text)
        {
            if (type == SheepTokenType.LiteralFloat)
            {
                float data;
                if (text.TryParseFloat(out data))
                    return data;
            }
            else if (type == SheepTokenType.LiteralInteger)
            {
                int data;
                if (text.TryParseInt(out data))
                    return (float)data;
            }

            throw new SheepCompilerException();
        }

        private int lookupStringConstant(BetterString str, bool addIfNotFound)
        {
            string s = str.ToString();

            int index;
            if (_stringConstants.TryGetValue(s, out index))
                return index;

            if (addIfNotFound)
            {
                index = _stringConstants.Count;
                _stringConstants.Add(s, index);
                return index;
            }
            else
            {
                return -1;
            }
        }

        private int getNewNodeIndex(BetterList<SheepTreeNode> tree)
        {
            SheepTreeNode node = new SheepTreeNode();
            node.FirstChildIndex = -1;
            node.NumChildren = 0;

            tree.Add(node);
            return tree.Count - 1;
        }

        private int getNewNodeIndex(BetterList<SheepTreeNodeExtras> extras)
        {
            SheepTreeNodeExtras extra = new SheepTreeNodeExtras();

            extras.Add(extra);
            return extras.Count - 1;
        }
    }
}
