using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SheepVMDotNet
{
    public class SheepMachine
    {
        private Stack<SheepContext> _context = new Stack<SheepContext>();

        void SetOutputCallback()
        {
            throw new NotImplementedException();
        }

        void SetCompileOutputCallback()
        {
            throw new NotImplementedException();
        }

        public void Run(IntermediateOutput code, string function)
        {
            SheepFunction? sheepFunction = null;
            foreach(var f in code.Functions)
            {
                if (string.Equals(f.Name, function, StringComparison.OrdinalIgnoreCase))
                {
                    sheepFunction = f;
                    break;
                }
            }

            if (sheepFunction.HasValue == false)
                throw new Exception("Unknown function");

            SheepContext c = new SheepContext();
            c.FullCode = code;
            c.CodeBuffer = new SheepCodeBuffer(new System.IO.MemoryStream(sheepFunction.Value.Code));
            c.FunctionOffset = sheepFunction.Value.CodeOffset;
            c.InstructionOffset = 0;

            prepareVariables(c);
            _context.Push(c);

            execute(c);

            _context.Pop();
        }

        int RunSnippet(string snippet, out int result)
        {
            throw new NotImplementedException();
        }

        void Resume()
        {
            throw new NotImplementedException();
        }

        void Suspend()
        {
            throw new NotImplementedException();
        }

        float PopFloatFromStack()
        {
            throw new NotImplementedException();
        }

        public string PopStringFromStack()
        {
            StackItem item = _context.Peek().Stack.Pop();

            if (item.Type != SheepSymbolType.String)
                throw new Exception("Expected string on stack");

            IntermediateOutput code = _context.Peek().FullCode;
            for (int i = 0; i < code.Constants.Count; i++)
            {
                if (code.Constants[i].Offset == item.IValue)
                    return code.Constants[i].Value;
            }

            throw new Exception("Invalid string offset found on stack");
        }

        public int PopIntFromStack()
        {
            StackItem item = _context.Peek().Stack.Pop();

            if (item.Type != SheepSymbolType.Int)
                throw new Exception("Expected int on stack");

            return item.IValue;
        }

        public float PopFloatFromStack()
        {
            StackItem item = _context.Peek().Stack.Pop();

            if (item.Type != SheepSymbolType.Float)
                throw new Exception("Expected float on stack");

            return item.FValue;
        }

        public void PushIntOntoStack(int i)
        {
            throw new NotImplementedException();
        }

        #region Privates

        void prepareVariables(SheepContext context)
        {
            for (int i = 0; i < context.FullCode.Symbols.Count; i++)
            {
                if (context.FullCode.Symbols[i].Type == SheepSymbolType.Int)
                    context.Variables.Add(new StackItem(SheepSymbolType.Int, context.FullCode.Symbols[i].InitialIntValue));
                else if (context.FullCode.Symbols[i].Type == SheepSymbolType.Float)
                    context.Variables.Add(new StackItem(SheepSymbolType.Float, context.FullCode.Symbols[i].InitialIntValue));
                else if (context.FullCode.Symbols[i].Type == SheepSymbolType.String)
                    context.Variables.Add(new StackItem(SheepSymbolType.String, context.FullCode.Symbols[i].InitialStringValue));
                else
                    throw new Exception("Unsupported variable type");
            }
        }

        void execute(SheepContext context)
        {
            context.CodeBuffer.BaseStream.Seek(context.InstructionOffset, System.IO.SeekOrigin.Begin);

            while (!context.Suspended && context.CodeBuffer.BaseStream.Position < context.CodeBuffer.BaseStream.Length)
            {
                if (context.InstructionOffset != context.CodeBuffer.BaseStream.Position)
                    context.CodeBuffer.BaseStream.Seek(context.InstructionOffset, System.IO.SeekOrigin.Begin);

                SheepInstruction instruction = (SheepInstruction)context.CodeBuffer.ReadByte();
                context.InstructionOffset++;

                int iparam1, iparam2;
                float fparam1, fparam2;

                switch (instruction)
                {
                    case SheepInstruction.SitnSpin:
                        break;
                    case SheepInstruction.Yield:
                        throw new NotImplementedException();
                    case SheepInstruction.CallSysFunctionV:
                        context.InstructionOffset += 4;
                        callVoidFunction(this, context.Stack, context.FullCode.Imports, context.CodeBuffer.ReadInt32());
                        break;
                    case SheepInstruction.CallSysFunctionI:
                        context.InstructionOffset += 4;
                        callIntFunction(this, context.Stack, context.FullCode.Imports, context.CodeBuffer.ReadInt32());
                        break;
                    case SheepInstruction.CallSysFunctionF:
                    case SheepInstruction.CallSysFunctionS:
                        throw new NotImplementedException();
                    case SheepInstruction.Branch:
                    case SheepInstruction.BranchGoto:
                        context.InstructionOffset = context.CodeBuffer.ReadUInt32() - context.FunctionOffset;
                        break;
                    case SheepInstruction.BranchIfZero:
                        if (context.Stack.Peek().Type == SheepSymbolType.Int)
                        {
                            if (context.Stack.Peek().IValue == 0)
                                context.InstructionOffset = context.CodeBuffer.ReadUInt32() - context.FunctionOffset;
                            else
                            {
                                context.CodeBuffer.ReadInt32(); // throw it away
                                context.InstructionOffset += 4;
                            }
                            context.Stack.Pop();
                        }
                        else
                        {
                            throw new Exception("expected integer on stack");
                        }
                        break;
                    case SheepInstruction.BeginWait:
                        context.InWaitSection = true;
                        break;
                    case SheepInstruction.EndWait:
                        context.InWaitSection = false;
                        // TODO: call end wait callback
                        break;
                    case SheepInstruction.ReturnV:
                        return;
                    case SheepInstruction.StoreI:
                        storeI(context.Stack, context.Variables, context.CodeBuffer.ReadInt32());
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.StoreF:
                        storeF(context.Stack, context.Variables, context.CodeBuffer.ReadInt32());
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.StoreS:
                        storeS(context.Stack, context.Variables, context.CodeBuffer.ReadInt32());
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.LoadI:
                        loadI(context.Stack, context.Variables, context.CodeBuffer.ReadInt32());
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.LoadF:
                        loadF(context.Stack, context.Variables, context.CodeBuffer.ReadInt32());
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.LoadS:
                        throw new NotImplementedException();
                    case SheepInstruction.PushI:
                        context.Stack.Push(new StackItem(SheepSymbolType.Int, context.CodeBuffer.ReadInt32()));
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.PushF:
                        context.Stack.Push(new StackItem(SheepSymbolType.Float, context.CodeBuffer.ReadSingle()));
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.PushS:
                        context.Stack.Push(new StackItem(SheepSymbolType.String, context.CodeBuffer.ReadInt32()));
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.Pop:
                        context.Stack.Pop();
                        break;
                    case SheepInstruction.AddI:
                        addI(context.Stack);
                        break;
                    case SheepInstruction.AddF:
                        addF(context.Stack);
                        break;
                    case SheepInstruction.SubtractI:
                        subI(context.Stack);
                        break;
                    case SheepInstruction.SubtractF:
                        subF(context.Stack);
                        break;
                    case SheepInstruction.MultiplyI:
                        mulI(context.Stack);
                        break;
                    case SheepInstruction.MultiplyF:
                        mulF(context.Stack);
                        break;
                    case SheepInstruction.DivideI:
                        divI(context.Stack);
                        break;
                    case SheepInstruction.DivideF:
                        divF(context.Stack);
                        break;
                    case SheepInstruction.NegateI:
                        negI(context.Stack);
                        break;
                    case SheepInstruction.NegateF:
                        negF(context.Stack);
                        break;
                    case SheepInstruction.IsEqualI:
                        get2Ints(context.Stack, out iparam1, out iparam2);
                        if (iparam1 == iparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IsEqualF:
                        get2Floats(context.Stack, out fparam1, out fparam2);
                        if (fparam1 == fparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.NotEqualI:
                        get2Ints(context.Stack, out iparam1, out iparam2);
                        if (iparam1 != iparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.NotEqualF:
                        get2Floats(context.Stack, out fparam1, out fparam2);
                        if (fparam1 != fparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IsGreaterI:
                        get2Ints(context.Stack, out iparam1, out iparam2);
                        if (iparam1 > iparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IsGreaterF:
                        get2Floats(context.Stack, out fparam1, out fparam2);
                        if (fparam1 > fparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IsLessI:
                        get2Ints(context.Stack, out iparam1, out iparam2);
                        if (iparam1 < iparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IsLessF:
                        get2Floats(context.Stack, out fparam1, out fparam2);
                        if (fparam1 < fparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IsGreaterEqualI:
                        get2Ints(context.Stack, out iparam1, out iparam2);
                        if (iparam1 >= iparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IsGreaterEqualF:
                        get2Floats(context.Stack, out fparam1, out fparam2);
                        if (fparam1 >= fparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IsLessEqualI:
                        get2Ints(context.Stack, out iparam1, out iparam2);
                        if (iparam1 <= iparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IsLessEqualF:
                        get2Floats(context.Stack, out fparam1, out fparam2);
                        if (fparam1 <= fparam2)
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 1));
                        else
                            context.Stack.Push(new StackItem(SheepSymbolType.Int, 0));
                        break;
                    case SheepInstruction.IToF:
                        itof(context.Stack, context.CodeBuffer.ReadInt32());
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.FToI:
                        ftoi(context.Stack, context.CodeBuffer.ReadInt32());
                        context.InstructionOffset += 4;
                        break;
                    case SheepInstruction.And:
                        andi(context.Stack);
                        break;
                    case SheepInstruction.Or:
                        ori(context.Stack);
                        break;
                    case SheepInstruction.Not:
                        noti(context.Stack);
                        break;
                    case SheepInstruction.GetString:
                        if (context.Stack.Peek().Type != SheepSymbolType.String)
                            throw new Exception("Expected string on stack");
                        break;
                    case SheepInstruction.DebugBreakpoint:
                        throw new Exception("DebugBreakpoint instruction not supported yet.");
                    default:
                        throw new Exception("Unknown instruction");
                }
            }
        }

        void executeContextsUntilSuspendedOrFinished()
        {
        }

        #region Helper statics

        private static int getInt(Stack<StackItem> stack)
        {
            StackItem item = stack.Pop();

            if (item.Type != SheepSymbolType.Int)
                throw new Exception("Expected integer on stack");

            return item.IValue;
        }

        private static float getFloat(Stack<StackItem> stack)
        {
            StackItem item = stack.Pop();

            if (item.Type != SheepSymbolType.Float)
                throw new Exception("Expected float on stack");

            return item.FValue;
        }

        private static void get2Ints(Stack<StackItem> stack, out int i1, out int i2)
        {
            StackItem item2 = stack.Pop();
            StackItem item1 = stack.Pop();

            if (item1.Type != SheepSymbolType.Int || item2.Type != SheepSymbolType.Int)
                throw new Exception("Expected integers on stack");

            i1 = item1.IValue;
            i2 = item2.IValue;
        }

        private static void get2Floats(Stack<StackItem> stack, out float f1, out float f2)
        {
            StackItem item2 = stack.Pop();
            StackItem item1 = stack.Pop();

            if (item1.Type != SheepSymbolType.Float || item2.Type != SheepSymbolType.Float)
                throw new Exception("Expected integers on stack");

            f1 = item1.FValue;
            f2 = item2.FValue;
        }

        private static void storeI(Stack<StackItem> stack, List<StackItem> variables, int variable)
        {
            int value = getInt(stack);

            if (variable >= variables.Count ||
                variables[variable].Type != SheepSymbolType.Int)
                throw new Exception("Invalid variable");

            StackItem item = variables[variable];
            item.IValue = value;

            variables[variable] = item;
        }

        private static void storeF(Stack<StackItem> stack, List<StackItem> variables, int variable)
        {
            float value = getFloat(stack);

            if (variable >= variables.Count ||
                variables[variable].Type != SheepSymbolType.Float)
                throw new Exception("Invalid variable");

            StackItem item = variables[variable];
            item.FValue = value;

            variables[variable] = item;
        }

        private static void storeS(Stack<StackItem> stack, List<StackItem> variables, int variable)
        {
            int value = getInt(stack);

            if (variable >= variables.Count ||
                variables[variable].Type != SheepSymbolType.String)
                throw new Exception("Invalid variable");

            StackItem item = variables[variable];
            item.IValue = value;

            variables[variable] = item;
        }

        private static void loadI(Stack<StackItem> stack, List<StackItem> variables, int variable)
        {
            if (variable >= variables.Count ||
                variables[variable].Type != SheepSymbolType.Int)
                throw new Exception("Invalid variable");

            stack.Push(new StackItem(SheepSymbolType.Int, variables[variable].IValue));
        }

        private static void loadF(Stack<StackItem> stack, List<StackItem> variables, int variable)
        {
            if (variable >= variables.Count ||
                variables[variable].Type != SheepSymbolType.Float)
                throw new Exception("Invalid variable");

            stack.Push(new StackItem(SheepSymbolType.Float, variables[variable].FValue));
        }

        private static void addI(Stack<StackItem> stack)
	    {
		    int i1, i2;
		    get2Ints(stack, out i1, out i2);

		    stack.Push(new StackItem(SheepSymbolType.Int, i1 + i2));
	    }

	    private static void addF(Stack<StackItem> stack)
	    {
		    float f1, f2;
		    get2Floats(stack, out f1, out f2);

		    stack.Push(new StackItem(SheepSymbolType.Float, f1 + f2));
	    }

	    private static void subI(Stack<StackItem> stack)
	    {
		    int i1, i2;
		    get2Ints(stack, out i1, out i2);

		    stack.Push(new StackItem(SheepSymbolType.Int, i1 - i2));
	    }

	    private static void subF(Stack<StackItem> stack)
	    {
		    float f1, f2;
		    get2Floats(stack, out f1, out f2);

		    stack.Push(new StackItem(SheepSymbolType.Float, f1 - f2));
	    }

	    private static void mulI(Stack<StackItem> stack)
	    {
		    int i1, i2;
		    get2Ints(stack, out i1, out i2);

		    stack.Push(new StackItem(SheepSymbolType.Int, i1 * i2));
	    }

	    private static void mulF(Stack<StackItem> stack)
	    {
		    float f1, f2;
		    get2Floats(stack, out f1, out f2);

		    stack.Push(new StackItem(SheepSymbolType.Float, f1 * f2));
	    }

	    private static void divI(Stack<StackItem> stack)
	    {
		    int i1, i2;
		    get2Ints(stack, out i1, out i2);

		    stack.Push(new StackItem(SheepSymbolType.Int, i1 / i2));
	    }

	    private static void divF(Stack<StackItem> stack)
	    {
		    float f1, f2;
		    get2Floats(stack, out f1, out f2);

		    stack.Push(new StackItem(SheepSymbolType.Float, f1 / f2));
	    }

	    private static void negI(Stack<StackItem> stack)
	    {
		    StackItem item = stack.Pop();

		    if (item.Type != SheepSymbolType.Int)
			    throw new Exception("Expected integer on stack");

		    stack.Push(new StackItem(SheepSymbolType.Int, -item.IValue));
	    }

	    private static void negF(Stack<StackItem> stack)
	    {
            StackItem item = stack.Pop();

		    if (item.Type != SheepSymbolType.Float)
			    throw new Exception("Expected float on stack");

		    stack.Push(new StackItem(SheepSymbolType.Float, -item.FValue));
	    }

        private static void itof(Stack<StackItem> stack, int stackOffset)
        {
            Stack<StackItem> tempStack = new Stack<StackItem>();
            for (int i = 0; i < stackOffset; i++)
            {
                tempStack.Push(stack.Pop());
            }

            StackItem item = stack.Pop();

            if (item.Type != SheepSymbolType.Int)
                throw new Exception("Expected integer on stack");

            stack.Push(new StackItem(SheepSymbolType.Float, (float)item.IValue));

            while (tempStack.Count > 0)
            {
                stack.Push(tempStack.Pop());
            }
        }

        private static void ftoi(Stack<StackItem> stack, int stackOffset)
        {
            Stack<StackItem> tempStack = new Stack<StackItem>();
            for (int i = 0; i < stackOffset; i++)
            {
                tempStack.Push(stack.Pop());
            }

            StackItem item = stack.Pop();

            if (item.Type != SheepSymbolType.Float)
                throw new Exception("Expected float on stack");

            stack.Push(new StackItem(SheepSymbolType.Int, (int)item.FValue));

            while (tempStack.Count > 0)
            {
                stack.Push(tempStack.Pop());
            }
        }

        private static void andi(Stack<StackItem> stack)
        {
            int i1, i2;
            get2Ints(stack, out i1, out i2);

            stack.Push(new StackItem(SheepSymbolType.Int, (i1 != 0 && i2 != 0) ? 1 : 0));
        }

        private static void ori(Stack<StackItem> stack)
        {
            int i1, i2;
            get2Ints(stack, out i1, out i2);

            stack.Push(new StackItem(SheepSymbolType.Int, (i1 != 0 || i2 != 0) ? 1 : 0));
        }

        private static void noti(Stack<StackItem> stack)
        {
            int i = getInt(stack);

            stack.Push(new StackItem(SheepSymbolType.Int, i == 0 ? 1 : 0));
        }

        private static void callVoidFunction(SheepMachine vm, Stack<StackItem> stack, List<SheepImport> imports, int index)
        {
            callFunction(vm, stack, imports, index, 0);

            stack.Push(new StackItem(SheepSymbolType.Int, 0));
        }

        private static void callIntFunction(SheepMachine vm, Stack<StackItem> stack, List<SheepImport> imports, int index)
        {
            callFunction(vm, stack, imports, index, 1);
        }

        private static void callFunction(SheepMachine vm, Stack<StackItem> stack, List<SheepImport> imports, int index, int numExpectedReturns)
        {
            int numParams = getInt(stack);

		    const int MAX_NUM_PARAMS = 16;
		    StackItem[] parms = new StackItem[MAX_NUM_PARAMS];

		    if (numParams >= MAX_NUM_PARAMS)
			    throw new Exception("More than the maximum number of allowed parameters found");

		    int numItemsOnStack = stack.Count;

		    // find the function
		    if (index < 0 || index >= imports.Count)
			    throw new Exception("Invalid import function");
		    if (imports[index].Parameters.Length != numParams)
			    throw new Exception("Invalid number of parameters to import function");
		    if (numParams > stack.Count)
		    {
			    throw new Exception("Stack is not in a valid state for calling this import function");
		    }

		    if (imports[index].Callback != null)
		    {
                // TODO: call the callback
			    imports[index].Callback(vm);
		    }

		    int paramsLeftOver = numParams - (int)(numItemsOnStack - stack.Count);
		    if (paramsLeftOver > numExpectedReturns)
		    {
			    // lazy bums didn't pop everything off!
			    for (int i = numExpectedReturns; i < paramsLeftOver; i++)
				    stack.Pop();
		    }
		    else if (paramsLeftOver < numExpectedReturns)
		    {
			    // the idiots popped too much, or didn't put enough stuff on the stack!
			    throw new Exception("Incorrect number of items on the stack after function call");
		    }
        }

        #endregion

        #endregion Privates


    }
}
