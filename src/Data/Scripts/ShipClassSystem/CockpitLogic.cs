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
        private IMyCockpit _cockpit;
        private CubeGridLogic GridLogic => _cockpit?.GetGridLogic();

        public override void OnAddedToScene()
        {
            // the base methods are usually empty, except for OnAddedToContainer()'s, which has some sync stuff making it required to be called.
            _cockpit = (IMyCockpit)Entity;
            Entity.OnPhysicsChanged += InitializeLogic;
        }

        public void InitializeLogic(IMyEntity _)
        {
            if (_cockpit.CubeGrid?.Physics == null)
                return; // ignore ghost/projected grids

            _cockpit.AppendingCustomInfo += AppendingCustomInfo;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            try 
            {
                if (MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.ControlPanel) return;
                //TODO only run this if grid check results actually change
                _cockpit.RefreshCustomInfo();
                _cockpit.SetDetailedInfoDirty();
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
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
                var concreteGrid = _cockpit.CubeGrid as MyCubeGrid;
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