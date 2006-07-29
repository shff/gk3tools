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

namespace Barn
{
	class BarnException : System.Exception
	{
		public BarnException(string message)
			: base(message)
		{}
	};
	
	class Barn : System.IDisposable
	{
		public Barn(string name)
		{
			IntPtr barn = brn_OpenBarn(name);
			
			if (barn == (IntPtr)null)
				throw new BarnException("Unable to open barn: " + name);
			
			barnHandle = new HandleRef(this, barn);
			
			numFiles = brn_GetNumFilesInBarn(barnHandle);
			
			disposed = false;
		}
		
		~Barn()
		{
			Dispose();
		}
		
		public void Dispose()
		{
			if (disposed == false)
				brn_CloseBarn(barnHandle);
			
			disposed = true;
		}
		
		public uint NumberOfFiles
		{
			get { return numFiles; }
		}
		
		public string GetFileName(uint index)
		{
			byte[] filename = new byte[256];
			int success = brn_GetFileName(barnHandle, index, filename, 255);
			
			if (success == 0)
			{
				System.Text.UTF7Encoding enc = new System.Text.UTF7Encoding();
				return enc.GetString(filename);
			}
			
			// still here? something bad must have happened
			throw new BarnException("Unable to get file name at index " + index);
		}
		
		#region Private Members
		
		[DllImport("barn")]
		private static extern IntPtr brn_OpenBarn(string name);
		
		[DllImport("barn")]
		private static extern void brn_CloseBarn(HandleRef barn);
		
		[DllImport("barn")]
		private static extern uint brn_GetNumFilesInBarn(HandleRef barn);
		
		[DllImport("barn")]
		private static extern int brn_GetFileName(HandleRef barn, uint index, byte[] buffer, int size);
		
		private uint numFiles;
		private HandleRef barnHandle;
		private bool disposed;
			
		#endregion
	}
}