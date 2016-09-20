using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using libDynamicAssembly;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace libSerialization
{
    public enum ModeSaving { Single, Many };

    [Serializable]
    public struct ParserInfo
    {
        public Type Return;
        public string Regex;
    }

    public class ParserUtil
    {

    }

    public class FolderShem
    {
        private string _fullPath;
        private string _name;
		private List<string> _files;
		private List<FolderShem> _folders;

		public FolderShem(string path,string name)
        {
			_fullPath = Path.Combine(path,name);
            _name = name;
			_files = new List<string> ();
			_folders = new List<FolderShem> ();
        }

        public string FullPath { get { return _fullPath; } }
        
        public string Name { get { return _name; } }

		public bool AddElement(string file)
		{
			if (_files.All (x => x != file)) {
				_files.Add (file);
				return true;
			}
			return false;
		}

		public bool AddElement(FolderShem folder)
		{
			if (_folders.All (x => x != folder)) {
				_folders.Add (folder);
				return true;
			}
			return false;
		}

		public string GetPath(string nameF)
		{
			if (_files.Any (x => x == nameF)) {
				return Path.Combine (_fullPath, nameF);
			}
			return "";
		}

		public string[] FilesRequired { get { return _files.ToArray ();} }

		public string[] FolderRequired { get { return _folders.Select (x => x.Name).ToArray ();} }
    
	}

    public class MySerializer
    {
        private string _path;
        private string _ext;

        public MySerializer() {
            _path = "";
            _ext = ".dat";
        }
        public string PathFolder
        {
            get { return _path; }
            set
            {
                if (Directory.Exists(_path))
                {
                    _path = value;
                }
            }
        }
        private string MakePath(string file) => Path.Combine(_path, file) + _ext;

        public bool AppendData(string filename,object[] data)
        {
            object ret = ReadData(filename);
            if(ret is Array)
            {
                Array o = (Array)ret;
                data = ObjectInfo.ConcatToObjArray(o, data);
                if (WriteData(filename, data))
                    return true;
            }   
            return false;
        }

        public bool WriteData<T>(string name,T data, FileMode f = FileMode.OpenOrCreate)
        {
            name = MakePath(name);
            using (Stream s = File.Open(name, f))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(s, data);
            }
            return true;
        }

        public object ReadData(string filename)
        {
            filename = MakePath(filename);
            object ret = null;
            if (File.Exists(filename))
            {
                using(Stream s = File.Open(filename, FileMode.Open))
                {
                    s.Seek(0, SeekOrigin.Begin);
                    BinaryFormatter bf = new BinaryFormatter();
                    ret = bf.Deserialize(s);
                }
            }
            return ret;
        }
    }

    public class RepetoryAdminitrator
    {
		private FolderShem _myshema;
        private MySerializer _seria;
        
		public RepetoryAdminitrator(FolderShem shema)
        {
            _seria = new MySerializer();
			_myshema = shema;
			_seria.PathFolder = _myshema.FullPath;

			if (!Directory.Exists(_myshema.FullPath))
            {
				Directory.CreateDirectory(_myshema.FullPath);
            }
			if (!RespectShema ())
				AdjustFolder();
            
        }

		public FolderShem ShemaFolder { get { return _myshema;}}

		public void AdjustFolder()
		{
			
		}

		public bool RespectShema()
		{
			string[] fL = Directory.GetFiles (_myshema.FullPath);
			if (fL.All (x => _myshema.FilesRequired.Any (y => x == y))) {
				// need to valid foldersheme inside to .. rec
				return true;
			}
			return false;
		}

		public bool AddFile(string pathfile)
		{
			string fn = Path.GetFileName (pathfile);
			if (_myshema.FilesRequired.Any (x => x == fn)) {
				File.Copy (pathfile, Path.Combine (_myshema.FullPath, fn));
				return true;
			}
			return false;
		}

		public bool IsFileInit(string name)
		{
			return File.Exists (Path.Combine (_myshema.FullPath, name));
		}
        public MySerializer IO { get { return _seria; } }
    }
}