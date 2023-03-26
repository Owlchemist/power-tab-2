using System.Collections.Generic;
using System.Linq;
using PowerTab.UIElements;
using RimWorld;
using UnityEngine;
using Verse;

namespace PowerTab
{
    public class PowerTab : ITab
    {
        const float LeftMargin = 5,
            RightMargin = 2,
            TopMargin = 30,
            BottomMargin = 5;
        public Vector2 scrollPos;
        public float lastY;
        public static Dictionary<Building, PowerTabThing> powerAudit;
        IGrouping<PowerTabUtility.PowerType, PowerTabGroup>[] categoryCache;
        List<PowerTabGroup> groupCache;
        public Vector2 innerSize;
        readonly HashSet<ushort> groupCollapsed;
        Thing lastSelectedThing;
        
        public PowerTab()
        {
            size = new Vector2(450f, 550f);
            innerSize = new Vector2(size.x - (LeftMargin + RightMargin), size.y - (TopMargin + BottomMargin));
            labelKey = "PowerSwitch_Power";
            groupCollapsed = new HashSet<ushort>();
        }
        
        public override void OnOpen()
        {
            Mod.powerTab = this;
            BuildCache();
        }

        public override void CloseTab()
        {
            powerAudit = null; //Free rmemory
            Mod.powerTab = null;
        }

        public override void FillTab()
        {
            //Update about every 5 seconds
            if (lastSelectedThing != SelThing || Current.gameInt.tickManager.ticksGameInt % 300 == 25) {
                BuildCache();
            }

            lastSelectedThing = SelThing;

            Widgets.BeginScrollView(
                outRect: new Rect(new Vector2(LeftMargin, TopMargin),new Vector2(innerSize.x, innerSize.y)).ContractedBy(GenUI.GapTiny),
                scrollPosition: ref scrollPos,
                viewRect: new Rect(default, new Vector2(innerSize.x - GenUI.GapTiny * 2 - GenUI.ScrollBarWidth, lastY)) 
            );
            
            float y = 10;
            
            foreach (IGrouping<PowerTabUtility.PowerType, PowerTabGroup> category in categoryCache)
            {
                var key = category.Key;
                var powerTabThings = new List<PowerTabThing>();
                float currentPowerOutput = 0f, desiredPowerOutput = 0f;
                foreach (var item in powerAudit)
                {
                    var value = item.Value;
                    if (value.powerType == key)
                    {
                        powerTabThings.Add(value);
                        currentPowerOutput += value.currentPowerOutput;
                        desiredPowerOutput += value.desiredPowerOutput;
                    }
                }

                var children = new List<PowerTabGroup>();
                for (int i = groupCache.Count; i-- > 0;)
                {
                    var powerTabGroup = groupCache[i];
                    if (powerTabGroup.powerType == key) children.Add(powerTabGroup);
                }

                PowerTabCategory powerTabCategory = new PowerTabCategory(
                    key,
                    currentPowerOutput,
                    currentPowerOutput / desiredPowerOutput,
                    children,
                    innerSize.x,
                    key == PowerTabUtility.PowerType.Battery);
                
                powerTabCategory.Draw(y);
                y += powerTabCategory.Height;
            }

            lastY = y;
            Widgets.EndScrollView();
        }

        public void BuildCache()
        {
            powerAudit = PowerTabUtility.GeneratePowerAudit(SelThing);

            groupCache = new List<PowerTabGroup>();
            foreach (IGrouping<ThingDef, KeyValuePair<Building, PowerTabThing>> group in powerAudit.GroupBy(t => t.Key.def))
            {
                var hash = group.Key.shortHash;
                
                groupCache.Add(new PowerTabGroup(
                    children: new List<PowerTabThing>(group.Select(x => x.Value)),
                    parentTabWidth: innerSize.x,
                    expanded: groupCollapsed.Contains(hash),
                    ifButtonPressed: delegate{
                        if (groupCollapsed.Contains(hash)) groupCollapsed.Remove(hash);
                        else groupCollapsed.Add(hash);
                    }
                ));
            }
            
            groupCache.SortByDescending(t => Mathf.Abs(t.power));

            //Compile and categories and send them off to the drawer
            categoryCache = groupCache.GroupBy(t => t.powerType).OrderBy(x => x.Key.ToString()).ToArray();
        }
    }
}