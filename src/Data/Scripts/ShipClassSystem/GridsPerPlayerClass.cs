using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Sandbox.ModAPI;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public class GridsPerPlayerClassManager
    {
        private readonly ModConfig _config;
        private readonly GridsPerPlayerClass _perPlayer = new GridsPerPlayerClass();

        public GridsPerPlayerClassManager(ModConfig config)
        {
            _config = config;
        }

        public bool IsGridWithinPlayerLimits(CubeGridLogic gridLogic)
        {
            if (!IsApplicableGrid(gridLogic)) return true;

            var playerId = gridLogic.MajorityOwningPlayerId;
            var gridClassId = gridLogic.GridClassId;

            if (!_config.IsValidGridClassId(gridClassId))
            {
                Utils.Log($"GridsPerUserClass::IsGridWithinPlayerLimits: Unknown grid class id {gridClassId}", 2);
                return false;
            }

            if (_perPlayer.ContainsKey(playerId) && _perPlayer[playerId].ContainsKey(gridClassId))
            {
                var numAllowedGrids = _config.GetGridClassById(gridClassId).MaxPerPlayer;
                var idx = _perPlayer[playerId][gridClassId].IndexOf(gridLogic.Entity.EntityId);

                if (idx == -1)
                    Utils.Log(
                        $"GridsPerUserClass::IsGridWithinPlayerLimits: Grid not stored within faction limits data {gridLogic.Entity.EntityId}",
                        2);

                return idx < numAllowedGrids;
            }

            Utils.Log(
                "GridsPerUserClass::IsGridWithinPlayerLimits: Faction or class not found in faction limits data",
                2);

            return true;
        }

        public void AddCubeGrid(CubeGridLogic gridLogic)
        {
            Utils.Log("GridsPerUserClass::AddCubeGrid: start");
            if (!IsApplicableGrid(gridLogic)) return;
            var playerId = gridLogic.MajorityOwningPlayerId;
            var gridClassId = gridLogic.GridClassId;
            Dictionary<long, List<long>> perGridClass;
            if (!_perPlayer.ContainsKey(playerId))
            {
                perGridClass = GetDefaultPLayerGridsSet();
                _perPlayer[playerId] = perGridClass;
            }
            else
            {
                perGridClass = _perPlayer[playerId];
            }

            if (!perGridClass.ContainsKey(gridClassId))
            {
                Utils.Log(
                    $"GridsPerFactionClass::AddCubeGrid: Missing list for grid class {gridClassId} for player {playerId}",
                    2);
                perGridClass[gridClassId] = new List<long>();
            }

            if (!perGridClass[gridClassId].Contains(gridLogic.Entity.EntityId))
                perGridClass[gridClassId].Add(gridLogic.Entity.EntityId);
        }

        public void Reset()
        {
            foreach (var gridsEntry in _perPlayer.SelectMany(factionClassesEntry => factionClassesEntry.Value))
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
            return MyAPIGateway.Utilities.SerializeToBinary(_perPlayer);
        }

        private Dictionary<long, List<long>> GetDefaultPLayerGridsSet()
        {
            var set = new Dictionary<long, List<long>>();

            foreach (var gridClass in _config.GridClasses) set[gridClass.Id] = new List<long>();

            return set;
        }
    }

    [ProtoContract]
    public class GridsPerPlayerClass : Dictionary<long, Dictionary<long, List<long>>>
    {
        public static GridsPerPlayerClass FromBytes(byte[] data)
        {
            return MyAPIGateway.Utilities.SerializeFromBinary<GridsPerPlayerClass>(data);
        }
    }
}