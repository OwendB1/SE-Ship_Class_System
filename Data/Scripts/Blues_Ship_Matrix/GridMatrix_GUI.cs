using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Sandboxs
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game;
using Sandbox.Game.Gui;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using Sandbox.ModAPI.Interfaces.Terminal;
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
    public static class CustomControls
    {
        public static bool controlsAdded = false;
        public static Dictionary<long, string> items = new Dictionary<long, string>();
        public static void AddControls(IMyModContext context)
        {
            if (controlsAdded)
                return;

            controlsAdded = true;
     
            //Create Drop Down Menu
            var combobox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("SetShipClass");          
            combobox.Visible = SetVisible;
            combobox.Enabled = SetVisible;
            combobox.Title = VRage.Utils.MyStringId.GetOrCompute($"Ship Class");
            combobox.Tooltip = VRage.Utils.MyStringId.GetOrCompute($"Select Your Desired Ship Class");
            combobox.SupportsMultipleBlocks = false;
            combobox.Getter = GetShipClass;
            combobox.Setter = SetShipClass;
            combobox.ComboBoxContent = SetComboboxContent;

            //Create Seperator
            IMyTerminalControlSeparator mySeparator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SeperateMyDumbEyes");
            mySeparator.SupportsMultipleBlocks = false;

            // Add the control to the ship controller's terminal
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(mySeparator);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(combobox);
        }


        private static bool SetVisible(IMyTerminalBlock block)
        {
            if (block?.GameLogic?.GetAs<ShipCore>() != null)
            {
                try{
                    var GridOwner=Manager.GetOwner(block.CubeGrid as MyCubeGrid);
                    if(GridOwner==block.OwnerId){return (true);}
                }catch{}
            }
            return false;
        }

        private static void SetComboboxContent(List<MyTerminalControlComboBoxItem> list)
        {
            list.Add(new MyTerminalControlComboBoxItem {Key=1L, Value=VRage.Utils.MyStringId.GetOrCompute(Manager.MySettings.Station_Basic.Name)});
            list.Add(new MyTerminalControlComboBoxItem {Key=2L, Value=VRage.Utils.MyStringId.GetOrCompute(Manager.MySettings.LargeShip_Basic.Name)});
            list.Add(new MyTerminalControlComboBoxItem {Key=3L, Value=VRage.Utils.MyStringId.GetOrCompute(Manager.MySettings.SmallShip_Basic.Name)});
            long itemKey = 4L;
            foreach (MyGridLimit Class in Manager.MySettings.GridLimits)
            {
                list.Add(new MyTerminalControlComboBoxItem {Key=itemKey,Value=VRage.Utils.MyStringId.GetOrCompute(Class.Name)});
                itemKey++;
            }

        }
        private static long GetShipClass(IMyTerminalBlock block)
        {
            var ShipCore = block?.GameLogic?.GetAs<ShipCore>();
            var CoreGridClass=ShipCore.CoreGridClass;
            if (ShipCore != null && ShipCore.CoreGridClass != null)
            {
                if (CoreGridClass.Name==(Manager.MySettings.Station_Basic.Name)) { return 1L; }
                if (CoreGridClass.Name==(Manager.MySettings.LargeShip_Basic.Name)) { return 2L; }
                if (CoreGridClass.Name==(Manager.MySettings.SmallShip_Basic.Name)) { return 3L; }
                long itemKey = 4L;
                foreach (MyGridLimit Class in Manager.MySettings.GridLimits)
                {
                    if (CoreGridClass.Name==Class.Name) { return itemKey; }
                    itemKey++;
                }
            }
            return 1L;
        }
        private static void SetShipClass(IMyTerminalBlock block, long key)
        {
            var ShipCore = block?.GameLogic?.GetAs<ShipCore>();
            MyGridLimit NewGridClass=null;
            if (ShipCore != null)
            {
                if(key==1L){NewGridClass=Manager.MySettings.Station_Basic;}
                if(key==2L){NewGridClass=Manager.MySettings.LargeShip_Basic;}
                if(key==3L){NewGridClass=Manager.MySettings.SmallShip_Basic;}
                long itemKey = 4L;
                foreach (MyGridLimit Class in Manager.MySettings.GridLimits)
                {
                    if(key==itemKey){NewGridClass=Class;}
                    itemKey++;
                }
                if(ShipCore.CoreGridClass!=null){
                    if(NewGridClass.Name==ShipCore.CoreGridClass.Name){return;}
                    if(ShipCore.CoreGrid.CustomName.Contains(ShipCore.CoreGridClass.Name)){ShipCore.CoreGrid.CustomName=ShipCore.CoreGrid.CustomName.Replace(ShipCore.CoreGridClass.Name,NewGridClass.Name);}
                    else{ShipCore.CoreGrid.CustomName+=": "+NewGridClass.Name;}
                    if(ShipCore.CoreBeacon.HudText.Contains(ShipCore.CoreGridClass.Name)){ShipCore.CoreBeacon.HudText=ShipCore.CoreBeacon.HudText.Replace(ShipCore.CoreGridClass.Name,NewGridClass.Name);}
                }
                //ShipCore.CoreGridClass=NewGridClass;
                //ShipCore.SyncGridClass.ValidateAndSet(NewGridClass);
                


            }
        }


        static List<IMyTerminalControl> GetControls<T>() where T : IMyTerminalBlock
        {
            List<IMyTerminalControl> controls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<T>(out controls);

            return controls;
        }
    }
}
