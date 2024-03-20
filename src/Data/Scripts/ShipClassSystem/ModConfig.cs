using ProtoBuf;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;

//TODO better unknown config handling

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public class ModConfig
    {
        private static readonly bool ForceRegenerateConfig = false;

        private static readonly string
            VariableId = nameof(ModConfig); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)

        private readonly Dictionary<long, GridClass> _gridClassesById = new Dictionary<long, GridClass>();

        private GridClass _defaultGridClass = DefaultGridClassConfig.DefaultGridClassDefinition;

        private GridClass[] _gridClasses;
        public string[] IgnoreFactionTags = Array.Empty<string>();

        public bool IncludeAiFactions = false;

        public GridClass[] GridClasses
        {
            get { return _gridClasses; }
            set
            {
                _gridClasses = value;
                UpdateGridClassesDictionary();
            }
        }

        public GridClass DefaultGridClass
        {
            get { return _defaultGridClass; }
            set
            {
                _defaultGridClass = value;
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

        public static ModConfig LoadOrGetDefaultConfig(string filename)
        {
            if (ForceRegenerateConfig) return DefaultGridClassConfig.DefaultModConfig;

            return LoadConfig(filename) ?? DefaultGridClassConfig.DefaultModConfig;
        }

        public static ModConfig LoadConfig(string filename)
        {
            //return null;//TEMP force load default config

            string fileContent = null;

            //If this is the server, initially try loading from world storage
            if (Constants.IsServer)
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(filename, typeof(ModConfig)))
                {
                    Utils.Log($"Loading config {filename} from world storage");
                    var Reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(filename, typeof(ModConfig));
                    fileContent = Reader.ReadToEnd();
                    Reader.Close();

                    Utils.Log(string.IsNullOrEmpty(fileContent)
                        ? $"Loaded config {filename} from world storage was empty"
                        : $"Loaded config {filename} from world storage size = {fileContent.Length}");
                }

            //If we do not have any data (either not the server, or no config file present on the server)
            //then try loading from the sandbox.sbc
            if (fileContent == null)
            {
                Utils.Log($"Loading config {filename} from sandbox data");
                if (!MyAPIGateway.Utilities.GetVariable(GetVariableName(filename), out fileContent)) return null;
            }

            //We didn't find any saved config, so return null
            if (fileContent == null)
            {
                Utils.Log($"No saved config found for {filename}");
                return null;
            }

            //Otherwise, attempt to parse the saved config data
            try
            {
                var loadedConfig = MyAPIGateway.Utilities.SerializeFromXML<ModConfig>(fileContent);

                if (loadedConfig != null) return loadedConfig;
                Utils.Log($"Failed to load ModConfig from {filename}", 2);

                return null;

            }
            catch (Exception e)
            {
                Utils.Log($"Failed to parse saved config file {filename}, reason = {e.Message}", 2);
                Utils.Log($"{e.StackTrace}");
            }

            return null;
        }

        public static void SaveConfig(ModConfig config, string filename)
        {
            if (!Constants.IsServer) return;
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

            Utils.SaveConfig(GetVariableName(filename), filename, config);
        }

        private static string GetVariableName(string filename)
        {
            return $"{VariableId}/{filename}";
        }
    }

    public class GridClass
    {
        public BlockLimit[] BlockLimits;
        public bool ForceBroadCast = false;
        public float ForceBroadCastRange = 0;
        public int Id;
        public bool LargeGrid = false;
        public bool SmallGrid = false;
        public bool StaticOnly = false;
        public int MaxBlocks = -1;
        public float MaxMass = -1;
        public int MaxPCU = -1;
        public int MaxPerFaction = -1;
        public int MaxPerPlayer = -1;
        public int MinBlocks = -1;
        public GridModifiers Modifiers = new GridModifiers();
        public MyGridDamageModifiers DamageModifiers = new MyGridDamageModifiers();
        public string Name;

        //if (BlockLimits != null)
        //{
        //    //Init the result objects
        //    blockLimitResults = new BlockLimitCheckResult[BlockLimits.Length];

        //    for (var i = 0; i < BlockLimits.Length; i++)
        //        blockLimitResults[i] = new BlockLimitCheckResult { Max = BlockLimits[i].MaxCount };

        //    //Get all blocks to check
        //    var blocksOnGrid = grid.GetFatBlocks<IMyFunctionalBlock>();

        //    //Check all blocks against the limits
        //    foreach (var block in blocksOnGrid)
        //        for (var i = 0; i < BlockLimits.Length; i++)
        //        {
        //            float weightedCount;

        //            if (!BlockLimits[i].IsLimitedBlock(block, out weightedCount)) continue;
        //            blockLimitResults[i].Blocks++;
        //            blockLimitResults[i].Score += weightedCount;
        //        }

        //    //Check if the limits were exceeded & decide if test was passed
        //    for (var i = 0; i < blockLimitResults.Length; i++)
        //        blockLimitResults[i].Passed = blockLimitResults[i].Score <= blockLimitResults[i].Max;
        //}
        //else
        //{
        //    Utils.Log("No blocklimits");
        //}

        
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
        [ProtoMember(2)] public BlockType[] BlockTypes;

        [ProtoMember(4)] public float MaxCount;

        [ProtoMember(1)] public string Name;

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

    public class MyGridDamageModifiers
    {
        public float Bullet = 1f;
        public float Rocket = 1f;
        public float Explosion = 1f;
        public float Environment = 1f;
        public float Energy = 1f;
        public float Kinetic = 1f;
    }
}