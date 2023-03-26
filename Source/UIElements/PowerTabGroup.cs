using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PowerTab.UIElements
{
	public class PowerTabGroup
	{
		public string label;
		public int count, power;
		public float barFill, parentTabWidth;
		public List<PowerTabThing> children;
		public bool expanded, isBattery;
		readonly Action<PowerTabGroup> ifButtonPressed;
		public PowerTabUtility.PowerType powerType;
		static float SelfHeight => Text.SmallFontHeight + GenUI.GapTiny * 2 + 2;
		public float Height => SelfHeight + (expanded ? count * PowerTabThing.Height : 1);

		public PowerTabGroup(List<PowerTabThing> children, float parentTabWidth, bool expanded,  Action<PowerTabGroup> ifButtonPressed = null)
		{
			this.label = children.First().thing.def.LabelCap;
			this.count = children.Count();
			this.power = children.Sum(t => t.currentPowerOutput);
			this.barFill = power / children.Sum(t => t.desiredPowerOutput);
			this.parentTabWidth = parentTabWidth;
			this.expanded = expanded;
			this.ifButtonPressed = ifButtonPressed;
			this.children = children;
			this.powerType = children.First().powerType;
			this.isBattery = powerType == PowerTabUtility.PowerType.Battery;
		}
		
		public void Draw(float y)
		{
			Rect mainRect = new Rect(3, y, parentTabWidth - GenUI.GapTiny * 3 - GenUI.ScrollBarWidth, Text.SmallFontHeight + GenUI.GapTiny * 2);
			Widgets.DrawOptionSelected(mainRect);
			
			Rect buttonRect = new Rect(5, y + 1, GenUI.ListSpacing, GenUI.ListSpacing);
			if (Widgets.ButtonText(buttonRect.ContractedBy(2), expanded ? "-" : "+"))
			{
				ifButtonPressed.Invoke(this);
				Mod.powerTab.BuildCache();
			}

			Rect labelRect = new Rect(38, y + 4, parentTabWidth / 2.5f, Text.SmallFontHeight);
			Widgets.Label(labelRect, $"{count} {label}");
			
			Rect barRect = new Rect(parentTabWidth / 2.5f + 43, y + 1, parentTabWidth / 2 - 25, GenUI.ListSpacing);
			Widgets.FillableBar(barRect.ContractedBy(2), Mathf.Clamp(barFill, 0, 1)); 
			
			var unit = isBattery ? "Wd" : "W";
			string powerDrawStr = $" {power} {unit} ";
			float textWidth = Text.CalcSize(powerDrawStr).x; // Calculate here instead of using cache since the numbers can change fast, and the cache can become outdated, leading to minor graphical issues.
			
			Rect wattBkgRect = new Rect(parentTabWidth / 2.5f + 43, y + 1, textWidth + 16, GenUI.ListSpacing);
			Widgets.DrawRectFast(wattBkgRect.ContractedBy(GenUI.GapTiny * 1.5f), Color.black);

			Rect wattLabelRect = new Rect(wattBkgRect.x + 9, y + 5, textWidth /*Small buffer to prevent potential overflow*/, GenUI.ListSpacing);
			Widgets.Label(wattLabelRect, powerDrawStr);

			y += SelfHeight;

			if (!expanded) return;
			for (int i = children.Count; i-- > 0;)
			{
				PowerTabThing powerTabThing = children[i];
				powerTabThing.Draw(y);
				y += PowerTabThing.Height;
			}
		}
	}
}