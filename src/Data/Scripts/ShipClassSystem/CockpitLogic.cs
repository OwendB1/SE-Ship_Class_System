using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Cockpit), false)]
    public class CockpitLogic : MyGameLogicComponent
    {
        private IMyCockpit _Cockpit;
        private CubeGridLogic GridLogic => _Cockpit?.GetGridLogic();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // the base methods are usually empty, except for OnAddedToContainer()'s, which has some sync stuff making it required to be called.
            base.Init(objectBuilder);

            _Cockpit = (IMyCockpit)Entity;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            if (_Cockpit.CubeGrid?.Physics == null)
                return; // ignore ghost/projected grids

            _Cockpit.AppendingCustomInfo += AppendingCustomInfo;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            //UpdateCockpit();

            try // only for non-critical code
            {
                // ideally you want to refresh this only when necessary but this is a good compromise to only refresh it if player is in the terminal.
                // this check still doesn't mean you're looking at even the same grid's terminal as this block, for that there's other ways to check it if needed.
                //TODO only if correct grid/block?
                //"one way is to hook MyAPIGateway.TerminalControls.CustomControlGetter and store the block or entityId of that, and that is your last selected block (gets triggered per selected block including for multiple selected)"

                if (MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.ControlPanel) return;
                //TODO only run this if grid check results actually change
                _Cockpit.RefreshCustomInfo();
                _Cockpit.SetDetailedInfoDirty();
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public void UpdateCockpit()
        {
            var gridClass =
                GridLogic?.GridClass; //<this was returning null, either because Cockpit = null, or GetGridLogic isn't working

            if (gridClass == null) return;

            /*if(primaryOwnerId != -1)
            {
                Cockpit.own
                Cockpit.OwnerId = primaryOwnerId;
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
                    Utils.Log("Updating MyCockpit detailed info failed, grid is missing CubeGridLogic", 3);
                    return;
                }

                var gridClass = gridLogic.GridClass;
                var concreteGrid = _Cockpit.CubeGrid as MyCubeGrid;
                if (gridClass == null || concreteGrid == null) return;

                var infoBuilder = new StringBuilder();
                infoBuilder.Append($"\nClass: {gridClass.Name} \n\n");

                FormatRangeCheckResult("Blocks", infoBuilder, gridClass.MinBlocks, gridClass.MaxBlocks, concreteGrid.BlocksCount);
                FormatMaxCheckResult("Mass", infoBuilder, gridClass.MaxMass, concreteGrid.Mass);
                FormatMaxCheckResult("PCU", infoBuilder, gridClass.MaxPCU, concreteGrid.BlocksPCU);

                if (gridClass.BlockLimits != null)
                    foreach (var blockLimit in gridClass.BlockLimits)
                    {
                        var relevantBlocks = gridLogic.Blocks.Where(b => blockLimit.BlockTypes
                            .Any(bl => bl.SubtypeId == Utils.GetBlockSubtypeId(b) && 
                                       bl.TypeId == Utils.GetBlockId(b))).ToList();
                        FormatBlockLimitCheckResult(infoBuilder, blockLimit, relevantBlocks);
                    }
                        

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

        private static void FormatBlockLimitCheckResult(StringBuilder sb, BlockLimit blockLimit, IReadOnlyCollection<IMyCubeBlock> blocks)
        {
            sb.Append($"{blockLimit.Name}: {blocks.Count}/{blockLimit.MaxCount}{(blocks.Count <= blockLimit.MaxCount ? "\n" : " (fail)\n")}");
        }

        private static void FormatMaxCheckResult(string name, StringBuilder sb, float max,float value)
        {
            if (max > 0)
                sb.Append($"{name}: {value}/{max}{(value <= max ? "\n" : " (fail)\n")}");
        }

        private static void FormatMaxCheckResult(string name, StringBuilder sb, int max, int value)
        {
            if (max > 0)
                sb.Append($"{name}: {value}/{max}{(value <= max ? "\n" : " (fail)\n")}");
        }

        private static void FormatRangeCheckResult(string name, StringBuilder sb, int min, int max, int value)
        {
            if (min < 1 && max < 1) return;
            var passed = value <= max && value >= min;
            var range = min > 0 && max > 0
                ? $"{min} - {max}"
                : min > 0
                    ? $">= {min}"
                    : $"<= {max}";

            sb.Append($"{name}: {value}/{range}{(passed ? "\n" : " (fail)\n")}");
        }
    }
}