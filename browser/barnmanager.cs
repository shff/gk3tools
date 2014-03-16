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
			_typeMap.Add("BSP", Strings.TypeBsp);
			_typeMap.Add("MUL", Strings.TypeMul);
			_typeMap.Add("WAV", Strings.TypeWav);
			_typeMap.Add("MOD", Strings.TypeMod);
			_typeMap.Add("BMP", Strings.TypeBmp);
			_typeMap.Add("HTM", Strings.TypeHtml);
			_typeMap.Add("HTML", Strings.TypeHtml);
            _typeMap.Add("TXT", Strings.TypeTxt);
            _typeMap.Add("DOC", Strings.TypeDoc);
			_typeMap.Add("SCN", Strings.TypeScn);
            _typeMap.Add("SIF", Strings.TypeSif);
			_typeMap.Add("ACT", Strings.TypeAct);
			_typeMap.Add("ANM", Strings.TypeAnm);
			_typeMap.Add("GAS", Strings.TypeGas);
			_typeMap.Add("YAK", Strings.TypeYak);
            _typeMap.Add("EXE", Strings.TypeExe);
            _typeMap.Add("SHP", Strings.TypeShp);
            _typeMap.Add("NVC", Strings.TypeNvc);
            _typeMap.Add("FON", Strings.TypeFon);
            _typeMap.Add("CUR", Strings.TypeCur);
            _typeMap.Add("MOM", Strings.TypeMom);

            _typeMap.Add("SEQ", Strings.TypeSeq);
            _typeMap.Add("STK", Strings.TypeStk);
            
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
            foreach (var barn in _children)
                barn.Dispose();

			if (_barn != null) _barn.Dispose();
			_barn = null;
		}
		
		public static List<BarnFile> GetFiles()
		{
			List<BarnFile> files = new List<BarnFile>();
		
			for (int i = 0; i < _barn.NumberOfFiles; i++)
			{
				string filename = _barn.GetFileName(i);
				uint size = _barn.GetFileSize(i);
				Compression c = _barn.GetFileCompression(i);
				string barn = _barn.GetBarnName(i);
				
				BarnFile file = new BarnFile(i, filename, size, c, barn);
				
				files.Add(file);
			}
			
			return files;
		}
		
		public static string GetFileName(int index)
		{
			return _barn.GetFileName(index);
		}
		
		public static void Extract(int index)
		{
            string filename = _barn.GetFileName(index);
            byte[] buffer = ReadFile(index, _decompress);

            using(FileStream fs = new FileStream(_extractPath + filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Write(buffer, 0, buffer.Length);
            }
		}

        public static byte[] ExtractData(int index)
        {
            return ReadFile(index, _decompress);
        }
		
		public static string ExtractPath
		{
			get { return _extractPath; }
			set { _extractPath = value; }
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

        private static byte[] ReadFile(int index, bool decompress)
        {
            string barn = _barn.GetBarnName(index);
            if (barn == string.Empty)
                return _barn.ReadFile(index, decompress);

            // it's a child barn!
            Barn child = FindOrAddChildBarn(barn);

            string filename = _barn.GetFileName(index);
            index = child.GetFileIndex(filename);

            return child.ReadFile(index, decompress);
        }

        private static Barn FindOrAddChildBarn(string name)
        {
            foreach (var child in _children)
            {
                if (child.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return child;
            }

            // guess we need to open the barn ourselves
            string path = System.IO.Path.GetDirectoryName(_barn.Path) + "/" + name;

            Barn barn = new Barn(path);
            _children.Add(barn);

            return barn;
        }
		
		private static Barn _barn = null;
        private static List<Barn> _children = new List<Barn>();
		private static string _extractPath = String.Empty;
		private static bool _decompress = true;
		
		private static Dictionary<string, string> _typeMap;
	}
	
	public class BarnFile
	{
		public BarnFile(int index, string name, uint size, Compression compression, string barn)
		{
			_index = index;
			_name = name;
			_size = size;
			_compression = compression;
			_barn = barn;
		}
		
		public int Index
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
		
		private int _index;
		private string _name;
		private uint _size;
		private Compression _compression;
		private string _barn;
	}
}