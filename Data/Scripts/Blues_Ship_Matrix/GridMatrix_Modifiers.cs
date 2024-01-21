using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
//Sandboxs
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using Sandbox.Definitions;
//Vrage
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Network;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Game.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using VRage.Network;
using VRage.Sync;
using Blues_Ship_Matrix;

namespace Blues_Ship_Matrix
{
    public static class Modify
    {
		public static float ShouldRececiveModifiers(float bonus, IMyCubeGrid CoreGrid, MyGridLimit CoreGridClass)
		{
			if (bonus <= 1.0f)
			{
				return 1f;
			}

			List<float> ratios = new List<float>();

			Action<float, float> addRatio = (value, maxLimit) =>
			{
				float ratio = value / maxLimit;
				if (ratio < 0.5f)
				{
					ratios.Add(ratio);
				}
			};

			addRatio((CoreGrid as MyCubeGrid).BlocksCount, CoreGridClass.MaxBlocks);
			addRatio(Convert.ToSingle((CoreGrid as MyCubeGrid).GetCurrentMass()), Convert.ToSingle(CoreGridClass.MaxMass));
			addRatio((CoreGrid as MyCubeGrid).BlocksPCU, CoreGridClass.MaxPCU);

			if (ratios.Count == 0)
			{
				return bonus;
			}

			float averageRatio = ratios.Sum() / ratios.Count;
			float giveBonus = averageRatio * bonus;

			// Ensure the returned value is greater than 1.0
			return giveBonus > 1.0f ? giveBonus : 1.0f;
		}


		//Manuverability
		public static  string Thrusters(IMyCubeGrid CoreGrid, MyGridLimit CoreGridClass)
		{
			IEnumerable<IMyThrust> ThrustEnum =CoreGrid.GetFatBlocks<IMyThrust>();
			List<IMyThrust> Thrusters = ThrustEnum.ToList();
			float Force=ShouldRececiveModifiers(CoreGridClass.Modifiers.Thruster_Force,CoreGrid,CoreGridClass);
			float Efficiency=ShouldRececiveModifiers(CoreGridClass.Modifiers.Thruster_Efficiency,CoreGrid,CoreGridClass);
			foreach(IMyThrust Thruster in Thrusters)
			{
					if(Thruster==null){continue;}
					Thruster.ThrustMultiplier=Force;
					Thruster.PowerConsumptionMultiplier = Efficiency;
			}
			if(Force==1f&&Efficiency==1f){return("");}
			return("\n>	Thrust Force: "+Force+"\n>	Thrust Efficiency: "+(Efficiency/Force));
		}
		public static  string Gyros(IMyCubeGrid CoreGrid, MyGridLimit CoreGridClass)
		{
			IEnumerable<IMyGyro> GyroEnum =CoreGrid.GetFatBlocks<IMyGyro>();
			List<IMyGyro> Gyroscopes = GyroEnum.ToList();
			float Strength =ShouldRececiveModifiers(CoreGridClass.Modifiers.Gyro_Force,CoreGrid,CoreGridClass);
			float Efficiency =ShouldRececiveModifiers(CoreGridClass.Modifiers.Gyro_Efficiency,CoreGrid,CoreGridClass);
			foreach(IMyGyro Gyro in Gyroscopes)
			{
					if(Gyro==null){continue;}
					Gyro.GyroStrengthMultiplier=Strength;
					Gyro.PowerConsumptionMultiplier = Efficiency;
			}
			if(Strength==1f&&Efficiency==1f){return("");}
			return("\n>	Gyro Force: "+Strength+"\n>	Gyro Efficiency: "+(Efficiency/Strength));
		}
		//Production
		public static  string Refineries(IMyCubeGrid CoreGrid, MyGridLimit CoreGridClass)
		{
			IEnumerable<IMyRefinery> Refineries =CoreGrid.GetFatBlocks<IMyRefinery>();
			foreach(IMyCubeBlock ProductionBlock in Refineries)
			{
					if(ProductionBlock==null){continue;}
					ProductionBlock.UpgradeValues["Productivity"]=CoreGridClass.Modifiers.Refine_Speed;
					ProductionBlock.UpgradeValues["Effectiveness"]=CoreGridClass.Modifiers.Refine_Efficiency;
					ProductionBlock.UpgradeValues["PowerEfficiency"]=CoreGridClass.Modifiers.Refine_Speed;
			}
			if(CoreGridClass.Modifiers.Refine_Speed==1f&&CoreGridClass.Modifiers.Refine_Efficiency==1f){return("");}
			return("\n>	Refineries:\n >Speed:"+CoreGridClass.Modifiers.Refine_Speed+"\n >Efficiency:"+CoreGridClass.Modifiers.Refine_Efficiency+"\n >Power Efficiency:"+(CoreGridClass.Modifiers.Refine_Speed*0.5f));
		}
		public static  string Assemblers(IMyCubeGrid CoreGrid, MyGridLimit CoreGridClass)
		{
			IEnumerable<IMyAssembler> Assemblers =CoreGrid.GetFatBlocks<IMyAssembler>();
			foreach(IMyCubeBlock ProductionBlock in Assemblers.ToList())
			{
					if(ProductionBlock==null){continue;}
					ProductionBlock.UpgradeValues["Productivity"]=CoreGridClass.Modifiers.Assembler_Speed;
					ProductionBlock.UpgradeValues["PowerEfficiency"]=CoreGridClass.Modifiers.Assembler_Speed;
			}
			if(CoreGridClass.Modifiers.Assembler_Speed==1f){return("");}
			return("\n>	Assemblers:\n >Speed:"+CoreGridClass.Modifiers.Assembler_Speed+"\n >Power Efficiency:"+(CoreGridClass.Modifiers.Assembler_Speed));
		}
		//Power
		public static  string Reactors(IMyCubeGrid CoreGrid, MyGridLimit CoreGridClass)
		{
			IEnumerable<IMyReactor> Reactors =CoreGrid.GetFatBlocks<IMyReactor>();
			float Power = ShouldRececiveModifiers(CoreGridClass.Modifiers.Power_Producers_Output,CoreGrid,CoreGridClass);
			foreach(IMyReactor  MyPowerBlock in Reactors.ToList())
			{
					if(MyPowerBlock==null){continue;}
					MyPowerBlock.PowerOutputMultiplier=Power;//Power_Producers_Output
			}
			if(Power==1f){return("");}
			return("\n>	Reactor Output: "+Power);
		}
		public static  string Drills(IMyCubeGrid CoreGrid, MyGridLimit CoreGridClass)
		{
			IEnumerable<IMyShipDrill> Drills =CoreGrid.GetFatBlocks<IMyShipDrill>();
			float OreOutput =ShouldRececiveModifiers(CoreGridClass.Modifiers.Drill_Harvest_Mutiplier,CoreGrid,CoreGridClass);
			foreach(IMyShipDrill  MyDrillBlock in Drills.ToList())
			{
					if(MyDrillBlock==null){continue;}
					MyDrillBlock.DrillHarvestMultiplier=OreOutput;//Power_Producers_Output
					//MyDrillBlock.SensorRadius=10.0f
			}
			if(OreOutput==1f){return("");}
			return("\n>	Drill Output: "+OreOutput);
		}
		/*public static  void PowerStorage(IMyCubeGrid CoreGrid, MyGridLimit CoreGridClass)
		{
			IEnumerable<IMyBatteryBlock> Batteries =CoreGrid.GetFatBlocks<IMyBatteryBlock>();
			foreach(IMyBatteryBlock MyPowerBlock in Batteries.ToList())
			{
					if(MyPowerBlock==null){continue;}
					(MyPowerBlock as MyBatteryBlock).BlockDefinition.MaxStoredPower*=2;//Power_Producers_Output
			}
		}*/
		//Tools

		public static void GridClassDamageHandler(object target,ref MyDamageInformation DamageInfo)
		{
			if(target is IMySlimBlock)
			{
				IMySlimBlock MyBlock = target as IMySlimBlock;

				if(MyBlock==null){return;}
				IMyCubeGrid MyGrid = MyBlock.CubeGrid;
				IEnumerable<IMyBeacon> Beacons = MyGrid.GetFatBlocks<IMyBeacon>();
				List<IMyBeacon> BeaconsList=Beacons.ToList();
				if(BeaconsList.Count>0)
				{
					IMyBeacon CoreBeacon=BeaconsList.First();
					if (CoreBeacon?.GameLogic?.GetAs<ShipCore>() != null)
					{
						var MyShipCore = CoreBeacon?.GameLogic?.GetAs<ShipCore>();
						if(DamageInfo.Type==MyDamageType.Bullet){DamageInfo.Amount*=MyShipCore.CoreGridClass.DamageModifiers.Bullet;}
						if(DamageInfo.Type==MyDamageType.Rocket){DamageInfo.Amount*=MyShipCore.CoreGridClass.DamageModifiers.Rocket;}
						if(DamageInfo.Type==MyDamageType.Explosion){DamageInfo.Amount*=MyShipCore.CoreGridClass.DamageModifiers.Explosion;}
						if(DamageInfo.Type==MyDamageType.Environment){DamageInfo.Amount*=MyShipCore.CoreGridClass.DamageModifiers.Environment;}
					}
				}
				else
				{
					DamageInfo.Amount*=1.5f;
				}	
			}
			return;
		}
		
	}
}