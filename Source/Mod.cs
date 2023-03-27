using HarmonyLib;
using Verse;
using RimWorld;

namespace PowerTab
{
	[StaticConstructorOnStartup]
	public class Mod
	{
		public static PowerTab powerTab;
		public static readonly string wattDays = "WattDays".Translate();
		public static readonly string watt = "Watt".Translate();
		static Mod()
		{
			new Harmony("Owlchemist.PowerTab").PatchAll();
		}
	}
}