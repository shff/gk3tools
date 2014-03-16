// Copyright (c) 2006 Brad Farris
// This file is licensed under the MIT license. You can do whatever you
// want with this file as long as this notice remains at the top.

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Runtime.InteropServices;

namespace BarnLib
{
    public enum Compression : int
    {
        None = 0,
        ZLib = 1,
        LZO = 2
    }

    public class BarnException : System.Exception
    {
        public BarnException(string message)
            : base(message)
        { }

        public BarnException(string message, Exception inner)
            : base(message, inner)
        {
        }
    };

    public class Barn : System.IDisposable
    {
        public Barn(string path)
        {
            try
            {
                _path = path;
                _name = System.IO.Path.GetFileName(path);

                IntPtr barn = brn_OpenBarn(path);

                if (barn == (IntPtr)null)
                    throw new BarnException("Unable to open barn: " + path);

                barnHandle = barn;

                numFiles = brn_GetNumFilesInBarn(barnHandle);

                disposed = false;
            }
            catch (DllNotFoundException ex)
            {
                throw new BarnException("Unable to load the Barn library.", ex);
            }
        }

        ~Barn()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (disposed == false && barnHandle != IntPtr.Zero)
                brn_CloseBarn(barnHandle);

            disposed = true;
        }

        public uint NumberOfFiles
        {
            get { return numFiles; }
        }

        public string Path { get { return _path; } }
        public string Name { get { return _name; } }

        public string GetFileName(int index)
        {
            System.Text.StringBuilder filename = new System.Text.StringBuilder(255);
            int success = brn_GetFileName(barnHandle, index, filename, filename.Capacity + 1);

            if (success == -1)
                throw new BarnException("Unable to get file name at index " + index);

            return filename.ToString();
        }

        public int GetFileIndex(string name)
        {
            return brn_GetFileIndex(barnHandle, name);
        }

        public uint GetFileSize(int index)
        {
            int size = brn_GetFileSize(barnHandle, index);

            if (size >= 0)
                return (uint)size;

            throw new BarnException("Unable to get file size at index " + index);
        }

        public uint GetDecompressedFileSize(int index)
        {
            int size = brn_GetDecompressedFileSize(barnHandle, index);

            if (size >= 0)
                return (uint)size;

            throw new BarnException("Unable to get file size at index " + index);
        }

        public Compression GetFileCompression(int index)
        {
            int compression = brn_GetFileCompression(barnHandle, index);

            if (compression == -1)
                throw new BarnException("Unable to get file compression type at index " + index);

            return (Compression)compression;
        }

        public string GetBarnName(int index)
        {
            System.Text.StringBuilder buffer = new System.Text.StringBuilder(255);
            int success = brn_GetFileBarn(barnHandle, index, buffer, buffer.Capacity + 1);

            if (success == -1)
                throw new BarnException("Unable to get barn name at index " + index);

            return buffer.ToString();
        }

        public void ReadFile(int index, byte[] buffer)
        {
            int success = brn_ReadFileDecompress(barnHandle, index, buffer, buffer.Length);

            if (success == -4)
                throw new BarnException("Unable to open child barn");
            else if (success < 0)
                throw new BarnException("Unable to read file");
        }

        public byte[] ReadFile(int barnFileIndex, bool decompress)
        {
            try
            {
                if (decompress)
                {
                    int size = brn_GetDecompressedFileSize(barnHandle, barnFileIndex);

                    if (size < 0) return null;

                    byte[] buffer = new byte[size];
                    if (brn_ReadFileDecompress(barnHandle, barnFileIndex, buffer, size) > 0)
                    {
                        return buffer;
                    }
                }
                else
                {
                    int size = brn_GetFileSize(barnHandle, barnFileIndex);

                    if (size < 0) return null;

                    byte[] buffer = new byte[size];
                    if (brn_ReadFileRaw(barnHandle, barnFileIndex, buffer, size) > 0)
                    {
                        return buffer;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetLibBarnInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(255);

            brn_GetLibInfo(sb, sb.Capacity);

            return sb.ToString();
        }

        #region Private Members

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr brn_OpenBarn(string name);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern void brn_CloseBarn(IntPtr barn);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint brn_GetNumFilesInBarn(IntPtr barn);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern int brn_GetFileName(IntPtr barn, int index,
            [MarshalAs(UnmanagedType.LPStr)]System.Text.StringBuilder buffer, int size);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern int brn_GetFileIndex(IntPtr barn, string name);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern int brn_GetFileSize(IntPtr barn, int index);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern int brn_GetDecompressedFileSize(IntPtr barn, int index);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern int brn_GetFileCompression(IntPtr barn, int index);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern int brn_GetFileBarn(IntPtr barn, int index,
            [MarshalAs(UnmanagedType.LPStr)]System.Text.StringBuilder buffer, int size);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern int brn_ReadFileRaw(IntPtr barn, int index, byte[] buffer, int bufferSize);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern int brn_ReadFileDecompress(IntPtr barn, int index, byte[] buffer, int bufferSize);

        [DllImport("barn", CallingConvention = CallingConvention.Cdecl)]
        private static extern int brn_GetLibInfo(
            [MarshalAs(UnmanagedType.LPStr)]System.Text.StringBuilder buffer, int size);

        private string _path;
        private string _name;
        private uint numFiles;
        private IntPtr barnHandle;
        private bool disposed;

        #endregion
    }
}