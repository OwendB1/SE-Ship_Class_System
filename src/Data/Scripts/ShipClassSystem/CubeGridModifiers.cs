using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public static class CubeGridModifiers
    {
        public static void ApplyModifiers(IMyCubeBlock block, GridModifiers modifiers)
        {
            if (!Constants.IsServer) return;
            try
            {
                var thruster = block as IMyThrust;
                if (thruster != null)
                {
                    thruster.ThrustMultiplier = modifiers.ThrusterForce;
                    thruster.PowerConsumptionMultiplier = 1f / modifiers.ThrusterEfficiency;
                }

                var gyro = block as IMyGyro;
                if (gyro != null)
                {
                    gyro.GyroStrengthMultiplier = modifiers.GyroForce;
                    gyro.PowerConsumptionMultiplier = 1f / modifiers.GyroEfficiency;
                }

                var refinery = block as IMyRefinery;
                if (refinery != null)
                {
                    var rawRefinery = block as MyCubeBlock;
                    if (rawRefinery?.CurrentAttachedUpgradeModules != null)
                    {
                        var productivity = 1f;
                        var effectiveness = 1f;
                        foreach (var module in rawRefinery.CurrentAttachedUpgradeModules)
                        {
                            if (module.Value.Block.UpgradeValues["Productivity"] > 0f)
                                productivity += module.Value.Block.UpgradeValues["Productivity"];
                            else
                            if (module.Value.Block.UpgradeValues["Effectiveness"] > 0f)
                                effectiveness += module.Value.Block.UpgradeValues["Effectiveness"];
                        }
                        refinery.UpgradeValues["Productivity"] = productivity * modifiers.RefineSpeed;
                        refinery.UpgradeValues["Effectiveness"] = effectiveness * modifiers.RefineEfficiency;
                    }
                    else
                    {
                        refinery.UpgradeValues["Productivity"] = modifiers.RefineSpeed;
                        refinery.UpgradeValues["Effectiveness"] = modifiers.RefineEfficiency;
                    }
                }

                var assembler = block as IMyAssembler;
                if (assembler != null)
                {
                    assembler.UpgradeValues["Productivity"] *= modifiers.AssemblerSpeed;
                    var rawAssembler = block as MyCubeBlock;
                    if (rawAssembler?.CurrentAttachedUpgradeModules != null)
                    {
                        var productivity = rawAssembler.CurrentAttachedUpgradeModules
                            .Where(module => module.Value.Block.UpgradeValues["Productivity"] > 0f)
                            .Sum(module => module.Value.Block.UpgradeValues["Productivity"]);
                        assembler.UpgradeValues["Productivity"] = productivity * modifiers.RefineSpeed;
                    }
                    else
                    {
                        assembler.UpgradeValues["Productivity"] = modifiers.AssemblerSpeed;
                    }
                }

                var reactor = block as IMyReactor;
                if (reactor != null)
                {
                    reactor.PowerOutputMultiplier = modifiers.PowerProducersOutput;
                }

                var drill = block as IMyShipDrill;
                if (drill != null)
                {
                    drill.DrillHarvestMultiplier = modifiers.DrillHarvestMultiplier;
                }
            }
            catch
            {
                Utils.Log("Could not set modifier, most likely due to server closure.");
            }
        }

        public static void GridClassDamageHandler(object target, ref MyDamageInformation damageInfo)
        {
            var myBlock = target as IMySlimBlock;
            if (myBlock == null) return;
            var myGrid = myBlock.CubeGrid;
            var myGridLogic = myGrid.GetMainGridLogic();
            if (myGridLogic == null) return;
            if (damageInfo.Type == MyDamageType.Bullet) { damageInfo.Amount *= myGridLogic.DamageModifiers.Bullet; }
            if (damageInfo.Type == MyDamageType.Rocket) { damageInfo.Amount *= myGridLogic.DamageModifiers.Rocket; }
            if (damageInfo.Type == MyDamageType.Explosion) { damageInfo.Amount *= myGridLogic.DamageModifiers.Explosion; }
            if (damageInfo.Type == MyDamageType.Environment) { damageInfo.Amount *= myGridLogic.DamageModifiers.Environment; }
            if (damageInfo.Type == MyStringHash.GetOrCompute("Energy")) { damageInfo.Amount *= myGridLogic.DamageModifiers.Energy; }
            if (damageInfo.Type == MyStringHash.GetOrCompute("Kinetic")) { damageInfo.Amount *= myGridLogic.DamageModifiers.Kinetic; }
        }
    }
}