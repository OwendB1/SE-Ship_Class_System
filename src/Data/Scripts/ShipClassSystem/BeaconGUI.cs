using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public static class BeaconGUI
    {
        private static int waitTicks;
        private static bool controlsAdded;
        private static readonly string[] ControlsToHideIfForceBroadcast = { "Radius", "HudText" };

        public static void AddControls(IMyModContext context)
        {
            if (controlsAdded) return;

            if (waitTicks <
                100) //TODO I don't know why I need this, but without it, I lose all vanilla controls on dedicated servers - I'm going to leave this for now
            {
                waitTicks++;
                return;
            }

            controlsAdded = true;

            // Create Drop Down Menu and add the control to the grid controller's terminal
            // Different comboboxes available depending on grid type
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(GetCombobox("SetGridClassLargeStatic",
                SetComboboxContentLargeStatic,
                block => block.CubeGrid.IsStatic && block.CubeGrid.GridSizeEnum == MyCubeSize.Large));
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(GetCombobox("SetGridClassLargeMobile",
                SetComboboxContentLargeGrid,
                block => !block.CubeGrid.IsStatic && block.CubeGrid.GridSizeEnum == MyCubeSize.Large));
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(GetCombobox("SetGridClassSmallMobile",
                SetComboboxContentSmallMobile,
                block => !block.CubeGrid.IsStatic && block.CubeGrid.GridSizeEnum == MyCubeSize.Small));

            var controls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyBeacon>(out controls);

            foreach (var control in controls.Where(control => ControlsToHideIfForceBroadcast.Contains(control.Id)))
                control.Visible = TerminalChainedDelegate.Create(control.Visible, VisibleIfClassNotForceBroadcast);
        }

        private static bool VisibleIfClassNotForceBroadcast(IMyTerminalBlock block)
        {
            return !(block.GetGridLogic()?.GridClass?.ForceBroadCast ?? false);
        }

        private static IMyTerminalControlCombobox GetCombobox(string name,
            Action<List<MyTerminalControlComboBoxItem>> setComboboxContent, Func<IMyTerminalBlock, bool> isVisible)
        {
            var combobox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>(name);
            combobox.Visible = isVisible;
            combobox.Enabled = isVisible;
            combobox.Title = MyStringId.GetOrCompute("Grid class");
            combobox.Tooltip = MyStringId.GetOrCompute("Select your desired grid class");
            combobox.SupportsMultipleBlocks = false;
            combobox.Getter = GetGridClass;
            combobox.Setter = SetGridClass;
            combobox.ComboBoxContent = setComboboxContent;

            return combobox;
        }

        private static void SetComboboxContentLargeStatic(List<MyTerminalControlComboBoxItem> list)
        {
            list.AddRange(from gridLimit in ModSessionManager.GetAllGridClasses() where gridLimit.StaticOnly 
                select new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = MyStringId.GetOrCompute(gridLimit.Name) });
        }

        private static void SetComboboxContentLargeGrid(List<MyTerminalControlComboBoxItem> list)
        {
            list.AddRange(from gridLimit in ModSessionManager.GetAllGridClasses() where gridLimit.LargeGrid 
                select new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = MyStringId.GetOrCompute(gridLimit.Name) });
        }

        private static void SetComboboxContentSmallMobile(List<MyTerminalControlComboBoxItem> list)
        {
            list.AddRange(from gridLimit in ModSessionManager.GetAllGridClasses() where gridLimit.SmallGrid
                select new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = MyStringId.GetOrCompute(gridLimit.Name) });
        }

        private static long GetGridClass(IMyTerminalBlock block)
        {
            var cubeGridLogic = block.GetGridLogic();

            return cubeGridLogic?.GridClassId ?? 0;
        }

        private static void SetGridClass(IMyTerminalBlock block, long key)
        {
            var cubeGridLogic = block.GetGridLogic();

            if (cubeGridLogic != null)
            {
                Utils.Log(
                    $"BeaconGUI::SetGridClass: Sending change grid class message, entityId = {block.CubeGrid.EntityId}, grid class id = {key}",
                    2);
                ModSessionManager.Comms.SendChangeGridClassMessage(block.CubeGrid.EntityId, key);
            }
            else
            {
                Utils.Log("BeaconGUI::SetGridClass: Unable to set GridClassId, GetGridLogic is returning null", 3);
            }
        }
    }

    //From Digi's examples: https://github.com/THDigi/SE-ModScript-Examples/blob/master/Data/Scripts/Examples/TerminalControls/Hiding/TerminalChainedDelegate.cs
    public class TerminalChainedDelegate
    {
        private readonly bool CheckOR;
        private readonly Func<IMyTerminalBlock, bool> CustomFunc;

        private readonly Func<IMyTerminalBlock, bool> OriginalFunc;

        private TerminalChainedDelegate(Func<IMyTerminalBlock, bool> originalFunc,
            Func<IMyTerminalBlock, bool> customFunc, bool checkOR)
        {
            OriginalFunc = originalFunc;
            CustomFunc = customFunc;
            CheckOR = checkOR;
        }

        /// <summary>
        ///     <paramref name="originalFunc" /> should always be the delegate this replaces, to properly chain with other mods
        ///     doing the same.
        ///     <para><paramref name="customFunc" /> should be your custom condition to append to the chain.</para>
        ///     <para>
        ///         As for <paramref name="checkOR" />, leave false if you want to hide controls by returning false with your
        ///         <paramref name="customFunc" />.
        ///     </para>
        ///     <para>
        ///         Otherwise set to true if you want to force-show otherwise hidden controls by returning true with your
        ///         <paramref name="customFunc" />.
        ///     </para>
        /// </summary>
        public static Func<IMyTerminalBlock, bool> Create(Func<IMyTerminalBlock, bool> originalFunc,
            Func<IMyTerminalBlock, bool> customFunc, bool checkOR = false)
        {
            return new TerminalChainedDelegate(originalFunc, customFunc, checkOR).ResultFunc;
        }

        private bool ResultFunc(IMyTerminalBlock block)
        {
            if (block?.CubeGrid == null)
                return false;

            var originalCondition = OriginalFunc?.Invoke(block) ?? true;
            var customCondition = CustomFunc?.Invoke(block) ?? true;

            if (CheckOR)
                return originalCondition || customCondition;
            return originalCondition && customCondition;
        }
    }
}