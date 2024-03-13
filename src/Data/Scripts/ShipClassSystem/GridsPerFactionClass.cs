using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedVsBlueClassSystem
{
    public class GridsPerFactionClassManager
    {
        private ModConfig Config;
        private GridsPerFactionClass PerFaction = new GridsPerFactionClass();

        public GridsPerFactionClassManager(ModConfig config)
        {
            Config = config;
        }

        public bool IsGridWithinFactionLimits(CubeGridLogic gridLogic)
        {
            if (!IsApplicableGrid(gridLogic))
            {
                return true;
            }

            var factionId = gridLogic.OwningFaction == null ? -1 : gridLogic.OwningFaction.FactionId;
            var gridClassId = gridLogic.GridClassId;

            if(!Config.IsValidGridClassId(gridClassId))
            {
                Utils.Log($"GridsPerFactionClass::IsGridWithinFactionLimits: Unknown grid class id {gridClassId}", 2);

                return false;
            }

            if (PerFaction.ContainsKey(factionId) && PerFaction[factionId].ContainsKey(gridClassId))
            {
                int numAllowedGrids = Config.GetGridClassById(gridClassId).MaxPerFaction;
                int idx = PerFaction[factionId][gridClassId].IndexOf(gridLogic.Entity.EntityId);

                if (idx == -1)
                {
                    Utils.Log($"GridsPerFactionClass::IsGridWithinFactionLimits: Grid not stored within faction limits data {gridLogic.Entity.EntityId}", 2);
                }

                return idx < numAllowedGrids;
            }
            else {
                Utils.Log($"GridsPerFactionClass::IsGridWithinFactionLimits: Faction or class not found in faction limits data", 2);
            }

            return true;
        }

        public void AddCubeGrid(CubeGridLogic gridLogic)
        {
            Utils.Log($"GridsPerFactionClass::AddCubeGrid: start");
            if(!IsApplicableGrid(gridLogic))
            {
                return;
            }
            Utils.Log($"1");
            var factionId = gridLogic.OwningFaction == null ? -1 : gridLogic.OwningFaction.FactionId;
            var gridClassId = gridLogic.GridClassId;
            Dictionary<long, List<long>> perGridClass;
            Utils.Log($"2");
            if (!PerFaction.ContainsKey(factionId))
            {
                perGridClass = GetDefaultFactionGridsSet();
                PerFaction[factionId] = perGridClass;
            } else
            {
                perGridClass = PerFaction[factionId];
            }
            Utils.Log($"3");
            if (!perGridClass.ContainsKey(gridClassId))
            {
                Utils.Log($"GridsPerFactionClass::AddCubeGrid: Missing list for grid class {gridClassId} in faction {factionId}", 2);
                perGridClass[gridClassId] = new List<long>();
            }
            Utils.Log($"4");
            if (!perGridClass[gridClassId].Contains(gridLogic.Entity.EntityId))
            {
                perGridClass[gridClassId].Add(gridLogic.Entity.EntityId);
            }
            Utils.Log($"5");
        }

        public Dictionary<long, List<long>> GetFactionGridsByClass(long factionId)
        {
            if (PerFaction.ContainsKey(factionId))
            {
                return PerFaction[factionId];
            }

            return null;
        }

        public void Reset()
        {
            foreach(var factionClassesEntry in PerFaction)
            {
                foreach(var gridsEntry in factionClassesEntry.Value)
                {
                    gridsEntry.Value.Clear();
                }
            }
        }

        public bool IsApplicableGrid(CubeGridLogic gridLogic) {
            if (!Config.IncludeAIFactions && gridLogic.OwningFaction != null && gridLogic.OwningFaction.IsEveryoneNpc())
            {
                return false;
            }
            
            if (Config.IgnoreFactionTags != null && gridLogic.OwningFaction != null && Config.IgnoreFactionTags.Contains(gridLogic.OwningFaction.Tag))
            {
                return false;
            }
            
            return true;
        }

        public byte[] GetDataBytes()
        {
            return MyAPIGateway.Utilities.SerializeToBinary<GridsPerFactionClass>(PerFaction);
        }

        private Dictionary<long, List<long>> GetDefaultFactionGridsSet()
        {
            var set = new Dictionary<long, List<long>>();

            foreach(var gridClass in Config.GridClasses)
            {
                set[gridClass.Id] = new List<long>();
            }

            return set;
        }
    }

    [ProtoContract]
    public class GridsPerFactionClass : Dictionary<long, Dictionary<long, List<long>>> {
        public static GridsPerFactionClass FromBytes(byte[] data)
        {
            return MyAPIGateway.Utilities.SerializeFromBinary<GridsPerFactionClass>(data);
        }
    }
}
