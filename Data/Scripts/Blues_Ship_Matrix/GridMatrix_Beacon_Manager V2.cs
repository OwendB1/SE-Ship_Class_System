using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using Sandbox.Definitions;
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
        public IMyBeacon CoreBeacon;
        private int ticks = 0;
        public IMyCubeBlock CoreBlock;
        public IMyCubeGrid CoreGrid;
        public MyGridLimit CoreGridClass;
        public string Info = "\n  Server Failed to Update GUI";
        public string Warning = "Server Acquired Block:";
        public static bool IsClient => !(IsServer && IsDedicated);
        public static bool IsDedicated => MyAPIGateway.Utilities.IsDedicated;
        public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;
        public static bool IsActive => MyAPIGateway.Multiplayer.MultiplayerActive;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            Instance = this;
            CoreBeacon = Entity as Sandbox.ModAPI.IMyBeacon;
            CoreBlock = Entity as IMyCubeBlock;
            CoreBeacon.CustomData = "Initialized";
            CoreGrid = CoreBlock.CubeGrid;
            Info = "Unable to retrieve class info from the server";
            CoreGridClass = Manager.MySettings.Station_Basic;
            Globals.BeaconList.Add(CoreBeacon);
            Action<IMyTerminalBlock, StringBuilder> GUI_ACTION = (termBlock, builder) =>
            {
                builder.Clear();
                builder.Append(Info);
            };
            CoreBeacon.AppendingCustomInfo += GUI_ACTION;

            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            
            IEnumerable<IMyBeacon> Beacons = CoreGrid.GetFatBlocks<IMyBeacon>();
			List<IMyBeacon> BeaconsList=Beacons.ToList();
            while(BeaconsList.Count>1)
            {
                if(BeaconsList.Count>0)
                {
                    if (BeaconsList.First()?.GameLogic?.GetAs<ShipCore>() != null){break;}
                    else{BeaconsList.Remove(BeaconsList.First());}
                }
            }
            if(BeaconsList.Count>0)
            {
                if(CoreBeacon.EntityId!=BeaconsList.First().EntityId)
                {
                    Info="This Beacon is not the primary beacon!";
                    CoreBeacon.RefreshCustomInfo();
                    return;
                }
            }
            var OwnerFaction=MyAPIGateway.Session.Factions.TryGetPlayerFaction(CoreBeacon.OwnerId);
            if(OwnerFaction!=null)
            {
                if(!String.IsNullOrEmpty(OwnerFaction.Tag))
                {
                    if(Manager.MySettings.IgnoredFactions.Contains(OwnerFaction.Tag))
                    {
                        Info=$"Your Faction: {OwnerFaction.Tag} Is ignored by the ShipClass System";
                        CoreBeacon.RefreshCustomInfo();
                        return;
                    }
                    //MyLog.Default.WriteLine($"BlueShipMatrix: Ignored Faction - {OwnerFaction.Tag}");
                }
            }
            MyGridLimit NewGridClass = Globals.GetClass(CoreGrid);
            if (NewGridClass != null && NewGridClass != CoreGridClass)
            {
                DoubleReturn ClassLimit = Globals.CheckClassLimits(CoreBeacon, NewGridClass);
                if (!ClassLimit.Penalty || NewGridClass == Manager.MySettings.Station_Basic || NewGridClass == Manager.MySettings.LargeShip_Basic || NewGridClass == Manager.MySettings.SmallShip_Basic)
                {
                    CoreGridClass = NewGridClass;
                }
            }

            if (!IsDedicated)
            {
                CustomControls.AddControls(ModContext);
            }
            MyAPIGateway.Parallel.Start(delegate{
                ticks += 1;
                bool Penalize = false;
                Warning = "\n<<< Class Limits Info: >>>\nClass Name:" + CoreGridClass.Name;
                string PerStatWarning = "";
                string PerBlockWarning = "";

                if (ticks % 3 == 0)
                {
                    if (CoreGridClass.ForceBroadCast)
                    {
                        if (!CoreBeacon.Enabled) { CoreBeacon.Enabled = true; }
                        if (CoreBeacon.Radius <= CoreGridClass.ForceBroadCastRange) { CoreBeacon.Radius = CoreGridClass.ForceBroadCastRange; }
                    }
                }

                if (ticks % 6 == 0)
                {
                    try
                    {
                        DoubleReturn GridStatLimit = Globals.CheckGridStatLimits(CoreBeacon, CoreGridClass);
                        if (GridStatLimit.Penalty) { Penalize = GridStatLimit.Penalty; }
                        PerStatWarning += GridStatLimit.Warning;
                    }
                    catch (Exception e) { MyLog.Default.WriteLine($"BlueShipMatrix: Error @GridStats - {e.Message}"); }
                    try { PerBlockWarning += Globals.CheckAndPenalizeBlockLimits(CoreBeacon, CoreGridClass); }
                    catch (Exception e) { MyLog.Default.WriteLine($"BlueShipMatrix: Error @BlockLimits - {e.Message}"); }
                }

                if (ticks % 12 == 0)
                {
                    ticks = 0;
                    try
                    {
                        DoubleReturn ClassLimit = Globals.CheckClassLimits(CoreBeacon, CoreGridClass);
                        if (ClassLimit.Penalty) { Penalize = ClassLimit.Penalty; Warning += ClassLimit.Warning; }
                    }
                    catch (Exception e) { MyLog.Default.WriteLine($"BlueShipMatrix: Error @ClassLimits - {e.Message}"); }

                    if (Manager.GetOwner(CoreGrid as MyCubeGrid) != CoreBeacon.OwnerId) { Warning = "Warning: Beacon Owner Does NOT Own Grid\n" + Warning; }

                    List<MyGridLimit> Classes = Manager.MySettings.GridLimits.ToList();
                    Classes.Add(Manager.MySettings.Station_Basic);
                    Classes.Add(Manager.MySettings.LargeShip_Basic);
                    Classes.Add(Manager.MySettings.SmallShip_Basic);

                    foreach (MyGridLimit Limit in Classes)
                    {
                        if (Limit.Name == CoreGridClass.Name)
                        {
                            CoreGrid.CustomName = string.Join(" ", CoreGrid.CustomName.Split(' ').Distinct().ToArray());
                            CoreBeacon.HudText = string.Join(" ", CoreBeacon.HudText.Split(' ').Distinct().ToArray());
                        }

                        if (CoreBeacon.HudText.Contains(Limit.Name)) { CoreBeacon.HudText.Replace(Limit.Name, ""); }
                        if (CoreGrid.CustomName.Contains(Limit.Name)) { CoreGrid.CustomName.Replace(Limit.Name, ""); }
                    }

                    try
                    {
                        Warning += PerStatWarning + PerBlockWarning + "\nActive Modifiers:" + Modify.Thrusters(CoreGrid, CoreGridClass) + Modify.Gyros(CoreGrid, CoreGridClass) + Modify.Assemblers(CoreGrid, CoreGridClass) + Modify.Refineries(CoreGrid, CoreGridClass) + Modify.Reactors(CoreGrid, CoreGridClass) + Modify.Drills(CoreGrid, CoreGridClass);
                    }
                    catch (Exception e) { MyLog.Default.WriteLine($"BlueShipMatrix: Error @Modify - {e.Message}"); }

                    Info = Warning;
                    CoreBeacon.RefreshCustomInfo();
                }

                try
                {
                    if (Penalize && IsServer) { Globals.Penalize(CoreBeacon); }
                }
                catch (Exception e) { MyLog.Default.WriteLine($"BlueShipMatrix: Error @Penalize - {e.Message}"); }
            });
        }

        public string LoadCustomData()
        {
            return (CoreBeacon.CustomData);
        }

        public override void Close()
        {
            if (Globals.BeaconList.Contains(CoreBeacon))
            {
                Globals.BeaconList.Remove(CoreBeacon);
            }
            if (Entity == null)
            {
                return;
            }
        }
    }
}