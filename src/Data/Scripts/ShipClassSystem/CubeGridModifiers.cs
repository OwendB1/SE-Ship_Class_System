using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace RedVsBlueClassSystem
{
    public static class CubeGridModifiers
    {
        public static void ApplyModifiers(IMyCubeBlock block, GridModifiers modifiers)
        {
            if (block is IMyThrust)
            {
                var thruster = block as IMyThrust;

                thruster.ThrustMultiplier = modifiers.ThrusterForce;
                thruster.PowerConsumptionMultiplier = 1f / modifiers.ThrusterEfficiency;
            }

            if (block is IMyGyro)
            {
                var gyro = block as IMyGyro;

                gyro.GyroStrengthMultiplier = modifiers.GyroForce;
                gyro.PowerConsumptionMultiplier = 1f / modifiers.GyroEfficiency;
            }

            if (block is IMyRefinery)
            {
                var refinery = block as IMyRefinery;

                refinery.UpgradeValues["Productivity"] = modifiers.RefineSpeed;
                refinery.UpgradeValues["Effectiveness"] = modifiers.RefineEfficiency;
                refinery.UpgradeValues["PowerEfficiency"] = modifiers.RefineSpeed;
            }

            if (block is IMyAssembler)
            {
                var assembler = block as IMyAssembler;

                assembler.UpgradeValues["Productivity"] = modifiers.RefineSpeed;
                assembler.UpgradeValues["PowerEfficiency"] = modifiers.RefineSpeed;
            }

            if (block is IMyReactor)
            {
                var reactor = block as IMyReactor;

                reactor.PowerOutputMultiplier = modifiers.PowerProducersOutput;
            }

            if (block is IMyShipDrill)
            {
                var drill = block as IMyShipDrill;

                drill.DrillHarvestMultiplier = modifiers.DrillHarvestMutiplier;
            }
        }
    }
}