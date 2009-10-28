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
        public uint NumSections;
        public uint Unknown;
        public uint DataSize;
        public string ModelName;
    }

    struct ActSectionHeader
    {
        public ushort Unknown;
        public uint DataSize;
    }

    struct ActSubsectionHeader
    {
        public byte Unknown;
        public uint DataSize;
    }

    public class ActResource : Resource.Resource
    {
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
            header.NumSections = reader.ReadUInt32();
            header.Unknown = reader.ReadUInt32();
            header.DataSize = reader.ReadUInt32();
            header.ModelName = Gk3Main.Utils.ConvertAsciiToString(reader.ReadBytes(32));

            // read the offsets
            uint[] offsets = new uint[header.NumSections];
            for (int i = 0; i < header.NumSections; i++)
            {
                offsets[i] = reader.ReadUInt32();
            }

            for (int i = 0; i < header.NumSections; i++)
            {
                stream.Seek(currentStreamPosition + offsets[i], System.IO.SeekOrigin.Begin);

                ActSectionHeader sectionHeader;
                sectionHeader.Unknown = reader.ReadUInt16();
                sectionHeader.DataSize = reader.ReadUInt32();

                

                int numSubsections = 0;
                while (stream.Position < currentStreamPosition + offsets[i] + sectionHeader.DataSize)
                {
                    ActSubsectionHeader subsection;
                    subsection.Unknown = reader.ReadByte();
                    subsection.DataSize = reader.ReadUInt32();

                    if (subsection.Unknown == 2 && subsection.DataSize != 48)
                        throw new Exception("NOT 48!!!");
                    else if (subsection.Unknown == 3 && subsection.DataSize != 24)
                        throw new Exception("NOT 24!!");


                   // if (subsection.Unknown != 1)
                        reader.ReadBytes((int)subsection.DataSize);
                   /* else
                    {
                        //if (subsection.Unknown != 1 && subsection.DataSize % 4 != 0)
                        //   Console.CurrentConsole.WriteLine("Not divisible by 4! {0} {1}", subsection.DataSize, subsection.Unknown);

                        // seems like unknown = 1 means there's a good chance there's a sub-subsection
                        long pos = stream.Position;
                        while (stream.Position < pos + subsection.DataSize)
                        {
                            ActSectionHeader subsubsection;
                            subsubsection.Unknown = reader.ReadUInt16();
                            subsubsection.DataSize = reader.ReadUInt32();

                            reader.ReadBytes((int)subsubsection.DataSize);

                            Console.CurrentConsole.WriteLine("\t\t{0}\t{1}", subsubsection.Unknown, subsubsection.DataSize);
                        }
                    }
                    */
                    numSubsections++;
                }
            }
        }

        public override void Dispose()
        {
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
