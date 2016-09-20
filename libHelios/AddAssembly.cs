using System;
using System.Linq;
using System.Reflection;
using System.IO;

namespace libDynamicAssembly
{
    public static class AddAssemblies
    {
        public static Assembly AddFromPath(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (!Directory.Exists(fi.Directory.FullName))
            {
                throw new InvalidOperationException(String.Format("Directory not found {0}", fi.Directory.FullName));
            }
            try
            {
                Assembly ass = Assembly.LoadFrom(path);
                AppDomain.CurrentDomain.AssemblyResolve += ResolveAssEventHandle;
                return ass;
            }
            catch (Exception) { throw new InvalidOperationException(); }
        }

        private static Assembly ResolveAssEventHandle(object sender, ResolveEventArgs args)
        {
            string name = args.Name;
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == name);
        }
    }
}
