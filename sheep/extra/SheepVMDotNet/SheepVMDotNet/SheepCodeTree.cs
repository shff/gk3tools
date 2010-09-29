using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SheepVMDotNet
{
    enum SheepTreeNodeType
    {
        Root,
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

    class SheepCompilerException : Exception
    {
    }

    class SheepCodeTreeNode2
    {
        public SheepCodeTree2 Tree;
        public SheepTreeNodeType Type;

        public int IntegerData;
        public float FloatData;
        public BetterString Name;
        public SheepScannerPosition FunctionStart;

        public SheepCodeTreeNode2 Parent;
        public SheepCodeTreeNode2 Sibling;
        public SheepCodeTreeNode2 FirstChild;
    }

    class SheepCodeTree2
    {
        private SheepCodeTreeNode2 _root;
        private SheepCodeTreeNode2 _codeSection;
        private Stack<SheepCodeTreeNode2> _pool;

        public SheepCodeTree2()
        {
            _pool = new Stack<SheepCodeTreeNode2>();
            _root = new SheepCodeTreeNode2();
            _root.Type = SheepTreeNodeType.Root;
        }

        public SheepCodeTreeNode2 CreateNode()
        {
            return getNewNodeFromPool();
        }

        public SheepCodeTreeNode2 AddAsChild(SheepCodeTreeNode2 parent)
        {
            SheepCodeTreeNode2 node = getNewNodeFromPool();

            AddAsChild(parent, node);

            return node;
        }

        public void AddAsChild(SheepCodeTreeNode2 parent, SheepCodeTreeNode2 child)
        {
            child.Parent = parent;
            child.Tree = this;

            if (parent.FirstChild == null)
                parent.FirstChild = child;
            else
            {
                SheepCodeTreeNode2 c = parent.FirstChild;
                while (c.Sibling != null)
                {
                    c = c.Sibling;
                }

                c.Sibling = child;
            }
        }

        public SheepCodeTreeNode2 Root
        {
            get { return _root; }
        }

        public SheepCodeTreeNode2 FirstFunction
        {
            get { return _codeSection.FirstChild; }
        }

        internal void SetCodeSectionNode(SheepCodeTreeNode2 code)
        {
            _codeSection = code;
        }

        private SheepCodeTreeNode2 getNewNodeFromPool()
        {
            if (_pool.Count > 0)
                return _pool.Pop();

            // guess the pool was empty
            return new SheepCodeTreeNode2();
        }
    }

    class SheepCodeTree
    {
        private SheepScanner _scanner = new SheepScanner();
        //private BetterList<SheepTreeNode> _codeTree;
        //private BetterList<SheepTreeNodeExtras> _extras;
        private SheepCodeTree2 _tree;
        private Dictionary<string, int> _stringConstants;


        public SheepCodeTree(string code)
        {
            _stringConstants = new Dictionary<string, int>();
            //_codeTree = new BetterList<SheepTreeNode>();
            //_extras = new BetterList<SheepTreeNodeExtras>();
            _scanner = new SheepScanner();
            _scanner.Begin(code);

            _tree = new SheepCodeTree2();
            parseRoot();

            // now go back and parse the function contents
            SheepCodeTreeNode2 func = _tree.FirstFunction;
            while(func != null)
            {
                _scanner.Seek(func.FunctionStart);

                parseFunctionContents(func);

                func = func.Sibling;
            }
        }

        public void Print()
        {
            printNodes(0, _tree.Root);
        }

        private void printNodes(int indention, SheepCodeTreeNode2 node)
        {
            for (int i = 0; i < indention; i++)
                Console.Write('\t');

            if (node.Type == SheepTreeNodeType.IntDeclaration ||
                node.Type == SheepTreeNodeType.FloatDeclaration ||
                node.Type == SheepTreeNodeType.StringDeclaration)
            {
                Console.Write(node.Type);
                Console.Write(": ");
                Console.Write(node.Name.ToString());
                Console.Write(" = ");

                if (node.Type == SheepTreeNodeType.FloatDeclaration)
                    Console.WriteLine(node.FloatData);
                else
                    Console.WriteLine(node.IntegerData);
            }
            else if (node.Type == SheepTreeNodeType.FunctionDeclaration)
            {
                Console.Write(node.Type);
                Console.Write(": ");
                Console.WriteLine(node.Name.ToString());
            }
            else if (node.Type == SheepTreeNodeType.Assignment)
            {
                Console.Write(node.Type);
                Console.WriteLine(" (" + node.Name.ToString() + ")");
            }
            else if (node.Type == SheepTreeNodeType.IntegerLiteral)
            {
                Console.Write(node.Type);
                Console.WriteLine(" (" + node.IntegerData + ")");
            }
            else
            {
                Console.WriteLine(node.Type);
            }

            SheepCodeTreeNode2 child = node.FirstChild;
            while (child != null)
            {
                printNodes(indention + 1, child);

                child = child.Sibling;
            }
        }

        private void parseRoot()
        {
            ScannedToken token = _scanner.GetNextToken();
            while (token.Type != SheepTokenType.None)
            {
                if (token.Type == SheepTokenType.Symbols)
                    parseSymbolsSection(_tree.Root);
                else if (token.Type == SheepTokenType.Code)
                    parseCodeSection(_tree.Root);
                else
                    throw new SheepCompilerException();

                token = _scanner.GetNextToken();
            }
        }

        private void parseSymbolsSection(SheepCodeTreeNode2 node)
        {
            SheepCodeTreeNode2 symbols = _tree.AddAsChild(node);
            symbols.Type = SheepTreeNodeType.SymbolsSection;

            if (_scanner.GetNextToken().Type != SheepTokenType.LBrace)
                throw new SheepCompilerException();

            ScannedToken t = _scanner.GetNextToken();
            while (t.Type != SheepTokenType.RBrace)
            {
                if (t.Type == SheepTokenType.Int)
                    parseSymbolDeclaration(SymbolType.Int, symbols);
                else if (t.Type == SheepTokenType.Float)
                    parseSymbolDeclaration(SymbolType.Float, symbols);
                else if (t.Type == SheepTokenType.String)
                    parseSymbolDeclaration(SymbolType.String, symbols);
                else
                    throw new SheepCompilerException();

                t = _scanner.GetNextToken();
            }
        }

        private enum SymbolType { Int, Float, String };
        private void parseSymbolDeclaration(SymbolType type, SheepCodeTreeNode2 node)
        {
            ScannedToken t = _scanner.GetNextToken();
            if (t.Type != SheepTokenType.LocalIdentifier)
                throw new SheepCompilerException();

            SheepCodeTreeNode2 symbol = _tree.AddAsChild(node);
            symbol.Name = t.Text;
            if (type == SymbolType.Int)
                symbol.Type = SheepTreeNodeType.IntDeclaration;
            else if (type == SymbolType.Float)
                symbol.Type = SheepTreeNodeType.FloatDeclaration;
            else if (type == SymbolType.String)
                symbol.Type = SheepTreeNodeType.StringDeclaration;

            ScannedToken next = _scanner.GetNextToken();
            if (next.Type == SheepTokenType.Equal)
            {
                next = _scanner.GetNextToken();
                if (type == SymbolType.Int)
                    symbol.IntegerData = getIntegerFromLiteralToken(next.Type, next.Text);
                else if (type == SymbolType.Float)
                    symbol.FloatData = getFloatFromLiteralToken(next.Type, next.Text);
                else if (type == SymbolType.String)
                    symbol.IntegerData = lookupStringConstant(next.Text, true);

                if (_scanner.GetNextToken().Type != SheepTokenType.Semicolon)
                    throw new SheepCompilerException();
            }
            else if (next.Type != SheepTokenType.Semicolon)
                throw new SheepCompilerException();
        }

        private void parseCodeSection(SheepCodeTreeNode2 node)
        {
            SheepCodeTreeNode2 code = _tree.AddAsChild(node);
            code.Type = SheepTreeNodeType.CodeSection;
            _tree.SetCodeSectionNode(code);

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

                    SheepCodeTreeNode2 function = _tree.AddAsChild(code);
                    function.Type = SheepTreeNodeType.FunctionDeclaration;
                    function.Name = t.Text;
                    function.FunctionStart = _scanner.CurrentPosition;

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

        private void parseFunctionContents(SheepCodeTreeNode2 node)
        {
            parseStatementList(node);
        }

        private void parseStatementList(SheepCodeTreeNode2 node)
        {
            ScannedToken token = _scanner.GetNextToken();
            while (token.Type != SheepTokenType.None)
            {
                if (token.Type == SheepTokenType.RBrace)
                    break; // done with this function

                SheepScannerPosition currentPos = _scanner.CurrentPosition;
                parseClosedStatement(token, node);

                token = _scanner.GetNextToken();
            }
        }

        private void parseClosedStatement(ScannedToken token, SheepCodeTreeNode2 node)
        {
            if (token.Type == SheepTokenType.Semicolon)
            {
                // empty statement, do nothing
            }
            else if (token.Type == SheepTokenType.LocalIdentifier)
            {
                BetterString id = token.Text;
                token = _scanner.GetNextToken();
                if (token.Type == SheepTokenType.Equal)
                {
                    SheepCodeTreeNode2 assignment = _tree.AddAsChild(node);
                    assignment.Type = SheepTreeNodeType.Assignment;
                    assignment.Name = id;

                    SheepCodeTreeNode2 expr = parseExpression();
                    _tree.AddAsChild(assignment, expr);
                }
                else
                {
                    throw new SheepCompilerException();
                }
            }
            else if (token.Type == SheepTokenType.Return)
            {
                // return statement
                if (_scanner.GetNextToken().Type != SheepTokenType.Semicolon)
                    throw new SheepCompilerException();
            }

            // the only thing left to try is an expression
            parseExpression();
        }

        private SheepCodeTreeNode2 parseExpression()
        {
            ScannedToken token = _scanner.GetNextToken();

            if (token.Type == SheepTokenType.LiteralInteger ||
                token.Type == SheepTokenType.LiteralFloat ||
                token.Type == SheepTokenType.LiteralString)
            {
                return parseConstantExpression(token);
            }
            else if (token.Type == SheepTokenType.Identifier)
            {
                if (_scanner.GetNextToken().Type != SheepTokenType.LParen)
                    throw new SheepCompilerException();

                //parseParameterList();
            }
            else if (token.Type == SheepTokenType.LocalIdentifier)
            {
                // TODO
            }
            else if (token.Type == SheepTokenType.LParen)
            {
                // TODO
            }
            else if (token.Type == SheepTokenType.Bang)
            {
                // TODO
            }
            else
            {
                //tryParseExpression(0);

                token = _scanner.GetNextToken();

                if (token.Type == SheepTokenType.Plus)
                {
                }
                else if (token.Type == SheepTokenType.Minus)
                {
                }
            }

            return null;
        }

        private SheepCodeTreeNode2 parseConstantExpression(ScannedToken token)
        {
            SheepCodeTreeNode2 constant = _tree.CreateNode();

            if (token.Type == SheepTokenType.LiteralInteger)
            {
                constant.Type = SheepTreeNodeType.IntegerLiteral;
                constant.IntegerData = token.LiteralIntValue;
            }
            else if (token.Type == SheepTokenType.LiteralFloat)
            {
                constant.Type = SheepTreeNodeType.FloatLiteral;
                constant.FloatData = token.LiteralFloatValue;
            }
            else if (token.Type == SheepTokenType.LiteralString)
            {
                constant.Type = SheepTreeNodeType.StringLiteral;
                constant.IntegerData = lookupStringConstant(token.Text, true);
            }

            return constant;
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
    }
}
