using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace libDynamicAssembly
{
    public class DynamicCalling
    {
        private Type _t;
        private object _instance;

        public DynamicCalling(Type t)
        {
            _t = t;
            _instance = null;
        }

        public object InitTypeFromProprety(KeyValuePair<string,object>[] paramas)
        {
            _instance = Activator.CreateInstance(_t);
            PropertyInfo[] piL = _t.GetProperties();
            KeyValuePair<string, object> ite;
            foreach (var fi in piL)
            {
                ite = paramas.FirstOrDefault(x => x.Key == fi.Name);
                if (ite.Equals(new KeyValuePair<string, object>()))
                {
                    _instance = null;
                    return null;
                }
                fi.SetValue(_instance, ite.Value);
            }
            return _instance;
        }

        public object Invoke(string nameMember, object[] paramas)
        {
            return Invoke(_instance, nameMember, paramas);
        }

        public object Invoke(object o,string nameMember,object[] paramas)
        {
            MemberInfo mi = _t.GetMembers().FirstOrDefault(x => x.Name == nameMember);
            if (mi != null && o != null)
            {
                if (mi is MethodInfo)
                {
                    return ((MethodInfo)mi).Invoke(o, paramas);
                }
                else if (mi is PropertyInfo)
                {
                    return ((PropertyInfo)mi).GetValue(o);
                }
                else if (mi is FieldInfo)
                {
                    return ((FieldInfo)mi).GetValue(0);
                }
            }
            return null;
        }

        public object Instance
        {
            get
            {
                return _instance;
            }
        }

        public string Name { get { return _t.Name; } }

        public KeyValuePair<string,object>[] InvokeAllProprety()
        {
            if (_instance == null)
                return null;
            return _t.GetProperties().Select(x=> new KeyValuePair<string,object>(x.Name,x.GetValue(_instance))).ToArray();
        }
    }

    public class DynamicAssembly
    {
        private Assembly _ass;
        private List<DynamicCalling> _callL;
        public DynamicAssembly(Assembly ass,bool dynamicAllType=true)
        {
            _ass = ass;
            if (dynamicAllType)
                _callL = _ass.GetTypes().Select(x=>new DynamicCalling(x)).ToList();
            else
                _callL = new List<DynamicCalling>();
        }



        public DynamicCalling AccessDynamic(string name)
        {
            return _callL.FirstOrDefault(x => x.Name == name);
        }
        
    }

    public static class ObjectInfo
    {
        public static object[] ConcatToObjArray(params Array[] arrs)
        {
            Type t = null;
            object[] e = null;
            int indice = 0;
            if(arrs.Length > 0)
            {
                t = TryGetArrayType(arrs.FirstOrDefault());
                if(t!= null)
                {
                    e = new object[arrs.Sum(x=>x.Length)];
                    foreach(Array a in arrs)
                    {
                        foreach(object i in a)
                        {
                            if (i.GetType() != t)
                                return null;
                            e[indice] = i; 
                            indice++; 
                        }
                    }
                    return e;
                }
            }
            return null;
        }

        public static Type TryGetArrayType(Array arr)
        {
            string s = arr.GetType().Name;
            s = s.Replace("[]", "");
            string[] ar = s.Split('.');
            string aaaa = ar.LastOrDefault();
            if (aaaa != "")
            {
                return GetTypesAppDomain(x => x.Name == aaaa, y => true).FirstOrDefault();
            }
            return null;
        }

        public static Type[] GetTypesAppDomain(Func<Type, bool> funcT, Func<Assembly, bool> funcA)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => funcA(x))
                   .SelectMany(x => x.GetTypes()).Where(x => funcT(x)).ToArray();
        }

        public static Type[] DiffTypeFieldAcrossNamespace(string ns, Func<Type, bool> func)
        {
            return GetTypesAppDomain(x => func(x), x => x.GetName().Name == ns)
                   .SelectMany(x => x.GetFields().Select(y => y.FieldType))
                   .Distinct().ToArray();
        }

        public static bool AllTypesInsideNamespace(string namspace,Type[] tL)
        {
            return GetTypesAppDomain(
                x => true,
                x => x.GetName().Name == namspace)
                .All(
                x => tL.Any(y => x == y));
        }
    }
}
