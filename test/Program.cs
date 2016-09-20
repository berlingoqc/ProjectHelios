using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libHelios;
using libClassGenerator;
using libDynamicAssembly;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
		static libBG.BoardGameBuilder _boardbuilder = 
			new libBG.BoardGameBuilder ("Axie&Allies");

        static void Main(string[] args)
        {
			StepOne ();
			string pInfo = TestAssembly.ShowEnums (AppDomain.CurrentDomain.BaseDirectory + "/values.dll");
			Console.WriteLine (pInfo);
			//StepTwo ();
        }

		static void StepOne()
		{
			// Create the values.dll files ... 
			// with the enum of my game and the class for the stats of units
			// because that is all the type i need to add to use them in field
			// of the main types of the game ...
			libBG.TypeBuilderBG valueBuilder = (libBG.TypeBuilderBG)_boardbuilder.NextStep(1);
			Dictionary<string,string[]> vEnum = new Dictionary<string, string[]> ();
			vEnum.Add ("Alliance", new[] { "Axies","Allies","Neutral" });
			vEnum.Add ("Color", new[] { "Tan","Green","Red","Black","Orange"});
			vEnum.Add ("TypeZone", new[] { "Water,Land" });
			vEnum.Add ("TypeUnit", new[] { "Air","Naval","Land"});
			vEnum.Add ("TypeCity", new[] { "Major", "Minor" });

			foreach (var i in vEnum) {
				valueBuilder.Builder.AddNewEnum(
					new DynamicEnumInfo(i.Key,i.Value));
			}
			valueBuilder.Builder.AddNewEnum (
				new DynamicEnumInfo (
					"Victory",
					new KeyValuePair<string, int>[] {
						new KeyValuePair<string, int> ("Minor", 6),
						new KeyValuePair<string, int> ("Major", 8)
					}));
			
			_boardbuilder.ValuesAssembly (valueBuilder);

		}

		static void StepTwo()
        {

            List<KeyValuePair<string, Type>> list = new List<KeyValuePair<string, Type>>();
            List<DynamicTypeInfo> dti = new List<DynamicTypeInfo>();

            list.Add(new KeyValuePair<string, Type>("Name", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Continent", typeof(string[])));

            dti.Add(new DynamicTypeInfo("World", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("ID", typeof(int)));
            list.Add(new KeyValuePair<string, Type>("Name", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("TypeZone", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("BelongIn", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Value", typeof(int)));

            dti.Add(new DynamicTypeInfo("Zone", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("ID", typeof(int)));
            list.Add(new KeyValuePair<string, Type>("Type", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Name", typeof(string)));

            dti.Add(new DynamicTypeInfo("City", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("ID", typeof(int)));
            list.Add(new KeyValuePair<string, Type>("ID_Zone_Control", typeof(int[])));
            list.Add(new KeyValuePair<string, Type>("ID_Zone_Access", typeof(int[])));

            dti.Add(new DynamicTypeInfo("Canal", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("Name", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Alliance", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Color", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Country", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("UnitName", typeof(KeyValuePair<string, string>)));

            dti.Add(new DynamicTypeInfo("Faction", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("Name", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Country", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Description", typeof(string)));

            dti.Add(new DynamicTypeInfo("FactionAbility", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("Name", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("UnitsCanUse", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Description", typeof(string)));

            dti.Add(new DynamicTypeInfo("UnitAbility", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("ID", typeof(int)));
            list.Add(new KeyValuePair<string, Type>("Name", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Description", typeof(string)));

            dti.Add(new DynamicTypeInfo("Technologie", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("Name", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("IsCombat", typeof(bool)));
            list.Add(new KeyValuePair<string, Type>("Type", typeof(string)));
            list.Add(new KeyValuePair<string, Type>("Name_UnitAbility", typeof(string[])));
            list.Add(new KeyValuePair<string, Type>("Move", typeof(int)));
            list.Add(new KeyValuePair<string, Type>("Cost", typeof(int)));
            list.Add(new KeyValuePair<string, Type>("Attack", typeof(int)));
            list.Add(new KeyValuePair<string, Type>("Defense", typeof(int)));

            dti.Add(new DynamicTypeInfo("Unit", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("ID_Zone", typeof(int)));
            list.Add(new KeyValuePair<string, Type>("Units", typeof(KeyValuePair<string, int>)));

            dti.Add(new DynamicTypeInfo("StartingGroup", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("ID_StartingGroups", typeof(int[])));
            list.Add(new KeyValuePair<string, Type>("ID_Army", typeof(int)));
            list.Add(new KeyValuePair<string, Type>("IPC", typeof(int)));

            dti.Add(new DynamicTypeInfo("Technologie", list.ToArray()));
            list.Clear();

            list.Add(new KeyValuePair<string, Type>("OrderArmy", typeof(string[])));
            list.Add(new KeyValuePair<string, Type>("Sequence", typeof(string[][])));

            dti.Add(new DynamicTypeInfo("GameStructure", list.ToArray()));
            list.Clear();
        }
    }
}
