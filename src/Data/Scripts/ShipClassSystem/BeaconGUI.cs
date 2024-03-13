using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace RedVsBlueClassSystem
{
    public static class BeaconGUI
    {
        private static int waitTicks = 0;
        private static bool controlsAdded = false;
        private static string[] ControlsToHideIfForceBroadcast = { "Radius", "HudText" };
        public static void AddControls(IMyModContext context)
        {
            if (controlsAdded) {
                return;
            }

            if(waitTicks < 100)//TODO I don't know why I need this, but without it, I lose all vanilla controls on dedicated servers - I'm going to leave this for now
            {
                waitTicks++;

                return;
            }

            controlsAdded = true;

            // Create Drop Down Menu and add the control to the grid controller's terminal
            // Different comboboxes available depending on grid type
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(GetCombobox($"SetGridClassLargeStatic", SetComboboxContentLargeStatic, (IMyTerminalBlock block) => block.CubeGrid.IsStatic && block.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large));
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(GetCombobox($"SetGridClassLargeMobile", SetComboboxContentLargeGrid, (IMyTerminalBlock block) => !block.CubeGrid.IsStatic && block.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large));
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(GetCombobox($"SetGridClassSmallStatic", SetComboboxContentSmallStatic, (IMyTerminalBlock block) => block.CubeGrid.IsStatic && block.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Small));
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(GetCombobox($"SetGridClassSmallMobile", SetComboboxContentSmallMobile, (IMyTerminalBlock block) => !block.CubeGrid.IsStatic && block.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Small));

            List<IMyTerminalControl> controls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyBeacon>(out controls);

            foreach (var control in controls)
            {
                if (ControlsToHideIfForceBroadcast.Contains(control.Id))
                {
                    control.Visible = TerminalChainedDelegate.Create(control.Visible, VisibleIfClassNotForceBroadcast);
                }
            }
        }

        private static bool VisibleIfClassNotForceBroadcast(IMyTerminalBlock block)
        {
            return !(block.GetGridLogic()?.GridClass?.ForceBroadCast ?? false);
        }

        private static IMyTerminalControlCombobox GetCombobox(string name, Action<List<MyTerminalControlComboBoxItem>> setComboboxContent, Func<IMyTerminalBlock, bool> isVisible) {
            var combobox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>(name);
            combobox.Visible = isVisible;
            combobox.Enabled = isVisible;
            combobox.Title = VRage.Utils.MyStringId.GetOrCompute("Grid class");
            combobox.Tooltip = VRage.Utils.MyStringId.GetOrCompute("Select your desired grid class");
            combobox.SupportsMultipleBlocks = false;
            combobox.Getter = GetGridClass;
            combobox.Setter = SetGridClass;
            combobox.ComboBoxContent = setComboboxContent;

            return combobox;
        }

        private static void SetComboboxContentLargeStatic(List<MyTerminalControlComboBoxItem> list)
        {
            foreach(var gridLimit in ModSessionManager.GetAllGridClasses())
            {
                if(gridLimit.LargeGridStatic)
                {
                    list.Add(new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = VRage.Utils.MyStringId.GetOrCompute(gridLimit.Name) });
                }
            }
        }

        private static void SetComboboxContentLargeGrid(List<MyTerminalControlComboBoxItem> list)
        {
            foreach (var gridLimit in ModSessionManager.GetAllGridClasses())
            {
                if(gridLimit.LargeGridMobile)
                {
                    list.Add(new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = VRage.Utils.MyStringId.GetOrCompute(gridLimit.Name) });
                }
            }
        }

        private static void SetComboboxContentSmallStatic(List<MyTerminalControlComboBoxItem> list)
        {
            foreach (var gridLimit in ModSessionManager.GetAllGridClasses())
            {
                if(gridLimit.SmallGridStatic)
                {
                    list.Add(new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = VRage.Utils.MyStringId.GetOrCompute(gridLimit.Name) });
                }
            }
        }

        private static void SetComboboxContentSmallMobile(List<MyTerminalControlComboBoxItem> list)
        {
            foreach (var gridLimit in ModSessionManager.GetAllGridClasses())
            {
                if(gridLimit.SmallGridMobile)
                {
                    list.Add(new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = VRage.Utils.MyStringId.GetOrCompute(gridLimit.Name) });
                }
            }
        }
        private static long GetGridClass(IMyTerminalBlock block)
        {
            CubeGridLogic cubeGridLogic = block.GetGridLogic();

            return cubeGridLogic?.GridClassId ?? 0;
        }
        private static void SetGridClass(IMyTerminalBlock block, long key)
        {
            CubeGridLogic cubeGridLogic = block.GetGridLogic();

            if(cubeGridLogic != null)
            {
                Utils.Log($"BeaconGUI::SetGridClass: Sending change grid class message, entityId = {block.CubeGrid.EntityId}, grid class id = {key}", 2);
                ModSessionManager.Comms.SendChangeGridClassMessage(block.CubeGrid.EntityId, key);
            }
            else
            {
                Utils.Log($"BeaconGUI::SetGridClass: Unable to set GridClassId, GetGridLogic is returning null", 3);
            }
        }
    }

    //From Digi's examples: https://github.com/THDigi/SE-ModScript-Examples/blob/master/Data/Scripts/Examples/TerminalControls/Hiding/TerminalChainedDelegate.cs
    public class TerminalChainedDelegate
    {
        /// <summary>
        /// <paramref name="originalFunc"/> should always be the delegate this replaces, to properly chain with other mods doing the same.
        /// <para><paramref name="customFunc"/> should be your custom condition to append to the chain.</para>
        /// <para>As for <paramref name="checkOR"/>, leave false if you want to hide controls by returning false with your <paramref name="customFunc"/>.</para>
        /// <para>Otherwise set to true if you want to force-show otherwise hidden controls by returning true with your <paramref name="customFunc"/>.</para> 
        /// </summary>
        public static Func<IMyTerminalBlock, bool> Create(Func<IMyTerminalBlock, bool> originalFunc, Func<IMyTerminalBlock, bool> customFunc, bool checkOR = false)
        {
            return new TerminalChainedDelegate(originalFunc, customFunc, checkOR).ResultFunc;
        }

        readonly Func<IMyTerminalBlock, bool> OriginalFunc;
        readonly Func<IMyTerminalBlock, bool> CustomFunc;
        readonly bool CheckOR;

        TerminalChainedDelegate(Func<IMyTerminalBlock, bool> originalFunc, Func<IMyTerminalBlock, bool> customFunc, bool checkOR)
        {
            OriginalFunc = originalFunc;
            CustomFunc = customFunc;
            CheckOR = checkOR;
        }

        bool ResultFunc(IMyTerminalBlock block)
        {
            if (block?.CubeGrid == null)
                return false;

            bool originalCondition = (OriginalFunc == null ? true : OriginalFunc.Invoke(block));
            bool customCondition = (CustomFunc == null ? true : CustomFunc.Invoke(block));

            if (CheckOR)
                return originalCondition || customCondition;
            else
                return originalCondition && customCondition;
        }
    }
}
