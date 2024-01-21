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
    [MyEntityComponentDescriptor(typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Beacon), false, new string[] { "SmallBlockBeacon", "LargeBlockBeacon", "SmallBlockBeaconReskin", "LargeBlockBeaconReskin" })]
    public class ShipCore : MyGameLogicComponent
    {
        public static ShipCore Instance;
		//public MyObjectBuilder_EntityBase builder;
		public IMyBeacon CoreBeacon;
        private int ticks=0;
        public IMyCubeBlock CoreBlock;
        public IMyCubeGrid CoreGrid;
        public MyGridLimit CoreGridClass;
        //public BlueSync<MyGridLimit> SyncGridClass;
        //public BlueSync<string> GUIText;
        public string Info="\n  Server Failed to Update GUI";
        public string Warning="Server Aquired Block:";
		public static bool IsClient => !(IsServer && IsDedicated);
		public static bool IsDedicated => MyAPIGateway.Utilities.IsDedicated;
		public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;
		public static bool IsActive => MyAPIGateway.Multiplayer.MultiplayerActive;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            
            Instance=this;
            CoreBeacon = Entity as Sandbox.ModAPI.IMyBeacon;
            CoreBlock = Entity as IMyCubeBlock;
			//builder = objectBuilder;
			CoreBeacon.CustomData="Initilized";
			CoreGrid = CoreBlock.CubeGrid;
            //GUIText= new BlueSync<string>(6060);
            MyLog.Default.WriteLine("BlueSync: Try Entity Id Update");
            //GUIText.EntityId=CoreBeacon.EntityId;
            //GUIText.ValidateAndSet("Welcome To Blues Grid Matrix");
            Info = "Unable to retrive class info from server";
            //SyncGridClass= new BlueSync<MyGridLimit>(8080);
            //SyncGridClass.EntityId=CoreBeacon.EntityId;
            //SyncGridClass.ValidateAndSet(Manager.MySettings.LargeShip_Basic);
            CoreGridClass=Manager.MySettings.Station_Basic;
            Globals.BeaconList.Add(CoreBeacon);
            Action<IMyTerminalBlock, StringBuilder> GUI_ACTON = (termBlock, builder) =>
                {
                    //CoreBeacon.CustomData=Warning;
                    builder.Clear();
                    //string Message=LoadCustomData();
                    builder.Append(Info);//CoreBeacon.CustomData
                    
                };
            CoreBeacon.AppendingCustomInfo += GUI_ACTON;
			
            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            //MyLog.Default.WriteLine("BlueSync: Updated ID");
            //if(IsServer){CoreBeacon.CustomData="Aquired By Server";}
        }
        public override void UpdateAfterSimulation()
        {
            ticks+=1;
            //CheckGridClass
            MyGridLimit NewGridClass=Globals.GetClass(CoreGrid);  
            if(NewGridClass!=null && NewGridClass!=CoreGridClass)
            {
                DoubleReturn ClassLimit = Globals.CheckClassLimits(CoreBeacon,NewGridClass);
                if(!ClassLimit.Penalty || NewGridClass==Manager.MySettings.Station_Basic|| NewGridClass==Manager.MySettings.LargeShip_Basic|| NewGridClass==Manager.MySettings.SmallShip_Basic)
                {
                    CoreGridClass=NewGridClass;
                }
            }
            //Add GUI Controls to block, dedicated server have no local player and no need for GUI controls
            if(!IsDedicated)
            {
                CustomControls.AddControls(ModContext);
            }

            bool Penalize=false;
            Warning="\n<<< Class Limits Info: >>>\nClass Name:"+CoreGridClass.Name;
            string PerStatWarning="";
            string PerBlockWarning="";
            //Start Parrallel

                if(ticks % 3 == 0)
                {
                    //try{
                    PerBlockWarning+=Globals.CheckAndPenalizeBlockLimits(CoreBeacon,CoreGridClass);
                    //}catch (Exception e){MyLog.Default.WriteLine($"BlueShipMatrix: Error @BlockLimits - {e.Message}");}
                    if(CoreGridClass.ForceBroadCast)
                    {
                        if(!CoreBeacon.Enabled){CoreBeacon.Enabled=true;}
                        if(CoreBeacon.Radius<=CoreGridClass.ForceBroadCastRange){CoreBeacon.Radius=CoreGridClass.ForceBroadCastRange;}
                    }
                }
                if(ticks % 6 == 0)
                {
                    try{
                    DoubleReturn GridStatLimit = Globals.CheckGridStatLimits(CoreBeacon,CoreGridClass);
                    if(GridStatLimit.Penalty){Penalize=GridStatLimit.Penalty;}
                    PerStatWarning+=GridStatLimit.Warning;
                    }catch (Exception e){MyLog.Default.WriteLine($"BlueShipMatrix: Error @GridStats - {e.Message}");}
                }
                if(ticks % 12 == 0)
                {
                    ticks=0;
                    try{
                        DoubleReturn ClassLimit = Globals.CheckClassLimits(CoreBeacon,CoreGridClass);
                        if(ClassLimit.Penalty){Penalize=ClassLimit.Penalty;Warning+=ClassLimit.Warning;}
                    }catch (Exception e){MyLog.Default.WriteLine($"BlueShipMatrix: Error @ClassLimits - {e.Message}");}
                    if(Manager.GetOwner(CoreGrid as MyCubeGrid)!=CoreBeacon.OwnerId){Warning="Warning: Beacon Owner Does NOT Own Grid\n"+Warning;}
                    List<MyGridLimit> Classes =Manager.MySettings.GridLimits.ToList();
                    Classes.Add(Manager.MySettings.Station_Basic);
                    Classes.Add(Manager.MySettings.LargeShip_Basic);
                    Classes.Add(Manager.MySettings.SmallShip_Basic);
                    foreach(MyGridLimit Limit in Classes)
                    {
                        if(Limit.Name==CoreGridClass.Name)
                        {
                            CoreGrid.CustomName=string.Join(" ",CoreGrid.CustomName.Split(' ').Distinct().ToArray());
                            CoreBeacon.HudText=string.Join(" ",CoreBeacon.HudText.Split(' ').Distinct().ToArray());
                        }
                        if(CoreBeacon.HudText.Contains(Limit.Name)){CoreBeacon.HudText.Replace(Limit.Name,"");}
                        if(CoreGrid.CustomName.Contains(Limit.Name)){CoreGrid.CustomName.Replace(Limit.Name,"");}
                    }
                    try
                    {
                        Warning+=PerStatWarning+PerBlockWarning+"\nActive Modifiers:"+Modify.Thrusters(CoreGrid,CoreGridClass)+Modify.Gyros(CoreGrid,CoreGridClass)+Modify.Assemblers(CoreGrid,CoreGridClass)+Modify.Refineries(CoreGrid,CoreGridClass)+Modify.Reactors(CoreGrid,CoreGridClass)+Modify.Drills(CoreGrid,CoreGridClass);
                    }
                    catch (Exception e){MyLog.Default.WriteLine($"BlueShipMatrix: Error @Modify - {e.Message}");}
                    Info=Warning;
                    //Tell The block to refresh it's output display
                    CoreBeacon.RefreshCustomInfo();
                }

                try{
                    if(Penalize && IsServer){Globals.Penalize(CoreBeacon);}
                }catch (Exception e){MyLog.Default.WriteLine($"BlueShipMatrix: Error @Penalize - {e.Message}");}

            //end parrallel
        }
        public string LoadCustomData()
        {
            return(CoreBeacon.CustomData);
        }
        public override void Close()
        {
            //MyAPIGateway.Multiplayer.UnregisterMessageHandler(SyncGridClass.modID, SyncGridClass.MessageHandler);
            // MyAPIGateway.Multiplayer.UnregisterMessageHandler(GUIText.modID, GUIText.MessageHandler);
            if(Globals.BeaconList.Contains(CoreBeacon))
            {Globals.BeaconList.Remove(CoreBeacon);}
            if (Entity == null)
            {
                return;
            }
        }
		
	
    }
}
