﻿using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;

namespace ShipClassSystem
{
    [ProtoContract]
    public class ModConfig
    {
        private readonly Dictionary<long, GridClass> _gridClassesById = new Dictionary<long, GridClass>();
        [ProtoMember(1)] public bool DebugMode;
        [ProtoMember(2)] public List<Zones> NoFlyZones;
        [ProtoMember(3)] public string[] IgnoreFactionTags;
        [ProtoMember(4)] public bool IncludeAiFactions;
        [ProtoMember(5)] public float MaxPossibleSpeedMetersPerSecond;
        [ProtoMember(6)] public GridClass DefaultGridClass;
        [ProtoMember(7)] public GridClass[] GridClasses;

        public GridClass GetGridClassById(long gridClassId)
        {
            GridClass id;
            if (_gridClassesById.TryGetValue(gridClassId, out id)) return id;

            Utils.Log($"Unknown grid class {gridClassId}, using default grid class");

            return DefaultGridClass;
        }

        public bool IsValidGridClassId(long gridClassId)
        {
            return _gridClassesById.ContainsKey(gridClassId);
        }

        public void UpdateGridClassesDictionary()
        {
            _gridClassesById.Clear();

            if (DefaultGridClass != null)
                _gridClassesById[0] = DefaultGridClass;
            else
                _gridClassesById[0] = DefaultGridClassConfig.DefaultGridClassDefinition;

            if (GridClasses == null) return;
            foreach (var gridClass in GridClasses)
                _gridClassesById[gridClass.Id] = gridClass;
        }

        public static ModConfig LoadConfig()
        {
            ModConfig config = null;
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(Constants.ConfigFilename, typeof(ModConfig)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(Constants.ConfigFilename, typeof(ModConfig));
                    var myText = reader.ReadToEnd();
                    reader.Close();
                    config = MyAPIGateway.Utilities.SerializeFromXML<ModConfig>(myText);
                }
            }
            catch (Exception x)
            {
                MyAPIGateway.Utilities.ShowMessage("Debug", $"ConfigLoad crashed because...\n{x.Message}");
            }
            return config;
        }

        public static void SaveConfig(ModConfig config, string filename)
        {
            try
            {
                var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(ModConfig));
                //writer.Write("// Please use this GUI based tool by Skiittz for configuration https://github.com/skiittz/Ship-Class-System-Config-Editor\n");
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(config));
                writer.Close();
            }
            catch (Exception e)
            {
                Utils.Log($"Failed to save ModConfig file {filename}, reason {e.Message}", 3);
            }
        }
    }
    [ProtoContract]
	public class Zones {
        [ProtoMember(1)]
		public int Id {get; set;}
        [ProtoMember(2)]
		public double X {get; set;}
        [ProtoMember(3)]
		public double Y {get; set;}
        [ProtoMember(4)]
		public double Z {get; set;}
        [ProtoMember(5)]
		public double Radius{get; set;}
        [ProtoMember(6)]
        public List<long> AllowedClassesById = new List<long>();

		}
    [ProtoContract]
    public class GridClass
    {
        [ProtoMember(1)]
        public int Id = 0;
        [ProtoMember(2)]
        public string Name = string.Empty;
        [ProtoMember(3)]
        public bool ForceBroadCast = false;
        [ProtoMember(4)]
        public float ForceBroadCastRange = 0;
        [ProtoMember(5)]
        public bool LargeGridStatic = false;
        [ProtoMember(6)]
        public bool LargeGridMobile = false;
        [ProtoMember(7)]
        public bool SmallGrid = false;
        [ProtoMember(8)]
        public int MaxBlocks = -1;
        [ProtoMember(9)]
        public float MaxMass = -1;
        [ProtoMember(10)]
        public int MaxPCU = -1;
        [ProtoMember(11)]
        public int MaxPerFaction = -1;
        [ProtoMember(12)]
        public int MaxPerPlayer = -1;
        [ProtoMember(13)]
        public int MinBlocks = -1;
        [ProtoMember(14)]
        public int MinPlayers = -1;
        [ProtoMember(15)]
        public GridModifiers Modifiers = new GridModifiers();
        [ProtoMember(16)]
        public GridDamageModifiers DamageModifiers = new GridDamageModifiers();
        [ProtoMember(17)]
        public BlockLimit[] BlockLimits = Array.Empty<BlockLimit>();
    }

    [ProtoContract]
    public class GridModifiers
    {
        [ProtoMember(1)]
        public float AssemblerSpeed = 1;
        [ProtoMember(2)]
        public float DrillHarvestMultiplier = 1;
        [ProtoMember(3)]
        public float GyroEfficiency = 1;
        [ProtoMember(4)]
        public float GyroForce = 1;
        [ProtoMember(5)]
        public float PowerProducersOutput = 1;
        [ProtoMember(6)]
        public float RefineEfficiency = 1;
        [ProtoMember(7)]
        public float RefineSpeed = 1;
        [ProtoMember(8)]
        public float ThrusterEfficiency = 1;
        [ProtoMember(9)]
        public float ThrusterForce = 1;
        [ProtoMember(10)]
        public float MaxSpeed = 80.0f;
        [ProtoMember(11)]
        public float MaxBoost = 1.2f;
        [ProtoMember(12)]
        public float BoostDuration = 10f; 
        [ProtoMember(13)]
        public float BoostCoolDown = 60f; 

        public override string ToString()
        {
            return
                $"<GridModifiers ThrusterForce={ThrusterForce} ThrusterEfficiency={ThrusterEfficiency} GyroForce={GyroForce} GyroEfficiency={GyroEfficiency} RefineEfficiency={RefineEfficiency} RefineSpeed={RefineSpeed} AssemblerSpeed={AssemblerSpeed} PowerProducersOutput={PowerProducersOutput} DrillHarvestMutiplier={DrillHarvestMultiplier} />";
        }

        public List<ModifierNameValue> GetModifierValues()
        {
            return new List<ModifierNameValue>
            {
                new ModifierNameValue("Thruster force", ThrusterForce),
                new ModifierNameValue("Thruster efficiency", ThrusterEfficiency),
                new ModifierNameValue("Gyro force", GyroForce),
                new ModifierNameValue("Gyro efficiency", GyroEfficiency),
                new ModifierNameValue("Refinery efficiency", RefineEfficiency),
                new ModifierNameValue("Refinery speed", RefineSpeed),
                new ModifierNameValue("Assembler speed", AssemblerSpeed),
                new ModifierNameValue("Power output", PowerProducersOutput),
                new ModifierNameValue("Drill harvest", DrillHarvestMultiplier),
                new ModifierNameValue("Max speed", MaxSpeed),
                new ModifierNameValue("Max boost", MaxBoost),
                new ModifierNameValue("Boost duration", BoostDuration),
                new ModifierNameValue("Boost cooldown", BoostCoolDown)
            };
        }
    }

    public struct ModifierNameValue
    {
        public string Name;
        public float Value;

        public ModifierNameValue(string name, float value)
        {
            Name = name;
            Value = value;
        }
    }

    [ProtoContract]
    public class BlockLimit
    {
        [ProtoMember(1)] 
        public string Name;

        [ProtoMember(2)] 
        public BlockType[] BlockTypes;

        [ProtoMember(3)] 
        public float MaxCount;

        [ProtoMember(4)] 
        public bool TurnedOffByNoFlyZone;
    }


    [ProtoContract]
    public class BlockType
    {
        [ProtoMember(1)] 
        public string TypeId;

        [ProtoMember(2)] 
        public string SubtypeId;

        [ProtoMember(3)] 
        public float CountWeight;

        public BlockType()
        {
        }

        public BlockType(string typeId, string subtypeId = "", float countWeight = 1)
        {
            TypeId = typeId;
            SubtypeId = subtypeId;
            CountWeight = countWeight;
        }
    }

    [ProtoContract]
    public class GridDamageModifiers
    {
        [ProtoMember(1)]
        public float Bullet = 1f;
        [ProtoMember(2)]
        public float Rocket = 1f;
        [ProtoMember(3)]
        public float Explosion = 1f;
        [ProtoMember(4)]
        public float Environment = 1f;
        [ProtoMember(5)]
        public float Energy = 1f;
        [ProtoMember(6)]
        public float Kinetic = 1f;
    }
}