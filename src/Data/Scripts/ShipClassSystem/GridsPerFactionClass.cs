using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Sandbox.ModAPI;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public class GridsPerFactionClassManager
    {
        private readonly ModConfig _config;
        private readonly GridsPerFactionClass _perFaction = new GridsPerFactionClass();

        public GridsPerFactionClassManager(ModConfig config)
        {
            _config = config;
        }

        public bool IsGridWithinFactionLimits(CubeGridLogic gridLogic)
        {
            if (!IsApplicableGrid(gridLogic)) return true;

            var factionId = gridLogic.OwningFaction?.FactionId ?? -1;
            var gridClassId = gridLogic.GridClassId;

            if (!_config.IsValidGridClassId(gridClassId))
            {
                Utils.Log($"GridsPerFactionClass::IsGridWithinFactionLimits: Unknown grid class id {gridClassId}", 2);

                return false;
            }

            if (_perFaction.ContainsKey(factionId) && _perFaction[factionId].ContainsKey(gridClassId))
            {
                var numAllowedGrids = _config.GetGridClassById(gridClassId).MaxPerFaction;
                var idx = _perFaction[factionId][gridClassId].IndexOf(gridLogic.Entity.EntityId);

                if (idx == -1)
                    Utils.Log(
                        $"GridsPerFactionClass::IsGridWithinFactionLimits: Grid not stored within faction limits data {gridLogic.Entity.EntityId}",
                        2);

                return idx < numAllowedGrids;
            }

            Utils.Log(
                "GridsPerFactionClass::IsGridWithinFactionLimits: Faction or class not found in faction limits data",
                2);

            return true;
        }

        public void AddCubeGrid(CubeGridLogic gridLogic)
        {
            Utils.Log("GridsPerFactionClass::AddCubeGrid: start");
            if (!IsApplicableGrid(gridLogic)) return;
            var factionId = gridLogic.OwningFaction?.FactionId ?? -1;
            var gridClassId = gridLogic.GridClassId;
            Dictionary<long, List<long>> perGridClass;
            if (!_perFaction.ContainsKey(factionId))
            {
                perGridClass = GetDefaultFactionGridsSet();
                _perFaction[factionId] = perGridClass;
            }
            else
            {
                perGridClass = _perFaction[factionId];
            }

            if (!perGridClass.ContainsKey(gridClassId))
            {
                Utils.Log(
                    $"GridsPerFactionClass::AddCubeGrid: Missing list for grid class {gridClassId} in faction {factionId}",
                    2);
                perGridClass[gridClassId] = new List<long>();
            }

            if (!perGridClass[gridClassId].Contains(gridLogic.Entity.EntityId))
                perGridClass[gridClassId].Add(gridLogic.Entity.EntityId);
        }

        public Dictionary<long, List<long>> GetFactionGridsByClass(long factionId)
        {
            Dictionary<long, List<long>> value;
            return _perFaction.TryGetValue(factionId, out value) ? value : null;
        }

        public void Reset()
        {
            foreach (var gridsEntry in _perFaction.SelectMany(factionClassesEntry => factionClassesEntry.Value))
                gridsEntry.Value.Clear();
        }

        public bool IsApplicableGrid(CubeGridLogic gridLogic)
        {
            if (!_config.IncludeAiFactions && gridLogic.OwningFaction != null &&
                gridLogic.OwningFaction.IsEveryoneNpc()) return false;

            return _config.IgnoreFactionTags == null || gridLogic.OwningFaction == null ||
                   !_config.IgnoreFactionTags.Contains(gridLogic.OwningFaction.Tag);
        }

        public byte[] GetDataBytes()
        {
            return MyAPIGateway.Utilities.SerializeToBinary(_perFaction);
        }

        private Dictionary<long, List<long>> GetDefaultFactionGridsSet()
        {
            var set = new Dictionary<long, List<long>>();

            foreach (var gridClass in _config.GridClasses) set[gridClass.Id] = new List<long>();

            return set;
        }
    }

    [ProtoContract]
    public class GridsPerFactionClass : Dictionary<long, Dictionary<long, List<long>>>
    {
        public static GridsPerFactionClass FromBytes(byte[] data)
        {
            return MyAPIGateway.Utilities.SerializeFromBinary<GridsPerFactionClass>(data);
        }
    }
}