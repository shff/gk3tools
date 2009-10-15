using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SheepVMDotNet
{
    public enum SheepSymbolType
    {
        Void = 0,
        Int,
        Float,
        String,
        LocalFunction,
        Import,
        Label
    }

    public struct SheepSymbol
    {
        public string Name;
        public SheepSymbolType Type;

        public int InitialIntValue;
        public float InitialFloatValue;
        public int InitialStringValue;
    }

    public struct SheepFunction
    {
        public string Name;
        public byte[] Code;
        public uint CodeOffset;

        public List<string> Imports;
    }

    public delegate void ImportCallback(SheepMachine vm);

    public struct SheepImport
    {
        public string Name;
        public SheepSymbolType ReturnType;
        public SheepSymbolType[] Parameters;

        public ImportCallback Callback;
    }

    public struct SheepStringConstant
    {
        public string Value;
        public uint Offset;
    }
}
