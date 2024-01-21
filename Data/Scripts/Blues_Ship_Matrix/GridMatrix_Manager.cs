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
	[MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
	public class Manager : MySessionComponentBase
	{
		public static long GetOwner(MyCubeGrid grid)
        {

            var gridOwnerList = grid.BigOwners;
            var ownerCnt = gridOwnerList.Count;
            var gridOwner = 0L;

            if (ownerCnt > 0 && gridOwnerList[0] != 0)
                return gridOwnerList[0];
            else if (ownerCnt > 1)
                return gridOwnerList[1];

            return gridOwner;
        }
		public static List<IMyCubeGrid> GridList = new List<IMyCubeGrid>();
		public static bool IsClient => !(IsServer && IsDedicated);
		public static bool IsDedicated => MyAPIGateway.Utilities.IsDedicated;
		public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;
		public static bool IsActive => MyAPIGateway.Multiplayer.MultiplayerActive;
		public BlueSync<ShipMatrixConfig> MySettingsSynced;
		public static ShipMatrixConfig MySettings;
		public MySync<bool, SyncDirection.BothWays> requiresUpdate;
		private int ticks=0;
		public override void Init(MyObjectBuilder_SessionComponent SessionComponent)
		{
			MySettings = ShipMatrixConfig.Load();
			MySettingsSynced= new BlueSync<ShipMatrixConfig>();
			if(IsServer)
			{
				MySettings = ShipMatrixConfig.Load();
				ShipMatrixConfig.Save(MySettings);
				MyLog.Default.WriteLine("Blues_Ship_Matrix: Server Loaded and Saved Config");
				MySettingsSynced.ValidateAndSet(MySettings);
				//Get All Grids
				var entityHashSet = new HashSet<IMyEntity>();
				MyAPIGateway.Entities.GetEntities(entityHashSet, entity => entity is IMyCubeGrid);
				foreach (var Entity in entityHashSet)
				{
					if (Entity is IMyCubeGrid)
					{
						GridList.Add(Entity as IMyCubeGrid);
					}
				}
				
				MyLog.Default.WriteLine("Blues_Ship_Matrix: Grabbed All Grids, Bc I'm Clingy Like That");
				//Damage Handler
				
				MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(89,Modify.GridClassDamageHandler);
				//SetUpEntityStuff
				MyAPIGateway.Entities.OnEntityAdd+=EntityCreatedHandler;
				MyAPIGateway.Entities.OnEntityRemove+=EntityRemovalHandler;
				MyLog.Default.WriteLine("Blues_Ship_Matrix: Added Handlers For Grids");
			}
		}
		
		public override void UpdateAfterSimulation()
		{
			base.UpdateBeforeSimulation();
			if(MySettingsSynced.Value!=null && !IsDedicated){MySettings=MySettingsSynced.Value;}
			if(IsServer)
			{
				MyAPIGateway.Parallel.Start(delegate{
					//Only Send Config If you are the server
					ticks+=1;
					if(ticks > 240) {ticks=0;MySettingsSynced.ValidateAndSet(MySettings);}
					foreach(IMyCubeGrid CoreGrid in GridList.ToList())
					{
						if((CoreGrid as MyCubeGrid).BlocksCount<4){continue;}
						IEnumerable<IMyBeacon> Beacons = CoreGrid.GetFatBlocks<IMyBeacon>();
						List<IMyBeacon> BeaconsList=Beacons.ToList();
						try{
							long GridOwner=GetOwner(CoreGrid as MyCubeGrid);
							var OwnerFaction=MyAPIGateway.Session.Factions.TryGetPlayerFaction(GridOwner);
							if(OwnerFaction!=null)
							{
								if(!String.IsNullOrEmpty(OwnerFaction.Tag))
								{
									if(MySettings.IgnoredFactions.Contains(OwnerFaction.Tag)){continue;}
									//MyLog.Default.WriteLine($"BlueShipMatrix: Ignored Faction - {OwnerFaction.Tag}");
								}
							}
						}catch (Exception e){MyLog.Default.WriteLine($"BlueShipMatrix: Error @Skip-Ignored-Factions - {e.Message}");}
						if(BeaconsList.Count<1){
							IEnumerable<IMyFunctionalBlock> BlocksOnGrid = CoreGrid.GetFatBlocks<IMyFunctionalBlock>();
							foreach (IMyFunctionalBlock Block in BlocksOnGrid.ToList())
							{
								if(Block==null){continue;}
								if(Block.Enabled){Block.Enabled = false;}	
							}
						}
					}
				});
			}
				/*if(IsDedicated)
				{
					foreach(IMyCubeGrid CoreGrid in GridList.ToList())
					{
						IEnumerable<IMyBeacon> Beacons = CoreGrid.GetFatBlocks<IMyBeacon>();
						List<IMyBeacon> BeaconsList=Beacons.ToList();
						IMyBeacon CoreBeacon=BeaconsList.First();
						CoreBeacon.RefreshCustomInfo();
					}
				}*/

		}			

	    protected override void UnloadData()
		{
		  MyAPIGateway.Entities.OnEntityAdd-=EntityCreatedHandler;
		  MyAPIGateway.Entities.OnEntityRemove-=EntityRemovalHandler;
		  //MyAPIGateway.Utilities.MessageEntered-= Client_Chat_Manager;
		  //MyAPIGateway.Multiplayer.UnregisterMessageHandler(MySettingsSynced.modID, MySettingsSynced.MessageHandler);
		  
		}
		public void EntityCreatedHandler(IMyEntity Entity)
		{
			if (Entity is IMyCubeGrid && !GridList.Contains(Entity as IMyCubeGrid))
			{
				GridList.Add(Entity as IMyCubeGrid);
			}
			//MyAPIGateway.Utilities.ShowMessage("Blues_Ship_Matrix", Convert.ToString(GridList.Count));
		}
		public void EntityRemovalHandler(IMyEntity Entity)
		{
			if (Entity is IMyCubeGrid)
			{
				GridList.Remove(Entity as IMyCubeGrid);
			//MyAPIGateway.Utilities.ShowMessage("Blues_Ship_Matrix", Convert.ToString(GridList.Count));
			}
		}

	}
	
}