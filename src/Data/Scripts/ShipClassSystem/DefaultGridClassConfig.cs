namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    internal static class DefaultGridClassConfig
    {
        private static readonly BlockType[] VanillaSmallGridFixedWeapons =
        {
            new BlockType("SmallGatlingGun"),
            new BlockType("SmallMissileLauncher"),
            new BlockType("SmallMissileLauncher", "SmallMissileLauncherWarfare2"),
            new BlockType("SmallMissileLauncherReload", "SmallMissileLauncherReload"),
            new BlockType("SmallMissileLauncherReload", "SmallBlockMediumCalibreGun"),
            new BlockType("SmallGatlingGun", "SmallGatlingGunWarfare2"),
            new BlockType("SmallGatlingGun", "SmallBlockAutocannon"),
            new BlockType("ConveyorSorter", "SmallRailgun")
        };

        //vanilla small grid turrets
        private static readonly BlockType[] VanillaSmallGridTurretWeapons =
        {
            new BlockType("LargeMissileTurret", "SmallBlockMediumCalibreTurret"),
            new BlockType("LargeMissileTurret", "AutoCannonTurret"),
            new BlockType("LargeMissileTurret", "SmallMissileTurret"),
            new BlockType("LargeGatlingTurret", "SmallGatlingTurret")
        };

        //vanilla large grid fixed weapons
        private static readonly BlockType[] VanillaLargeGridFixedWeapons =
        {
            new BlockType("LargeGatlingTurret", "SmallGatlingTurret"),
            new BlockType("SmallMissileLauncher", "LargeBlockLargeCalibreGun"),
            new BlockType("ConveyorSorter", "LargeRailgun")
        };

        //vanilla large grid turrets
        private static readonly BlockType[] VanillaLargeGridTurretWeapons =
        {
            new BlockType("LargeMissileTurret"),
            new BlockType("LargeMissileTurret", "LargeBlockMediumCalibreTurret"),
            new BlockType("LargeMissileTurret", "LargeCalibreTurret"),
            new BlockType("LargeGatlingTurret"),
            new BlockType("InteriorTurret",  "LargeInteriorTurret")
        };

        //Tools

        private static readonly BlockType[] Drills =
        {
            new BlockType("Drill", "SmallBlockDrill"),
            new BlockType("Drill", "LargeBlockDrill")
        };

        private static readonly BlockType[] Welders =
        {
            new BlockType("ShipWelder", "SmallShipWelder"),
            new BlockType("ShipWelder", "LargeShipWelder")
        };

        private static readonly BlockType[] Grinders =
        {
            new BlockType("ShipGrinder", "SmallShipGrinder"),
            new BlockType("ShipGrinder", "LargeShipGrinder")
        };

        //Misc vanilla
        private static readonly BlockType[] SafeZone =
        {
            new BlockType("SafeZoneBlock", "SafeZoneBlock")
        };

        private static readonly BlockType[] ProgrammableBlocks =
        {
            new BlockType("MyProgrammableBlock", "LargeProgrammableBlock"),
            new BlockType("MyProgrammableBlock", "LargeProgrammableBlockReskin"),
            new BlockType("MyProgrammableBlock", "SmallProgrammableBlock"),
            new BlockType("MyProgrammableBlock", "SmallProgrammableBlockReskin")
        };

        private static readonly BlockType[] Assemblers =
        {
            new BlockType("Assembler", "BasicAssembler", 0.5f),
            new BlockType("Assembler", "LargeAssemblerIndustrial"),
            new BlockType("Assembler", "LargeAssembler")
        };

        private static readonly BlockType[] Refineries =
        {
            new BlockType("Refinery", "Blast Furnace", 0.5f),
            new BlockType("Refinery", "LargeRefineryIndustrial"),
            new BlockType("Refinery", "LargeRefinery")
        };

        private static readonly BlockType[] LargeHydrogenTanks =
        {
            new BlockType("OxygenTank", "LargeHydrogenTank"),
            new BlockType("OxygenTank", "SmallHydrogenTank")
        };

        private static readonly BlockType[] SmallHydrogenTanks =
        {
            new BlockType("OxygenTank", "LargeHydrogenTankSmall"),
            new BlockType("OxygenTank", "SmallHydrogenTankSmall")
        };

        private static readonly BlockType[] SmallCargoContainers =
        {
            new BlockType("CargoContainer", "SmallBlockSmallContainer"),
            new BlockType("CargoContainer", "LargeBlockSmallContainer")
        };

        private static readonly BlockType[] MediumCargoContainers =
        {
            new BlockType("CargoContainer", "SmallBlockMediumContainer")
        };

        private static readonly BlockType[] LargeCargoContainers =
        {
            new BlockType("CargoContainer", "SmallBlockLargeContainer"),
            new BlockType("CargoContainer", "LargeBlockLargeContainer")
        };

        private static readonly BlockType[] Gyros =
        {
            new BlockType("Gyro", "SmallBlockGyro"),
            new BlockType("Gyro", "LargeBlockGyro")
        };

        private static readonly BlockType[] StaticResourceDrills =
        {
            new BlockType("Drill", "AdvancedStaticDrill"),
            new BlockType("Drill", "StaticDrill"),
            new BlockType("Drill", "BasicStaticDrill")
        };

        private static readonly BlockType[] Collectors =
        {
            new BlockType("Collector", "Collector")
        };

        private static readonly BlockType[] ConveyorJunctions =
        {
            new BlockType("Conveyor", "SmallShipConveyorHub"),
            new BlockType("Conveyor", "SmallBlockConveyor"),
            new BlockType("Conveyor", "LargeBlockConveyor"),
            new BlockType("Conveyor", "LargeBlockConveyorPipeJunction"),
            new BlockType("AirVent", "AirVentFull")
        };

        //TIO
        private static readonly BlockType[] TIOStaticWeaponry =
        {
            new BlockType("ConveyorSorter", "SG_Missile_Bay_Block"),
            new BlockType("ConveyorSorter", "ThunderBoltGatlingGun_Block"),
            new BlockType("ConveyorSorter", "SG_Flare_Block"),
            new BlockType("ConveyorSorter", "VMLS_Block"),
            new BlockType("ConveyorSorter", "FixedTorpedo_Block"),
            new BlockType("ConveyorSorter", "SGTorpedoBayRight_Block"),
            new BlockType("ConveyorSorter", "SGTorpedoBayLeft_Block"),
            new BlockType("ConveyorSorter", "CoilgunFixedEnd_Block"),
            new BlockType("ConveyorSorter", "CoilgunFixedStart_Block"),
            new BlockType("ConveyorSorter", "CoilgunFixedCore_Block"),
            new BlockType("ConveyorSorter", "Null_Point_Jump_Disruptor_Large"),
        };

        private static readonly BlockType[] TIOTurrets =
        {
            new BlockType("ConveyorSorter", "SG_TankCannon_Block"),
            new BlockType("ConveyorSorter", "SG_Vulcan_AutoCannon_Block"),
            new BlockType("ConveyorSorter", "SG_Vulcan_SAMS_Block"),

            new BlockType("ConveyorSorter", "Laser_Block"),
            new BlockType("ConveyorSorter", "CoilgunMk2_Block"),
            new BlockType("ConveyorSorter", "Concordia_Block"),
            new BlockType("ConveyorSorter", "IronMaiden_Block"),
            new BlockType("ConveyorSorter", "MBA57Bofors_Block"),

            new BlockType("ConveyorSorter", "MK1BattleshipGun_Block"),
            new BlockType("ConveyorSorter", "MK2BattleshipGun_Block"),
            new BlockType("ConveyorSorter", "MK3BattleshipGun_Block"),

            new BlockType("ConveyorSorter", "MK1_Railgun_Block"),
            new BlockType("ConveyorSorter", "MK2_Railgun_Block"),
            new BlockType("ConveyorSorter", "MK3_Railgun_Block"),

            new BlockType("ConveyorSorter", "PDCTurret_Block"),
            new BlockType("ConveyorSorter", "Torp_Block"),
            new BlockType("ConveyorSorter", "PriestReskin_Block"),
        };

        private static readonly BlockType[] TIOBattleshipTurrets =
        {
            new BlockType("ConveyorSorter", "MK1BattleshipGun_Block"),
            new BlockType("ConveyorSorter", "MK2BattleshipGun_Block"),
            new BlockType("ConveyorSorter", "MK3BattleshipGun_Block"),
        };

        private static readonly BlockType[] TIORailgunTurrets =
        {
            new BlockType("ConveyorSorter", "MK1_Railgun_Block"),
            new BlockType("ConveyorSorter", "MK2_Railgun_Block"),
            new BlockType("ConveyorSorter", "MK3_Railgun_Block"),
        };

        private static readonly BlockType SuperLaserLoader = new BlockType("ConveyorSorter", "SuperLaserLoader_Block");
        private static readonly BlockType SuperLaserCore = new BlockType("ConveyorSorter", "SuperLaserCore_Block");
        private static readonly BlockType SuperLaserMuzzle = new BlockType("ConveyorSorter", "SuperLaserMuzzle_Block");
        private static readonly BlockType SuperLaserUpgrade = new BlockType("ConveyorSorter", "SuperLaserUpgrade_Block");

        private static readonly BlockType[] SuperLaserParts =
        {
            SuperLaserLoader,
            SuperLaserCore,
            SuperLaserMuzzle,
            SuperLaserUpgrade
        };

        // Concatenated block types

        private static readonly BlockType[] StaticWeaponry = Utils.ConcatArrays(VanillaSmallGridFixedWeapons, VanillaLargeGridFixedWeapons, TIOStaticWeaponry);
        private static readonly BlockType[] Turrets = Utils.ConcatArrays(VanillaLargeGridTurretWeapons, VanillaSmallGridTurretWeapons, TIOTurrets);
        private static readonly BlockType[] Production = Utils.ConcatArrays(Refineries, Assemblers);

        // Block limits

        private static readonly BlockLimit NoDrillsLimit = new BlockLimit
            { Name = "Drills", MaxCount = 0, BlockTypes = Drills };

        private static readonly BlockLimit NoProductionLimit = new BlockLimit
            { Name = "Production", MaxCount = 0, BlockTypes = Production };

        private static readonly BlockLimit NoShipToolsLimit = new BlockLimit
            { Name = "Ship Tools", MaxCount = 0, BlockTypes = Utils.ConcatArrays(Grinders, Welders)};

        private static readonly BlockLimit NoSuperLaserLimit = new BlockLimit
            { Name = "Super Laser", MaxCount = 0, BlockTypes = SuperLaserParts };

        private static readonly BlockLimit NoWeaponsLimit = new BlockLimit
            { Name = "Weapons", MaxCount = 0, BlockTypes = Utils.ConcatArrays(Turrets, StaticWeaponry) };

        private static readonly BlockLimit NoSafeZoneLimit = new BlockLimit
            { Name = "Safe Zone", MaxCount = 0, BlockTypes = SafeZone };

        private static readonly BlockLimit SingleSafeZoneLimit = new BlockLimit
            { Name = "Safe Zone", MaxCount = 1, BlockTypes = SafeZone };

        private static readonly BlockLimit NoStaticDrillLimit = new BlockLimit
            { Name = "Static Resource Drill", MaxCount = 0, BlockTypes = StaticResourceDrills };

        private static readonly BlockLimit SingleStaticDrillLimit = new BlockLimit
            { Name = "Static Resource Drill", MaxCount = 1, BlockTypes = StaticResourceDrills };

        private static readonly BlockLimit NoCollectorLimit = new BlockLimit
            { Name = "Collector", MaxCount = 1, BlockTypes = Collectors };

        private static readonly BlockLimit SingleCollectorLimit = new BlockLimit
            { Name = "Collector", MaxCount = 1, BlockTypes = Collectors };

        private static readonly BlockLimit NoGyroLimit = new BlockLimit
            { Name = "Gyro", MaxCount = 1, BlockTypes = Gyros };

        public static GridModifiers DefaultGridModifiers = new GridModifiers
        {
            ThrusterForce = 0.5f,
            ThrusterEfficiency = 1,
            GyroForce = 0,
            GyroEfficiency = 0,
            AssemblerSpeed = 0,
            DrillHarvestMultiplier = 0,
            PowerProducersOutput = 1,
            RefineEfficiency = 0,
            RefineSpeed = 0
        };

        public static GridDamageModifiers DefaultGridDamageModifiers2X = new GridDamageModifiers
        {
            Explosion = 2f,
            Environment = 2f,
            Energy = 2f,
            Kinetic = 2f,
            Bullet = 2f,
            Rocket = 2f
        };

        public static GridClass DefaultGridClassDefinition = new GridClass
        {
            Id = 0,
            Name = "Derelict",
            MaxBlocks = 10000,
            SmallGrid = true,
            LargeGridMobile = true,
            LargeGridStatic = true,
            ForceBroadCast = false,
            ForceBroadCastRange = 2000,
            Modifiers = DefaultGridModifiers,
            BlockLimits = new []
            {
                NoWeaponsLimit,
                NoSafeZoneLimit,
                NoShipToolsLimit,
                NoSuperLaserLimit,
                NoDrillsLimit,
                NoStaticDrillLimit,
                NoCollectorLimit,
                new BlockLimit
                {
                    Name = "Assemblers",
                    MaxCount = 2,
                    BlockTypes = Assemblers,
                },
                new BlockLimit
                {
                    Name = "Refineries",
                    MaxCount = 2,
                    BlockTypes = Refineries
                },
                new BlockLimit
                {
                    Name = "Conveyor Junctions",
                    MaxCount = 200,
                    BlockTypes = ConveyorJunctions
                },
                new BlockLimit
                {
                    Name = "Small Hydrogen Tanks",
                    MaxCount = 20,
                    BlockTypes = SmallHydrogenTanks
                },
                new BlockLimit
                {
                    Name = "Large Hydrogen Tanks",
                    MaxCount = 10,
                    BlockTypes = LargeHydrogenTanks
                },
                new BlockLimit
                {
                    Name = "Small Cargo Containers",
                    MaxCount = 20,
                    BlockTypes = SmallCargoContainers
                },
                new BlockLimit
                {
                    Name = "Large Cargo Containers",
                    MaxCount = 10,
                    BlockTypes = LargeCargoContainers
                },
                new BlockLimit
                {
                    Name = "Gyros",
                    MaxCount = 400,
                    BlockTypes = Gyros
                }
            }
        };

        public static ModConfig DefaultModConfig = new ModConfig
        {
            DefaultGridClass = DefaultGridClassDefinition,
            GridClasses = new[]
            {
                new GridClass
                {
                    Id = 1,
                    Name = "Station",
                    LargeGridStatic = true,
                    MaxBlocks = 25000,
                    ForceBroadCast = true,
                    ForceBroadCastRange = 10000,
                    MaxPerFaction = 1,
                    MaxPerPlayer = 1,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 0,
                        GyroForce = 0,
                        AssemblerSpeed = 3,
                        RefineEfficiency = 3,
                        RefineSpeed = 3,
                        DrillHarvestMultiplier = 3,
                        PowerProducersOutput = 3
                    },
                    BlockLimits = new[]
                    {
                        SingleSafeZoneLimit,
                        SingleCollectorLimit,
                        SingleStaticDrillLimit,
                        NoSuperLaserLimit,
                        NoDrillsLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoGyroLimit,
                        new BlockLimit
                        {
                            Name = "Assemblers",
                            MaxCount = 80,
                            BlockTypes = Assemblers,
                        },
                        new BlockLimit
                        {
                            Name = "Refineries",
                            MaxCount = 80,
                            BlockTypes = Refineries
                        },
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 400,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Welders",
                            MaxCount = 40,
                            BlockTypes = Welders
                        },
                        new BlockLimit
                        {
                            Name = "Grinders",
                            MaxCount = 40,
                            BlockTypes = Grinders
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 20,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 50,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 20,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 100,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 40,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 40,
                            BlockTypes = StaticWeaponry
                        },
                        new BlockLimit
                        {
                            Name = "TIO Railgun Turrets",
                            MaxCount = 10,
                            BlockTypes = TIORailgunTurrets
                        },
                        new BlockLimit
                        {
                            Name = "TIO Battleship Turrets",
                            MaxCount = 10,
                            BlockTypes = TIOBattleshipTurrets
                        }
                    }
                },
                new GridClass
                {
                    Id = 2,
                    Name = "Outpost",
                    LargeGridStatic = true,
                    MaxBlocks = 5000,
                    ForceBroadCast = true,
                    ForceBroadCastRange = 2000,
                    MaxPerFaction = 6,
                    MaxPerPlayer = 2,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 0,
                        GyroForce = 0,
                        AssemblerSpeed = 2,
                        RefineEfficiency = 2,
                        RefineSpeed = 2,
                        DrillHarvestMultiplier = 3,
                        PowerProducersOutput = 2
                    },
                    BlockLimits = new[]
                    {
                        SingleCollectorLimit,
                        SingleStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoSuperLaserLimit,
                        NoDrillsLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoGyroLimit,
                        new BlockLimit
                        {
                            Name = "Assemblers",
                            MaxCount = 10,
                            BlockTypes = Assemblers,
                        },
                        new BlockLimit
                        {
                            Name = "Refineries",
                            MaxCount = 10,
                            BlockTypes = Refineries
                        },
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 100,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Welders",
                            MaxCount = 10,
                            BlockTypes = Welders
                        },
                        new BlockLimit
                        {
                            Name = "Grinders",
                            MaxCount = 10,
                            BlockTypes = Grinders
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 10,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 6,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 10,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 20,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 10,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 0,
                            BlockTypes = StaticWeaponry
                        },
                        new BlockLimit
                        {
                            Name = "TIO Railgun Turrets",
                            MaxCount = 4,
                            BlockTypes = TIORailgunTurrets
                        },
                        new BlockLimit
                        {
                            Name = "TIO Battleship Turrets",
                            MaxCount = 4,
                            BlockTypes = TIOBattleshipTurrets
                        }
                    }
                },
                new GridClass
                {
                    Id = 10,
                    Name = "Support",
                    LargeGridStatic = true,
                    LargeGridMobile = true,
                    MaxBlocks = 10000,
                    MaxPerFaction = 20,
                    MaxPerPlayer = 4,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 0.8f,
                        GyroForce = 0.8f,
                        AssemblerSpeed = 1.5f,
                        RefineEfficiency = 1.5f,
                        RefineSpeed = 1.5f,
                        DrillHarvestMultiplier = 2
                    },
                    BlockLimits = new[]
                    {
                        NoCollectorLimit,
                        NoStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoSuperLaserLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        new BlockLimit
                        {
                            Name = "Assemblers",
                            MaxCount = 4,
                            BlockTypes = Assemblers,
                        },
                        new BlockLimit
                        {
                            Name = "Refineries",
                            MaxCount = 4,
                            BlockTypes = Refineries
                        },
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 200,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Welders",
                            MaxCount = 20,
                            BlockTypes = Welders
                        },
                        new BlockLimit
                        {
                            Name = "Grinders",
                            MaxCount = 20,
                            BlockTypes = Grinders
                        },
                        new BlockLimit
                        {
                            Name = "Drills",
                            MaxCount = 40,
                            BlockTypes = Drills
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 10,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 40,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 10,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 40,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Gyros",
                            MaxCount = 200,
                            BlockTypes = Gyros
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 6,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 0,
                            BlockTypes = StaticWeaponry
                        }
                    }
                },
                new GridClass
                {
                    Id = 11,
                    Name = "Scout",
                    LargeGridStatic = true,
                    LargeGridMobile = true,
                    MaxBlocks = 1500,
                    MaxPerFaction = 8,
                    MaxPerPlayer = 2,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 3,
                        ThrusterEfficiency = 2.5f,
                        GyroForce = 3,
                        GyroEfficiency = 2.5f,
                        AssemblerSpeed = 0,
                        RefineEfficiency = 0,
                        RefineSpeed = 0,
                        DrillHarvestMultiplier = 0,
                    },
                    BlockLimits = new[]
                    {
                        NoCollectorLimit,
                        NoStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoSuperLaserLimit,
                        NoDrillsLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoProductionLimit,
                        NoShipToolsLimit,
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 60,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 10,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 4,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 10,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 2,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Gyros",
                            MaxCount = 50,
                            BlockTypes = Gyros
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 10,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 2,
                            BlockTypes = StaticWeaponry
                        },
                        new BlockLimit
                        {
                            Name = "TIO Railgun Turrets",
                            MaxCount = 0,
                            BlockTypes = TIORailgunTurrets
                        },
                        new BlockLimit
                        {
                            Name = "TIO Battleship Turrets",
                            MaxCount = 0,
                            BlockTypes = TIOBattleshipTurrets
                        }
                    }
                },
                new GridClass
                {
                    Id = 12,
                    Name = "Corvette",
                    LargeGridStatic = true,
                    LargeGridMobile = true,
                    MaxBlocks = 3000,
                    MaxPerFaction = 6,
                    MaxPerPlayer = 2,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 2,
                        ThrusterEfficiency = 2,
                        GyroForce = 2,
                        GyroEfficiency = 2,
                        AssemblerSpeed = 0,
                        RefineEfficiency = 0,
                        RefineSpeed = 0,
                        DrillHarvestMultiplier = 0,
                        PowerProducersOutput = 1.2f
                    },
                    BlockLimits = new[]
                    {
                        NoCollectorLimit,
                        NoStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoSuperLaserLimit,
                        NoDrillsLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoProductionLimit,
                        NoShipToolsLimit,
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 80,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 20,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 8,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 6,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 6,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Gyros",
                            MaxCount = 80,
                            BlockTypes = Gyros
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 20,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 6,
                            BlockTypes = StaticWeaponry
                        },
                        new BlockLimit
                        {
                            Name = "TIO Railgun Turrets",
                            MaxCount = 1,
                            BlockTypes = TIORailgunTurrets
                        },
                        new BlockLimit
                        {
                            Name = "TIO Battleship Turrets",
                            MaxCount = 1,
                            BlockTypes = TIOBattleshipTurrets
                        }
                    }
                },
                new GridClass
                {
                    Id = 13,
                    Name = "Destroyer",
                    LargeGridStatic = true,
                    LargeGridMobile = true,
                    MaxBlocks = 5000,
                    MaxPerFaction = 4,
                    MaxPerPlayer = 2,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 1.6f,
                        ThrusterEfficiency = 1.4f,
                        GyroForce = 1.6f,
                        GyroEfficiency = 1.4f,
                        AssemblerSpeed = 0,
                        RefineEfficiency = 0,
                        RefineSpeed = 0,
                        DrillHarvestMultiplier = 0,
                        PowerProducersOutput = 2f
                    },
                    BlockLimits = new[]
                    {
                        NoCollectorLimit,
                        NoStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoSuperLaserLimit,
                        NoDrillsLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoProductionLimit,
                        NoShipToolsLimit,
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 100,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 20,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 10,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 20,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 6,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Gyros",
                            MaxCount = 120,
                            BlockTypes = Gyros
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 20,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 32,
                            BlockTypes = StaticWeaponry
                        },
                        new BlockLimit
                        {
                            Name = "TIO Railgun Turrets",
                            MaxCount = 3,
                            BlockTypes = TIORailgunTurrets
                        },
                        new BlockLimit
                        {
                            Name = "TIO Battleship Turrets",
                            MaxCount = 3,
                            BlockTypes = TIOBattleshipTurrets
                        }
                    }
                },
                new GridClass
                {
                    Id = 14,
                    Name = "Cruiser",
                    LargeGridStatic = true,
                    LargeGridMobile = true,
                    MaxBlocks = 8000,
                    MaxPerFaction = 3,
                    MaxPerPlayer = 1,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 1.2f,
                        ThrusterEfficiency = 1f,
                        GyroForce = 1.2f,
                        GyroEfficiency = 1f,
                        AssemblerSpeed = 0,
                        RefineEfficiency = 0,
                        RefineSpeed = 0,
                        DrillHarvestMultiplier = 0,
                        PowerProducersOutput = 1.2f
                    },
                    DamageModifiers = new GridDamageModifiers
                    {
                        Bullet = 0.9f,
                        Energy = 0.9f,
                        Environment = 0.9f,
                        Explosion = 0.9f,
                        Kinetic = 0.9f,
                        Rocket = 0.9f
                    },
                    BlockLimits = new[]
                    {
                        NoCollectorLimit,
                        NoStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoSuperLaserLimit,
                        NoDrillsLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoProductionLimit,
                        NoShipToolsLimit,
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 150,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 20,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 14,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 20,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 8,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Gyros",
                            MaxCount = 180,
                            BlockTypes = Gyros
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 54,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 12,
                            BlockTypes = StaticWeaponry
                        },
                        new BlockLimit
                        {
                            Name = "TIO Railgun Turrets",
                            MaxCount = 6,
                            BlockTypes = TIORailgunTurrets
                        },
                        new BlockLimit
                        {
                            Name = "TIO Battleship Turrets",
                            MaxCount = 6,
                            BlockTypes = TIOBattleshipTurrets
                        }
                    }
                },
                new GridClass
                {
                    Id = 15,
                    Name = "Battleship",
                    LargeGridStatic = true,
                    LargeGridMobile = true,
                    MaxBlocks = 12000,
                    MaxPerFaction = 2,
                    MaxPerPlayer = 1,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 0.8f,
                        ThrusterEfficiency = 1f,
                        GyroForce = 0.8f,
                        GyroEfficiency = 1f,
                        AssemblerSpeed = 0.5f,
                        RefineEfficiency = 0.5f,
                        RefineSpeed = 0.5f,
                        DrillHarvestMultiplier = 0,
                        PowerProducersOutput = 0.8f
                    },
                    DamageModifiers = new GridDamageModifiers
                    {
                        Bullet = 0.8f,
                        Energy = 0.8f,
                        Environment = 0.8f,
                        Explosion = 0.8f,
                        Kinetic = 0.8f,
                        Rocket = 0.8f
                    },
                    BlockLimits = new[]
                    {
                        NoCollectorLimit,
                        NoStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoSuperLaserLimit,
                        NoDrillsLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoShipToolsLimit,
                        new BlockLimit
                        {
                            Name = "Assemblers",
                            MaxCount = 2,
                            BlockTypes = Assemblers,
                        },
                        new BlockLimit
                        {
                            Name = "Refineries",
                            MaxCount = 2,
                            BlockTypes = Refineries
                        },
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 200,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 20,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 20,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 20,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 10,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Gyros",
                            MaxCount = 240,
                            BlockTypes = Gyros
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 68,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 20,
                            BlockTypes = StaticWeaponry
                        },
                        new BlockLimit
                        {
                            Name = "TIO Railgun Turrets",
                            MaxCount = 8,
                            BlockTypes = TIORailgunTurrets
                        },
                        new BlockLimit
                        {
                            Name = "TIO Battleship Turrets",
                            MaxCount = 8,
                            BlockTypes = TIOBattleshipTurrets
                        }
                    }
                },
                new GridClass
                {
                    Id = 16,
                    Name = "Dreadnought",
                    LargeGridStatic = true,
                    LargeGridMobile = true,
                    MaxBlocks = 15000,
                    MaxPerFaction = 1,
                    MaxPerPlayer = 1,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 0.6f,
                        ThrusterEfficiency = 0.8f,
                        GyroForce = 0.6f,
                        GyroEfficiency = 0.8f,
                        AssemblerSpeed = 0.6f,
                        RefineEfficiency = 0.6f,
                        RefineSpeed = 0.6f,
                        DrillHarvestMultiplier = 0
                    },
                    DamageModifiers = new GridDamageModifiers
                    {
                        Bullet = 0.7f,
                        Energy = 0.7f,
                        Environment = 0.7f,
                        Explosion = 0.7f,
                        Kinetic = 0.7f,
                        Rocket = 0.7f
                    },
                    BlockLimits = new[]
                    {
                        NoCollectorLimit,
                        NoStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoDrillsLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoShipToolsLimit,
                        new BlockLimit
                        {
                            Name = "Assemblers",
                            MaxCount = 4,
                            BlockTypes = Assemblers,
                        },
                        new BlockLimit
                        {
                            Name = "Refineries",
                            MaxCount = 4,
                            BlockTypes = Refineries
                        },
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 300,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 20,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 24,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 20,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 12,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Gyros",
                            MaxCount = 400,
                            BlockTypes = Gyros
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 80,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 24,
                            BlockTypes = StaticWeaponry
                        },
                        new BlockLimit
                        {
                            Name = "TIO Railgun Turrets",
                            MaxCount = 12,
                            BlockTypes = TIORailgunTurrets
                        },
                        new BlockLimit
                        {
                            Name = "TIO Battleship Turrets",
                            MaxCount = 12,
                            BlockTypes = TIOBattleshipTurrets
                        },
                        new BlockLimit
                        {
                            Name = "Super Laser Loader",
                            MaxCount = 1,
                            BlockTypes = new [] { SuperLaserLoader }
                        },
                        new BlockLimit
                        {
                            Name = "Super Laser Core",
                            MaxCount = 4,
                            BlockTypes = new [] { SuperLaserCore }
                        },
                        new BlockLimit
                        {
                            Name = "Super Laser Muzzle",
                            MaxCount = 1,
                            BlockTypes = new [] { SuperLaserMuzzle }
                        },
                        new BlockLimit
                        {
                            Name = "Super Laser Upgrade",
                            MaxCount = 16,
                            BlockTypes = new [] { SuperLaserUpgrade }
                        }
                    }
                },
                new GridClass
                {
                    Id = 20,
                    Name = "Utility",
                    SmallGrid = true,
                    MaxBlocks = 3000,
                    MaxPerFaction = 12,
                    MaxPerPlayer = 3,
                    Modifiers = new GridModifiers
                    {
                        AssemblerSpeed = 2,
                        RefineEfficiency = 2,
                        RefineSpeed = 2,
                        DrillHarvestMultiplier = 3,
                    },
                    DamageModifiers = new GridDamageModifiers
                    {
                        Rocket = 0.8f,
                        Explosion = 0.8f,
                        Environment = 0.8f,
                        Energy = 0.8f,
                        Kinetic = 0.8f,
                        Bullet = 0.8f
                    },
                    BlockLimits = new[]
                    {
                        NoCollectorLimit,
                        NoStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoSuperLaserLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoProductionLimit,
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 60,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Welders",
                            MaxCount = 10,
                            BlockTypes = Welders
                        },
                        new BlockLimit
                        {
                            Name = "Grinders",
                            MaxCount = 10,
                            BlockTypes = Grinders
                        },
                        new BlockLimit
                        {
                            Name = "Drills",
                            MaxCount = 32,
                            BlockTypes = Drills
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 10,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 12,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 12,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Medium Cargo Containers",
                            MaxCount = 8,
                            BlockTypes = MediumCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 16,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Gyros",
                            MaxCount = 50,
                            BlockTypes = Gyros
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 6,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 0,
                            BlockTypes = StaticWeaponry
                        }
                    }
                },
                new GridClass
                {
                    Id = 21,
                    Name = "Fighter",
                    SmallGrid = true,
                    MaxBlocks = 2000,
                    MaxPerFaction = 36,
                    MaxPerPlayer = 6,
                    Modifiers = new GridModifiers
                    {
                        ThrusterForce = 2,
                        ThrusterEfficiency = 2,
                        GyroForce = 2,
                        GyroEfficiency = 2,
                        AssemblerSpeed = 0,
                        RefineEfficiency = 0,
                        RefineSpeed = 0,
                        DrillHarvestMultiplier = 0
                    },
                    BlockLimits = new[]
                    {
                        NoCollectorLimit,
                        NoStaticDrillLimit,
                        NoSafeZoneLimit,
                        NoSuperLaserLimit,
                        NoDrillsLimit,
                        NoStaticDrillLimit,
                        NoCollectorLimit,
                        NoProductionLimit,
                        NoShipToolsLimit,
                        new BlockLimit
                        {
                            Name = "Conveyor Junctions",
                            MaxCount = 50,
                            BlockTypes = ConveyorJunctions
                        },
                        new BlockLimit
                        {
                            Name = "Small Hydrogen Tanks",
                            MaxCount = 20,
                            BlockTypes = SmallHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Large Hydrogen Tanks",
                            MaxCount = 4,
                            BlockTypes = LargeHydrogenTanks
                        },
                        new BlockLimit
                        {
                            Name = "Small Cargo Containers",
                            MaxCount = 12,
                            BlockTypes = SmallCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Large Cargo Containers",
                            MaxCount = 2,
                            BlockTypes = LargeCargoContainers
                        },
                        new BlockLimit
                        {
                            Name = "Gyros",
                            MaxCount = 30,
                            BlockTypes = Gyros
                        },
                        new BlockLimit
                        {
                            Name = "Turrets",
                            MaxCount = 8,
                            BlockTypes = Turrets
                        },
                        new BlockLimit
                        {
                            Name = "Static Weaponry",
                            MaxCount = 8,
                            BlockTypes = StaticWeaponry
                        },
                        new BlockLimit
                        {
                            Name = "TIO Railgun Turrets",
                            MaxCount = 0,
                            BlockTypes = TIORailgunTurrets
                        },
                        new BlockLimit
                        {
                            Name = "TIO Battleship Turrets",
                            MaxCount = 0,
                            BlockTypes = TIOBattleshipTurrets
                        }
                    }
                },
            }
        };
    }
}