using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Network;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Sync;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeGrid), false)]
    public class CubeGridLogic : MyGameLogicComponent
    {
        private static readonly Dictionary<long, CubeGridLogic> CubeGridLogics = new Dictionary<long, CubeGridLogic>();

        public readonly Dictionary<BlockLimit, List<IMyCubeBlock>> BlocksPerLimit = new Dictionary<BlockLimit, List<IMyCubeBlock>>();
        public HashSet<IMyCubeBlock> Blocks;

        private static readonly GridsPerFactionClassManager GridsPerFactionClassManager =
            new GridsPerFactionClassManager(ModSessionManager.Instance.Config);

        private static readonly GridsPerPlayerClassManager GridsPerPlayerClassManager =
            new GridsPerPlayerClassManager(ModSessionManager.Instance.Config);

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private MySync<long, SyncDirection.FromServer> _gridClassSync;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private MySync<bool, SyncDirection.FromServer> _gridTypeSync;

        private IMyCubeGrid _grid;

        private bool _initializedFully;

        public bool IsMainGrid
        {
            get { return _gridTypeSync.Value; }
            set
            {
                _gridTypeSync.Value = value;
            }
        }

        public IMyFaction OwningFaction => GetOwningFaction();

        public long MajorityOwningPlayerId => GetMajorityOwner();

        public long GridClassId
        {
            get { return _gridClassSync.Value; }
            set
            {
                var withinFactionLimit = GridsPerFactionClassManager.WillGridBeWithinFactionLimits(this, value);
                var withinPlayerLimit = GridsPerPlayerClassManager.WillGridBeWithinPlayerLimits(this, value);
                Utils.Log($"Within Faction limit: { withinFactionLimit } | Within Player limit: { withinPlayerLimit }");
                if (!withinFactionLimit || !withinPlayerLimit) return;

                var concreteGrid = _grid as MyCubeGrid;
                var maxBlocks = ModSessionManager.GetGridClassById(value).MaxBlocks;
                var maxPCU = ModSessionManager.GetGridClassById(value).MaxPCU;
                var maxMass = ModSessionManager.GetGridClassById(value).MaxMass;
                if (concreteGrid == null) return;
                if (maxBlocks > 1 && concreteGrid.BlocksCount > maxBlocks) return;
                if (maxPCU > 1 && concreteGrid.BlocksPCU > maxPCU) return;
                if (maxMass > 1 && concreteGrid.Mass > maxMass) return;

                if (!ModSessionManager.IsValidGridClass(value))
                    throw new Exception($"CubeGridLogic:: set GridClassId: invalid grid class id {value}");

                Utils.Log($"CubeGridLogic::GridClassId setting grid class to {value}", 1);

                _gridClassSync.Value = value;
            }
        }

        public GridClass GridClass => ModSessionManager.GetGridClassById(GridClassId);
        public GridModifiers Modifiers => GridClass.Modifiers;
        public GridDamageModifiers DamageModifiers = new GridDamageModifiers();

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            _grid = (IMyCubeGrid)Entity;
            if (ModSessionManager.GetIgnoredFactionTags().Any(item => item == OwningFaction.Tag))
                return;
            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            MyAPIGateway.Session.Factions.FactionStateChanged += FactionsOnFactionStateChanged;
        }

        private void FactionsOnFactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            if ((action != MyFactionStateChange.FactionMemberAcceptJoin &&
                 action != MyFactionStateChange.FactionMemberLeave &&
                 action != MyFactionStateChange.FactionMemberKick) || _grid.BigOwners[0] != playerId) return;
            if (!GridsPerFactionClassManager.WillGridBeWithinFactionLimits(this, GridClassId)) _gridClassSync.Value = 0;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            // ignore projected and other non-physical grids
            if (_grid?.Physics == null) return;
            Blocks = _grid.GetFatBlocks<IMyCubeBlock>().ToHashSet();
            if (Blocks == null) return;

            // If subgrid then blacklist and add blocks to main grid
            if (!AddGridLogic(this)) return;
            
            if (Entity.Storage == null) Entity.Storage = new MyModStorageComponent();

            //Init event handlers
            _gridClassSync.ValueChanged += OnGridClassChanged;
            _gridTypeSync.ValueChanged += OnMainGridChanged;

            _grid.OnBlockOwnershipChanged += OnBlockOwnershipChanged;
            _grid.OnIsStaticChanged += OnIsStaticChanged;
            _grid.OnBlockAdded += OnBlockAdded;
            _grid.OnBlockRemoved += OnBlockRemoved;
            _grid.OnGridMerge += OnGridMerge;

            MyAPIGateway.Session.OnSessionReady += RunAfterInitChecks;

            //Load persisted grid class id from storage (if server)
            if (Entity.Storage.ContainsKey(Constants.GridClassStorageGUID))
            {
                long gridClassId = 0;

                try
                {
                    gridClassId = long.Parse(Entity.Storage[Constants.GridClassStorageGUID]);
                }
                catch (Exception e)
                {
                    var msg =
                        $"[CubeGridLogic] Error parsing serialized GridClassId: {Entity.Storage[Constants.GridClassStorageGUID]}, EntityId = {_grid.EntityId}";

                    Utils.Log(msg, 1);
                    Utils.Log(e.Message, 1);
                }

                Utils.Log($"[CubeGridLogic] Assigning GridClassId = {gridClassId}");
                _gridClassSync.Value = gridClassId;
            }
            else
            {
                _gridClassSync.Value = DefaultGridClassConfig.DefaultGridClassDefinition.Id;
                Entity.Storage[Constants.GridClassStorageGUID] = DefaultGridClassConfig.DefaultGridClassDefinition.Id.ToString();
            }
        }

        private bool AddGridLogic(CubeGridLogic gridLogic)
        {
            Utils.Log("Try Add GridLogic for: " + gridLogic.Entity.EntityId);
            try
            {
                if (gridLogic == null) throw new Exception("gridLogic cannot be null");

                if (gridLogic._grid == null) throw new Exception("gridLogic.Grid cannot be null");

                if (GridsPerFactionClassManager == null)
                    throw new Exception("gridsPerFactionClassManager cannot be null");

                if (CubeGridLogics == null) throw new Exception("CubeGridLogics cannot be null");

                GridsPerFactionClassManager.AddCubeGrid(gridLogic);
                GridsPerPlayerClassManager.AddCubeGrid(gridLogic);
                CubeGridLogics[gridLogic._grid.EntityId] = gridLogic;

                var concreteGrid = gridLogic._grid as MyCubeGrid;
                if (concreteGrid == null) return false;

                foreach (var funcBlock in gridLogic.Blocks.OfType<IMyFunctionalBlock>())
                {
                    funcBlock.EnabledChanged += _ => gridLogic.FuncBlockOnEnabledChanged(funcBlock);
                }

                var id = GetMainCubeGridId();
                _gridTypeSync.Value = id == _grid.EntityId;

                return true;
            }
            catch (Exception e)
            {
                Utils.Log("CubeGridLogic::AddGridLogic: caught error", 3);
                Utils.LogException(e);
                return false;
            }
        }

        private void RunAfterInitChecks()
        {
            var mainLogic = GetCubeGridLogicByEntityId(GetMainCubeGridId());
            if (IsMainGrid)
            {
                var logics = GetLinkedCubeGridLogics();
                foreach (var logic in logics)
                {
                    Blocks.UnionWith(logic.Blocks);
                }
            }

            if (IsMainGrid)
            {
                foreach (var blockLimit in GridClass.BlockLimits)
                {
                    var relevantBlocks = Blocks.Where(block => blockLimit.BlockTypes
                        .Any(t => t.SubtypeId == Utils.GetBlockSubtypeId(block) &&
                                  t.TypeId == Utils.GetBlockId(block))).ToList();
                    BlocksPerLimit[blockLimit] = relevantBlocks;
                }
            }
            else
            {
                foreach (var blockLimit in mainLogic.GridClass.BlockLimits)
                {
                    var relevantBlocks = mainLogic.Blocks.Where(block => blockLimit.BlockTypes
                        .Any(t => t.SubtypeId == Utils.GetBlockSubtypeId(block) &&
                                  t.TypeId == Utils.GetBlockId(block))).ToList();
                    mainLogic.BlocksPerLimit[blockLimit] = relevantBlocks;
                }
            }

            foreach (var block in Blocks)
            {
                var funcBlock = block as IMyFunctionalBlock;
                if (funcBlock != null)
                {
                    EnforceFunctionalBlockPunishment(funcBlock);
                }
                else
                {
                    EnforceNonFunctionalBlockPunishment(block);
                }
            }
            ApplyModifiers();
            _initializedFully = true;
        }

        public override void MarkForClose()
        {
            // called when entity is about to be removed for whatever reason (block destroyed, entity deleted, grid despawn because of sync range, etc)
            base.MarkForClose();

            RemoveGridLogic(this);

            MyAPIGateway.Session.Factions.FactionStateChanged -= FactionsOnFactionStateChanged;
        }

        private void ApplyModifiers()
        {
            if (IsMainGrid)
            {
                DamageModifiers = GridClass.DamageModifiers;
                foreach (var block in _grid.GetFatBlocks<IMyTerminalBlock>())
                {
                    CubeGridModifiers.ApplyModifiers(block, Modifiers);
                }
            }
            else
            {
                var mainLogic = GetCubeGridLogicByEntityId(GetMainCubeGridId());
                DamageModifiers = mainLogic.GridClass.DamageModifiers;
                foreach (var block in _grid.GetFatBlocks<IMyTerminalBlock>())
                {
                    CubeGridModifiers.ApplyModifiers(block, mainLogic.Modifiers);
                }
            }
        }

        private long GetMajorityOwner()
        {
            return _grid.BigOwners.FirstOrDefault();
        }

        private IMyFaction GetOwningFaction()
        {
            switch (_grid.BigOwners.Count)
            {
                case 0:
                    return null;
                case 1:
                    return MyAPIGateway.Session.Factions.TryGetPlayerFaction(_grid.BigOwners[0]);
            }

            var ownersPerFaction = new Dictionary<IMyFaction, int>();

            //Find the faction with the most owners
            foreach (var ownerFaction in _grid.BigOwners.Select(owner => MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner)).Where(ownerFaction => ownerFaction != null))
            {
                if (!ownersPerFaction.ContainsKey(ownerFaction))
                    ownersPerFaction[ownerFaction] = 1;
                else
                    ownersPerFaction[ownerFaction]++;
            }

            return ownersPerFaction.Count == 0 ? null :
                //new select the faction with the most owners
                ownersPerFaction.MaxBy(kvp => kvp.Value).Key;
        }

        //Event handlers
        private void OnIsStaticChanged(IMyCubeGrid grid, bool isStatic)
        {
            if (GridClass.LargeGridStatic && !GridClass.LargeGridMobile && !isStatic) grid.IsStatic = true;
            if (!GridClass.LargeGridStatic && isStatic) grid.IsStatic = false;
        }

        private void OnMainGridChanged(MySync<bool, SyncDirection.FromServer> newMainGridSetting)
        {
            Utils.Log($"CubeGridLogic::OnMainGridChanged: main grid set to {newMainGridSetting}", 2);
            if (!newMainGridSetting) return;
            if (!_initializedFully) return;

            Blocks = _grid.GetFatBlocks<IMyCubeBlock>().ToHashSet();
            var logics = GetLinkedCubeGridLogics();
            foreach (var gridLogic in logics)
            {
                Blocks.UnionWith(gridLogic.Blocks);
                gridLogic._gridTypeSync.Value = false;
            }

            var mainLogic = GetCubeGridLogicByEntityId(GetMainCubeGridId());
            if (IsMainGrid)
            {
                foreach (var blockLimit in GridClass.BlockLimits)
                {
                    var relevantBlocks = Blocks.Where(block => blockLimit.BlockTypes
                        .Any(t => t.SubtypeId == Utils.GetBlockSubtypeId(block) &&
                                  t.TypeId == Utils.GetBlockId(block))).ToList();
                    BlocksPerLimit[blockLimit] = relevantBlocks;
                }
            }
            else
            {
                foreach (var blockLimit in mainLogic.GridClass.BlockLimits)
                {
                    var relevantBlocks = mainLogic.Blocks.Where(block => blockLimit.BlockTypes
                        .Any(t => t.SubtypeId == Utils.GetBlockSubtypeId(block) &&
                                  t.TypeId == Utils.GetBlockId(block))).ToList();
                    mainLogic.BlocksPerLimit[blockLimit] = relevantBlocks;
                }
            }
        }

        private void OnGridClassChanged(MySync<long, SyncDirection.FromServer> newGridClassId)
        {
            Utils.Log($"CubeGridLogic::OnGridClassChanged: new grid class id = {newGridClassId}", 2);
            if (!_initializedFully) return;

            ApplyModifiers();
            UpdateGridsPerFactionClass();
            UpdateGridsPerPlayerClass();

            var mainLogic = GetCubeGridLogicByEntityId(GetMainCubeGridId());

            if (IsMainGrid)
            {
                foreach (var blockLimit in GridClass.BlockLimits)
                {
                    var relevantBlocks = Blocks.Where(block => blockLimit.BlockTypes
                        .Any(t => t.SubtypeId == Utils.GetBlockSubtypeId(block) &&
                                  t.TypeId == Utils.GetBlockId(block))).ToList();
                    BlocksPerLimit[blockLimit] = relevantBlocks;
                }
            }
            else
            {
                foreach (var blockLimit in mainLogic.GridClass.BlockLimits)
                {
                    var relevantBlocks = mainLogic.Blocks.Where(block => blockLimit.BlockTypes
                        .Any(t => t.SubtypeId == Utils.GetBlockSubtypeId(block) &&
                                  t.TypeId == Utils.GetBlockId(block))).ToList();
                    mainLogic.BlocksPerLimit[blockLimit] = relevantBlocks;
                }
            }

            foreach (var funcBlock in Blocks.OfType<IMyFunctionalBlock>())
            {
                EnforceFunctionalBlockPunishment(funcBlock);
            }

            Entity.Storage[Constants.GridClassStorageGUID] = newGridClassId.Value.ToString();
        }

        private void OnBlockAdded(IMySlimBlock obj)
        {
            if (IsMainGrid)
            {
                var concreteGrid = _grid as MyCubeGrid;
                if (concreteGrid?.BlocksCount > GridClass.MaxBlocks)
                {
                    _grid.RemoveBlock(obj);
                    return;
                }

                var relevantLimits = GetRelevantLimits(obj);
                foreach (var limit in relevantLimits)
                {
                    var currentCount = BlocksPerLimit[limit].Count;
                    if (currentCount + 1 > limit.MaxCount)
                    {
                        _grid.RemoveBlock(obj);
                        return;
                    }
                    BlocksPerLimit[limit].Add(obj.FatBlock);
                }
            }
            else
            {
                var concreteGrid = _grid as MyCubeGrid;
                var mainLogic = GetCubeGridLogicByEntityId(GetMainCubeGridId());
                var mainConcreteGrid = mainLogic._grid as MyCubeGrid;
                if (concreteGrid?.BlocksCount + mainConcreteGrid?.BlocksCount > GridClass.MaxBlocks)
                {
                    _grid.RemoveBlock(obj);
                    return;
                }

                var relevantLimits = GetRelevantLimits(obj, mainLogic);
                foreach (var limit in relevantLimits)
                {
                    var currentCount = mainLogic.BlocksPerLimit[limit].Count;
                    if (currentCount + 1 > limit.MaxCount)
                    {
                        _grid.RemoveBlock(obj);
                        return;
                    }
                    mainLogic.BlocksPerLimit[limit].Add(obj.FatBlock);
                }
                mainLogic.Blocks.Add(obj.FatBlock);
            }
            
            Blocks.Add(obj.FatBlock);

            var funcBlock = obj.FatBlock as IMyFunctionalBlock;
            if (funcBlock != null)
                funcBlock.EnabledChanged += _ => FuncBlockOnEnabledChanged(funcBlock);

            if (obj.FatBlock != null)
                CubeGridModifiers.ApplyModifiers(obj.FatBlock, Modifiers);
        }

        private void OnBlockRemoved(IMySlimBlock obj)
        {
            if (IsMainGrid)
            {
                if (obj.FatBlock != null && HasFunctioningBeaconIfNeeded())
                {
                    DamageModifiers = DefaultGridClassConfig.DefaultGridDamageModifiers2X;
                    foreach (var block in _grid.GetFatBlocks<IMyTerminalBlock>())
                    {
                        CubeGridModifiers.ApplyModifiers(block, DefaultGridClassConfig.DefaultGridModifiers);
                    }
                }

                var relevantLimits = GetRelevantLimits(obj);
                foreach (var limit in relevantLimits)
                {
                    BlocksPerLimit[limit].Remove(obj.FatBlock);
                }

                var concreteGrid = _grid as MyCubeGrid;
                if (concreteGrid?.BlocksCount < GridClass.MinBlocks)
                {
                    _gridClassSync.Value = 0;
                }
            }
            else
            {
                try
                {
                    var mainLogic = GetCubeGridLogicByEntityId(GetMainCubeGridId());
                    mainLogic.Blocks.Remove(obj.FatBlock as IMyFunctionalBlock);

                    var relevantLimits = GetRelevantLimits(obj, mainLogic);
                    foreach (var limit in relevantLimits)
                    {
                        mainLogic.BlocksPerLimit[limit].Remove(obj.FatBlock);
                    }
                }
                catch
                {
                    Utils.Log("Could not remove block. Block is probably not duplicated.");
                }
            }
            Blocks.Remove(obj.FatBlock as IMyFunctionalBlock);
        }

        private void OnBlockOwnershipChanged(IMyCubeGrid obj)
        {
            if (IsMainGrid)
            {
                foreach (var blockLimit in GridClass.BlockLimits)
                {
                    var functionalBlocks = Blocks.Where(block => blockLimit.BlockTypes
                        .Any(t => t.SubtypeId == block.BlockDefinition.SubtypeId &&
                                  t.TypeId == Utils.GetBlockId(block))).ToList();
                    for (var i = (int)blockLimit.MaxCount + 1; i < functionalBlocks.Count; i++)
                    {
                        var funcBlock = functionalBlocks[i] as IMyFunctionalBlock;
                        if (funcBlock != null) funcBlock.Enabled = false;
                        else
                        {
                            var slim = functionalBlocks[i].SlimBlock;
                            EnforceNonFunctionalBlockPunishment(slim.FatBlock);
                        }
                    }
                }
            }
            else
            {
                var mainLogic = GetCubeGridLogicByEntityId(GetMainCubeGridId());
                foreach (var blockLimit in mainLogic.GridClass.BlockLimits)
                {
                    var functionalBlocks = mainLogic.Blocks.Where(block => blockLimit.BlockTypes
                        .Any(t => t.SubtypeId == block.BlockDefinition.SubtypeId &&
                                  t.TypeId == Utils.GetBlockId(block))).ToList();
                    for (var i = (int)blockLimit.MaxCount + 1; i < functionalBlocks.Count; i++)
                    {
                        var funcBlock = functionalBlocks[i] as IMyFunctionalBlock;
                        if (funcBlock != null) funcBlock.Enabled = false;
                        else
                        {
                            var slim = functionalBlocks[i].SlimBlock;
                            EnforceNonFunctionalBlockPunishment(slim.FatBlock);
                        }
                    }
                }
            }
        }

        private static void OnGridMerge(IMyCubeGrid grid1, IMyCubeGrid grid2)
        {
            var gridLogicToBeRemoved = CubeGridLogics.FirstOrDefault(l => l.Key == grid2.EntityId);
            CubeGridLogics.Remove(gridLogicToBeRemoved.Key);
        }

        private void FuncBlockOnEnabledChanged(IMyFunctionalBlock func)
        {
            if (IsMainGrid && func is IMyBeacon && func.Enabled == false && HasFunctioningBeaconIfNeeded())
            {
                DamageModifiers = DefaultGridClassConfig.DefaultGridDamageModifiers2X;
                foreach (var block in _grid.GetFatBlocks<IMyTerminalBlock>())
                    CubeGridModifiers.ApplyModifiers(block, DefaultGridClassConfig.DefaultGridModifiers);
            }
            if (func.Enabled)
                EnforceFunctionalBlockPunishment(func);
        }

        private void EnforceFunctionalBlockPunishment(IMyFunctionalBlock block)
        {
            if (IsMainGrid)
            {
                var relevantLimits = GetRelevantLimits(block.SlimBlock);
                foreach (var limit in relevantLimits)
                {
                    var currentBlocks = BlocksPerLimit[limit];
                    if (currentBlocks.IndexOf(block) >= limit.MaxCount)
                    {
                        block.Enabled = false;
                    }
                }
            }
            else
            {
                var mainLogic = GetCubeGridLogicByEntityId(GetMainCubeGridId());
                var relevantLimits = GetRelevantLimits(block.SlimBlock, mainLogic);
                foreach (var limit in relevantLimits)
                {
                    var currentBlocks = mainLogic.BlocksPerLimit[limit];
                    if (currentBlocks.IndexOf(block) >= limit.MaxCount)
                    {
                        block.Enabled = false;
                    }
                }
            }
        }

        private void EnforceNonFunctionalBlockPunishment(IMyCubeBlock block)
        {
            if (IsMainGrid)
            {
                var relevantLimits = GetRelevantLimits(block.SlimBlock);
                foreach (var limit in relevantLimits)
                {
                    var currentBlocks = BlocksPerLimit[limit];
                    if (!(currentBlocks.IndexOf(block) >= limit.MaxCount)) continue;
                    var slim = block.SlimBlock;
                    var targetIntegrity = slim.MaxIntegrity * 0.2;
                    var damageRequired = slim.Integrity - targetIntegrity;

                    if (damageRequired < 0)
                    {
                        damageRequired = 0;
                    }
                    slim.DoDamage((float)damageRequired, MyDamageType.Bullet, true);
                }
            }
            else
            {
                var mainLogic = GetCubeGridLogicByEntityId(GetMainCubeGridId());
                var relevantLimits = GetRelevantLimits(block.SlimBlock, mainLogic);

                foreach (var limit in relevantLimits)
                {
                    var currentBlocks = mainLogic.BlocksPerLimit[limit];
                    if (!(currentBlocks.IndexOf(block) >= limit.MaxCount)) continue;
                    var slim = block.SlimBlock;
                    var targetIntegrity = slim.MaxIntegrity * 0.2;
                    var damageRequired = slim.Integrity - targetIntegrity;

                    if (damageRequired < 0)
                    {
                        damageRequired = 0;
                    }
                    slim.DoDamage((float)damageRequired, MyDamageType.Bullet, true);
                }
            }
        }

        public static CubeGridLogic GetCubeGridLogicByEntityId(long entityId)
        {
            CubeGridLogic id;
            return CubeGridLogics.TryGetValue(entityId, out id) ? id : null;
        }

        public static void UpdateGridsPerFactionClass()
        {
            GridsPerFactionClassManager.Reset();
            foreach (var gridLogic in CubeGridLogics) GridsPerFactionClassManager.AddCubeGrid(gridLogic.Value);
        }

        public static void UpdateGridsPerPlayerClass()
        {
            GridsPerPlayerClassManager.Reset();
            foreach (var gridLogic in CubeGridLogics) GridsPerPlayerClassManager.AddCubeGrid(gridLogic.Value);
        }

        private static void RemoveGridLogic(CubeGridLogic gridLogic)
        {
            CubeGridLogics.Remove(gridLogic._grid.EntityId);
        }

        private bool HasFunctioningBeaconIfNeeded()
        {
            return GridClass.ForceBroadCast == false || Blocks.OfType<IMyFunctionalBlock>().Any(block => block is IMyBeacon && block.Enabled);
        }

        private List<CubeGridLogic> GetLinkedCubeGridLogics(IMyCubeGrid cubeGrid = null)
        {
            var group = cubeGrid != null ? cubeGrid.GetGridGroup(GridLinkTypeEnum.Mechanical) 
                : _grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
            var grids = new List<IMyCubeGrid>();
            group.GetGrids(grids);
            return grids.Select(grid => GetCubeGridLogicByEntityId(grid.EntityId))
                .Where(grid => grid.IsMainGrid == false && grid._grid.EntityId != _grid.EntityId).ToList();
        }

        private long GetMainCubeGridId(IMyCubeGrid cubeGrid = null)
        {
            var group = cubeGrid != null ? cubeGrid.GetGridGroup(GridLinkTypeEnum.Mechanical) 
                : _grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
            var grids = new List<IMyCubeGrid>();

            group?.GetGrids(grids);
            var potentialMainGrids = new Dictionary<long, int>();

            var concreteGrid = _grid as MyCubeGrid;
            if (concreteGrid == null) return 0;
            var biggest = concreteGrid.GetBiggestGridInGroup();

            foreach (var g in grids)
            {
                var beaconPresent = g.GetFatBlocks<IMyCubeBlock>().Any(b => b is IMyBeacon);
                if (beaconPresent && biggest.EntityId == g.EntityId) return g.EntityId;
                potentialMainGrids.Add(g.EntityId, concreteGrid.BlocksCount);
            }

            var chosenMainGrid = potentialMainGrids.MaxBy(e => e.Value);
            return chosenMainGrid.Key;
        }

        private IEnumerable<BlockLimit> GetRelevantLimits(IMySlimBlock block, CubeGridLogic mainLogic = null)
        {
            if (mainLogic == null)
            {
                return GridClass.BlockLimits.Where(limit => limit.BlockTypes
                    .Any(type => type.TypeId == Utils.GetBlockId(block) && type.SubtypeId == Utils.GetBlockSubtypeId(block)));
            }

            return mainLogic.GridClass.BlockLimits.Where(limit => limit.BlockTypes
                .Any(type => type.TypeId == Utils.GetBlockId(block) && type.SubtypeId == Utils.GetBlockSubtypeId(block)));
        }
    }
}