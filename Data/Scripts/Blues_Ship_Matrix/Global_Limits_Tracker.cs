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
//My Mod
using Blues_Ship_Matrix;

namespace Blues_Ship_Matrix
{
	public class  MessageStorage
	{
		public  Dictionary<long, string> MSG {get; set;}
	}
	public class DoubleReturn
	{
		public bool Penalty{get; set;}
		public string Warning{get; set;}
	}
    public static class Globals
    {
		public static List<IMyBeacon> BeaconList = new List<IMyBeacon>();
		
		public static MyGridLimit GetClass(IMyCubeGrid CoreGrid)
		{
			MyGridLimit CoreGridClass;

			if (CoreGrid == null)
			{
				return Manager.MySettings.Station_Basic;
			}

			if (Convert.ToString(CoreGrid.GridSizeEnum) == "Large")
			{
				if (CoreGrid.IsStatic)
				{
					CoreGridClass = Manager.MySettings.Station_Basic;
				}
				else
				{
					CoreGridClass = Manager.MySettings.LargeShip_Basic;
				}
			}
			else
			{
				CoreGridClass = Manager.MySettings.SmallShip_Basic;
			}

			foreach (MyGridLimit Class in Manager.MySettings.GridLimits)
			{
				if(Class==null){continue;}
				if (Class.CubeSize == "Small" || Class.CubeSize == "Large")
				{
					if (CoreGrid.CustomName != null &&
						Class.Name != null &&
						Class.CubeSize == Convert.ToString(CoreGrid.GridSizeEnum) &&
						Class.isStatic == CoreGrid.IsStatic &&
						CoreGrid.CustomName.Contains(Class.Name))
					{
						CoreGridClass = Class;
					}
				}
			}

			return CoreGridClass;
		}
		public static DoubleReturn CheckGridStatLimits(IMyBeacon CoreBeacon,MyGridLimit CoreGridClass )
		{
			bool Penalize=false;
			string warningMessage = "";
			IMyCubeGrid CoreGrid = CoreBeacon.CubeGrid;
			//Max Blocks
			if(CoreGridClass.MaxBlocks!=0)
			{
				if((CoreGrid as MyCubeGrid).BlocksCount>CoreGridClass.MaxBlocks){Penalize=true;warningMessage="\nX Grid too large for ship class\n >Please remove "+((CoreGrid as MyCubeGrid).BlocksCount-CoreGridClass.MaxBlocks)+" block(s)"+warningMessage+"\nX ";}else{warningMessage+="\n> ";}
				warningMessage+="Blocks Limit: "+(CoreGrid as MyCubeGrid).BlocksCount+"/"+CoreGridClass.MaxBlocks;
			}
			//Mass
			if(CoreGridClass.MaxMass!=0)
			{
				if((CoreGrid as MyCubeGrid).GetCurrentMass()> CoreGridClass.MaxMass){Penalize=true;warningMessage="\nX Grid too large for ship class\n >Please remove "+((CoreGrid as MyCubeGrid).GetCurrentMass()-CoreGridClass.MaxMass)+" worth of mass"+warningMessage+"\nX ";}else{warningMessage+="\n> ";}
				warningMessage+="Mass Limit: "+(CoreGrid as MyCubeGrid).GetCurrentMass()+"/"+CoreGridClass.MaxMass;
			}
			//PCU
			if(CoreGridClass.MaxPCU!=0)
			{
				if((CoreGrid as MyCubeGrid).BlocksPCU>CoreGridClass.MaxPCU){Penalize=true;warningMessage="\nX Grid too large for ship class\n >Please remove"+((CoreGrid as MyCubeGrid).BlocksPCU-CoreGridClass.MaxPCU)+" worth of PCU"+warningMessage+"\nX ";}else{warningMessage+="\n> ";}
				warningMessage+="PCU Limit: "+(CoreGrid as MyCubeGrid).BlocksPCU+"/"+CoreGridClass.MaxPCU;
			}
			return new DoubleReturn{Penalty=Penalize,Warning=warningMessage};
			
		}
		public static DoubleReturn CheckClassLimits(IMyBeacon CoreBeacon,MyGridLimit CoreGridClass )
		{
			bool Penalize=false;
			string warningMessage = "";
			IMyCubeGrid CoreGrid = CoreBeacon.CubeGrid;
			int FactionCount=0;
			int PlayerCount=0;
			//IMyFactionCollection Factions =MyAPIGateway.Session.Factions;
			//List<IMyBeacon> PausedBeaconList = BeaconList.AsReadOnly();;
			if(CoreGridClass.MaxPerFaction!=0||CoreGridClass.MaxPerPlayer!=0){
				foreach(IMyBeacon Beacon in BeaconList.ToList())
				{
					if(Beacon==null){continue;MyAPIGateway.Utilities.ShowMessage("Debug","No Beacon WTF!");}
					if(Beacon==CoreBeacon){FactionCount+=1;PlayerCount+=1;continue;}
					if(Beacon.CubeGrid==CoreBeacon.CubeGrid){continue;}
					if (Beacon?.GameLogic?.GetAs<ShipCore>() != null)
					{
						var OtherShipCore = Beacon?.GameLogic?.GetAs<ShipCore>();
						if(OtherShipCore.CoreGridClass==null){continue;}
						if(CoreGridClass.Name==OtherShipCore.CoreGridClass.Name)
						{
							if(CoreBeacon.OwnerId==Beacon.OwnerId){PlayerCount+=1;}
							if(Beacon.GetOwnerFactionTag()==CoreBeacon.GetOwnerFactionTag()){FactionCount+=1;}
						}
					}
				}
			}
			if(CoreGridClass.MaxPerFaction!=0)
			{
				if(FactionCount>CoreGridClass.MaxPerFaction){Penalize=true;warningMessage="\nX Faction Limit reached. Please reclassify or decommision grid.\n"+warningMessage+"\nX ";}else{warningMessage+="\n> ";}
				warningMessage+="Max Per Faction: "+FactionCount+"/"+CoreGridClass.MaxPerFaction;
			}
			if(CoreGridClass.MaxPerPlayer!=0)
			{
			if(PlayerCount>CoreGridClass.MaxPerPlayer){Penalize=true;warningMessage="\nX Player Limit reached. Please reclassify or decommision grid.\n"+warningMessage+"\nX ";}else{warningMessage+="\n> ";}	
				warningMessage+="Max Per Player: "+PlayerCount+"/"+CoreGridClass.MaxPerPlayer;
			}
			return new DoubleReturn{Penalty=Penalize,Warning=warningMessage};
		}
		
		
		public static string CheckAndPenalizeBlockLimits(IMyBeacon CoreBeacon,MyGridLimit CoreGridClass )
		{
			//BlockLimits
			string limitsMessage="\nBlock Limits:";
			IEnumerable<IMyFunctionalBlock> BlocksOnGrid = CoreBeacon.CubeGrid.GetFatBlocks<IMyFunctionalBlock>();
			int BlockCount=0;
			string unit="";
			foreach (MyBlockLimit BlockLimit in CoreGridClass.BlockLimits.ToList())
			{
				if(BlockLimit==null){continue;}
				BlockCount = 0;
				foreach(IMyFunctionalBlock Block in BlocksOnGrid.ToList())
				{
					if(Block==null){continue;}
					foreach(MyBlockId BlockId in BlockLimit.BlockIds.ToList())
					{
						if (Convert.ToString(Block.BlockDefinition.TypeId).Replace("MyObjectBuilder_","")==BlockId.TypeId && Convert.ToString(Block.BlockDefinition.SubtypeId)==BlockId.SubtypeId)
						{
							BlockCount += BlockId.CountWeight;
						}
							
					}
				}
				if(BlockLimit.Name == "Shield"){
					unit = " Kpts";
				}else if(BlockLimit.Name == "Weapons"){
					unit=" weapon(s)";
				}else{
					unit=" block(s)";
				}
				if(BlockCount>BlockLimit.MaxCount)
				{
					foreach (IMyFunctionalBlock Block in BlocksOnGrid.ToList())
					{
						if(Block==null){continue;}
						foreach(MyBlockId BlockId in BlockLimit.BlockIds.ToList())
						{
							if (Convert.ToString(Block.BlockDefinition.TypeId).Replace("MyObjectBuilder_","")==BlockId.TypeId && Convert.ToString(Block.BlockDefinition.SubtypeId)==BlockId.SubtypeId)
							{
								if(Block is IMyBeacon){continue;}
								if(Block.Enabled)Block.Enabled = false;
							}
						}
					}
					
					limitsMessage="\nX "+BlockLimit.Warning+"\n >Please remove "+(BlockCount-BlockLimit.MaxCount)+unit+"\n"+limitsMessage+"\n  X ";
				}else{
					limitsMessage+="\n  > ";
				}
				limitsMessage+=BlockLimit.Name+":"+Convert.ToString(BlockCount)+"/"+Convert.ToString(BlockLimit.MaxCount)+unit;
				
				
			}
			return limitsMessage;
		}		
		public static void Penalize(IMyBeacon CoreBeacon)
		{
				IEnumerable<IMyFunctionalBlock> BlocksOnGrid = CoreBeacon.CubeGrid.GetFatBlocks<IMyFunctionalBlock>();
				foreach (IMyFunctionalBlock Block in BlocksOnGrid.ToList())
				{
					if(Block==null){continue;}
					if(Block == CoreBeacon){continue;}
					if(Block.Enabled){Block.Enabled = false;}
					
				}

			
		}
		/*
		public static List<long> BeaconsAdded = new List<long>();
		public static Dictionary<long,string> Messages=new Dictionary<long,string>();
		public static void UpdateBlockInfo(this IMyTerminalBlock block)
        {
            if (!BeaconsAdded.Contains(block.EntityId))
            {
                Action<IMyTerminalBlock, StringBuilder> action = (termBlock, builder) =>
                {
                    if (!Messages.ContainsKey(termBlock.EntityId)){
                        return;
                    }
                    builder.Clear();
                    builder.Append(Messages[termBlock.EntityId]);
                };
                block.AppendingCustomInfo += action;
                BeaconsAdded.Add(block.EntityId);
            }
		}
		*/
		






	}
}