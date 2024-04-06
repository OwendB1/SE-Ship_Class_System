using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.Network;
using VRage.Utils;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class CockpitGUI : MySessionComponentBase, IMyEventProxy
    {
        private static readonly string[] ControlsToHideIfNotMainCockpit = { "SetGridClassLargeStatic", "SetGridClassLargeMobile", "SetGridClassSmall" };
        public override void BeforeStart()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter += CustomControlGetter;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= CustomControlGetter;
        }

        public void CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (!(block is IMyCockpit)) return;
            if (controls.Any(control => ControlsToHideIfNotMainCockpit.Contains(control.Id))) return;
            controls.Add(GetCombobox("SetGridClassLargeStatic", SetComboboxContentLargeStatic,
                cockpit => cockpit.CubeGrid.IsStatic && cockpit.CubeGrid.GridSizeEnum == MyCubeSize.Large));
            controls.Add(GetCombobox("SetGridClassLargeMobile", SetComboboxContentLargeGridMobile,
                cockpit => !cockpit.CubeGrid.IsStatic && cockpit.CubeGrid.GridSizeEnum == MyCubeSize.Large));
            controls.Add(GetCombobox("SetGridClassSmall", SetComboboxContentSmall,
                cockpit => !cockpit.CubeGrid.IsStatic && cockpit.CubeGrid.GridSizeEnum == MyCubeSize.Small));
            foreach (var control in controls.Where(control => ControlsToHideIfNotMainCockpit.Contains(control.Id)))
                control.Visible = TerminalChainedDelegate.Create(control.Visible, VisibleIfIsMainCockpit);
        }

        private static bool VisibleIfIsMainCockpit(IMyTerminalBlock block)
        {
            var cockpit = block as IMyCockpit;
            return cockpit?.IsMainCockpit ?? false;
        }

        private static IMyTerminalControlCombobox GetCombobox(string name,
            Action<List<MyTerminalControlComboBoxItem>> setComboboxContent, Func<IMyTerminalBlock, bool> isVisible)
        {
            var combobox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyCockpit>(name);
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
            list.AddRange(from gridLimit in ModSessionManager.Config.GridClasses
                          where gridLimit.LargeGridStatic
                          select new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = MyStringId.GetOrCompute(gridLimit.Name) });
        }

        private static void SetComboboxContentLargeGridMobile(List<MyTerminalControlComboBoxItem> list)
        {
            list.AddRange(from gridLimit in ModSessionManager.Config.GridClasses
                          where gridLimit.LargeGridMobile
                          select new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = MyStringId.GetOrCompute(gridLimit.Name) });
        }

        private static void SetComboboxContentSmall(List<MyTerminalControlComboBoxItem> list)
        {
            list.AddRange(from gridLimit in ModSessionManager.Config.GridClasses
                          where gridLimit.SmallGrid
                          select new MyTerminalControlComboBoxItem { Key = gridLimit.Id, Value = MyStringId.GetOrCompute(gridLimit.Name) });
        }

        private static long GetGridClass(IMyTerminalBlock block)
        {
            var cubeGridLogic = block.GetMainGridLogic();
            return cubeGridLogic?.GridClassId ?? 0;
        }

        private static void SetGridClass(IMyTerminalBlock block, long key)
        {
            var cubeGridLogic = block.GetMainGridLogic();
            if (cubeGridLogic != null)
            {
                Utils.Log(
                    $"CockpitGUI::SetGridClass: Sending change grid class message, entityId = {block.CubeGrid.EntityId}, grid class id = {key}",
                    2);
                ModSessionManager.Comms.ChangeGridClass(cubeGridLogic.Grid.EntityId, key);
            }
            else
            {
                Utils.Log("CockpitGUI::SetGridClass: Unable to set GridClassId, GetGridLogic is returning null", 3);
            }
        }
    }

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