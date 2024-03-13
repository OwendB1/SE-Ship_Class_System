using ProtoBuf;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

//TODO better unknown config handling

namespace RedVsBlueClassSystem
{
    public class ModConfig
    {
        private static bool ForceRegenerateConfig = false;
        private static readonly string VariableId = nameof(ModConfig); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)

        private GridClass[] _GridClasses;
        private GridClass _DefaultGridClass = DefaultGridClassConfig.DefaultGridClassDefinition;
        private Dictionary<long, GridClass> _GridClassesById = new Dictionary<long, GridClass>();

        public bool IncludeAIFactions = false;
        public string[] IgnoreFactionTags = new string[0];

        public GridClass[] GridClasses { get { return _GridClasses; } set { _GridClasses = value; UpdateGridClassesDictionary(); } }
        public GridClass DefaultGridClass { get { return _DefaultGridClass; } set { _DefaultGridClass = value; UpdateGridClassesDictionary(); } }
        
        public GridClass GetGridClassById(long gridClassId)
        {
            if(_GridClassesById.ContainsKey(gridClassId))
            {
                return _GridClassesById[gridClassId];
            }

            Utils.Log($"Unknown grid class {gridClassId}, using default grid class");

            return DefaultGridClass;
        }

        public bool IsValidGridClassId(long gridClassId)
        {
            return _GridClassesById.ContainsKey(gridClassId);
        }

        private void UpdateGridClassesDictionary()
        {
            _GridClassesById.Clear();

            if(_DefaultGridClass != null)
            {
                _GridClassesById[0] = DefaultGridClass;
            } else
            {
                _GridClassesById[0] = DefaultGridClassConfig.DefaultGridClassDefinition;
            }
            
            if(_GridClasses != null)
            {
                foreach (var gridClass in _GridClasses)
                {
                    _GridClassesById[gridClass.Id] = gridClass;
                }
            }
        }

        public static ModConfig LoadOrGetDefaultConfig(string filename)
        {
            if(ForceRegenerateConfig)
            {
                return DefaultGridClassConfig.DefaultModConfig;
            }

            return LoadConfig(filename) ?? DefaultGridClassConfig.DefaultModConfig;
        }

        public static ModConfig LoadConfig(string filename)
        {
            //return null;//TEMP force load default config

            string fileContent = null;

            //If this is the server, initially try loading from world storage
            if (Constants.IsServer)
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(filename, typeof(ModConfig)))
                {
                    Utils.Log($"Loading config {filename} from world storage");
                    TextReader Reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(filename, typeof(ModConfig));
                    fileContent = Reader.ReadToEnd();
                    Reader.Close();

                    if (string.IsNullOrEmpty(fileContent))
                    {
                        Utils.Log($"Loadied config {filename} from world storage was empty");
                    }
                    else
                    {
                        Utils.Log($"Loaded config {filename} from world storage size = {fileContent.Length}");
                    }
                }
            }

            //If we do not have any data (either not the server, or no config file present on the server)
            //then try loading from the sandbox.sbc
            if (fileContent == null)
            {
                Utils.Log($"Loading config {filename} from sandbox data");
                if (!MyAPIGateway.Utilities.GetVariable<string>(GetVariableName(filename), out fileContent))
                {
                    return null;
                }
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
                ModConfig loadedConfig = MyAPIGateway.Utilities.SerializeFromXML<ModConfig>(fileContent);

                if (loadedConfig == null)
                {
                    Utils.Log($"Failed to load ModConfig from {filename}", 2);

                    return null;
                }

                return loadedConfig;
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
            if (Constants.IsServer)
            {
                try
                {
                    TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(ModConfig));
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(config));
                    writer.Close();
                }
                catch (Exception e)
                {
                    Utils.Log($"Failed to save ModConfig file {filename}, reason {e.Message}", 3);
                }

                Utils.SaveConfig(GetVariableName(filename), filename, config);
            }
        }

        private static string GetVariableName(string filename)
        {
            return $"{VariableId}/{filename}";
        }
    }

    public class GridClass
    {
        public int Id;
        public string Name;
        public bool SmallGridStatic = false;
        public bool SmallGridMobile = false;
        public bool LargeGridStatic = false;
        public bool LargeGridMobile = false;
        public int MaxBlocks = -1;
        public int MinBlocks = -1;
        public int MaxPCU = -1;
        public float MaxMass = -1;
        public bool ForceBroadCast = false;
        public float ForceBroadCastRange = 0;
        public int MaxPerFaction = -1;
        public int MaxPerPlayer = -1;
        public GridModifiers Modifiers = new GridModifiers();
        public BlockLimit[] BlockLimits;

        public bool IsGridEligible(IMyCubeGrid grid)
        {
            return grid.IsStatic
                ? grid.GridSizeEnum == VRage.Game.MyCubeSize.Large
                    ? LargeGridStatic
                    : SmallGridStatic
                : grid.GridSizeEnum == VRage.Game.MyCubeSize.Large
                    ? LargeGridMobile
                    : SmallGridMobile;
        }

        public DetailedGridClassCheckResult CheckGrid(IMyCubeGrid grid) {
            var concreteGrid = (grid as MyCubeGrid);

            GridCheckResult<int> MaxBlocksResult = new GridCheckResult<int>(
                MaxBlocks > 0, 
                MaxBlocks > 0 ? concreteGrid.BlocksCount <= MaxBlocks : true, 
                concreteGrid.BlocksCount, 
                MaxBlocks
            );

            GridCheckResult<int> MinBlocksResult = new GridCheckResult<int>(
                MinBlocks > 0, 
                MinBlocks > 0 ? concreteGrid.BlocksCount >= MinBlocks : true, 
                concreteGrid.BlocksCount, 
                MinBlocks
            );

            GridCheckResult<int> MaxPCUResult = new GridCheckResult<int>(
                MaxPCU > 0, 
                MaxPCU > 0 ? concreteGrid.BlocksPCU <= MaxPCU : true, 
                concreteGrid.BlocksPCU, 
                MaxPCU
            );

            GridCheckResult<float> MaxMassResult = new GridCheckResult<float>(
                MaxMass > 0, 
                MaxMass > 0 ? concreteGrid.Mass <= MaxMass : true, 
                concreteGrid.Mass, 
                MaxMass
            );

            BlockLimitCheckResult[] BlockLimitResults = null;

            if (BlockLimits != null)
            {
                //Init the result objects
                BlockLimitResults = new BlockLimitCheckResult[BlockLimits.Length];

                for (int i = 0; i < BlockLimits.Length; i++)
                {
                    BlockLimitResults[i] = new BlockLimitCheckResult() { Max = BlockLimits[i].MaxCount };
                }

                //Get all blocks to check
                IEnumerable<IMyFunctionalBlock> BlocksOnGrid = grid.GetFatBlocks<IMyFunctionalBlock>();

                //Check all blocks against the limits
                foreach (var block in BlocksOnGrid)
                {
                    for (int i = 0; i < BlockLimits.Length; i++)
                    {
                        float weightedCount;

                        if (BlockLimits[i].IsLimitedBlock(block, out weightedCount))
                        {
                            BlockLimitResults[i].Blocks++;
                            BlockLimitResults[i].Score += weightedCount;
                        }
                    }
                }

                //Check if the limits were exceeded & decide if test was passed
                for(int i = 0; i < BlockLimitResults.Length; i++)
                {
                    BlockLimitResults[i].Passed = BlockLimitResults[i].Score <= BlockLimitResults[i].Max;
                }
            }
            else
            {
                Utils.Log("No blocklimits");
            }

            return new DetailedGridClassCheckResult(
                IsGridEligible(grid),
                MaxBlocksResult,
                MinBlocksResult,
                MaxPCUResult,
                MaxMassResult,
                BlockLimitResults
            );
        }
    }

    public class GridModifiers
    {
        public float ThrusterForce = 1;
        public float ThrusterEfficiency = 1;
        public float GyroForce = 1;
        public float GyroEfficiency = 1;
        public float RefineEfficiency = 1;
        public float RefineSpeed = 1;
        public float AssemblerSpeed = 1;
        public float PowerProducersOutput = 1;
        public float DrillHarvestMutiplier = 1;

        public override string ToString()
        {
            return $"<GridModifiers ThrusterForce={ThrusterForce} ThrusterEfficiency={ThrusterEfficiency} GyroForce={GyroForce} GyroEfficiency={GyroEfficiency} RefineEfficiency={RefineEfficiency} RefineSpeed={RefineSpeed} AssemblerSpeed={AssemblerSpeed} PowerProducersOutput={PowerProducersOutput} DrillHarvestMutiplier={DrillHarvestMutiplier} />";
        }

        public IEnumerable<ModifierNameValue> GetModifierValues()
        {
            yield return new ModifierNameValue("Thruster force", ThrusterForce);
            yield return new ModifierNameValue("Thruster efficiency", ThrusterEfficiency);
            yield return new ModifierNameValue("Gyro force", GyroForce);
            yield return new ModifierNameValue("Gryo efficiency", GyroEfficiency);
            yield return new ModifierNameValue("Refinery efficiency", RefineEfficiency);
            yield return new ModifierNameValue("Refinery speed", RefineSpeed);
            yield return new ModifierNameValue("Assembler speed", AssemblerSpeed);
            yield return new ModifierNameValue("Power output", PowerProducersOutput);
            yield return new ModifierNameValue("Drill harvest", DrillHarvestMutiplier);
        }
    }

    public struct ModifierNameValue {
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
        [ProtoMember(4)]
        public float MaxCount;

        public bool IsLimitedBlock(IMyFunctionalBlock block, out float blockCountWeight)
        {
            blockCountWeight = 0;
            
            foreach (var blockType in BlockTypes)
            {
                if(blockType.IsBlockOfType(block))
                {
                    blockCountWeight = blockType.CountWeight;

                    return true;
                }
            }

            return false;
        }
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
        
        public BlockType() { }

        public BlockType(string typeId, string subtypeId = "", float countWeight = 1)
        {
            TypeId = typeId;
            SubtypeId = subtypeId;
            CountWeight = countWeight;
        } 
        public bool IsBlockOfType(IMyFunctionalBlock block)
        {
            return Utils.GetBlockId(block) == TypeId && (String.IsNullOrEmpty(SubtypeId) || Convert.ToString(block.BlockDefinition.SubtypeId) == SubtypeId);
        }
    }
}
