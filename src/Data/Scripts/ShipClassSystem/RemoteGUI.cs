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
using System.Text;
using System.IO;

namespace ShipClassSystem
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class RemoteGUI : MySessionComponentBase, IMyEventProxy
    {
        private static readonly string[] ControlsToHideIfNotMainRemote = { "SetGridClassLargeStatic", "SetGridClassLargeMobile", "SetGridClassSmall" };
        private readonly List<IMyTerminalControl> _remoteControls = new List<IMyTerminalControl>();
        private readonly List<IMyTerminalAction> _remoteActions = new List<IMyTerminalAction>();
        public override void BeforeStart()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter += CustomControlGetter;
            MyAPIGateway.TerminalControls.CustomActionGetter += CustomActionGetter;
            _remoteControls.Add(GetCombobox("SetGridClassLargeStatic", SetComboboxContentLargeStatic,
                remote => remote.CubeGrid.IsStatic && remote.CubeGrid.GridSizeEnum == MyCubeSize.Large));
            _remoteControls.Add(GetCombobox("SetGridClassLargeMobile", SetComboboxContentLargeGridMobile,
                remote => !remote.CubeGrid.IsStatic && remote.CubeGrid.GridSizeEnum == MyCubeSize.Large));
            _remoteControls.Add(GetCombobox("SetGridClassSmall", SetComboboxContentSmall,
                remote => !remote.CubeGrid.IsStatic && remote.CubeGrid.GridSizeEnum == MyCubeSize.Small));
            _remoteActions.Add(GetBoostButton("BoostButton", BoostButtonAvalibility));
        }
        private void BoostButtonWriter(IMyTerminalBlock block, StringBuilder sb)
        {
            var gridLogic = block.CubeGrid.GetMainGridLogic();
            if (gridLogic != null)
            {
                sb.Append(gridLogic.EnableBoost ? $"Go: {(int)Math.Round(gridLogic.BoostDuration/60.0f)}" : (gridLogic.BoostCoolDown>0? $"Wait: {(int)Math.Round(gridLogic.BoostCoolDown/60.0f)}" : "Ready"));
            }
            else
            {
                sb.Append("Boost: N/A");
            }
        }
        private static bool BoostButtonAvalibility(IMyTerminalBlock obj)
        {
            var GridLogic = obj.GetMainGridLogic();
            if(GridLogic==null){Utils.Log("gridnotfound");return(false);}
            if(GridLogic.BoostCoolDown==null){Utils.Log("BoostCooldown");return(false);}

            bool Enabled = !(GridLogic.BoostCoolDown>0);
            return(true);
        }
        private IMyTerminalAction GetBoostButton(string name, Func<IMyTerminalBlock, bool> isEnabled)
        {
            var BoostButton = MyAPIGateway.TerminalControls.CreateAction<IMyRemoteControl>(name);
            BoostButton.Enabled = isEnabled;
            BoostButton.Action = BoostButtonClicked;
            BoostButton.Icon=Path.Combine(ModContext.ModPath, "Textures", "BoostButton_Sad_Static.png");
            BoostButton.Writer = BoostButtonWriter;
            BoostButton.Name = new StringBuilder("Boost");
            return BoostButton;
        }

        private static void BoostButtonClicked(IMyTerminalBlock block)
        {
            var GridLogic = block.GetMainGridLogic();
            if(GridLogic==null){Utils.Log("gridnotfound");return;}
            if(GridLogic.EnableBoost==null){Utils.Log("BoostDataNotFOund");return;}
            if(GridLogic.BoostCoolDown>0){GridLogic.EnableBoost=false;Utils.ShowNotification("Booster On Cooldown!",block.CubeGrid,600);return;}
            GridLogic.EnableBoost= !GridLogic.EnableBoost;
            Utils.ShowNotification(GridLogic.EnableBoost ? "Booster Engaged!" : "Booster Disengaged!",block.CubeGrid,600);
        }
        protected override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= CustomControlGetter;
            MyAPIGateway.TerminalControls.CustomActionGetter -= CustomActionGetter;
        }

        public void CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (!(block is IMyRemoteControl)) return;
            if (controls.Any(control => _remoteControls.Contains(control))) return;
            controls.AddRange(_remoteControls);
            foreach (var control in controls.Where(control => ControlsToHideIfNotMainRemote.Contains(control.Id)))
                control.Enabled = TerminalChainedDelegate.Create(control.Visible, VisibleIfIsMainOwner);
        }
        public void CustomActionGetter(IMyTerminalBlock block, List<IMyTerminalAction> controls)
        {
            if (!(block is IMyRemoteControl)) return;
            if (controls.Any(control => _remoteActions.Contains(control))) return;
            controls.AddRange(_remoteActions);
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
                    $"RemoteGUI::SetGridClass: Sending change grid class message, entityId = {block.CubeGrid.EntityId}, grid class id = {key}",
                    2);
                cubeGridLogic.GridClassId = key;
                if (!Constants.IsServer)
                    ModSessionManager.Comms.ChangeGridClass(cubeGridLogic.Grid.EntityId, key);
            }
            else
            {
                Utils.Log($"RemoteGUI::SetGridClass: Unable to set GridClassId, GetGridLogic is returning null on {block.EntityId}", 3);
            }
        }
    }
}