using System;
using System.Collections.Generic;
using System.Text;

namespace Gk3Main.Graphics
{
    struct ActHeader
    {
        public uint Magic;
        public ushort Version1;
        public ushort Version2;
        public uint NumFrames;
        public uint NumMeshes;
        public uint DataSize;
        public string ModelName;
    }

    struct ActSectionHeader
    {
        public ushort MeshIndex;
        public uint DataSize;
    }

    struct ActSubsectionHeader
    {
        public byte Type;
        public uint DataSize;
    }

    enum ActSubsectionType
    {
        Group = 0,
        DeltaGroup,
        Transform,
        BoundingBox
    }

    class ActResource : Resource.Resource
    {
        const int _millisecondsPerFrame = 67;

        private enum VertexChangeType
        {
            None = 0,
            Short,
            Long,
            Absolute
        }

        private struct ActFrame
        {
            public int FrameNum;
            public Math.Matrix Transform;
        }

        private string _modelName;
        private ActFrame[] _frames;
        private uint _numMeshes;

        public ActResource(string name, System.IO.Stream stream)
            : base(name, true)
        {
            int currentStreamPosition = (int)stream.Position;
            System.IO.BinaryReader reader = new System.IO.BinaryReader(stream, Encoding.ASCII);

            ActHeader header;
            header.Magic = reader.ReadUInt32();

            if (header.Magic != 0x41435448)
                throw new Resource.InvalidResourceFileFormat("Invalid ACT file header");

            header.Version1 = reader.ReadUInt16();
            header.Version2 = reader.ReadUInt16();
            header.NumFrames = reader.ReadUInt32();
            header.NumMeshes = reader.ReadUInt32();
            header.DataSize = reader.ReadUInt32();
            header.ModelName = Gk3Main.Utils.ConvertAsciiToString(reader.ReadBytes(32));

            _modelName = header.ModelName;
            _numMeshes = header.NumMeshes;
            _frames = new ActFrame[header.NumFrames];

            // read the offsets
            uint[] offsets = new uint[header.NumFrames];
            for (int i = 0; i < header.NumFrames; i++)
            {
                offsets[i] = reader.ReadUInt32();
            }

            for (int i = 0; i < header.NumFrames; i++)
            {
                stream.Seek(currentStreamPosition + offsets[i], System.IO.SeekOrigin.Begin);

                ActSectionHeader sectionHeader;
                sectionHeader.MeshIndex = reader.ReadUInt16();
                sectionHeader.DataSize = reader.ReadUInt32();

                int numSubsections = 0;
                while (stream.Position < currentStreamPosition + offsets[i] + sectionHeader.DataSize)
                {
                    ActSubsectionHeader subsection;
                    subsection.Type = reader.ReadByte();
                    subsection.DataSize = reader.ReadUInt32();

                    if (subsection.Type == (byte)ActSubsectionType.Group)
                    {
                        ushort groupIndex = reader.ReadUInt16();
                        ushort numVertices = reader.ReadUInt16();

                        for (ushort j = 0; j < numVertices; j++)
                        {
                            reader.ReadSingle();
                            reader.ReadSingle();
                            reader.ReadSingle();
                        }
                    }
                    else if (subsection.Type == (byte)ActSubsectionType.DeltaGroup)
                    {
                        ushort groupIndex = reader.ReadUInt16();
                        ushort numVertices = reader.ReadUInt16();

                        byte[] bitfield = reader.ReadBytes(numVertices / 4 + 1);

                        for (ushort j = 0; j < numVertices; j++)
                        {
                            int type = getDeltaType(j, bitfield);
                            if (type == (int)VertexChangeType.None)
                            {
                                // nothing
                            }
                            else if (type == (int)VertexChangeType.Short)
                            {
                                // TODO: read and handle the data
                                reader.ReadByte();
                                reader.ReadByte();
                                reader.ReadByte();
                            }
                            else if (type == (int)VertexChangeType.Long)
                            {
                                reader.ReadUInt16();
                                reader.ReadUInt16();
                                reader.ReadUInt16();
                            }
                            else if (type == (int)VertexChangeType.Absolute)
                            {
                                reader.ReadSingle();
                                reader.ReadSingle();
                                reader.ReadSingle();
                            }
                        }
                    }
                    else if (subsection.Type == (byte)ActSubsectionType.Transform)
                    {
                        // read the 4x3 transform matrix
                        float[] transform = new float[16];
                        transform[0] = reader.ReadSingle();
                        transform[1] = reader.ReadSingle();
                        transform[2] = reader.ReadSingle();
                        transform[3] = 0;

                        transform[4] = reader.ReadSingle();
                        transform[5] = reader.ReadSingle();
                        transform[6] = reader.ReadSingle();
                        transform[7] = 0;

                        transform[8] = reader.ReadSingle();
                        transform[9] = reader.ReadSingle();
                        transform[10] = reader.ReadSingle();
                        transform[11] = 0;

                        transform[12] = reader.ReadSingle();
                        transform[13] = reader.ReadSingle();
                        transform[14] = reader.ReadSingle();
                        transform[15] = 1.0f;

                        _frames[i].FrameNum = i;
                        _frames[i].Transform = new Math.Matrix(transform);
                    }
                    else if (subsection.Type == (byte)ActSubsectionType.BoundingBox)
                    {
                        // TODO: read the bounding box
                        reader.ReadBytes(sizeof(float) * 6);
                    }

                    numSubsections++;
                }
            }
        }

        public override void Dispose()
        {
        }

        public void Animate(ModelResource model, int timeSinceStart)
        {
            if (model.Name.Equals(_modelName, StringComparison.OrdinalIgnoreCase) == false ||
                model.Meshes.Length != _numMeshes)
                throw new ArgumentException("The model is not compatible with this animation");

            int frame = timeSinceStart / _millisecondsPerFrame;
            int nextFrame = (frame == _frames.Length - 1 ? 0 : frame + 1);
            float percent = (float)(timeSinceStart - frame * _millisecondsPerFrame) / _millisecondsPerFrame;

            for (int i = 0; i < model.Meshes.Length; i++)
            {
                // TODO: handle all the different kinds of animation frames
                model.Meshes[i].AnimatedTransformMatrix = model.Meshes[i].TransformMatrix * _frames[frame].Transform;
            }
        }

        private int getDeltaType(int index, byte[] mask)
        {
            return (mask[index / 4] >> ((index & 3) * 2)) & 0x3;
        }

        private float uncompress(ushort data)
        {
            // data is stored as s1i7f8

            // read the sign
            int sign = data & 0x8000;

            // get the fraction part
            float fract = (data & 0xff) / 256.0f;

            // get the whole part
            int i = (data & 0x7fff) >> 8;

            // combine everything
            float f = i + fract;
            if (sign != 0) f = -f;

            return f;
        }
    }

    public class ActResourceLoader : Resource.IResourceLoader
    {
        public Resource.Resource Load(string name)
        {
            System.IO.Stream stream = FileSystem.Open(name);

            ActResource resource = new ActResource(name, stream);

            stream.Close();

            return resource;
        }

        public string[] SupportedExtensions { get { return m_supportedExtensions; } }
        public bool EmptyResourceIfNotFound { get { return true; } }

        private static string[] m_supportedExtensions = new string[] { "ACT" };
    }
}
