using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;

//TODO better unknown config handling

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public class ModConfig
    {
        private readonly Dictionary<long, GridClass> _gridClassesById = new Dictionary<long, GridClass>();

        private GridClass _defaultGridClass = DefaultGridClassConfig.DefaultGridClassDefinition;

        private GridClass[] _gridClasses;
        public string[] IgnoreFactionTags = Array.Empty<string>();

        public bool IncludeAiFactions = false;

        public GridClass DefaultGridClass
        {
            get { return _defaultGridClass; }
            set
            {
                _defaultGridClass = value;
                UpdateGridClassesDictionary();
            }
        }

        public GridClass[] GridClasses
        {
            get { return _gridClasses; }
            set
            {
                _gridClasses = value;
                UpdateGridClassesDictionary();
            }
        }

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

        private void UpdateGridClassesDictionary()
        {
            _gridClassesById.Clear();

            if (_defaultGridClass != null)
                _gridClassesById[0] = DefaultGridClass;
            else
                _gridClassesById[0] = DefaultGridClassConfig.DefaultGridClassDefinition;

            if (_gridClasses == null) return;
            foreach (var gridClass in _gridClasses)
                _gridClassesById[gridClass.Id] = gridClass;
        }

        public static ModConfig LoadConfig()
        {
            var config = DefaultGridClassConfig.DefaultModConfig;
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(Constants.ConfigFilename, typeof(ModConfig)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(Constants.ConfigFilename, typeof(ModConfig));
                    var myText = reader.ReadToEnd();
                    reader.Close();
                    config = MyAPIGateway.Utilities.SerializeFromXML<ModConfig>(myText);
                    if (config == null) { throw new Exception("Word Settings Empty! :(.... \n ...Fixed!"); }
                }

            }
            catch (Exception x)
            {
                config = DefaultGridClassConfig.DefaultModConfig;
                MyAPIGateway.Utilities.ShowMessage("Debug", $"Blue's sketchy ConfigLoad crashed because...\n{x.Message}");
            }

            return config;
        }

        public static void SaveConfig(ModConfig config, string filename)
        {
            try
            {
                var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(ModConfig));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(config));
                writer.Close();
            }
            catch (Exception e)
            {
                Utils.Log($"Failed to save ModConfig file {filename}, reason {e.Message}", 3);
            }
        }
    }

    public class GridClass
    {
        public int Id;
        public string Name;
        public bool ForceBroadCast = false;
        public float ForceBroadCastRange = 0;
        public bool LargeGridMobile = false;
        public bool SmallGrid = false;
        public bool LargeGridStatic = false;
        public int MaxBlocks = -1;
        public float MaxMass = -1;
        public int MaxPCU = -1;
        public int MaxPerFaction = -1;
        public int MaxPerPlayer = -1;
        public int MinBlocks = -1;
        public GridModifiers Modifiers = new GridModifiers();
        public GridDamageModifiers DamageModifiers = new GridDamageModifiers();
        public BlockLimit[] BlockLimits;
    }

    public class GridModifiers
    {
        public float AssemblerSpeed = 1;
        public float DrillHarvestMultiplier = 1;
        public float GyroEfficiency = 1;
        public float GyroForce = 1;
        public float PowerProducersOutput = 1;
        public float RefineEfficiency = 1;
        public float RefineSpeed = 1;
        public float ThrusterEfficiency = 1;
        public float ThrusterForce = 1;

        public override string ToString()
        {
            return
                $"<GridModifiers ThrusterForce={ThrusterForce} ThrusterEfficiency={ThrusterEfficiency} GyroForce={GyroForce} GyroEfficiency={GyroEfficiency} RefineEfficiency={RefineEfficiency} RefineSpeed={RefineSpeed} AssemblerSpeed={AssemblerSpeed} PowerProducersOutput={PowerProducersOutput} DrillHarvestMutiplier={DrillHarvestMultiplier} />";
        }

        public IEnumerable<ModifierNameValue> GetModifierValues()
        {
            yield return new ModifierNameValue("Thruster force", ThrusterForce);
            yield return new ModifierNameValue("Thruster efficiency", ThrusterEfficiency);
            yield return new ModifierNameValue("Gyro force", GyroForce);
            yield return new ModifierNameValue("Gyro efficiency", GyroEfficiency);
            yield return new ModifierNameValue("Refinery efficiency", RefineEfficiency);
            yield return new ModifierNameValue("Refinery speed", RefineSpeed);
            yield return new ModifierNameValue("Assembler speed", AssemblerSpeed);
            yield return new ModifierNameValue("Power output", PowerProducersOutput);
            yield return new ModifierNameValue("Drill harvest", DrillHarvestMultiplier);
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
        [ProtoMember(1)] public string Name;

        [ProtoMember(2)] public BlockType[] BlockTypes;

        [ProtoMember(4)] public float MaxCount;

        public bool IsLimitedBlock(IMyFunctionalBlock block, out float blockCountWeight)
        {
            blockCountWeight = 0;

            foreach (var blockType in BlockTypes)
                if (blockType.IsBlockOfType(block))
                {
                    blockCountWeight = blockType.CountWeight;

                    return true;
                }

            return false;
        }
    }


    [ProtoContract]
    public class BlockType
    {
        [ProtoMember(3)] public float CountWeight;

        [ProtoMember(2)] public string SubtypeId;

        [ProtoMember(1)] public string TypeId;

        public BlockType()
        {
        }

        public BlockType(string typeId, string subtypeId = "", float countWeight = 1)
        {
            TypeId = typeId;
            SubtypeId = subtypeId;
            CountWeight = countWeight;
        }

        public bool IsBlockOfType(IMyFunctionalBlock block)
        {
            return Utils.GetBlockId(block) == TypeId && (string.IsNullOrEmpty(SubtypeId) ||
                                                         Convert.ToString(block.BlockDefinition.SubtypeId) ==
                                                         SubtypeId);
        }
    }

    public class GridDamageModifiers
    {
        public float Bullet = 1f;
        public float Rocket = 1f;
        public float Explosion = 1f;
        public float Environment = 1f;
        public float Energy = 1f;
        public float Kinetic = 1f;
    }
}