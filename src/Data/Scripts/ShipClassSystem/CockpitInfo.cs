﻿using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Linq;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace ShipClassSystem
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Cockpit), false)]
    public class CockpitInfo : MyGameLogicComponent
    {
        private IMyCockpit _cockpit;
        //public CockpitInfo Instance;
        private CubeGridLogic GridLogic => _cockpit?.GetMainGridLogic();
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // the base methods are usually empty, except for OnAddedToContainer()'s, which has some sync stuff making it required to be called.
            base.Init(objectBuilder);
            //Instance=this;
            if (!Constants.IsClient) return;

            _cockpit = (IMyCockpit)Entity;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (!Constants.IsClient) return;

            if (_cockpit.CubeGrid?.Physics == null)
                return; // ignore ghost/projected grids
            _cockpit.AppendingCustomInfo += AppendingCustomInfo;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }
        public override void UpdateAfterSimulation()
        {
            if (GridLogic == null || _cockpit.CubeGrid == null || _cockpit.CubeGrid.IsStatic || !_cockpit.IsUnderControl){return;}
            GridLogic.EnforceSpeedLimit(_cockpit.CubeGrid);
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
            var gridLogic = block.GetMainGridLogic();
            if (gridLogic == null) return;

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
                    if (gridLogic.Blocks == null) continue;
                    var countWeight = gridLogic.BlocksPerLimit[blockLimit].Sum(l => l.Value);
                    FormatBlockLimitCheckResult(infoBuilder, blockLimit, countWeight);
                }
                    

            infoBuilder.Append("\nApplied Modifiers: \n\n");

            foreach (var modifierValue in GridLogic.Modifiers.GetModifierValues())
                infoBuilder.Append($"{modifierValue.Name}: {modifierValue.Value}\n");

            sb.Append(infoBuilder);
            
        }

        private static void FormatBlockLimitCheckResult(StringBuilder sb, BlockLimit blockLimit, double countWeight)
        {
            sb.Append($"{blockLimit.Name}: {countWeight}/{blockLimit.MaxCount}{(countWeight <= blockLimit.MaxCount ? "\n" : " (fail)\n")}");
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