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
using VRage.Network;
using VRage.Sync;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeGrid), false)]
    public class CubeGridLogic : MyGameLogicComponent, IMyEventProxy
    {
        private static readonly Dictionary<long, CubeGridLogic> CubeGridLogics = new Dictionary<long, CubeGridLogic>();
        public static Queue<CubeGridLogic> ToBeInitialized = new Queue<CubeGridLogic>();

        public readonly Dictionary<BlockLimit, List<IMyCubeBlock>> BlocksPerLimit = new Dictionary<BlockLimit, List<IMyCubeBlock>>();
        public HashSet<IMyCubeBlock> Blocks;

        private static readonly GridsPerFactionClassManager GridsPerFactionClassManager =
            new GridsPerFactionClassManager(ModSessionManager.Instance.Config);

        private static readonly GridsPerPlayerClassManager GridsPerPlayerClassManager =
            new GridsPerPlayerClassManager(ModSessionManager.Instance.Config);

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private MySync<long, SyncDirection.FromServer> _gridClassSync;

        private IMyCubeGrid _grid;

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

                var maxBlocks = ModSessionManager.GetGridClassById(value).MaxBlocks;
                var maxPCU = ModSessionManager.GetGridClassById(value).MaxPCU;
                var maxMass = ModSessionManager.GetGridClassById(value).MaxMass;

                List<IMyCubeGrid> subgrids;
                var main = GetMainCubeGrid(out subgrids);
                var concreteGrid = main as MyCubeGrid;
                if (concreteGrid == null) return;
                var actualBlocks = concreteGrid.BlocksCount;
                var actualPCU = concreteGrid.BlocksPCU;
                var actualMass = concreteGrid.Mass;

                foreach (var concreteSubgrid in subgrids.OfType<MyCubeGrid>())
                {
                    actualBlocks += concreteSubgrid.BlocksCount;
                    actualPCU += concreteSubgrid.BlocksPCU;
                    actualMass += concreteSubgrid.Mass;
                }
                
                if (maxBlocks > 1 && actualBlocks > maxBlocks) return;
                if (maxPCU > 1 && actualPCU > maxPCU) return;
                if (maxMass > 1 && actualMass > maxMass) return;

                if (!ModSessionManager.IsValidGridClass(value))
                    throw new Exception($"CubeGridLogic:: set GridClassId: invalid grid class id {value}");

                Utils.Log($"CubeGridLogic::GridClassId setting grid class to {value}", 1);

                _gridClassSync.Value = value;
            }
        }

        public GridClass GridClass => ModSessionManager.GetGridClassById(GridClassId);
        public GridModifiers Modifiers => GridClass.Modifiers;
        public GridDamageModifiers DamageModifiers = new GridDamageModifiers();

        public override void OnAddedToScene()
        {
            _grid = Entity as IMyCubeGrid;
            if (_grid == null) return;

            if (ModSessionManager.GetIgnoredFactionTags().Any(item => item == OwningFaction.Tag))
                return;

            ToBeInitialized.Enqueue(this);


            

            //if (MyAPIGateway.Session.GameplayFrameCounter > 0)
            //    InitializeLogic();
            //else MyAPIGateway.Session.OnSessionReady += InitializeLogic;
        }

        public void InitializeLogic()
        {
            var config = ModSessionManager.Instance.Config;
            if (OwningFaction != null)
            {
                if (!config.IncludeAiFactions && OwningFaction.IsEveryoneNpc()) return;
                if (config.IgnoreFactionTags.Contains(OwningFaction.Tag)) return;
            }
            
            List<IMyCubeGrid> subs;
            if (CubeGridLogics.ContainsKey(Entity.EntityId)) return;
            if (GetMainCubeGrid(out subs).EntityId != _grid.EntityId) return;
            // ignore projected and other non-physical grids
            if (_grid?.Physics == null) return;

            Blocks = _grid.GetFatBlocks<IMyCubeBlock>().ToHashSet();
            if (Blocks == null) return;

            if (Entity.Storage == null) Entity.Storage = new MyModStorageComponent();
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
                    var msg = $"[CubeGridLogic] Error parsing serialized GridClassId: {Entity.Storage[Constants.GridClassStorageGUID]}, EntityId = {_grid.EntityId}";
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

            // If subgrid then blacklist and add blocks to main grid
            if (!AddGridLogic()) return;

            //Init event handlers
            _gridClassSync.ValueChanged += OnGridClassChanged;
            _grid.OnBlockOwnershipChanged += OnBlockOwnershipChanged;
            _grid.OnIsStaticChanged += OnIsStaticChanged;
            _grid.OnBlockAdded += OnBlockAdded;
            _grid.OnBlockRemoved += OnBlockRemoved;
            _grid.OnGridMerge += OnGridMerge;
            MyAPIGateway.Session.Factions.FactionStateChanged += FactionsOnFactionStateChanged;

            List<IMyCubeGrid> subgrids;
            GetMainCubeGrid(out subgrids);
            for (var i = 0; i < subgrids.Count; i++)
            {
                var subgrid = subgrids[i];
                subgrid.OnBlockOwnershipChanged += OnBlockOwnershipChanged;
                subgrid.OnIsStaticChanged += OnIsStaticChanged;
                subgrid.OnBlockAdded += OnBlockAdded;
                subgrid.OnBlockRemoved += OnBlockRemoved;
                subgrid.OnGridMerge += OnGridMerge;

                Blocks.UnionWith(subgrid.GetFatBlocks<IMyCubeBlock>());
            }

            for (var i = 0; i < GridClass.BlockLimits.Length; i++)
            {
                var blockLimit = GridClass.BlockLimits[i];
                var relevantBlocks = Blocks.Where(block => blockLimit.BlockTypes
                    .Any(t => t.SubtypeId == Utils.GetBlockSubtypeId(block) &&
                              t.TypeId == Utils.GetBlockId(block))).ToList();
                BlocksPerLimit[blockLimit] = relevantBlocks;
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
        }

        private bool AddGridLogic()
        {
            Utils.Log("Try Add GridLogic for: " + Entity.EntityId);
            try
            {
                if (this == null) throw new Exception("gridLogic cannot be null");
                if (_grid == null) throw new Exception("gridLogic.Grid cannot be null");
                if (GridsPerFactionClassManager == null)
                    throw new Exception("gridsPerFactionClassManager cannot be null");
                if (CubeGridLogics == null) throw new Exception("CubeGridLogics cannot be null");

                GridsPerFactionClassManager.AddCubeGrid(this);
                GridsPerPlayerClassManager.AddCubeGrid(this);
                CubeGridLogics[Entity.EntityId] = this;

                var concreteGrid = _grid as MyCubeGrid;
                if (concreteGrid == null) return false;

                foreach (var funcBlock in Blocks.OfType<IMyFunctionalBlock>())
                {
                    funcBlock.EnabledChanged += FuncBlockOnEnabledChanged;
                }
                return true;
            }
            catch (Exception e)
            {
                Utils.Log("CubeGridLogic::AddGridLogic: caught error", 3);
                Utils.LogException(e);
                return false;
            }
        }

        public override void OnRemovedFromScene()
        {
            try
            {
                // called when entity is about to be removed for whatever reason (block destroyed, entity deleted, grid despawn because of sync range, etc)
                RemoveGridLogic(this);
                MyAPIGateway.Session.Factions.FactionStateChanged -= FactionsOnFactionStateChanged;
                foreach (var funcBlock in Blocks.OfType<IMyFunctionalBlock>())
                {
                    funcBlock.EnabledChanged -= FuncBlockOnEnabledChanged;
                }
            }
            catch (Exception e)
            {
                Utils.Log(e.ToString());
            }
        }

        private void ApplyModifiers()
        {
            DamageModifiers = GridClass.DamageModifiers;
            foreach (var block in Blocks)
            {
                var terminalBlock = block as IMyTerminalBlock;
                if (terminalBlock == null) continue;
                CubeGridModifiers.ApplyModifiers(block, Modifiers);
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

        private void OnGridClassChanged(MySync<long, SyncDirection.FromServer> newGridClassId)
        {
            Utils.Log($"CubeGridLogic::OnGridClassChanged: new grid class id = {newGridClassId}", 2);

            ApplyModifiers();

            GridsPerFactionClassManager.Reset();
            foreach (var gridLogic in CubeGridLogics) GridsPerFactionClassManager.AddCubeGrid(gridLogic.Value);
            GridsPerPlayerClassManager.Reset();
            foreach (var gridLogic in CubeGridLogics) GridsPerPlayerClassManager.AddCubeGrid(gridLogic.Value);

            foreach (var blockLimit in GridClass.BlockLimits)
            {
                var relevantBlocks = Blocks.Where(block => blockLimit.BlockTypes
                    .Any(t => t.SubtypeId == Utils.GetBlockSubtypeId(block) &&
                              t.TypeId == Utils.GetBlockId(block))).ToList();
                BlocksPerLimit[blockLimit] = relevantBlocks;
            }
            
            foreach (var funcBlock in Blocks.OfType<IMyFunctionalBlock>())
            {
                EnforceFunctionalBlockPunishment(funcBlock);
            }

            Entity.Storage[Constants.GridClassStorageGUID] = newGridClassId.Value.ToString();
        }

        private void OnBlockAdded(IMySlimBlock obj)
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

            Blocks.Add(obj.FatBlock);

            var funcBlock = obj.FatBlock as IMyFunctionalBlock;
            if (funcBlock != null)
                funcBlock.EnabledChanged += FuncBlockOnEnabledChanged;

            if (obj.FatBlock != null)
                CubeGridModifiers.ApplyModifiers(obj.FatBlock, Modifiers);
        }

        private void OnBlockRemoved(IMySlimBlock obj)
        {
            if (obj.FatBlock != null && HasFunctioningBeaconIfNeeded())
            {
                DamageModifiers = DefaultGridClassConfig.DefaultGridDamageModifiers2X;
                foreach (var block in Blocks)
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
            Blocks.Remove(obj.FatBlock as IMyFunctionalBlock);
        }

        private void OnBlockOwnershipChanged(IMyCubeGrid obj)
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

        private static void OnGridMerge(IMyCubeGrid grid1, IMyCubeGrid grid2)
        {
            var gridLogicToBeRemoved = CubeGridLogics.FirstOrDefault(l => l.Key == grid2.EntityId);
            CubeGridLogics.Remove(gridLogicToBeRemoved.Key);
        }

        private void FuncBlockOnEnabledChanged(IMyTerminalBlock obj)
        {
            var func = obj as IMyFunctionalBlock;
            if (func == null) return;
            if (func is IMyBeacon && func.Enabled == false && HasFunctioningBeaconIfNeeded())
            {
                DamageModifiers = DefaultGridClassConfig.DefaultGridDamageModifiers2X;
                foreach (var block in Blocks)
                    CubeGridModifiers.ApplyModifiers(block, DefaultGridClassConfig.DefaultGridModifiers);
            }
            if (func.Enabled)
                EnforceFunctionalBlockPunishment(func);
        }

        private void FactionsOnFactionStateChanged(MyFactionStateChange action, long fromFactionId, long toFactionId, long playerId, long senderId)
        {
            if ((action != MyFactionStateChange.FactionMemberAcceptJoin &&
                 action != MyFactionStateChange.FactionMemberLeave &&
                 action != MyFactionStateChange.FactionMemberKick) || _grid.BigOwners[0] != playerId) return;
            if (!GridsPerFactionClassManager.WillGridBeWithinFactionLimits(this, GridClassId)) _gridClassSync.Value = 0;
        }

        private void EnforceFunctionalBlockPunishment(IMyFunctionalBlock block)
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

        private void EnforceNonFunctionalBlockPunishment(IMyCubeBlock block)
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

        public static CubeGridLogic GetCubeGridLogicByEntityId(long entityId)
        {
            try
            {
                return CubeGridLogics[entityId];
            }
            catch
            {
                Utils.Log($"Could not find entity with ID: {entityId}");
                return null;
            }
        }

        private void RemoveGridLogic(CubeGridLogic gridLogic)
        {
            _gridClassSync.ValueChanged -= OnGridClassChanged;
            _grid.OnBlockOwnershipChanged -= OnBlockOwnershipChanged;
            _grid.OnIsStaticChanged -= OnIsStaticChanged;
            _grid.OnBlockAdded -= OnBlockAdded;
            _grid.OnBlockRemoved -= OnBlockRemoved;
            _grid.OnGridMerge -= OnGridMerge;

            CubeGridLogics.Remove(gridLogic._grid.EntityId);
        }

        private bool HasFunctioningBeaconIfNeeded()
        {
            return GridClass.ForceBroadCast == false || Blocks.OfType<IMyFunctionalBlock>().Any(block => block is IMyBeacon && block.Enabled);
        }

        private IMyCubeGrid GetMainCubeGrid(out List<IMyCubeGrid> subgrids)
        {
            var group = _grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
            var grids = new List<IMyCubeGrid>();

            group?.GetGrids(grids);

            var concreteGrid = _grid as MyCubeGrid;
            if (concreteGrid == null)
            {
                Utils.Log("CONCRETE GRID IS NULL");
                subgrids = new List<IMyCubeGrid>();
                return null;
            };

            var biggestGrid = concreteGrid;
            foreach (var concrete in grids.OfType<MyCubeGrid>().Where(concrete => concrete.BlocksCount > biggestGrid.BlocksCount))
            {
                biggestGrid = concrete;
            }

            subgrids = grids.Where(grid => grid.EntityId != biggestGrid.EntityId).ToList();
            return biggestGrid;
        }

        private IEnumerable<BlockLimit> GetRelevantLimits(IMySlimBlock block)
        {
            return GridClass.BlockLimits.Where(limit => limit.BlockTypes
                .Any(type => type.TypeId == Utils.GetBlockId(block) && type.SubtypeId == Utils.GetBlockSubtypeId(block)));
        }
    }
}