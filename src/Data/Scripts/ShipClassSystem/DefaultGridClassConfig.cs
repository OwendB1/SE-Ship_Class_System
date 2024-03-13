using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedVsBlueClassSystem
{
    static class DefaultGridClassConfig
    {
        //vanilla small grid fixed weapons
        private static BlockType SmallMissileLauncher = new BlockType("SmallMissileLauncher");
        private static BlockType SmallWarfareMissileLauncher = new BlockType("SmallMissileLauncher", "SmallMissileLauncherWarfare2");
        private static BlockType SmallMissileLauncherReload = new BlockType("SmallMissileLauncherReload", "SmallMissileLauncherReload");
        private static BlockType SmallAssaultCannon = new BlockType("SmallMissileLauncherReload", "SmallBlockMediumCalibreGun");
        private static BlockType SmallGatlingGun = new BlockType("SmallGatlingGun", "");
        private static BlockType SmallWarfareGatlingGun = new BlockType("SmallGatlingGun", "SmallGatlingGunWarfare2");
        private static BlockType SmallAutocannon = new BlockType("SmallGatlingGun", "SmallBlockAutocannon");
        private static BlockType SmallRailgun = new BlockType("ConveyorSorter", "SmallRailgun");

        private static BlockType[] SmallGridFixedWeapons = new BlockType[] { SmallMissileLauncher, SmallWarfareMissileLauncher, SmallMissileLauncherReload, SmallAssaultCannon, SmallGatlingGun, SmallWarfareGatlingGun, SmallAutocannon, SmallRailgun };

        //vanilla small grid turrets
        private static BlockType AssaultCannonTurret = new BlockType("LargeMissileTurret", "SmallBlockMediumCalibreTurret");
        private static BlockType AutocannonTurret = new BlockType("LargeMissileTurret", "AutoCannonTurret");
        private static BlockType SmallMissileTurret = new BlockType("LargeMissileTurret", "SmallMissileTurret");
        private static BlockType SmallGatlingTurret = new BlockType("LargeGatlingTurret", "SmallGatlingTurret");

        private static BlockType[] SmallGridTurretWeapons = new BlockType[] { AssaultCannonTurret, AutocannonTurret, SmallMissileTurret, SmallGatlingTurret };

        //vanilla large grid fixed weapons
        private static BlockType LargeMissileLauncher = new BlockType("SmallMissileLauncher", "LargeMissileLauncher");
        private static BlockType LargeArtilleryGun = new BlockType("SmallMissileLauncher", "LargeBlockLargeCalibreGun");
        private static BlockType LargeRailgun = new BlockType("ConveyorSorter", "LargeRailgun");

        private static BlockType[] LargeGridFixedWeapons = new BlockType[] { LargeMissileLauncher, LargeArtilleryGun, LargeRailgun };

        //vanilla large grid turrets
        private static BlockType LargeMissileTurret = new BlockType("LargeMissileTurret");
        private static BlockType LargeAssaultCannonTurret = new BlockType("LargeMissileTurret", "LargeBlockMediumCalibreTurret");
        private static BlockType LargeArtilleryTurret = new BlockType("LargeMissileTurret", "LargeCalibreTurret");
        private static BlockType LargeGatlingTurret = new BlockType("LargeGatlingTurret");
        private static BlockType InteriorTurret = new BlockType() { CountWeight = 1, TypeId = "InteriorTurret", SubtypeId = "LargeInteriorTurret" };

        private static BlockType[] LargeGridTurretWeapons = new BlockType[] { LargeMissileTurret, LargeAssaultCannonTurret, LargeArtilleryTurret, LargeGatlingTurret, InteriorTurret };

        private static BlockType[] SmallGridWeapons = SmallGridFixedWeapons.Concat(SmallGridTurretWeapons).ToArray();
        private static BlockType[] LargeGridWeapons = LargeGridFixedWeapons.Concat(LargeGridTurretWeapons).ToArray();
        private static BlockType[] VanillaWeapons = SmallGridWeapons.Concat(LargeGridWeapons).ToArray();

        private static BlockType[] Artillery = new BlockType[] { LargeArtilleryGun, LargeArtilleryTurret };

        //Tools
        private static BlockType SmallDrill = new BlockType("Drill", "SmallBlockDrill");
        private static BlockType LargeDrill = new BlockType("Drill", "LargeBlockDrill");

        private static BlockType[] Drills = new BlockType[] {
            SmallDrill,
            LargeDrill,
        };

        private static BlockType SmallWelder = new BlockType("ShipWelder", "SmallShipWelder");
        private static BlockType LargeWelder = new BlockType("ShipWelder", "LargeShipWelder");

        private static BlockType[] Welders = new BlockType[] {
            SmallWelder,
            LargeWelder,
        };

        //Misc vanilla
        private static BlockType[] ProgrammableBlocks = new BlockType[] {
            new BlockType("MyProgrammableBlock", "LargeProgrammableBlock"),
            new BlockType("MyProgrammableBlock", "LargeProgrammableBlockReskin"),
            new BlockType("MyProgrammableBlock", "SmallProgrammableBlock"),
            new BlockType("MyProgrammableBlock", "SmallProgrammableBlockReskin"),
        };

        private static BlockType[] Assemblers = new BlockType[] {
            new BlockType("Assembler", "BasicAssembler"),
            new BlockType("Assembler", "LargeAssemblerIndustrial"),
            new BlockType("Assembler", "LargeAssembler"),
        };

        private static BlockType[] Refineries = new BlockType[] {
            new BlockType("Refinery", "Blast Furnace"),
            new BlockType("Refinery", "LargeRefineryIndustrial"),
            new BlockType("Refinery", "LargeRefinery"),
        };

        private static BlockType[] O2H2Generators = new BlockType[] {
            new BlockType("OxygenGenerator", "OxygenGeneratorSmall"),
            new BlockType("OxygenGenerator", ""),
        };

        private static BlockType[] Gyros = new BlockType[] {
            new BlockType("Gyro", "SmallBlockGyro"),
            new BlockType("Gyro", "LargeBlockGyro"),
        };

        //Build and Repair
        private static BlockType BuildAndRepair = new BlockType("ShipWelder", "SELtdLargeNanobotBuildAndRepairSystem");

        //Energy shields
        private static BlockType[] EnergyShieldGenerators = new BlockType[] {
            new BlockType("Refinery", "LargeShipSmallShieldGeneratorBase"),
            new BlockType("Refinery", "LargeShipLargeShieldGeneratorBase"),
            new BlockType("Refinery", "SmallShipMicroShieldGeneratorBase"),
            new BlockType("UpgradeModule", "ShieldCapacitor"),
        };

        //Star Citizen
        private static BlockType[] SCLargeLasers = new BlockType[] {
            new BlockType("ConveyorSorter", "LG_CF117"),
            new BlockType("ConveyorSorter", "LG_CF227", 2),
            new BlockType("ConveyorSorter", "LG_CF337", 3),
            new BlockType("ConveyorSorter", "LG_M3A"),
            new BlockType("ConveyorSorter", "LG_M4A", 2),
            new BlockType("ConveyorSorter", "LG_M5A", 3),
        };

        private static BlockType[] SCSmallLasers = new BlockType[] {
            new BlockType("ConveyorSorter", "SG_CF117"),
            new BlockType("ConveyorSorter", "SG_CF227", 2),
            new BlockType("ConveyorSorter", "SG_CF337", 3),
            new BlockType("ConveyorSorter", "SG_M3A"),
            new BlockType("ConveyorSorter", "SG_M4A", 2),
            new BlockType("ConveyorSorter", "SG_M5A", 3),
        };

        //TIO
        private static BlockType[] TIOSmallGuns = new BlockType[] {
            new BlockType("ConveyorSorter", "SG_Missile_Bay_Block"),
            new BlockType("ConveyorSorter", "SG_TankCannon_Block"),
            new BlockType("ConveyorSorter", "SG_Vulcan_AutoCannon_Block"),
            new BlockType("ConveyorSorter", "SG_Vulcan_SAMS_Block"),
            new BlockType("ConveyorSorter", "ThunderBoltGatlingGun_Block"),
        };

        private static BlockType[] TIOMissiles = new BlockType[] {
            new BlockType("ConveyorSorter", "SG_Missile_Bay_Block"),
            new BlockType("ConveyorSorter", "VMLS_Block"),
        };

        private static BlockType[] TIOTorpedo = new BlockType[] {
            new BlockType("ConveyorSorter", "Torp_Block", 3),
        };

        private static BlockType[] TIOLargeGeneralGuns = new BlockType[] {
            new BlockType("ConveyorSorter", "Laser_Block"),
            new BlockType("ConveyorSorter", "CoilgunMk2_Block"),
            new BlockType("ConveyorSorter", "IronMaiden_Block"),
            new BlockType("ConveyorSorter", "MK1BattleshipGun_Block", 2),
            new BlockType("ConveyorSorter", "MK1Railgun_Block", 2),
            new BlockType("ConveyorSorter", "PDCTurret_Block", 0.5f),
            new BlockType("ConveyorSorter", "PriestReskin_Block", 1),
            new BlockType("ConveyorSorter", "Concordia_Block", 0.5f),
            new BlockType("ConveyorSorter", "MBA57Bofors_Block", 0.5f),
        };

        private static BlockType[] TIOLargeMk2Guns = new BlockType[] {
            new BlockType("ConveyorSorter", "MK2_Battleship_Block", 2),
            new BlockType("ConveyorSorter", "MK2_Railgun_Block", 3),
        };

        private static BlockType[] TIOLargeMk3Guns = new BlockType[] {
            new BlockType("ConveyorSorter", "MK3_Battleship_Block", 3),
            new BlockType("ConveyorSorter", "MK3_Railgun_Block", 4),
        };

        private static BlockType[] Coilgun = new BlockType[] {
            new BlockType("ConveyorSorter", "CoilgunFixedEnd_Block", 1),
            new BlockType("ConveyorSorter", "CoilgunFixedStart_Block", 1),
            new BlockType("ConveyorSorter", "CoilgunFixedCore_Block", 1),
        };

        private static BlockType[] SuperLaser = new BlockType[] {
            new BlockType("ConveyorSorter", "SuperLaserLoader_Block", 1),
            new BlockType("ConveyorSorter", "SuperLaserCore_Block", 5),
            new BlockType("ConveyorSorter", "SuperLaserMuzzle_Block", 1),
        };

        //Stealthdrive
        private static BlockType[] StealthDrives = new BlockType[] {
            new BlockType("UpgradeModule", "StealthDrive"),
            new BlockType("UpgradeModule", "StealthDrive1x1"),
            new BlockType("UpgradeModule", "StealthHeatSink"),
            new BlockType("UpgradeModule", "StealthDriveSmall"),
            new BlockType("UpgradeModule", "StealthHeatSinkSmall"),
        };

        //XLBlocks
        /*private static BlockType[] XLBlocks = new BlockType[] {
            new BlockType("CubeBlock", "XL_1x"),
            new BlockType("CubeBlock", "XL_1xPlatform"),
            new BlockType("CubeBlock", "XL_1xMount"),
            new BlockType("CubeBlock", "XL_1xFrame"),
            new BlockType("CubeBlock", "XL_Block"),
            new BlockType("CubeBlock", "XL_BlockDetail"),
            new BlockType("CubeBlock", "XL_BlockHazard"),
            new BlockType("CubeBlock", "XL_BlockInvCorner"),
            new BlockType("CubeBlock", "XL_BlockSlope"),
            new BlockType("CubeBlock", "XL_BlockCorner"),
            new BlockType("CubeBlock", "XL_BlockBlockFrame"),
            new BlockType("CubeBlock", "XL_HalfBlock"),
            new BlockType("CubeBlock", "XL_HalfBlockCorner"),
            new BlockType("CubeBlock", "XL_HalfCornerBase"),
            new BlockType("CubeBlock", "XL_HalfCornerTip"),
            new BlockType("CubeBlock", "XL_HalfCornerTipInv"),
            new BlockType("CubeBlock", "XL_HalfCornerBaseInv"),
            new BlockType("CubeBlock", "XL_HalfSlopeBase"),
            new BlockType("CubeBlock", "XL_HalfHalfSlopeTip"),
            new BlockType("CubeBlock", "XL_PassageCorner"),
            new BlockType("CubeBlock", "XL_Passage"),
            new BlockType("CubeBlock", "XL_Hip"),
            new BlockType("CubeBlock", "XL_Brace"),
        };*/


        private static BlockLimit PBLimit = new BlockLimit() { Name = "PBs", MaxCount = 1, BlockTypes = ProgrammableBlocks };
        private static BlockLimit NoPBLimit = new BlockLimit() { Name = "PBs", MaxCount = 0, BlockTypes = ProgrammableBlocks };
        private static BlockLimit GyroLimit = new BlockLimit() { Name = "Gyros", MaxCount = 200, BlockTypes = Gyros };
        private static BlockLimit WelderLimit = new BlockLimit() { Name = "Welders", MaxCount = 10, BlockTypes = Welders };
        private static BlockLimit NoDrillsLimit = new BlockLimit() { Name = "Drills", MaxCount = 0, BlockTypes = Drills };
        private static BlockLimit DrillsLimit = new BlockLimit() { Name = "Drills", MaxCount = 80, BlockTypes = Drills };
        private static BlockLimit NoProductionLimit = new BlockLimit() { Name = "Production", MaxCount = 0, BlockTypes = Utils.ConcatArrays(Refineries, Assemblers) };
        private static BlockLimit NoShieldsLimit = new BlockLimit() { Name = "Shields", MaxCount = 0, BlockTypes = EnergyShieldGenerators };
        private static BlockLimit O2H2GeneratorsLimit = new BlockLimit() { Name = "O2/H2 gens", MaxCount = 5, BlockTypes = O2H2Generators };
        private static BlockLimit NoArtilleryLimit = new BlockLimit() { Name = "Artillery", MaxCount = 0, BlockTypes = Artillery };
        private static BlockLimit NoBuildAndRepairLimit = new BlockLimit() { Name = "B&R", MaxCount = 0, BlockTypes = new BlockType[] { BuildAndRepair } };
        private static BlockLimit BuildAndRepairLimit = new BlockLimit() { Name = "B&R", MaxCount = 1, BlockTypes = new BlockType[] { BuildAndRepair } };
        private static BlockLimit NoStealthLimit = new BlockLimit() { Name = "Stealth", MaxCount = 0, BlockTypes = StealthDrives };
        private static BlockLimit NoMissilesLimit = new BlockLimit() { Name = "Missiles", MaxCount = 0, BlockTypes = Utils.ConcatArrays(TIOMissiles, TIOTorpedo) };
        private static BlockLimit NoBigGunsLimit = new BlockLimit() { Name = "Capital Guns", MaxCount = 0, BlockTypes = Utils.ConcatArrays(TIOLargeMk2Guns, TIOLargeMk3Guns, Coilgun, SuperLaser) };
        private static BlockLimit NoTorpedosLimit = new BlockLimit() { Name = "Torpedos", MaxCount = 0, BlockTypes = Utils.ConcatArrays(TIOTorpedo) };
        private static BlockLimit MechanicalLimit = new BlockLimit() { Name = "Mech. Parts", MaxCount = 2, BlockTypes = new BlockType[] { 
            new BlockType("MotorStator", null),
            new BlockType("MotorAdvancedStator", null),
            new BlockType("ExtendedPistonBase", null),
        } };

        private static BlockLimit NoCapitalWeaponsLimit = new BlockLimit() { Name = "Capital Weapons", MaxCount = 0, BlockTypes = Utils.ConcatArrays(Coilgun, SuperLaser) };
        private static BlockLimit NoSuperLaserLimit = new BlockLimit() { Name = "Super Laser", MaxCount = 0, BlockTypes = SuperLaser };

        //private static BlockLimit NoXLBlocksLimit = new BlockLimit() { Name = "XL blocks", MaxCount = 0, BlockTypes = XLBlocks };

        public static GridModifiers DefaultGridModifiers = new GridModifiers()
        {
            ThrusterForce = 1,
            ThrusterEfficiency = 1,
            GyroForce = 0,
            GyroEfficiency = 0,
            AssemblerSpeed = 0,
            DrillHarvestMutiplier = 0,
            PowerProducersOutput = 1,
            RefineEfficiency = 0,
            RefineSpeed = 0
        };

        public static GridClass DefaultGridClassDefinition = new GridClass()
        {
            Id = 0,
            Name = "Derelict",
            SmallGridMobile = true,
            SmallGridStatic = true,
            LargeGridMobile = true,
            LargeGridStatic = true,
            ForceBroadCast = true,
            ForceBroadCastRange = 20000,
            Modifiers = DefaultGridModifiers
        };

        public static ModConfig DefaultModConfig = new ModConfig()
        {
            DefaultGridClass = DefaultGridClassDefinition,
            GridClasses = new GridClass[] {
            new GridClass() {
                Id = 1,
                Name = "Fighter",
                SmallGridMobile = true,
                MaxBlocks = 1500,
                ForceBroadCast = true,
                ForceBroadCastRange = 1500,
                MaxPerFaction = 7,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 2f,
                    ThrusterEfficiency = 2f,
                    GyroForce = 2f,
                    GyroEfficiency = 2f,
                    DrillHarvestMutiplier = 0,
                    PowerProducersOutput = 1,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 8, BlockTypes = Utils.ConcatArrays(SmallGridWeapons, SCSmallLasers, TIOMissiles, TIOSmallGuns) },
                    new BlockLimit() { Name = "Missiles", MaxCount = 2, BlockTypes = Utils.ConcatArrays(TIOMissiles) },
                    new BlockLimit() { Name = "Shields", MaxCount = 6, BlockTypes = EnergyShieldGenerators },
                    PBLimit,
                    GyroLimit,
                    O2H2GeneratorsLimit,
                    WelderLimit,
                    MechanicalLimit,
                    NoProductionLimit,
                    NoDrillsLimit,
                } },
            new GridClass() {
                Id = 10,
                Name = "Miner",
                SmallGridMobile = true,
                LargeGridMobile = true,
				MaxBlocks = 1250,
				ForceBroadCast = true,
                ForceBroadCastRange = 500,
                MaxPerFaction = 12,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 1.5f,
                    ThrusterEfficiency = 2,
                    GyroForce = 1,
                    GyroEfficiency = 1,
                    AssemblerSpeed = 1,
                    DrillHarvestMutiplier = 3,
                    PowerProducersOutput = 1,
                    RefineEfficiency = 1,
                    RefineSpeed = 1
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 6, BlockTypes = Utils.ConcatArrays(SmallGridWeapons, LargeGridWeapons, TIOSmallGuns, TIOLargeGeneralGuns, SCSmallLasers, SCLargeLasers) },
                    new BlockLimit() { Name = "Drills", MaxCount = 80, BlockTypes = new BlockType[] { LargeDrill, SmallDrill } },
                    new BlockLimit() { Name = "Shields", MaxCount = 1, BlockTypes = EnergyShieldGenerators },
                    GyroLimit,
                    O2H2GeneratorsLimit,
                    MechanicalLimit,
                    WelderLimit,
                    NoPBLimit,
                    new BlockLimit() { Name = "Artillery", MaxCount = 4, BlockTypes = Artillery },
                    NoMissilesLimit,
                    NoBigGunsLimit,
                    NoProductionLimit,
                    NoStealthLimit,
                    NoBuildAndRepairLimit,
                } },
            new GridClass() {
                Id = 11,
                Name = "PAM Miner",
                SmallGridMobile = true,
                LargeGridMobile = true,
                MaxBlocks = 1000,
                ForceBroadCast = false,
                ForceBroadCastRange = 2500,
                MaxPerFaction = 2,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 1,
                    ThrusterEfficiency = 1,
                    GyroForce = 1,
                    GyroEfficiency = 1,
                    AssemblerSpeed = 1,
                    DrillHarvestMutiplier = 3,
                    PowerProducersOutput = 1,
                    RefineEfficiency = 1,
                    RefineSpeed = 1
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 6, BlockTypes = Utils.ConcatArrays(SmallGridWeapons, LargeGridWeapons, TIOSmallGuns, TIOLargeGeneralGuns, SCSmallLasers, SCLargeLasers) },
                    new BlockLimit() { Name = "Drills", MaxCount = 40, BlockTypes = new BlockType[] { LargeDrill, SmallDrill } },
                    PBLimit,
                    GyroLimit,
                    O2H2GeneratorsLimit,
                    MechanicalLimit,
                    WelderLimit,
                    NoShieldsLimit,
                    NoArtilleryLimit,
                    NoMissilesLimit,
                    NoBigGunsLimit,
                    NoProductionLimit,
                    NoStealthLimit,
                    NoBuildAndRepairLimit,
                } },
            new GridClass() {
                Id = 20,
                Name = "Industrial Ship",
                SmallGridMobile = true,
                LargeGridMobile = true,
                MaxBlocks = 4000,
                ForceBroadCast = false,
                ForceBroadCastRange = 2500,
                MaxPerFaction = 12,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 1,
                    ThrusterEfficiency = 1,
                    GyroForce = 1,
                    GyroEfficiency = 1,
                    AssemblerSpeed = 0,
                    DrillHarvestMutiplier = 0,
                    PowerProducersOutput = 1,
                    RefineEfficiency = 0,
                    RefineSpeed = 0,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 6, BlockTypes = Utils.ConcatArrays(SmallGridWeapons, LargeGridWeapons, TIOSmallGuns, TIOLargeGeneralGuns, SCSmallLasers, SCLargeLasers) },
                    GyroLimit,
                    O2H2GeneratorsLimit,
                    MechanicalLimit,
                    WelderLimit,
                    NoPBLimit,
                    NoShieldsLimit,
                    NoDrillsLimit,
                    NoArtilleryLimit,
                    NoMissilesLimit,
                    NoBigGunsLimit,
                    NoProductionLimit,
                    NoStealthLimit,
                    NoBuildAndRepairLimit,
                } },
            new GridClass() {
                Id = 21,
                Name = "Builder Ship",
                SmallGridMobile = true,
                LargeGridMobile = true,
                MaxBlocks = 500,
                ForceBroadCast = true,
                ForceBroadCastRange = 500,
                MaxPerFaction = 1,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 1,
                    ThrusterEfficiency = 1,
                    GyroForce = 1,
                    GyroEfficiency = 1,
                    AssemblerSpeed = 0,
                    DrillHarvestMutiplier = 0,
                    PowerProducersOutput = 1,
                    RefineEfficiency = 0,
                    RefineSpeed = 0,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 6, BlockTypes = Utils.ConcatArrays(SmallGridWeapons, LargeGridWeapons, TIOSmallGuns, TIOLargeGeneralGuns, SCSmallLasers, SCLargeLasers) },
                    new BlockLimit() { Name = "B&R", MaxCount = 1, BlockTypes = new BlockType[] { BuildAndRepair } },
                    GyroLimit,
                    O2H2GeneratorsLimit,
                    MechanicalLimit,
                    WelderLimit,
                    NoPBLimit,
                    NoShieldsLimit,
                    NoDrillsLimit,
                    NoArtilleryLimit,
                    NoMissilesLimit,
                    NoBigGunsLimit,
                    NoProductionLimit,
                    NoStealthLimit,
                } },
            new GridClass() {
                Id = 110,
                Name = "Outpost",
                MaxBlocks = 2000,
                LargeGridStatic = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 1000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 0,
                    ThrusterEfficiency = 0,
                    GyroForce = 0,
                    GyroEfficiency = 0,
                    RefineEfficiency = 1,
                    RefineSpeed = 1,
                    PowerProducersOutput = 1,
                    DrillHarvestMutiplier = 1,
                    AssemblerSpeed = 1,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 10, BlockTypes = Utils.ConcatArrays(LargeGridWeapons, TIOLargeGeneralGuns, TIOLargeMk2Guns, TIOLargeMk3Guns, SCLargeLasers) },
                    new BlockLimit() { Name = "Assemblers", MaxCount = 1, BlockTypes = Assemblers },
                    new BlockLimit() { Name = "Refineries", MaxCount = 1, BlockTypes = Refineries },
                    WelderLimit,
                    O2H2GeneratorsLimit,
                    BuildAndRepairLimit,
                    PBLimit,
                    MechanicalLimit,
                    NoCapitalWeaponsLimit,
                    NoDrillsLimit,
                    NoShieldsLimit,
                    NoStealthLimit,
                }
            },
            new GridClass() {
                Id = 1600,
                Name = "Beacon Shrine",
                MaxBlocks = 500,
                LargeGridStatic = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 20000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 0,
                    ThrusterEfficiency = 0,
                    GyroForce = 0,
                    GyroEfficiency = 0,
                    RefineEfficiency = 0,
                    RefineSpeed = 0,
                    PowerProducersOutput = 1,
                    DrillHarvestMutiplier = 0,
                    AssemblerSpeed = 0,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 0, BlockTypes = Utils.ConcatArrays(LargeGridWeapons, TIOLargeGeneralGuns, TIOLargeMk2Guns, TIOLargeMk3Guns, SCLargeLasers, Coilgun, SuperLaser) },
                    new BlockLimit() { Name = "Welders", MaxCount = 0, BlockTypes = Welders },
                    new BlockLimit() { Name = "O2/H2 gens", MaxCount = 0, BlockTypes = O2H2Generators },
                    BuildAndRepairLimit,
                    PBLimit,
                    new BlockLimit() { Name = "Mech. Parts", MaxCount = 0, BlockTypes = new BlockType[] {
                        new BlockType("MotorStator", null),
                        new BlockType("MotorAdvancedStator", null),
                        new BlockType("ExtendedPistonBase", null),
                    } },
                    NoProductionLimit,
                    NoDrillsLimit,
                    NoShieldsLimit,
                    NoStealthLimit,
                }
            },
            new GridClass() {
                Id = 120,
                Name = "Assembler Outpost",
                MaxBlocks = 2000,
                LargeGridStatic = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 12000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 1,
                    ThrusterEfficiency = 1,
                    GyroForce = 1,
                    GyroEfficiency = 1,
                    RefineEfficiency = 3,
                    RefineSpeed = 1,
                    PowerProducersOutput = 1,
                    DrillHarvestMutiplier = 1,
                    AssemblerSpeed = 40,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 10, BlockTypes = Utils.ConcatArrays(LargeGridWeapons, TIOLargeGeneralGuns, TIOLargeMk2Guns, TIOLargeMk3Guns, SCLargeLasers) },
                    new BlockLimit() { Name = "Assemblers", MaxCount = 6, BlockTypes = Assemblers },
                    new BlockLimit() { Name = "Refineries", MaxCount = 1, BlockTypes = Refineries },
                    WelderLimit,
                    O2H2GeneratorsLimit,
                    BuildAndRepairLimit,
                    PBLimit,
                    MechanicalLimit,
                    NoCapitalWeaponsLimit,
                    NoDrillsLimit,
                    NoShieldsLimit,
                    NoStealthLimit,
                }
            },
            new GridClass() {
                Id = 130,
                Name = "Refinery Outpost",
                MaxBlocks = 2000,
                LargeGridStatic = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 12000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 1,
                    ThrusterEfficiency = 1,
                    GyroForce = 1,
                    GyroEfficiency = 1,
                    RefineEfficiency = 3,
                    RefineSpeed = 30,
                    PowerProducersOutput = 1,
                    DrillHarvestMutiplier = 1,
                    AssemblerSpeed = 2,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 10, BlockTypes = Utils.ConcatArrays(LargeGridWeapons, TIOLargeGeneralGuns, TIOLargeMk2Guns, TIOLargeMk3Guns, SCLargeLasers) },
                    new BlockLimit() { Name = "Assemblers", MaxCount = 1, BlockTypes = Assemblers },
                    new BlockLimit() { Name = "Refineries", MaxCount = 5, BlockTypes = Refineries },
                    WelderLimit,
                    O2H2GeneratorsLimit,
                    BuildAndRepairLimit,
                    PBLimit,
                    MechanicalLimit,
                    NoCapitalWeaponsLimit,
                    NoDrillsLimit,
                    NoShieldsLimit,
                    NoStealthLimit,
                }
            },
            new GridClass() {
                Id = 100,
                Name = "Faction base",
                MaxBlocks = 30000,
                MinBlocks = 2000,
                MaxPerFaction = 1,
                LargeGridStatic = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 20000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 1,
                    ThrusterEfficiency = 1,
                    GyroForce = 1,
                    GyroEfficiency = 1,
                    RefineEfficiency = 3,
                    RefineSpeed = 10,
                    PowerProducersOutput = 1,
                    DrillHarvestMutiplier = 1,
                    AssemblerSpeed = 10,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 40, BlockTypes = Utils.ConcatArrays(LargeGridWeapons, TIOLargeGeneralGuns, TIOLargeMk2Guns, TIOLargeMk3Guns, SCLargeLasers) },
                    new BlockLimit() { Name = "Assemblers", MaxCount = 5, BlockTypes = Assemblers },
                    new BlockLimit() { Name = "Refineries", MaxCount = 5, BlockTypes = Refineries },
                    WelderLimit,
                    O2H2GeneratorsLimit,
                    new BlockLimit() { Name = "B&R", MaxCount = 5, BlockTypes = new BlockType[]{BuildAndRepair } },
                    new BlockLimit() { Name = "PBs", MaxCount = 2, BlockTypes = ProgrammableBlocks },
                    MechanicalLimit,
                    NoCapitalWeaponsLimit,
                    NoDrillsLimit,
                    NoShieldsLimit,
                    NoStealthLimit,
                }
            },
            new GridClass() {
                Id = 200,
                Name = "Corvette",
                MaxBlocks = 1000,
                LargeGridMobile = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 1000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 6,
                    ThrusterEfficiency = 6,
                    GyroForce = 2,
                    GyroEfficiency = 2,
                    PowerProducersOutput = 1,
                    DrillHarvestMutiplier = 0,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 6, BlockTypes = Utils.ConcatArrays(LargeGridFixedWeapons, SCLargeLasers, TIOMissiles) },
                    new BlockLimit() { Name = "Shields", MaxCount = 1, BlockTypes = EnergyShieldGenerators },
                    new BlockLimit() { Name = "Missiles", MaxCount = 1, BlockTypes = TIOMissiles },
                    WelderLimit,
                    O2H2GeneratorsLimit,
                    GyroLimit,
                    PBLimit,
                    MechanicalLimit,
                    new BlockLimit() { Name = "Turrets", MaxCount = 0, BlockTypes = Utils.ConcatArrays(LargeGridTurretWeapons, TIOLargeGeneralGuns, TIOLargeMk2Guns, TIOLargeMk3Guns, TIOTorpedo) },
                    NoCapitalWeaponsLimit,
                    NoProductionLimit,
                    NoBuildAndRepairLimit,
                    NoDrillsLimit,
                }
            },
            new GridClass() {
                Id = 250,
                Name = "Frigate",
                MaxBlocks = 2000,
                MaxPerFaction = 6,
                LargeGridMobile = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 3000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 2,
                    ThrusterEfficiency = 2,
                    GyroForce = 2,
                    GyroEfficiency = 2,
                    RefineEfficiency = 1,
                    RefineSpeed = 1,
                    PowerProducersOutput = 1,
                    DrillHarvestMutiplier = 0,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 15, BlockTypes = Utils.ConcatArrays(LargeGridWeapons, TIOLargeGeneralGuns, SCLargeLasers, TIOMissiles) },
                    new BlockLimit() { Name = "Missiles", MaxCount = 1, BlockTypes = TIOMissiles },
                    new BlockLimit() { Name = "Mk1 guns", MaxCount = 6, BlockTypes = new BlockType[]{
                        new BlockType("ConveyorSorter", "MK1BattleshipGun_Block", 2),
                        new BlockType("ConveyorSorter", "MK1Railgun_Block", 2), 
                    } },
                    new BlockLimit() { Name = "Shields", MaxCount = 2, BlockTypes = EnergyShieldGenerators },
                    WelderLimit,
                    O2H2GeneratorsLimit,
                    GyroLimit,
                    PBLimit,
                    MechanicalLimit,
                    NoArtilleryLimit,
                    NoBigGunsLimit,
                    NoTorpedosLimit,
                    NoProductionLimit,
                    NoBuildAndRepairLimit,
                    NoDrillsLimit,
                    NoStealthLimit,
                }
            },
            new GridClass() {
                Id = 300,
                Name = "Destroyer",
                MaxBlocks = 4000,
                MinBlocks = 2000,
                MaxPerFaction = 3,
                LargeGridMobile = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 4000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 2f,
                    ThrusterEfficiency = 2f,
                    GyroForce = 2,
                    GyroEfficiency = 2,
                    RefineEfficiency = 1,
                    RefineSpeed = 1,
                    PowerProducersOutput = 1.2f,
                    DrillHarvestMutiplier = 0,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 30, BlockTypes = Utils.ConcatArrays(LargeGridWeapons, TIOLargeGeneralGuns, SCLargeLasers, TIOMissiles, TIOTorpedo, TIOLargeMk2Guns, TIOLargeMk3Guns, Coilgun) },
                    new BlockLimit() { Name = "Artillery", MaxCount = 10, BlockTypes = Artillery },
                    new BlockLimit() { Name = "Missiles", MaxCount = 8, BlockTypes = TIOMissiles },
                    new BlockLimit() { Name = "Torpedos", MaxCount = 6, BlockTypes = TIOTorpedo },
                    new BlockLimit() { Name = "Shields", MaxCount = 5, BlockTypes = EnergyShieldGenerators },
                    WelderLimit,
                    O2H2GeneratorsLimit,
                    GyroLimit,
                    PBLimit,
                    MechanicalLimit,
                    NoSuperLaserLimit,
                    NoProductionLimit,
                    NoBuildAndRepairLimit,
                    NoDrillsLimit,
                    NoStealthLimit,
                }
            },
            new GridClass() {
                Id = 400,
                Name = "Battleship",
                MaxBlocks = 8000,
                MinBlocks = 5000,
                MaxPerFaction = 2,
                LargeGridMobile = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 7000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 2.5f,
                    ThrusterEfficiency = 2.5f,
                    GyroForce = 4,
                    GyroEfficiency = 2,
                    RefineEfficiency = 0,
                    RefineSpeed = 0,
                    PowerProducersOutput = 1.5f,
                    DrillHarvestMutiplier = 0,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 45, BlockTypes = Utils.ConcatArrays(LargeGridWeapons, TIOLargeGeneralGuns, SCLargeLasers, TIOMissiles, TIOLargeMk2Guns, TIOLargeMk3Guns, Coilgun) },
                    new BlockLimit() { Name = "Artillery", MaxCount = 15, BlockTypes = Artillery },
                    new BlockLimit() { Name = "Missiles", MaxCount = 2, BlockTypes = TIOMissiles },
                    new BlockLimit() { Name = "Shields", MaxCount = 10, BlockTypes = EnergyShieldGenerators },
                    new BlockLimit() { Name = "B&R", MaxCount = 2, BlockTypes = new BlockType[]{ BuildAndRepair } },
                    WelderLimit,
                    O2H2GeneratorsLimit,
                    GyroLimit,
                    PBLimit,
                    MechanicalLimit,
                    NoTorpedosLimit,
                    NoSuperLaserLimit,
                    NoProductionLimit,
                    NoDrillsLimit,
                    NoStealthLimit,
                }
            },
            new GridClass() {
                Id = 500,
                Name = "Capital",
                MaxBlocks = 10000,
                MinBlocks = 6000,
                MaxPerFaction = 1,
                LargeGridMobile = true,
                ForceBroadCast = true,
                ForceBroadCastRange = 10000,
                Modifiers = new GridModifiers() {
                    ThrusterForce = 3.5f,
                    ThrusterEfficiency = 7f,
                    GyroForce = 5,
                    GyroEfficiency = 5,
                    RefineEfficiency = 1,
                    RefineSpeed = 1,
                    PowerProducersOutput = 1,
                    DrillHarvestMutiplier = 0,
                    AssemblerSpeed = 1,
                },
                BlockLimits = new BlockLimit[]{
                    new BlockLimit() { Name = "Weapons", MaxCount = 35, BlockTypes = Utils.ConcatArrays(LargeGridWeapons, TIOLargeGeneralGuns, SCLargeLasers, TIOMissiles, TIOLargeMk2Guns, Coilgun, SuperLaser) },
                    new BlockLimit() { Name = "Artillery", MaxCount = 5, BlockTypes = Artillery },
                    new BlockLimit() { Name = "Missiles", MaxCount = 2, BlockTypes = TIOMissiles },
                    new BlockLimit() { Name = "Shields", MaxCount = 15, BlockTypes = EnergyShieldGenerators },
                    new BlockLimit() { Name = "B&R", MaxCount = 2, BlockTypes = new BlockType[]{BuildAndRepair } },
                    WelderLimit,
                    O2H2GeneratorsLimit,
                    GyroLimit,
                    PBLimit,
                    MechanicalLimit,
                    new BlockLimit() { Name = "Mk3 guns", MaxCount = 0, BlockTypes = TIOLargeMk3Guns },
                    NoTorpedosLimit,
                    new BlockLimit() { Name = "Production", MaxCount = 4, BlockTypes = Utils.ConcatArrays(Refineries, Assemblers) },
                    NoDrillsLimit,
                    NoStealthLimit,
                }
            }
        }
        };
    }
}
