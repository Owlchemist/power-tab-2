using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using System.Linq;
using PowerTab.UIElements;

namespace PowerTab
{
	public static class PowerTabUtility
	{
		public enum PowerType
		{
			Battery,
			Producer,
			Consumer
		}
		public static Dictionary<Building, PowerTabThing> GeneratePowerAudit(Thing SelThing)
		{
			var powerNet = SelThing.TryGetComp<CompPower>()?.PowerNet;
			if (powerNet == null) return new Dictionary<Building, PowerTabThing>();

			//Compile list
			List<CompPower> list = new List<CompPower>(powerNet.powerComps);
			list.AddRange(powerNet.batteryComps); //Consumers
			
			var powerAudit = new Dictionary<Building, PowerTabThing>();
			for (int i = list.Count; i-- > 0;)
			{
				var compPower = list[i];
				if (compPower.parent is not Building building ) continue;

				var desiredPowerOutput = DesiredPowerOutput(compPower);
				var powerType = GetPowerType(compPower, desiredPowerOutput);
				if (compPower != null && !powerAudit.ContainsKey(building) && CurrentPowerOutput(compPower, powerType, building, out float currentPowerOutput))
				{
					powerAudit.Add(building, new PowerTabThing(
                        building, 
                        currentPowerOutput, 
                        desiredPowerOutput, 
                        0f, 
                        powerType));
				}
			}
			powerAudit.GroupBy(x => x.Key.def);
			return powerAudit;
		}

		static bool CurrentPowerOutput(CompPower compPower, PowerType powerType, Building building, out float result)
		{
			switch (powerType)
			{
				case PowerType.Consumer or PowerType.Producer:
				{
					CompFlickable compFlickable = building.GetComp<CompFlickable>();
					if (compFlickable != null && !compFlickable.SwitchIsOn) result = 0;
					else
					{
						var compPowerTrader = building.GetComp<CompPowerTrader>();
						result = compPowerTrader != null ? compPowerTrader.PowerOutput : 0;
					}
					break;
				}
				case PowerType.Battery:
				{
					var compPowerBattery = building.GetComp<CompPowerBattery>();
					result = compPowerBattery != null ? result = compPowerBattery.StoredEnergy : 0;
					break;
				}
				default:
				{
					result = 0;
					break;
				}
			}
			return result != 0;
		}
		
		static float DesiredPowerOutput(CompPower compPower)
		{
			if (compPower is CompPowerBattery compPowerBattery)
			{
				return compPowerBattery.Props.storedEnergyMax;
			}
			else if (compPower is CompPowerPlant compPowerPlant)
			{
				return Mathf.Max(-compPowerPlant.Props.PowerConsumption, compPowerPlant.GetPowerPlantPotential());
			}
			else if (compPower is CompPowerTrader compPowerTrader)
			{
				return -compPowerTrader.Props.PowerConsumption;
			}
			return 0;
		}

		static float GetPowerPlantPotential(this CompPowerPlant compPowerPlant)
		{
			if (compPowerPlant.Props.basePowerConsumption != -1f) {
				return -compPowerPlant.Props.PowerConsumption;
			}
			//Store sky brightness
			var skyManager = compPowerPlant.parent.Map.skyManager;
			var curSkyGlow = skyManager.CurSkyGlow;

			//Temporarily flip to 100% and fetch the potential
			skyManager.curSkyGlowInt = 1f;
			var potential = compPowerPlant.DesiredPowerOutput;

			//Set back
			skyManager.curSkyGlowInt = curSkyGlow;
			return potential;
		}

		static PowerType GetPowerType(CompPower compPower, float desiredPowerOutput)
		{
			if (compPower is CompPowerBattery) return PowerType.Battery;
			else
			{
				return desiredPowerOutput >= 0 ? PowerType.Producer : PowerType.Consumer;
			}
		}
	}
}