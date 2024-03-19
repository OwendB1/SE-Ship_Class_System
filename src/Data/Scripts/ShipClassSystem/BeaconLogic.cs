using System;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false, "SmallBlockBeacon", "LargeBlockBeacon",
        "SmallBlockBeaconReskin", "LargeBlockBeaconReskin")]
    public class BeaconLogic : MyGameLogicComponent
    {
        private IMyBeacon _beacon;
        private CubeGridLogic GridLogic => _beacon?.GetGridLogic();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // the base methods are usually empty, except for OnAddedToContainer()'s, which has some sync stuff making it required to be called.
            base.Init(objectBuilder);

            _beacon = (IMyBeacon)Entity;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            if (_beacon.CubeGrid?.Physics == null)
                return; // ignore ghost/projected grids

            _beacon.AppendingCustomInfo += AppendingCustomInfo;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            UpdateBeacon();

            try // only for non-critical code
            {
                // ideally you want to refresh this only when necessary but this is a good compromise to only refresh it if player is in the terminal.
                // this check still doesn't mean you're looking at even the same grid's terminal as this block, for that there's other ways to check it if needed.
                //TODO only if correct grid/block?
                //"one way is to hook MyAPIGateway.TerminalControls.CustomControlGetter and store the block or entityId of that, and that is your last selected block (gets triggered per selected block including for multiple selected)"

                if (MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.ControlPanel) return;
                //TODO only run this if grid check results actually change
                _beacon.RefreshCustomInfo();
                _beacon.SetDetailedInfoDirty();
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public void UpdateBeacon()
        {
            var gridClass =
                GridLogic?.GridClass; //<this was returning null, either because Beacon = null, or GetGridLogic isn't working

            if (gridClass == null) return;

            if (!gridClass.ForceBroadCast) return;
            _beacon.Enabled = true;
            _beacon.Radius = gridClass.ForceBroadCastRange;
            _beacon.HudText = $"{_beacon.CubeGrid.DisplayName} : {gridClass.Name}";

            /*if(primaryOwnerId != -1)
            {
                Beacon.own
                Beacon.OwnerId = primaryOwnerId;
            }*/
        }

        private void AppendingCustomInfo(IMyTerminalBlock block, StringBuilder sb)
        {
            // NOTE: don't Clear() the StringBuilder, it's the same instance given to all mods.

            try // only for non-critical code
            {
                var gridLogic = block.GetGridLogic();

                if (gridLogic == null)
                {
                    Utils.Log("Updating Beacon detailed info failed, grid is missing CubeGridLogic", 3);
                    return;
                }

                var gridClass = gridLogic.GridClass;
                if (gridClass == null) return;

                var infoBuilder = new StringBuilder();
                infoBuilder.Append($"\nClass: {gridClass.Name} \n\n");

                FormatRangeCheckResult("Blocks", infoBuilder, checkGridResult.MinBlocks, checkGridResult.MaxBlocks);
                FormatMaxCheckResult("Mass", infoBuilder, checkGridResult.MaxMass);
                FormatMaxCheckResult("PCU", infoBuilder, checkGridResult.MaxPCU);

                if (gridClass.BlockLimits != null)
                    for (var i = 0; i < gridClass.BlockLimits.Length; i++)
                        FormatBlockLimitCheckResult(infoBuilder, gridClass.BlockLimits[i],
                            checkGridResult.BlockLimits[i]);

                infoBuilder.Append("\nApplied Modifiers: \n\n");

                foreach (var modifierValue in GridLogic.Modifiers.GetModifierValues())
                    infoBuilder.Append($"{modifierValue.Name}: {modifierValue.Value}\n");

                sb.Append(infoBuilder);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        private static void FormatBlockLimitCheckResult(StringBuilder sb, BlockLimit blockLimit, BlockLimitCheckResult result)
        {
            sb.Append($"{blockLimit.Name}: {result.Score}/{result.Max}{(result.Passed ? "\n" : " (fail)\n")}");
        }

        private static void FormatMaxCheckResult<T>(string name, StringBuilder sb, GridCheckResult<T> result)
        {
            if (result.Active)
                sb.Append($"{name}: {result.Value}/{result.Limit}{(result.Passed ? "\n" : " (fail)\n")}");
        }

        private static void FormatRangeCheckResult<T>(string name, StringBuilder sb, GridCheckResult<T> min,
            GridCheckResult<T> max)
        {
            if (!min.Active && !max.Active) return;
            var value = min.Active ? min.Value : max.Value;
            var passed = min.Passed && max.Passed;
            var range = min.Active && max.Active
                ? $"{min.Limit} - {max.Limit}"
                : min.Active
                    ? $">= {min.Limit}"
                    : $"<= {max.Limit}";

            sb.Append($"{name}: {value}/{range}{(passed ? "\n" : " (fail)\n")}");
        }
    }
}