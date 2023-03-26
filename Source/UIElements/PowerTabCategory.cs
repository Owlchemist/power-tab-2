using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PowerTab.UIElements
{
	public class PowerTabCategory
	{
		PowerTabUtility.PowerType label;
		public int power;
		readonly float barFill, parentTabWidth;
		readonly List<PowerTabGroup> children;
		readonly bool isBattery;
		public float Height => 25 + children.Sum(t => t.Height) + 11;

		public PowerTabCategory(PowerTabUtility.PowerType label, float power, float barFill, List<PowerTabGroup> children, float parentTabWidth, bool isBattery = false)
		{
			this.label = label;
			this.power = (int)System.Math.Round(power);
			this.barFill = barFill;
			this.children = children;
			this.parentTabWidth = parentTabWidth;
			this.isBattery = isBattery;
		}
		
		public void Draw(float y)
		{
			Widgets.ListSeparator(ref y, parentTabWidth, label.ToString().Translate()); // y += 25 from ref
			
			Rect barRect = new Rect(parentTabWidth / 4.5f + 40, y - 30, parentTabWidth / 1.45f - 25, GenUI.ListSpacing);
			Widgets.FillableBar(barRect.ContractedBy(2), Mathf.Clamp(barFill, 0, 1)); 
			
			string powerDrawStr = $"{power} " + (isBattery ? "Wd" : "W");
			float textWidth = Text.CalcSize(powerDrawStr).x; // Calculate here instead of using cache since the numbers can change fast, and the cache can become outdated, leading to minor graphical issues.
			
			Rect wattBkgRect = new Rect(parentTabWidth / 4.5f + 40, y - 30, textWidth + 16, GenUI.ListSpacing);
			Widgets.DrawRectFast(wattBkgRect.ContractedBy(GenUI.GapTiny * 1.5f), Color.black);

			Rect wattLabelRect = new Rect(wattBkgRect.x + 6, y - 26, textWidth /*Small buffer to prevent potential overflow*/, GenUI.ListSpacing);
			Widgets.Label(wattLabelRect, powerDrawStr);
			
			for (int i = children.Count; i-- > 0;)
			{
				PowerTabGroup child = children[i];
				if (Mod.powerTab.lastY == 0 || child.expanded || (y > Mod.powerTab.scrollPos.y - 25f && y < Mod.powerTab.scrollPos.y + Mod.powerTab.size.y))
				{
					child.Draw(y + 3);
				}
				y += child.Height;
			}
		}
	}
}