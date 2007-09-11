using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using BarnLib;

namespace GK3BB
{
	public static class BarnManager
	{
		static BarnManager()
		{
			// create the typemap
			_typeMap = new Dictionary<string, string>();
		
			// fill the extension/type map
			_typeMap.Add("BSP", "Room geometry");
			_typeMap.Add("MUL", "Room lightmap");
			_typeMap.Add("WAV", "WAV audio");
			_typeMap.Add("MOD", "Model geometry");
			_typeMap.Add("BMP", "Bitmap");
			_typeMap.Add("HTM", "HTML document");
			_typeMap.Add("HTML", "HTML document");
            _typeMap.Add("TXT", "Text document");
            _typeMap.Add("DOC", "Word document");
			_typeMap.Add("SCN", "Scene definition");
            _typeMap.Add("SIF", "Scene initialization");
			_typeMap.Add("ACT", "ACT file?");
			_typeMap.Add("ANM", "ANM file?");
			_typeMap.Add("GAS", "GAS file?");
			_typeMap.Add("YAK", "YAK file");
            _typeMap.Add("EXE", "Executable");
            _typeMap.Add("SHP", "Sheep script");
            _typeMap.Add("NVC", "Noun-verb-case file");
            _typeMap.Add("FON", "Font");
            _typeMap.Add("CUR", "Cursor?");
            _typeMap.Add("MOM", "Mom file?");

            _typeMap.Add("SEQ", "SEQ file");
            _typeMap.Add("STK", "Sound track file");
            
		}
	
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
			_barn = null;
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
		
		public static string GetFileName(uint index)
		{
			return _barn.GetFileName(index);
		}
		
		public static void Extract(uint index)
		{
			_barn.ExtractByIndex(index, _extractPath, true, 
				_decompress, _convertBitmaps);
		}

        public static byte[] ExtractData(string name)
        {
            byte[] buffer = new byte[_barn.GetDecompressedFileSize(name)];

            _barn.ReadFile(name, buffer, true);

            return buffer;
        }
		
		public static string ExtractPath
		{
			get { return _extractPath; }
			set { _extractPath = value; }
		}
		
		public static bool ConvertBitmaps
		{
			get { return _convertBitmaps; }
			set { _convertBitmaps = value; }
		}
		
		public static bool Decompress
		{
			get { return _decompress; }
			set { _decompress = value; }
		}
		
		public static string MapExtensionToType(string extension)
		{
			string type;
			
			if (_typeMap.TryGetValue(extension, out type) == true)
				return type;
				
			return type;
		}
		
		private static Barn _barn = null;
		private static string _extractPath = "";
		private static bool _convertBitmaps = true;
		private static bool _decompress = true;
		
		private static Dictionary<string, string> _typeMap;
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
		
		public string Extension
		{
			get
			{
				int dotIndex = _name.LastIndexOf(".");
				
				if (dotIndex == -1) return "";
				
				return _name.Substring(dotIndex+1).ToUpper();
			}
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