using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace RedVsBlueClassSystem
{
	public static class CubeGridModifiers
	{
		public static void ApplyModifiers(IMyCubeBlock block, GridModifiers modifiers) {
			if(block is IMyThrust)
            {
				IMyThrust thruster = block as IMyThrust;

				thruster.ThrustMultiplier = modifiers.ThrusterForce;
				thruster.PowerConsumptionMultiplier = 1f / modifiers.ThrusterEfficiency;
            }

			if(block is IMyGyro)
            {
				IMyGyro gyro = block as IMyGyro;

				gyro.GyroStrengthMultiplier = modifiers.GyroForce;
				gyro.PowerConsumptionMultiplier = 1f / modifiers.GyroEfficiency;
            }

			if(block is IMyRefinery)
            {
				IMyRefinery refinery = block as IMyRefinery;

				refinery.UpgradeValues["Productivity"] = modifiers.RefineSpeed;
				refinery.UpgradeValues["Effectiveness"] = modifiers.RefineEfficiency;
				refinery.UpgradeValues["PowerEfficiency"] = modifiers.RefineSpeed;
			}

			if(block is IMyAssembler)
            {
				IMyAssembler assembler = block as IMyAssembler;

				assembler.UpgradeValues["Productivity"] = modifiers.RefineSpeed;
				assembler.UpgradeValues["PowerEfficiency"] = modifiers.RefineSpeed;
			}

			if(block is IMyReactor)
            {
				IMyReactor reactor = block as IMyReactor;

				reactor.PowerOutputMultiplier = modifiers.PowerProducersOutput;
			}

			if(block is IMyShipDrill)
            {
				IMyShipDrill drill = block as IMyShipDrill;

				drill.DrillHarvestMultiplier = modifiers.DrillHarvestMutiplier;
            }
		}
	}
}
