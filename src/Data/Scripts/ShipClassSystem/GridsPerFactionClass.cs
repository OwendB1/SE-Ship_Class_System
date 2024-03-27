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
        private readonly Dictionary<long, Dictionary<long, List<long>>> _perFaction = new Dictionary<long, Dictionary<long, List<long>>>();

        public GridsPerFactionClassManager(ModConfig config)
        {
            _config = config;
        }

        public bool WillGridBeWithinFactionLimits(CubeGridLogic gridLogic, long newClassId)
        {
            if (!IsApplicableGrid(gridLogic)) return true;
            var factionId = gridLogic.OwningFaction?.FactionId ?? -1;
            if (!_config.IsValidGridClassId(newClassId))
            {
                Utils.Log($"GridsPerFactionClass::IsGridWithinFactionLimits: Unknown grid class id {newClassId}", 2);
                return false;
            }
            if (_perFaction.ContainsKey(factionId) && _perFaction[factionId].ContainsKey(newClassId))
            {
                var numAllowedGrids = _config.GetGridClassById(newClassId).MaxPerFaction;
                if (numAllowedGrids < 0) return true;
                var idx = _perFaction[factionId][newClassId].IndexOf(gridLogic.Entity.EntityId);
                if (idx == -1)
                    Utils.Log($"GridsPerFactionClass::IsGridWithinFactionLimits: Grid not stored within faction limits data {gridLogic.Entity.EntityId}", 2);
                Utils.Log($"{idx} | {numAllowedGrids}");

                return idx <= numAllowedGrids;
            }
            Utils.Log("GridsPerFactionClass::IsGridWithinFactionLimits: Faction or class not found in faction limits data", 2);
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
}