using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SheepVMDotNet
{
    enum SheepTokenType2
    {
        Unknown,

        LParen,
        RParen,
        Not,
        And,
        Or,
        Equals,
        GreaterThan,
        LessThan,
        GreaterOrEqualThan,
        LessOrEqualThan,
        NotEqual,
        LocalIdentifier,
        GlobalIdentifier,
        Number,
        String,
        Comma,
        Quote,
        Times,
        Divide,
        Add,
        Subtract
    }

    struct SheepToken2
    {
        public SheepToken2(SheepTokenType type, string text)
        {
            Type = type;
            Text = text;
        }

        public SheepTokenType Type;
        public string Text;
    }

    public class SheepCompiler
    {
        private SheepCodeTree _tree;

        public SheepCompiler(string code)
        {
            _tree = new SheepCodeTree(code);
        }

        public void Print()
        {
            _tree.Print();
        }
    }
}
