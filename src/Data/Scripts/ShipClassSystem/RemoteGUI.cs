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

namespace ShipClassSystem
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class RemoteControlGUI : MySessionComponentBase, IMyEventProxy
    {
        private static readonly string[] ControlsToHideIfNotMainRemoteControl = { "SetGridClassLargeStatic", "SetGridClassLargeMobile", "SetGridClassSmall" };
        private readonly List<IMyTerminalControl> _remoteControls = new List<IMyTerminalControl>();
        public override void BeforeStart()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter += CustomControlGetter;

            _remoteControls.Add(GetCombobox("SetGridClassLargeStatic", SetComboboxContentLargeStatic,
                remote => remote.CubeGrid.IsStatic && remote.CubeGrid.GridSizeEnum == MyCubeSize.Large));
            _remoteControls.Add(GetCombobox("SetGridClassLargeMobile", SetComboboxContentLargeGridMobile,
                remote => !remote.CubeGrid.IsStatic && remote.CubeGrid.GridSizeEnum == MyCubeSize.Large));
            _remoteControls.Add(GetCombobox("SetGridClassSmall", SetComboboxContentSmall,
                remote => !remote.CubeGrid.IsStatic && remote.CubeGrid.GridSizeEnum == MyCubeSize.Small));
        }

        protected override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= CustomControlGetter;
        }

        public void CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (!(block is IMyRemoteControl)) return;
            if (controls.Any(control => _remoteControls.Contains(control))) return;
            controls.AddRange(_remoteControls);
            foreach (var control in controls.Where(control => ControlsToHideIfNotMainRemoteControl.Contains(control.Id)))
                control.Enabled = TerminalChainedDelegate.Create(control.Visible, VisibleIfIsMainOwner);
        }

        private static bool VisibleIfIsMainOwner(IMyTerminalBlock block)
        {
            var remote = block as IMyRemoteControl;
            if(remote.OwnerId==Utils.GetGridOwner(block.CubeGrid)){return true;}
            else if(MyAPIGateway.Session.Factions.TryGetPlayerFaction(remote.OwnerId)==MyAPIGateway.Session.Factions.TryGetPlayerFaction(Utils.GetGridOwner(block.CubeGrid))){return true;}
            else{return false;}
        }

        private static IMyTerminalControlCombobox GetCombobox(string name,
            Action<List<MyTerminalControlComboBoxItem>> setComboboxContent, Func<IMyTerminalBlock, bool> isVisible)
        {
            var combobox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyRemoteControl>(name);
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
                    $"RemoteControlGUI::SetGridClass: Sending change grid class message, entityId = {block.CubeGrid.EntityId}, grid class id = {key}",
                    2);
                cubeGridLogic.GridClassId = key;
                if (!Constants.IsServer)
                    ModSessionManager.Comms.ChangeGridClass(cubeGridLogic.Grid.EntityId, key);
            }
            else
            {
                Utils.Log($"RemoteControlGUI::SetGridClass: Unable to set GridClassId, GetGridLogic is returning null on {block.EntityId}", 3);
            }
        }
    }
}