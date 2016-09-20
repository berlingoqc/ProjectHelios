using System;
using System.Text;
using libDynamicAssembly;
using libHelios;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace libHelios
{
	public static class TestAssembly
	{
		public static string ShowEnums(string pathAssembly)
		{
			Assembly ass = AddAssemblies.AddFromPath (pathAssembly);
			Type[] enums = ass.GetTypes ();
			enums = ass.GetTypes ()
							  .Where(x => x.IsEnum)
							  .Select(x => x).ToArray ();
			StringBuilder sb = new StringBuilder ();
			sb.AppendLine (String.Format ("{0} Enum in Assembly {1}",enums.Count(),
				ass.GetName().Name));
			foreach (Type item in enums) {
				sb.AppendFormat ("  {0} : {1}",item.Name,String.Join(" , ",item.GetEnumValues()));
				sb.AppendLine ();
			}
			return sb.ToString ();
		}
	}
}

