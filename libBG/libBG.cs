using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using libDynamicAssembly;
using libSerialization;
using libClassGenerator;

namespace libBG
{
	/// <summary>
	///  object to create the first dll with the enum and the struct that gonna be
	/// as type for the field of the main types of the board game ...
	/// </summary>
	public class TypeBuilderBG
	{
		private MyAssemblyBuilder _assBuilder;
		private string _name;

		public TypeBuilderBG(string name)
		{
			_name = name;
			_assBuilder = new MyAssemblyBuilder (name);
		}

		public MyAssemblyBuilder Builder { get { return _assBuilder; }}

	}

	public class Flow
	{

		public Flow()
		{
			
		}
	}

	public class ObjectCreator
	{
		private RepetoryAdminitrator _repo;
		private Assembly[] _assW;

		public ObjectCreator(RepetoryAdminitrator repo,Assembly[] ass)
		{
			_repo = repo;
			_assW = ass;
		}


		public bool AddObject(string nameType,KeyValuePair<string,object>[] paramams)
		{


			return true;
		}

		public bool AddObject(object obj)
		{
			return true;
		}

		public bool AddObjects(params object[] objA)
		{
			IEnumerable<Type> t = objA.Select(x => x.GetType());
			if (ObjectInfo.AllTypesInsideNamespace("", t.ToArray()))
			{
				object[][] groupedByType = GroupByType(objA);
				foreach(object[] arr in groupedByType)
				{
					if (arr.Length > 0)
						_repo.IO.AppendData(arr.First().GetType().Name, arr);
				}
			}
			return true;
		}

		private object[][] GroupByType(object[] objs)
		{
			List<KeyValuePair<Type, List<object>>> ret = new List<KeyValuePair<Type, List<object>>>();
			var cond = new KeyValuePair<Type, List<object>>();
			foreach (object obj in objs)
			{
				var tmp = ret.FirstOrDefault(x => x.Key == obj.GetType());
				if(tmp.Equals(cond))
				{
					ret.Add(new KeyValuePair<Type, List<object>>(obj.GetType(), new List<object>() { obj }));
				}
				else
				{
					tmp.Value.Add(obj);
				}
			}
			return ret.Select(x=>x.Value.ToArray()).ToArray();
		}

		 
	}

    public class BoardGameBuilder
    {
        private string _nameBG;
        private RepetoryAdminitrator _repo;

        public BoardGameBuilder(string nameBg)
        {
            _nameBG = nameBg;

			var shema = new FolderShem ("/home/wquintal/", nameBg);
			string[] files = new string[] { "values.dll","types.dll","flow.dat","rules.dll"};
			foreach (string i in files)
				shema.AddElement (i);
			_repo = new RepetoryAdminitrator (shema);

        }

		public Type[] GetTypes(string name){
			string p = _repo.ShemaFolder.GetPath (name);
			Assembly ass = AddAssemblies.AddFromPath (p);
			return ass.GetTypes ();
		}

		public object NextStep(int num)
		{
			object ret = null;
			switch (num) {
			case 1:
				ret = new TypeBuilderBG ("values");
				break;
			case 2:
				ret = new TypeBuilderBG ("types");
				break;
			default: break;
			}
			return ret;
		}

		public void ValuesAssembly(TypeBuilderBG bvg) => endBuilder(bvg);

		public void TypesAssemmbly (TypeBuilderBG bvg) => endBuilder(bvg);

		private void endBuilder(TypeBuilderBG b)
		{
			b.Builder.SaveAssembly ();
			//string pFile = 
			//_repo.AddFile (p);
		}
    }

}
