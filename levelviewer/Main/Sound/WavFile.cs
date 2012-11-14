using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Gk3Main.Sound
{
    enum WavEncoding
    {
        PCM,
        MP3
    }

    class WavFile
    {
        private WavEncoding _encoding;
        private byte[] _data;
        private int _sampleRate;
        private int _sampleSize;
        private int _channels;
        private int _length;

        public WavFile(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            int chunkID = reader.ReadInt32();
            int fileSize = reader.ReadInt32();
            int riffType = reader.ReadInt32();
            int fmtID = reader.ReadInt32();
            int fmtSize = reader.ReadInt32();
            int fmtCode = reader.ReadInt16();
            _channels = reader.ReadInt16();
            _sampleRate = reader.ReadInt32();
            int fmtAvgBPS = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();

            if (fmtSize == 18)
            {
                int extraSize = reader.ReadInt16();
                reader.ReadBytes(extraSize);
            }

            if (fmtCode == 1)
                _encoding = WavEncoding.PCM;
            else if (fmtCode == 85)
                _encoding = WavEncoding.MP3;
            else
                throw new NotSupportedException("Encoding format not supported");

            if (_encoding == WavEncoding.MP3)
                loadMP3(reader);
            else
                loadPCM(reader);
        }

        public WavEncoding Encoding
        {
            get { return _encoding; }
        }

        public int SampleRate
        {
            get { return _sampleRate; }
        }

        public int SampleSize
        {
            get { return _sampleSize; }
        }

        public int Channels
        {
            get { return _channels; }
        }

        public int Length
        {
            get { return _length; }
        }

        public byte[] PcmData
        {
            get { return _data; }
        }

        private void loadPCM(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private void loadMP3(BinaryReader reader)
        {
            int size = reader.ReadInt16();
            int id = reader.ReadInt16();
            int flags = reader.ReadInt32();
            int blockSize = reader.ReadInt16();
            int framesPerBlock = reader.ReadInt16();
            int codecDelay = reader.ReadInt16();

            blockSize = 512;
            byte[] input = new byte[blockSize];


            MemoryStream output = new MemoryStream();
            Mp3Stream mp3 = new Mp3Stream(reader.BaseStream);

           /* Mp3Sharp.Mp3Stream mp3 = new Mp3Sharp.Mp3Stream(reader.BaseStream);*/

            int totalRead = 0;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (true)
            {
                int bytesRead = mp3.Read(input, 0, blockSize);
                totalRead += bytesRead;
                output.Write(input, 0, bytesRead);

                if (bytesRead == 0)
                    break;
            }
            sw.Stop();
            Console.CurrentConsole.WriteLine("Loaded mp3 in {0} ms", sw.ElapsedMilliseconds);

            _data = output.GetBuffer();
            _length = (int)output.Length;
        }
    }
}
