using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SheepVMDotNet
{
    public class SheepFileReader
    {
        private struct SheepHeader
        {
            public uint Magic1;
            public uint Magic2;
            public uint Unknown;
            public uint ExtraOffset;
            public uint DataOffset;
            public uint DataSize;
            public uint DataCount;

            public uint[] OffsetArray;

            public const uint SheepHeaderSize = 28;
            public const uint Magic1Value = 0x53334b47;
            public const uint Magic2Value = 0x70656568;
        }

        private struct SectionHeader
        {
            public string Label;
            public uint ExtraOffset;
            public uint DataOffset;
            public uint DataSize;
            public uint Datacount;

            public uint[] OffsetArray;

            public const uint SectionHeaderSize = 28;
        }

        private IntermediateOutput _intermediateOutput;

        public SheepFileReader(string filename)
        {
            using (System.IO.Stream stream = new System.IO.FileStream(filename, System.IO.FileMode.Open))
            {
                read(stream);
            }
        }

        public IntermediateOutput Output
        {
            get { return _intermediateOutput; }
        }

        private void read(System.IO.Stream stream)
        {
            _intermediateOutput = new IntermediateOutput();

            System.IO.BinaryReader reader = new System.IO.BinaryReader(stream);

            SheepHeader header;
            header.Magic1 = reader.ReadUInt32();
            header.Magic2 = reader.ReadUInt32();

            if (header.Magic1 != SheepHeader.Magic1Value ||
                header.Magic2 != SheepHeader.Magic2Value)
                throw new Exception("Input file is not a valid sheep file");

            header.Unknown = reader.ReadUInt32();
            header.ExtraOffset = reader.ReadUInt32();
            header.DataOffset = reader.ReadUInt32();
            header.DataSize = reader.ReadUInt32();
            header.DataCount = reader.ReadUInt32();

            header.OffsetArray = new uint[header.DataCount];

            for (uint i = 0; i < header.DataCount; i++)
                header.OffsetArray[i] = reader.ReadUInt32();

            for (uint i = 0; i < header.DataCount; i++)
            {
                if (header.OffsetArray[i] >= stream.Length)
                    throw new Exception("Input file is not a valid Sheep file");

                stream.Seek(header.DataOffset + header.OffsetArray[i], System.IO.SeekOrigin.Begin);

                SectionHeader sectionHeader = readSectionHeader(reader);
                long currentOffset = stream.Position;

                if (sectionHeader.Label == "SysImports")
                {
                    for (uint j = 0; j < sectionHeader.Datacount; j++)
                    {
                        SheepImport import;

                        stream.Seek(currentOffset + sectionHeader.OffsetArray[j], System.IO.SeekOrigin.Begin);

                        short lengthOfName = reader.ReadInt16();
                        import.Name = readString(reader, lengthOfName);
                        import.Callback = null;

                        // skip padding
                        reader.ReadByte();

                        byte numReturns = reader.ReadByte();
                        byte numParameters = reader.ReadByte();

                        import.ReturnType = (SheepSymbolType)numReturns;
                        import.Parameters = new SheepSymbolType[numParameters];

                        for (byte k = 0; k < numParameters; k++)
                        {
                            SheepSymbolType paramterType = (SheepSymbolType)reader.ReadByte();

                            if (paramterType == SheepSymbolType.Int ||
                                paramterType == SheepSymbolType.Float ||
                                paramterType == SheepSymbolType.String)
                                import.Parameters[k] = paramterType;
                        }
                        
                        _intermediateOutput.Imports.Add(import);
                    }
                }
                else if (sectionHeader.Label == "StringConsts")
                {
                    for (uint j = 0; j < sectionHeader.Datacount; j++)
                    {
                        stream.Seek(currentOffset + sectionHeader.OffsetArray[j], System.IO.SeekOrigin.Begin);

                        SheepStringConstant constant;
                        constant.Offset = sectionHeader.OffsetArray[j];
                        constant.Value = readString(reader);

                        _intermediateOutput.Constants.Add(constant);
                    }
                }
                else if (sectionHeader.Label == "Variables")
                {
                    for (uint j = 0; j < sectionHeader.Datacount; j++)
                    {
                        stream.Seek(currentOffset + sectionHeader.OffsetArray[j], System.IO.SeekOrigin.Begin);

                        SheepSymbol symbol = new SheepSymbol();
                        short len = reader.ReadInt16();
                        symbol.Name = readString(reader, len);

                        // skip padding
                        reader.ReadByte();

                        symbol.Type = (SheepSymbolType)reader.ReadUInt32();

                        if (symbol.Type == SheepSymbolType.Int)
                            symbol.InitialIntValue = reader.ReadInt32();
                        else if (symbol.Type == SheepSymbolType.Float)
                            symbol.InitialFloatValue = reader.ReadSingle();
                        else if (symbol.Type == SheepSymbolType.String)
                            symbol.InitialStringValue = reader.ReadInt32();
                        else
                            throw new Exception("???");

                        _intermediateOutput.Symbols.Add(symbol);
                    }
                }
                else if (sectionHeader.Label == "Functions")
                {
                    for (uint j = 0; j < sectionHeader.Datacount; j++)
                    {
                        SheepFunction func = new SheepFunction();
                        short len = reader.ReadInt16();
                        func.Name = readString(reader, len);

                        reader.ReadByte();
                        reader.ReadByte();
                        func.CodeOffset = reader.ReadUInt32();

                        _intermediateOutput.Functions.Add(func);
                    }
                }
                else if (sectionHeader.Label == "Code")
                {
                    if (sectionHeader.Datacount > 1)
                        throw new Exception("Extra code sections found");

                    for (int j = 0; j < _intermediateOutput.Functions.Count; j++)
                    {
                        stream.Seek(currentOffset + _intermediateOutput.Functions[j].CodeOffset, System.IO.SeekOrigin.Begin);

                        uint size;
                        if (j == _intermediateOutput.Functions.Count - 1)
                            size = sectionHeader.DataSize - _intermediateOutput.Functions[j].CodeOffset;
                        else
                            size = _intermediateOutput.Functions[j + 1].CodeOffset - _intermediateOutput.Functions[j].CodeOffset;

                        SheepFunction f = _intermediateOutput.Functions[j];
                        f.Code = reader.ReadBytes((int)size);
                        _intermediateOutput.Functions[j] = f;
                    }
                }
            }
        }

        private static SectionHeader readSectionHeader(System.IO.BinaryReader reader)
        {
            SectionHeader header;

            byte[] label = reader.ReadBytes(12);
            header.Label = System.Text.Encoding.ASCII.GetString(label).Trim((char)0);
            header.ExtraOffset = reader.ReadUInt32();
            header.DataOffset = reader.ReadUInt32();
            header.DataSize = reader.ReadUInt32();
            header.Datacount = reader.ReadUInt32();

            header.OffsetArray = new uint[header.Datacount];

            for (uint i = 0; i < header.Datacount; i++)
            {
                header.OffsetArray[i] = reader.ReadUInt32();
            }

            return header;
        }

        private static string readString(System.IO.BinaryReader reader, short length)
        {
            byte[] bytes = reader.ReadBytes(length);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        private static string readString(System.IO.BinaryReader reader)
        {
            StringBuilder b = new StringBuilder();

            long start = reader.BaseStream.Position;
            long end = start;

            byte c = reader.ReadByte();
            while (c != 0)
            {
                end++;
                c = reader.ReadByte();
            }

            reader.BaseStream.Seek(start, System.IO.SeekOrigin.Begin);
            byte[] buffer = reader.ReadBytes((int)(end - start));

            return System.Text.Encoding.ASCII.GetString(buffer);

            return reader.ReadString();
        }
    }
}
