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

    class FrameTransformation
    {
        private Math.Vector3 _scale;
        private Math.Vector3 _translation;
        private Math.Quaternion _rotation;
        private Math.Matrix _original;

        public FrameTransformation(float[] matrix43)
        {
            _original.M11 = matrix43[0];
            _original.M12 = matrix43[1];
            _original.M13 = matrix43[2];
            _original.M14 = 0;

            _original.M21 = matrix43[3];
            _original.M22 = matrix43[4];
            _original.M23 = matrix43[5];
            _original.M24 = 0;

            _original.M31 = matrix43[6];
            _original.M32 = matrix43[7];
            _original.M33 = matrix43[8];
            _original.M34 = 0;

            _original.M41 = matrix43[9];
            _original.M42 = matrix43[10];
            _original.M43 = matrix43[11];
            _original.M44 = 1.0f;

            // TODO: scale
            _scale = Math.Vector3.One;

            _scale.X = (float)System.Math.Sqrt(
                matrix43[0 * 3 + 0] * matrix43[0 * 3 + 0] +
                matrix43[0 * 3 + 1] * matrix43[0 * 3 + 1] +
                matrix43[0 * 3 + 2] * matrix43[0 * 3 + 2]);
            _scale.Y = (float)System.Math.Sqrt(
                matrix43[1 * 3 + 0] * matrix43[1 * 3 + 0] +
                matrix43[1 * 3 + 1] * matrix43[1 * 3 + 1] +
                matrix43[1 * 3 + 2] * matrix43[1 * 3 + 2]);
            _scale.Z = (float)System.Math.Sqrt(
                matrix43[2 * 3 + 0] * matrix43[2 * 3 + 0] +
                matrix43[2 * 3 + 1] * matrix43[2 * 3 + 1] +
                matrix43[2 * 3 + 2] * matrix43[2 * 3 + 2]);

            // translation
            _translation.X = matrix43[3 * 3 + 0];
            _translation.Y = matrix43[3 * 3 + 1];
            _translation.Z = matrix43[3 * 3 + 2];

            // TODO: rotation
            _rotation = Math.Quaternion.FromAxis(Math.Vector3.Up, 0);
        }

        public Math.Matrix Original
        {
            get { return _original; }
        }

        public static void LerpToMatrix(float amount, 
            ref FrameTransformation t1, ref FrameTransformation t2, 
            out Math.Matrix result)
        {
            float invAmount = 1.0f - amount;
            result = t1._original;

            result.M41 = t1._translation.X * invAmount + t2._translation.X * amount;
            result.M42 = t1._translation.Y * invAmount + t2._translation.Y * amount;
            result.M43 = t1._translation.Z * invAmount + t2._translation.Z * amount; 
        }
    }

    struct FrameSectionVertices
    {
        public int SectionIndex;
        public bool Delta;
        public float[] Vertices;
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
            public bool Active;

            public FrameTransformation Transform;
            public List<FrameSectionVertices> Vertices;
        }

        private struct MeshAnimationFrame
        {
            public uint Time;
            public FrameTransformation Transform;
            public FrameSectionVertices[] Vertices;
        }

        private string _modelName;
        private ActFrame[] _frames;
        private MeshAnimationFrame[][] _animationFrames;
        private uint _numMeshes;
        private uint _numFrames;

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
            _numFrames = header.NumFrames;
            _frames = new ActFrame[header.NumFrames * _numMeshes];

            // read the offsets
            uint[] offsets = new uint[header.NumFrames];
            for (int i = 0; i < header.NumFrames; i++)
            {
                offsets[i] = reader.ReadUInt32();
            }

            for (uint i = 0; i < header.NumFrames; i++)
            {
                stream.Seek(currentStreamPosition + offsets[i], System.IO.SeekOrigin.Begin);
                uint frameSize;
                if (i == header.NumFrames - 1)
                    frameSize = header.DataSize - offsets[i];
                else
                    frameSize = offsets[i + 1] - offsets[i];

                readFrame(reader, i, frameSize);
            }

            // now we have all the frames loaded from the ACT file,
            // so now it's time to convert the frame data into a more
            // usable layout
            _animationFrames = new MeshAnimationFrame[_numMeshes][];
            for (uint i = 0; i < _numMeshes; i++)
            {
                // count the active frames
                uint numActiveFramesThisMesh = 0;
                for (uint j = 0; j < _numFrames; j++)
                {
                    if (_frames[j * _numMeshes + i].Active)
                        numActiveFramesThisMesh++;
                }

                // create the frames
                _animationFrames[i] = new MeshAnimationFrame[numActiveFramesThisMesh];
                uint currentFrame = 0;
                for (uint j = 0; j < _numFrames; j++)
                {
                    if (_frames[j * _numMeshes + i].Active)
                    {
                        _animationFrames[i][currentFrame].Time = _millisecondsPerFrame * j;
                        _animationFrames[i][currentFrame].Transform = _frames[j * _numMeshes + i].Transform;
                        if (_frames[j * _numMeshes + i].Vertices != null)
                            _animationFrames[i][currentFrame].Vertices = _frames[j * _numMeshes + i].Vertices.ToArray();
                        
                        currentFrame++;
                    }
                }
            }
        }

        public override void Dispose()
        {
        }

        public string ModelName
        {
            get { return _modelName; }
        }

        public bool Animate(ModelResource model, int timeSinceStart, bool loop)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (model.Meshes.Length != _numMeshes)
                throw new ArgumentException("The model is not compatible with this animation");

            int frameNum = timeSinceStart / _millisecondsPerFrame;
            int nextFrameNum = frameNum + 1;
            //float percent = (float)(timeSinceStart - frameNum * _millisecondsPerFrame) / _millisecondsPerFrame;

            // is this animation finished?
            if (frameNum >= _numFrames)
            {
                frameNum = (int)_numFrames - 1;
            }

            for (int i = 0; i < model.Meshes.Length; i++)
            {
                int frame1, frame2;
                float percent;
                getFrames(timeSinceStart, i, out frame1, out frame2, out percent);

                if (frame1 >= 0 && _animationFrames[i][frame1].Transform != null &&
                    frame2 >= 0 && _animationFrames[i][frame2].Transform != null)
                {
                    Math.Matrix animatedTransform;
                    FrameTransformation.LerpToMatrix(percent, ref _animationFrames[i][frame1].Transform, ref _animationFrames[i][frame2].Transform, out animatedTransform);
                    model.Meshes[i].AnimatedTransformMatrix = animatedTransform;
                }
          

                /*if (frame.Vertices != null)
                {
                    for (int j = 0; j < frame.Vertices.Length; j++)
                    {
                        float[] dest = model.Meshes[i].sections[frame.Vertices[j].SectionIndex].vertices;
                        float[] source = frame.Vertices[j].Vertices;

                        if (frame.Vertices[j].Delta)
                        {
                            for (int k = 0; k < source.Length / 3; k++)
                            {
                                const int stride = 3 + 3 + 2;
                                dest[k * stride + 0] += source[k * 3 + 0];
                                dest[k * stride + 1] += source[k * 3 + 1];
                                dest[k * stride + 2] += source[k * 3 + 2];
                            }

                            //model.Meshes[i].VerticesModifiedWithDelta = true;
                        }
                        else
                        {
                            for (int k = 0; k < source.Length / 3; k++)
                            {
                                const int stride = 3 + 3 + 2;
                                dest[k * stride + 0] = source[k * 3 + 0];
                                dest[k * stride + 1] = source[k * 3 + 1];
                                dest[k * stride + 2] = source[k * 3 + 2];
                            }
                        }
                    }
                }*/

                
                //model.Meshes[i].AnimatedTransformMatrix = transform;// *model.Meshes[i].TransformMatrix;
            }

            return true;
        }

        private void getFrames(int elapsedTime, int meshIndex, out int frame1, out int frame2, out float percent)
        {
            int nextFrame = 0;
            for (; nextFrame < _animationFrames[meshIndex].Length; nextFrame++)
            {
                if (_animationFrames[meshIndex][nextFrame].Time > elapsedTime)
                    break;
            }

            if (nextFrame == _animationFrames[meshIndex].Length)
            {
                frame1 = nextFrame - 1;
                frame2 = frame1;
                percent = 0;
            }
            else if (nextFrame == 0)
            {
                frame1 = 0;
                frame2 = 0;
                percent = 0;
            }
            else
            {
                frame1 = nextFrame - 1;
                frame2 = nextFrame;

                uint frame1Time = _animationFrames[meshIndex][frame1].Time;
                uint frame2Time = _animationFrames[meshIndex][frame2].Time;
                percent = (elapsedTime - frame1Time) / (float)(frame2Time - frame1Time);
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

        private float uncompress(byte data)
        {
            // data is stored as s1i2f5

            // read the sign
	        int sign = data & 0x80;

            // get the fraction part
	        float fract = (data & 0x1f) / 32.0f;

            // get the whole part
	        int i = (data & 0x7f) >> 5;

            // combine everything
	        float f = i + fract;
	        if (sign != 0) f = -f;

	        return f;
        }

        private void readFrame(System.IO.BinaryReader reader, uint frameNum, uint frameSize)
        {
            long startPosition = reader.BaseStream.Position;

            for (uint i = 0; i < _numMeshes; i++)
            {
                ActSectionHeader sectionHeader;
                sectionHeader.MeshIndex = reader.ReadUInt16();
                sectionHeader.DataSize = reader.ReadUInt32();

                if (sectionHeader.MeshIndex >= _numMeshes)
                    throw new Exception("Invalid Mesh index found");

                _frames[frameNum * _numMeshes + i].Active = false;

                readMeshFrame(reader, frameNum, i, sectionHeader.DataSize);
            }
        }

        private void readMeshFrame(System.IO.BinaryReader reader, uint frameNum, uint meshIndex, uint sectionLength)
        {
            long end = reader.BaseStream.Position + sectionLength;
            uint frameIndex = frameNum * _numMeshes + meshIndex;

            while (reader.BaseStream.Position < end)
            {
                ActSubsectionHeader subsection;
                subsection.Type = reader.ReadByte();
                subsection.DataSize = reader.ReadUInt32();

                if (subsection.Type == (byte)ActSubsectionType.Group)
                {
                    ushort groupIndex = reader.ReadUInt16();
                    ushort numVertices = reader.ReadUInt16();

                    float[] vertices = new float[numVertices * 3];
                    for (ushort j = 0; j < numVertices; j++)
                    {
                        vertices[j * 3 + 0] = reader.ReadSingle();
                        vertices[j * 3 + 1] = reader.ReadSingle();
                        vertices[j * 3 + 2] = reader.ReadSingle();
                    }

                    _frames[frameIndex].Active = true;
                    if (_frames[frameIndex].Vertices == null)
                        _frames[frameIndex].Vertices = new List<FrameSectionVertices>();

                    FrameSectionVertices sectionVertices;
                    sectionVertices.SectionIndex = groupIndex;
                    sectionVertices.Vertices = vertices;
                    sectionVertices.Delta = false;
                    _frames[frameIndex].Vertices.Add(sectionVertices);
                }
                else if (subsection.Type == (byte)ActSubsectionType.DeltaGroup)
                {
                    ushort groupIndex = reader.ReadUInt16();
                    ushort numVertices = reader.ReadUInt16();

                    byte[] bitfield = reader.ReadBytes(numVertices / 4 + 1);

                    float[] vertices = new float[numVertices * 3];
                    for (ushort j = 0; j < numVertices; j++)
                    {
                        int type = getDeltaType(j, bitfield);
                        if (type == (int)VertexChangeType.None)
                        {
                            // nothing
                        }
                        else if (type == (int)VertexChangeType.Short)
                        {
                            vertices[j * 3 + 0] = uncompress(reader.ReadByte());
                            vertices[j * 3 + 1] = uncompress(reader.ReadByte());
                            vertices[j * 3 + 2] = uncompress(reader.ReadByte());
                        }
                        else if (type == (int)VertexChangeType.Long)
                        {
                            vertices[j * 3 + 0] = uncompress(reader.ReadUInt16());
                            vertices[j * 3 + 1] = uncompress(reader.ReadUInt16());
                            vertices[j * 3 + 2] = uncompress(reader.ReadUInt16());
                        }
                        else if (type == (int)VertexChangeType.Absolute)
                        {
                            vertices[j * 3 + 0] = reader.ReadSingle();
                            vertices[j * 3 + 1] = reader.ReadSingle();
                            vertices[j * 3 + 2] = reader.ReadSingle();
                        }
                    }

                    _frames[frameIndex].Active = true;
                    if (_frames[frameIndex].Vertices == null)
                        _frames[frameIndex].Vertices = new List<FrameSectionVertices>();

                    FrameSectionVertices sectionVertices;
                    sectionVertices.SectionIndex = groupIndex;
                    sectionVertices.Vertices = vertices;
                    sectionVertices.Delta = true;
                    _frames[frameIndex].Vertices.Add(sectionVertices);
                }
                else if (subsection.Type == (byte)ActSubsectionType.Transform)
                {
                    // read the 4x3 transform matrix
                    float[] transform = new float[4 * 3];
                    transform[0] = reader.ReadSingle();
                    transform[1] = reader.ReadSingle();
                    transform[2] = reader.ReadSingle();

                    transform[3] = reader.ReadSingle();
                    transform[4] = reader.ReadSingle();
                    transform[5] = reader.ReadSingle();

                    transform[6] = reader.ReadSingle();
                    transform[7] = reader.ReadSingle();
                    transform[8] = reader.ReadSingle();

                    transform[9] = reader.ReadSingle();
                    transform[10] = reader.ReadSingle();
                    transform[11] = reader.ReadSingle();

                    _frames[frameIndex].Active = true;
                    _frames[frameIndex].Transform = new FrameTransformation(transform);
                }
                else if (subsection.Type == (byte)ActSubsectionType.BoundingBox)
                {
                    // TODO: read the bounding box
                    reader.ReadBytes(sizeof(float) * 6);
                }
                else
                {
                    throw new Exception("Invalid subsection type found");
                }
            }
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
