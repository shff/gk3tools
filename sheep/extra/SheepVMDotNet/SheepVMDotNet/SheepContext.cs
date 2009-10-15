using System;
using System.Collections.Generic;

namespace SheepVMDotNet
{
    struct StackItem
    {
        public StackItem(SheepSymbolType type, int value)
        {
            IValue = value;
            Type = type;
            FValue = 0;
        }

        public StackItem(SheepSymbolType type, float value)
        {
            FValue = value;
            Type = type;
            IValue = 0;
        }

        public int IValue;
        public float FValue;
        public SheepSymbolType Type;
    }

    class SheepContext
    {
        public Stack<StackItem> Stack = new Stack<StackItem>();
        public List<StackItem> Variables = new List<StackItem>();

        public bool InWaitSection;
        public bool Suspended;
        public uint FunctionOffset;
        public uint InstructionOffset;
        public SheepCodeBuffer CodeBuffer;
        public IntermediateOutput FullCode;
    }
}
