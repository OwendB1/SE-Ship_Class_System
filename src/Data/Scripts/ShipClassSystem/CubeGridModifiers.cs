using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public static class CubeGridModifiers
    {
        public static void ApplyModifiers(IMyCubeBlock block, GridModifiers modifiers)
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
                refinery.UpgradeValues["Productivity"] *= modifiers.RefineSpeed;
                refinery.UpgradeValues["Effectiveness"] *= modifiers.RefineEfficiency;
                refinery.UpgradeValues["PowerEfficiency"] *= modifiers.RefineSpeed;
            }

            var assembler = block as IMyAssembler;
            if (assembler != null)
            {
                assembler.UpgradeValues["Productivity"] *= modifiers.RefineSpeed;
                assembler.UpgradeValues["PowerEfficiency"] *= modifiers.RefineSpeed;
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

        public static void GridClassDamageHandler(object target, ref MyDamageInformation damageInfo)
        {
            var myBlock = target as IMySlimBlock;
            if (myBlock == null) return;
            var myGrid = myBlock.CubeGrid;
            var myGridLogic = myGrid.GetGridLogic();
            if (damageInfo.Type == MyDamageType.Bullet) { damageInfo.Amount *= myGridLogic.GridClass.DamageModifiers.Bullet; }
            if (damageInfo.Type == MyDamageType.Rocket) { damageInfo.Amount *= myGridLogic.GridClass.DamageModifiers.Rocket; }
            if (damageInfo.Type == MyDamageType.Explosion) { damageInfo.Amount *= myGridLogic.GridClass.DamageModifiers.Explosion; }
            if (damageInfo.Type == MyDamageType.Environment) { damageInfo.Amount *= myGridLogic.GridClass.DamageModifiers.Environment; }
        }
    }
}