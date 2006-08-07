using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using BarnLib;

namespace GK3BB
{
	public static class BarnManager
	{
		public static void OpenBarn(string filename)
		{
			Console.WriteLine("Opening barn...");
			Unload();
			
			_barn = new Barn(filename);
			
			Console.WriteLine("Opened barn");
		}
		
		public static void Unload()
		{
			if (_barn != null) _barn.Dispose();
		}
		
		public static List<BarnFile> GetFiles()
		{
			List<BarnFile> files = new List<BarnFile>();
		
			for (uint i = 0; i < _barn.NumberOfFiles; i++)
			{
				string filename = _barn.GetFileName(i);
				uint size = _barn.GetFileSize(i);
				Compression c = _barn.GetFileCompression(i);
				string barn = _barn.GetBarnName(i);
				uint offset = _barn.GetOffset(i);
				
				BarnFile file = new BarnFile(i, filename, size, c, barn, offset);
				
				files.Add(file);
			}
			
			return files;
		}
		
		private static Barn _barn = null;
	}
	
	public class BarnFile
	{
		public BarnFile(uint index, string name, uint size, Compression compression,
			string barn, uint offset)
		{
			_index = index;
			_name = name;
			_size = size;
			_compression = compression;
			_barn = barn;
			_offset = offset;
		}
		
		public uint Index
		{
			get { return _index; }
		}
		
		public string Name
		{
			get { return _name; }
		}
		
		public uint InternalSize
		{
			get { return _size; }
		}
		
		public Compression Compression
		{
			get { return _compression; }
		}
		
		public string Barn
		{
			get { return _barn; }
		}
		
		public uint Offset
		{
			get { return _offset; }
		}
		
		private uint _index;
		private string _name;
		private uint _size;
		private Compression _compression;
		private string _barn;
		private uint _offset;
	}
}