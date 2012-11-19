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
		{}

        public BarnException(string message, Exception inner)
            : base(message, inner)
        {
        }
	};
	
	public class Barn : System.IDisposable
	{
		public Barn(string name)
		{
            try
            {
                this.name = name;

                IntPtr barn = brn_OpenBarn(name);

                if (barn == (IntPtr)null)
                    throw new BarnException("Unable to open barn: " + name);

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

        public string Name { get { return name; } }
		
		public string GetFileName(uint index)
		{
			System.Text.StringBuilder filename = new System.Text.StringBuilder(255);
			int success = brn_GetFileName(barnHandle, index, filename, filename.Capacity+1);
			
			if (success == -1)
				throw new BarnException("Unable to get file name at index " + index);
			
			return filename.ToString();
		}
		
		public uint GetFileSize(uint index)
		{
			int size = brn_GetFileSizeByIndex(barnHandle, index);
			
			if (size >= 0)
				return (uint)size;
			
			throw new BarnException("Unable to get file size at index " + index);
		}

        public uint GetDecompressedFileSize(uint index)
        {
            int size = brn_GetDecompressedFileSizeByIndex(barnHandle, index);

            if (size >= 0)
                return (uint)size;

            throw new BarnException("Unable to get file size at index " + index);
        }

        public uint GetDecompressedFileSize(string name)
        {
            int size = brn_GetDecompressedFileSizeByName(barnHandle, name);

            if (size >= 0)
                return (uint)size;

            throw new BarnException("Unable to get file size of " + name);
        }
        
        public uint GetFileSize(string name)
        {
            int size = brn_GetFileSizeByName(barnHandle, name);
            
            if (size > 0)
                return (uint)size;
            
            throw new BarnException("Unable to get file size of file " + name);
        }
		
		public Compression GetFileCompression(uint index)
		{
			int compression = brn_GetFileCompressionByIndex(barnHandle, index);
			
			if (compression == -1)
				throw new BarnException("Unable to get file compression type at index " + index);
			
			return (Compression)compression;
		}
		
		public string GetBarnName(uint index)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder(255);
			int success = brn_GetFileBarn(barnHandle, index, buffer, buffer.Capacity+1);
			
			if (success == -1)
				throw new BarnException("Unable to get barn name at index " + index);
			
			return buffer.ToString();
		}
		
		public uint GetOffset(uint index)
		{
			int offset = brn_GetFileOffsetByIndex(barnHandle, index);
			
			return (uint)offset;
		}
		
		public void ExtractByIndex(uint index, string outputPath, 
			bool openChildBarns, bool decompress, bool convertBitmaps)
		{
			int success = 0;
		
			if (decompress)
			//	success = brn_ExtractFileByIndex(barnHandle, index,
			//	outputPath, openChildBarns, decompress, convertBitmaps);
			success = brn_ExtractFileByIndex(barnHandle, index, outputPath,
				true, true);
				
			if (success == -3)
				throw new BarnException("invalid index");
			else if (success == -4)
				throw new BarnException("Unable to open child barn");
			else if (success == -5)
				throw new BarnException("Unable to open output file");
			else if (success != 0)
				throw new BarnException("Unable to extract file");
		}

        public void ReadFile(string filename, byte[] buffer, bool openChildBarns)
        {
            int success = brn_ReadFile(barnHandle, filename, buffer, buffer.Length, openChildBarns);

            if (success == -4)
                throw new BarnException("Unable to open child barn");
            else if (success < 0)
                throw new BarnException("Unable to read file");
        }

        public System.IO.Stream ReadFile(string filename, bool openChildBarns)
        {
            try
            {
                int size = brn_GetDecompressedFileSizeByName(barnHandle, filename);
                if (size < 0)
                    return null;

                byte[] buffer = new byte[size];
                if (brn_ReadFile(barnHandle, filename, buffer, size, openChildBarns) > 0)
                {
                    return new System.IO.MemoryStream(buffer);
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
		
		[DllImport("barn")]
		private static extern IntPtr brn_OpenBarn(string name);
		
		[DllImport("barn")]
		private static extern void brn_CloseBarn(IntPtr barn);
		
		[DllImport("barn")]
        private static extern uint brn_GetNumFilesInBarn(IntPtr barn);
		
		[DllImport("barn")]
        private static extern int brn_GetFileName(IntPtr barn, uint index,
			[MarshalAs(UnmanagedType.LPStr)]System.Text.StringBuilder buffer, int size);
		
		[DllImport("barn")]
        private static extern int brn_GetFileSizeByIndex(IntPtr barn, uint index);
        
        [DllImport("barn")]
        private static extern int brn_GetFileSizeByName(IntPtr barn, string name);

        [DllImport("barn")]
        private static extern int brn_GetDecompressedFileSizeByIndex(IntPtr barn, uint index);

        [DllImport("barn")]
        private static extern int brn_GetDecompressedFileSizeByName(IntPtr barn, string name); 

		[DllImport("barn")]
        private static extern int brn_GetFileCompressionByIndex(IntPtr barn, uint index);
		
		[DllImport("barn")]
        private static extern int brn_GetFileOffsetByIndex(IntPtr barn, uint index);
		
		[DllImport("barn")]
        private static extern int brn_GetFileBarn(IntPtr barn, uint index,
			[MarshalAs(UnmanagedType.LPStr)]System.Text.StringBuilder buffer, int size);
		
		[DllImport("barn")]
        private static extern int brn_ExtractFileByIndex(IntPtr barn,
			uint index, string outputPath, bool openChildBarns, bool decompress);

        [DllImport("barn")]
        private static extern int brn_ReadFile(IntPtr barn, string name,
            byte[] buffer, int bufferSize, bool openChildBarns);
		
		[DllImport("barn")]
		private static extern int brn_GetLibInfo(
			[MarshalAs(UnmanagedType.LPStr)]System.Text.StringBuilder buffer, int size);

        private string name;
		private uint numFiles;
		private IntPtr barnHandle;
		private bool disposed;
			
		#endregion
	}
}