using System;
using Formater;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Reflection.Emit;

namespace libClassGenerator
{
    public class MyAssemblyBuilder
    {
        private AssemblyBuilder _asBuilder;
        private ModuleBuilder _modBuilder;
        private List<Type> _createdType;

        public MyAssemblyBuilder(string nameModule) {

            _createdType = new List<Type>();

            AssemblyName nameAss = new AssemblyName(
                Guid.NewGuid().ToString());
            _asBuilder = AssemblyBuilder.DefineDynamicAssembly(
                nameAss, AssemblyBuilderAccess.Save);
            _modBuilder = _asBuilder.DefineDynamicModule(
                  nameModule);
        }

        private DynamicType CreateNewType(string name)
        {
            TypeBuilder tb = _modBuilder.DefineType(
                  name,
                  TypeAttributes.Public);
            DynamicType dt = new DynamicType(tb);
            return dt;
        }

        public void AddNewType(DynamicTypeInfo dti)
        {
            DynamicType dt = CreateNewType(dti.Name);
            dt.AddFields(dti.Fields);
            Type newT = dt.CompleteType();
            _createdType.Add(newT);
        }

		public void AddNewEnum(DynamicEnumInfo dei)
		{
			DynamicEnum de = new DynamicEnum (
				            _modBuilder.DefineEnum (dei.Name, TypeAttributes.Public, typeof(int)),
							dei.ValueEnum);
			Type newT = de.CompleteEnum ();
			_createdType.Add (newT);
		}

        public void SaveAssembly(){
			_asBuilder.Save(_modBuilder.ScopeName+".dll");
        }

		public Type[] CreatedType {get { return _createdType.ToArray ();}}

        public string Namespace { get { return _modBuilder.Name; } }

    }

	public class DynamicEnum
	{
		private EnumBuilder _eb;
		private List<KeyValuePair<string,int>> _values;

		private Func<int> Nexti;

		public DynamicEnum(EnumBuilder eb)
		{
			_eb = eb;
			int i = 0;
			Nexti = delegate {
				i++;
				return i;
			};
			if (_values == null)
				_values = new List<KeyValuePair<string, int>> ();
		}

		public DynamicEnum(EnumBuilder eb,string[] values) : this(eb)
		{
			_eb = eb;
			_values = values.Select
				(x => new KeyValuePair<string,int> (x, Nexti ())).ToList ();
		}

		public DynamicEnum(EnumBuilder eb,KeyValuePair<string,int>[] values) : this(eb)
		{
			_values = values.ToList();
		}

		public bool AddValue(string name)
		{
			if (_values.All (x => x.Key != name)) {
				_values.Add(new KeyValuePair<string, int>(name,Nexti()));
			}
			return true;
		}

		public bool AddValue(string name,int value)
		{
			return true;
		}

		public Type CompleteEnum()
		{
			foreach (var i in _values) {
				_eb.DefineLiteral (i.Key, i.Value);
			}
			Type ret = _eb.CreateType ();
			return ret;
		}
	}

    public class DynamicType
    {
        private TypeBuilder _tb;
        private List<FieldBuilder> _fbl;

        public DynamicType(TypeBuilder tb) {
            _tb = tb;
            AddDefaultConstrutor();
        }

        public void AddFields(KeyValuePair<string,Type>[] list){
              foreach(var i in list) {
                  AddField(i);
              }
        }

        public void AddField(KeyValuePair<string,Type> item){
              FieldBuilder fb = null;
              fb = _tb.DefineField(
                        "_"+item.Key,
                        item.Value,
                        FieldAttributes.Private);
                AddPropriety(fb);
            _fbl.Add(fb);
        }

        private void AddPropriety(FieldBuilder fb)
        {
            string name = FormatName.FieldToProp(fb.Name);
            PropertyBuilder p = _tb.DefineProperty(name,
                  PropertyAttributes.HasDefault, fb.FieldType,
                  null);//last parms c'est params

            MethodAttributes Att =
              MethodAttributes.Public | MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig;

            MethodBuilder getter = _tb.DefineMethod("get_"+name,
                Att,fb.FieldType,Type.EmptyTypes);

            ILGenerator getIL = getter.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fb);
            getIL.Emit(OpCodes.Ret);

            MethodBuilder setter = _tb.DefineMethod("set_"+name,
                Att,null,new Type[]{fb.FieldType});

            ILGenerator setIL = setter.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fb);
            setIL.Emit(OpCodes.Ret);

            p.SetGetMethod(getter);
            p.SetSetMethod(setter);
        }

        private void AddDefaultConstrutor() {
              Type[] paramas = new Type[0];
              ConstructorBuilder cb = _tb.DefineConstructor(
                    MethodAttributes.Public |
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName,
                    CallingConventions.Standard,
                    paramas);

              ConstructorInfo objCon = typeof(object).GetConstructor(paramas);

              ILGenerator il = cb.GetILGenerator();
              il.Emit(OpCodes.Ldarg_0);
              il.Emit(OpCodes.Call, objCon);
              il.Emit(OpCodes.Ret);
        }

        public Type CompleteType() {
            // Mon constructeur pour la serialization
            Type[] paramCon = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
            ConstructorBuilder cb = _tb.DefineConstructor(
                    MethodAttributes.Public |
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName,
                    CallingConventions.HasThis,
                    paramCon);
            // Pour le construteur d'object
            Type[] paramObj = new Type[0];
            ConstructorInfo objCon = typeof(object).GetConstructor(paramObj);

            ILGenerator il = cb.GetILGenerator();
            // Importe namespace pour permettre d'utiliser les types pour serializer
            il.UsingNamespace("System.Runtime.Serilization");
            // Get la method GetValue pour retrieve mes fields ...
            MethodInfo getValue = typeof(SerializationInfo).GetMethod("GetValue");
            // Call object constructeur avec le this
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, objCon);
            // Ajoute un ligne pour assigné toutes mes fields apres la deserialization
            foreach(FieldBuilder fb in _fbl)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldstr, fb.Name);
                il.Emit(OpCodes.Newobj, fb.DeclaringType);
                il.Emit(OpCodes.Call, getValue);
                if (fb.DeclaringType.IsValueType)
                    il.Emit(OpCodes.Box, fb.FieldType);
                else
                    il.Emit(OpCodes.Castclass, fb.DeclaringType);
                il.Emit(OpCodes.Starg, fb);
            }
            il.Emit(OpCodes.Ret);
            // Ajouter la fonction GetObjectData ....
            MethodBuilder getobjectdata = _tb.DefineMethod(
                    "GetObjectData", MethodAttributes.Public | MethodAttributes.HideBySig,
                    null, paramCon);

            MethodInfo setvalue = typeof(SerializationInfo).GetMethod("AddValue");

            ILGenerator il2 = getobjectdata.GetILGenerator();

            foreach(FieldBuilder fb in _fbl)
            {
                il2.Emit(OpCodes.Ldarg_0);
                il2.Emit(OpCodes.Ldarg_1);
                il2.Emit(OpCodes.Ldstr, fb.Name);
                il2.Emit(OpCodes.Newobj, fb.FieldType);
                il2.Emit(OpCodes.Call, setvalue);
            }
            il2.Emit(OpCodes.Ret);
            return _tb.CreateType();
        }
    }

    [Serializable]
    public class DynamicTypeInfo
    {
        private string _name;
        private KeyValuePair<string,Type>[] _fields;

        public DynamicTypeInfo(string name,KeyValuePair<string,Type>[] fields)
        {
            _name = name;
            _fields = fields;
        }

        public DynamicTypeInfo(SerializationInfo info, StreamingContext cont)
        {
            _name = (string)info.GetValue("a", typeof(string));
            _fields = (KeyValuePair<string, Type>[])info.GetValue("b", typeof(KeyValuePair<string, Type>[]));
        }

        public void GetObjectData(SerializationInfo info,StreamingContext cont)
        {
            info.AddValue("a", _name, typeof(string));
            info.AddValue("b", _name, typeof(string));
        }

        public string Name {get {return _name;}}
        public KeyValuePair<string,Type>[] Fields {get {return _fields;}}

    }

	public class DynamicEnumInfo
	{
		private string _name;
		private KeyValuePair<string,int>[] _values;

		public DynamicEnumInfo(string name,params KeyValuePair<string,int>[] values)
		{
			_name = name;
			_values = values;
		}

		public DynamicEnumInfo(string name,params string[] values)
		{
			int i = -1;
			Func<int> Next = delegate {
				i++;
				return i;
			};
			_name = name;
			_values = values.Select( x => new KeyValuePair<string,int>(x,Next())).ToArray();
		}

		public string Name { get { return _name;} }

		public KeyValuePair<string,int>[] ValueEnum { get { return _values;} }
	}
}
