using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PowerTab.UIElements
{
	public class PowerTabThing
	{
		public Thing thing; // Represents a battery, producer, or consumer
		public int currentPowerOutput;
		public float parentTabWidth, desiredPowerOutput;
		readonly float barFill;
		public readonly bool isBattery;
		public PowerTabUtility.PowerType powerType;

		public static float Height => GenUI.ListSpacing + GenUI.GapTiny;

		public PowerTabThing(Thing thing, float currentPowerOutput, float desiredPowerOutput, float parentTabWidth, PowerTabUtility.PowerType powerType)
		{
			this.thing = thing;
			this.currentPowerOutput = (int)System.Math.Round(currentPowerOutput);
			this.desiredPowerOutput = desiredPowerOutput;
			this.barFill = currentPowerOutput / desiredPowerOutput;
			this.parentTabWidth = parentTabWidth;
			this.powerType = powerType;
			this.isBattery = powerType == PowerTabUtility.PowerType.Battery;
		}

		public void Draw(float y)
		{
			parentTabWidth = Mod.powerTab.innerSize.x;
			
			Rect mainRect = new Rect(0, y, parentTabWidth - GenUI.GapTiny * 3 - GenUI.ScrollBarWidth, GenUI.ListSpacing);
			Widgets.DrawHighlightIfMouseover(mainRect);
			if (Widgets.ButtonInvisible(mainRect)) CameraJumper.TryJumpAndSelect(new GlobalTargetInfo(thing));

			Rect iconRect = new Rect(0, y, GenUI.ListSpacing, GenUI.ListSpacing);
			Widgets.ThingIcon(iconRect, thing);

			Rect labelRect = new Rect(35, y + 3, parentTabWidth / 2.5f, Text.SmallFontHeight); // Not dynamic width because area to right contains bar and watt info
			Widgets.Label(labelRect, thing.LabelCap);

			Rect barRect = new Rect(parentTabWidth / 2.5f + 40, y, parentTabWidth / 2 - 25, GenUI.ListSpacing);
			Widgets.FillableBar(barRect.ContractedBy(2), Mathf.Clamp(barFill, 0, 1)); 

			string powerDrawStr = $"{currentPowerOutput} " + (isBattery ? "Wd" : "W");
			float textWidth = Text.CalcSize(powerDrawStr).x; // Calculate here instead of using cache since the numbers can change fast, and the cache can become outdated, leading to minor graphical issues.
			
			Rect wattBkgRect = new Rect(parentTabWidth / 2.5f + 40, y, textWidth + 16, GenUI.ListSpacing);
			Widgets.DrawRectFast(wattBkgRect.ContractedBy(GenUI.GapTiny * 1.5f), Color.black);

			Rect wattLabelRect = new Rect(wattBkgRect.x + 6, y + 3, textWidth /*Small buffer to prevent potential overflow*/, GenUI.ListSpacing);
			Widgets.Label(wattLabelRect, powerDrawStr);
		}
	}
}