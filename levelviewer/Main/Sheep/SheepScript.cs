using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Gk3Main.Sheep
{
    struct SysImport
    {
        public string Name;
        public byte NumReturns;
        public byte NumParameters;
        public byte[] ParameterTypes;
    }

    struct Function
    {
        public string Name;
        public byte NumReturns;
        public byte NumParameters;
        public uint CodeOffset;
        public byte[] Code;
    }

    public enum ParameterType
    {
        String,
        Integer,
        Float
    }

    public struct Parameter
    {
        public ParameterType Type;

        public int Integer;
        public float Float;
        public string String;
    }

    class SheepScript
    {
        public SheepScript(string filename, SheepStateManager state)
        {
            _name = filename;
            _state = state;

            Stream stream = FileSystem.Open(filename);
            BinaryReader reader = new BinaryReader(stream);

            if (convertToString(reader.ReadBytes(8)) != "GK3Sheep")
                loadTextSheep(stream);
            else
                loadBinarySheep(reader);

            stream.Close();
        }

        public void Execute(string function)
        {
            // get the current function
            Function? currentFunction = null;
            foreach (Function f in _functions)
            {
                if (f.Name == function)
                {
                    currentFunction = f;
                    break;
                }
            }

            if (currentFunction.HasValue == false)
                throw new ArgumentException("Function not found: " + function);

            int instructionPtr = 0;
            byte[] code = currentFunction.Value.Code;
            uint codeOffset = currentFunction.Value.CodeOffset;

            while (instructionPtr < code.Length)
            {
                byte op = code[instructionPtr++];
                int param;
                Parameter[] functionParams;

                switch (op)
                {
                    case 0x00: // SitnSpin
                        // do nothing
                        break;
                    case 0x02: // CallSysFunctionV
                        param = getIntFromBytes(code, ref instructionPtr);
                        functionParams = buildParameterArray();
                        SheepMachine.CallVoidSystemFunction(Imports[param].Name, functionParams);
                        _state.PushStack(0);
                        break;
                    case 0x03: // CallSysFunctionI
                        param = getIntFromBytes(code, ref instructionPtr);
                        functionParams = buildParameterArray();
                        _state.PushStack(SheepMachine.CallIntSystemFunction(Imports[param].Name, functionParams));
                        break;
                    case 0x05: // CallSysFunctionS
                        param = getIntFromBytes(code, ref instructionPtr);
                        functionParams = buildParameterArray();
                        SheepMachine.CallStringSystemFunction(Imports[param].Name, functionParams);
                        _state.PushString(0);
                        break;
                    case 0x06: // Branch
                        param = getIntFromBytes(code, ref instructionPtr);
                        instructionPtr = param - (int)codeOffset;
                        break;
                    case 0x08: // BranchIfZero
                        param = getIntFromBytes(code, ref instructionPtr);
                        if (_state.PeekStack().Type != ParameterType.Integer)
                            throw new InvalidOperationException("stack is bad");
                        if (_state.PopStack().Integer == 0)
                            instructionPtr = param - (int)codeOffset;
                        break;
                    case 0x09: // BeginWait
                        // do nothing
                        break;
                    case 0x0A: // EndWait
                        // do nothing
                        break;
                    case 0x0B: // ReturnV
                        return;
                    case 0x13: // PushI
                        param = getIntFromBytes(code, ref instructionPtr);
                        _state.PushStack(param);
                        break;
                    case 0x15: // PushS
                        param = getIntFromBytes(code, ref instructionPtr);
                        _state.PushString(param);
                        break;
                    case 0x16: // Pop
                        _state.PopStack();
                        break;
                    case 0x21: // IsEqualI
                        if (_state.PeekStack().Type != ParameterType.Integer ||
                           _state.PeekStack().Type != ParameterType.Integer)
                            throw new InvalidOperationException("IsLessI found two non-integers on the stack");
                        if (_state.PopStack().Integer == _state.PopStack().Integer)
                            _state.PushStack(1);
                        else
                            _state.PushStack(0);
                        break;
                    case 0x27: // IsLessI
                        if (_state.PeekStack().Type != ParameterType.Integer ||
                            _state.PeekStack().Type != ParameterType.Integer)
                            throw new InvalidOperationException("IsLessI found two non-integers on the stack");
                        if (_state.PopStack().Integer >= _state.PopStack().Integer)
                            _state.PushStack(1);
                        else
                            _state.PushStack(0);
                        break;
                    case 0x2D:
                        param = getIntFromBytes(code, ref instructionPtr);
                        _state.PushStack((float)param);
                        break;
                    case 0x30:
                        // do nothing
                        break;
                    case 0x31:
                        // do nothing
                        break;
                    case 0x33: // GetString
                        {
                            if (_state.PeekStack().Type != ParameterType.String)
                                throw new InvalidOperationException("GetString failed because value on stack isn't a string");

                            param = _state.PopStack().Integer;
                            Parameter p = new Parameter();
                            p.Type = ParameterType.String;
                            p.Integer = param;
                            p.String = Constants[_constantOffsetIndexMap[param]];

                            _state.PushStack(p);
                        }
                        break;
                    default:
                        throw new NotImplementedException("Instruction " +
                            op.ToString("X") + " not implemented");
                }
            }

            return;
        }

        public string Name
        {
            get { return _name; }
        }

        public string[] Constants
        {
            get { return _constants; }
        }

        public SysImport[] Imports
        {
            get { return _imports; }
        }

        public Function[] Functions
        {
            get { return _functions; }
        }

        #region Privates
        private void loadTextSheep(Stream stream)
        {
            throw new NotImplementedException();
        }

        private void loadBinarySheep(BinaryReader reader)
        {
            reader.ReadUInt32();
            uint offset = reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();

            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            if (convertToString(reader.ReadBytes(12)) != "SysImports")
                throw new InvalidDataException();

            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            uint numImports = reader.ReadUInt32();

            uint[] importOffsets = new uint[numImports];
            for (uint i = 0; i < numImports; i++)
            {
                importOffsets[i] = reader.ReadUInt32();
            }

            uint endOfOffsetArray = (uint)reader.BaseStream.Position;

            _imports = new SysImport[numImports];
            for (uint i = 0; i < numImports; i++)
            {
                reader.BaseStream.Seek(endOfOffsetArray + importOffsets[i], SeekOrigin.Begin);

                ushort len = reader.ReadUInt16();
                _imports[i].Name = convertToString(reader.ReadBytes(len + 1));
                _imports[i].NumReturns = reader.ReadByte();
                _imports[i].NumParameters = reader.ReadByte();

                if (_imports[i].NumParameters > 0)
                {
                    _imports[i].ParameterTypes = new byte[_imports[i].NumParameters];
                    for (ushort j = 0; j < _imports[i].NumParameters; j++)
                        _imports[i].ParameterTypes[j] = reader.ReadByte();
                }
            }

            if (convertToString(reader.ReadBytes(12)) != "StringConsts")
                throw new InvalidDataException();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            uint numConstants = reader.ReadUInt32();

            int[] constantOffsets = new int[numConstants];

            for (uint i = 0; i < numConstants; i++)
            {
                constantOffsets[i] = reader.ReadInt32();
            }

            endOfOffsetArray = (uint)reader.BaseStream.Position;
            _constants = new string[numConstants];

            for (uint i = 0; i < numConstants; i++)
            {
                reader.BaseStream.Seek(endOfOffsetArray + constantOffsets[i], SeekOrigin.Begin);

                if (i < numConstants - 1)
                    _constants[i] = convertToString(reader.ReadBytes((int)(constantOffsets[i + 1] - constantOffsets[i])));
                else
                {
                    // the last constant, so this will be slightly trickier...
                    byte[] mybyte = new byte[1];
                    while (true)
                    {
                        mybyte[0] = reader.ReadByte();

                        if (mybyte[0] == 0)
                            break;

                        _constants[i] += convertToString(mybyte);
                    }
                }

                _constantOffsetIndexMap.Add(constantOffsets[i], i);
            }

            // read functions
            if (convertToString(reader.ReadBytes(12)) != "Functions")
                throw new InvalidDataException();
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            uint numFunctions = reader.ReadUInt32();

            uint[] functionOffsets = new uint[numFunctions];
            for (uint i = 0; i < numFunctions; i++)
            {
                functionOffsets[i] = reader.ReadUInt32();
            }

            endOfOffsetArray = (uint)reader.BaseStream.Position;
            _functions = new Function[numFunctions];

            for (uint i = 0; i < numFunctions; i++)
            {
                reader.BaseStream.Seek(endOfOffsetArray + functionOffsets[i], SeekOrigin.Begin);

                Function func;

                ushort nameLen = reader.ReadUInt16();
                func.Name = convertToString(reader.ReadBytes(nameLen + 1));
                func.NumReturns = reader.ReadByte();
                func.NumParameters = reader.ReadByte();
                func.CodeOffset = reader.ReadUInt32();
                func.Code = null;

                _functions[i] = func;
            }

            // read bytecode
            if (convertToString(reader.ReadBytes(12)) != "Code")
                throw new InvalidDataException();
            
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            uint dataSize = reader.ReadUInt32();
            for (uint i = 0; i < dataSize; i++)
                reader.ReadUInt32();

            endOfOffsetArray = (uint)reader.BaseStream.Position;

            for (uint i = 0; i < numFunctions; i++)
            {
                uint codeLength;
                if (i < numFunctions - 1)
                    codeLength = _functions[i + 1].CodeOffset - _functions[i].CodeOffset;
                else
                    codeLength = (uint)reader.BaseStream.Length - _functions[i].CodeOffset;

                _functions[i].Code = reader.ReadBytes((int)codeLength);
            }
        }

        private static int getIntFromBytes(byte[] bytes, ref int index)
        {
            int i = BitConverter.ToInt32(bytes, index);

            index += 4;

            return i;
        }

        private string convertToString(byte[] ascii)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(ascii).Trim('\0');
        }

        private Parameter[] buildParameterArray()
        {
            Parameter numParametersParam = _state.PopStack();

            if (numParametersParam.Type != ParameterType.Integer)
                throw new InvalidOperationException("Stack is bad!");

            Parameter[] parameters = new Parameter[numParametersParam.Integer];
            for (int i = 0; i < numParametersParam.Integer; i++)
            {
                parameters[i] = _state.PopStack();
            }

            return parameters;
        }

        private string _name;
        private SheepStateManager _state;
        private string[] _constants;
        private Function[] _functions;
        private SysImport[] _imports;
        private Dictionary<int, uint> _constantOffsetIndexMap = new Dictionary<int, uint>();

        #endregion Privates
    }
}
