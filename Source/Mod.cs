using HarmonyLib;
using Verse;

namespace PowerTab
{
	[StaticConstructorOnStartup]
	public class Mod
	{
		public static PowerTab powerTab;
		static Mod()
		{
			new Harmony("Owlchemist.PowerTab").PatchAll();
		}
	}
}