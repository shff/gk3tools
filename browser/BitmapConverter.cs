using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace GK3BB
{
    class GK3Bitmap : IDisposable
    {
        public GK3Bitmap(byte[] data)
        {
            int currentIndex = 0;

            // read the first 4 bytes
            if (BitConverter.ToUInt32(data, currentIndex) != 0x4d6e3136)
            {
                _data = new MemoryStream(data);
            }
            else
            {
                currentIndex += 4;

                // read the width
                ushort height = BitConverter.ToUInt16(data, currentIndex);
                currentIndex += 2;

                // read the height
                ushort width = BitConverter.ToUInt16(data, currentIndex);
                currentIndex += 2;

                int numPaddingBytes = 0;

                if ((width * 3) % 4 != 0)
                    numPaddingBytes = 4 - (width * 3) % 4;

                _data = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(_data);

                // write the file header
                writer.Write((UInt16)19778);
                writer.Write((UInt32)(54 + height * (width * 3 + numPaddingBytes)));
                writer.Write((UInt16)0);
                writer.Write((UInt16)0);
                writer.Write((UInt32)54);

                // write the info header
                writer.Write((UInt32)40);
                writer.Write((UInt32)width);
                writer.Write((UInt32)height);
                writer.Write((UInt16)1);
                writer.Write((UInt16)24);
                writer.Write((UInt32)0);
                writer.Write((UInt32)height * width * 3);
                writer.Write((UInt32)0);
                writer.Write((UInt32)0);
                writer.Write((UInt32)0);
                writer.Write((UInt32)0);

                // write the pixel data
                for (int row = height - 1; row >= 0; row--)
                {
                    currentIndex = row * width * 2 + 8;

                    // skip the GK3 padding
                    if ((width & 0x01) == 0x01)
                        currentIndex += row * 2;

                    for (int i = 0; i < width; i++)
                    {
                        ushort pixel = BitConverter.ToUInt16(data, currentIndex);
                        currentIndex += 2;

                        // convert the pixel into 24bit color
                        int tb = (byte)((pixel & 0xf800) >> 11);
                        int tg = (byte)((pixel & 0x07e0) >> 5);
                        int tr = (byte)(pixel & 0x001f);
                        
                        // now scale the values up to max of 255
                        byte r = (byte)(tr * 255 / 31);
                        byte g = (byte)(tg * 255 / 63);
                        byte b = (byte)(tb * 255 / 31);

                        writer.Write(r);
                        writer.Write(g);
                        writer.Write(b);
                    }

                    // add any padding
                    for (int i = 0; i < numPaddingBytes; i++)
                    {
                        byte zero = 0;

                        writer.Write(zero);
                    }
                }
            }
            //writer.Write(data, currentIndex, data.Length - 8);


            /*_rawData = System.Runtime.InteropServices.Marshal.AllocHGlobal(data.Length - currentIndex);

            System.Runtime.InteropServices.Marshal.Copy(data, currentIndex, _rawData, data.Length - currentIndex);
            _bitmap = new Bitmap(width, height, width * 2,
                System.Drawing.Imaging.PixelFormat.Format16bppRgb555, _rawData);*/
        }

        public void Dispose()
        {
            if (_data == null) throw new InvalidOperationException();

            _data.Close();

            _data = null;
        }

        public void Save(string filename)
        {
            if (_data == null) throw new InvalidOperationException();

            System.IO.FileStream file = new FileStream(filename, FileMode.Create);
            _data.WriteTo(file);
            file.Close();
   
        }

        private MemoryStream _data = null;
    }
}
