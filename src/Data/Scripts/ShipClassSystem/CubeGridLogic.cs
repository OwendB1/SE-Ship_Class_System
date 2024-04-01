using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public class CubeGridLogic
    {
        public readonly Dictionary<BlockLimit, List<IMyCubeBlock>> BlocksPerLimit = new Dictionary<BlockLimit, List<IMyCubeBlock>>();
        public readonly HashSet<IMyCubeBlock> Blocks = new HashSet<IMyCubeBlock>();

        private GridsPerFactionClassManager _gridsPerFactionClassManager;
        private GridsPerPlayerClassManager _gridsPerPlayerClassManager;

        private Dictionary<long, CubeGridLogic> CubeGridLogics => ModSessionManager.Instance.CubeGridLogics;

        private long _gridClassId;

        public IMyCubeGrid Grid;

        public IMyFaction OwningFaction => GetOwningFaction();

        public long MajorityOwningPlayerId => GetMajorityOwner();

        public long GridClassId
        {
            get { return _gridClassId; }
            set
            {
                var withinFactionLimit = _gridsPerFactionClassManager.WillGridBeWithinFactionLimits(this, value);
                var withinPlayerLimit = _gridsPerPlayerClassManager.WillGridBeWithinPlayerLimits(this, value);
                Utils.Log($"Within Faction limit: { withinFactionLimit } | Within Player limit: { withinPlayerLimit }");
                if (!withinFactionLimit)
                {
                    Utils.ShowNotification("Grid is not within allowed limit assigned to the faction!");
                    return;
                }
                if (!withinPlayerLimit)
                {
                    Utils.ShowNotification("Grid is not within allowed limit assigned to the player!");
                    return;
                }

                var gridClass = ModSessionManager.Config.GetGridClassById(value);
                var maxBlocks = gridClass.MaxBlocks;
                var maxPCU = gridClass.MaxPCU;
                var maxMass = gridClass.MaxMass;

                List<IMyCubeGrid> subgrids;
                var main = Utils.GetMainCubeGrid(Grid, out subgrids);
                var concreteGrid = main as MyCubeGrid;
                if (concreteGrid == null) return;
                var actualBlocks = concreteGrid.BlocksCount;
                var actualPCU = concreteGrid.BlocksPCU;
                var actualMass = concreteGrid.Mass;

                if (gridClass.LargeGridStatic && gridClass.LargeGridMobile == false && main.IsStatic == false)
                {
                    Utils.ShowNotification($"Can not set grid to class {gridClass.Name}, grid is supposed to be static!");
                    return;
                }

                foreach (var concreteSubgrid in subgrids.OfType<MyCubeGrid>())
                {
                    actualBlocks += concreteSubgrid.BlocksCount;
                    actualPCU += concreteSubgrid.BlocksPCU;
                    actualMass += concreteSubgrid.Mass;
                }
                
                if (maxBlocks > 1 && actualBlocks > maxBlocks) return;
                if (maxPCU > 1 && actualPCU > maxPCU) return;
                if (maxMass > 1 && actualMass > maxMass) return;

                if (!ModSessionManager.Config.IsValidGridClassId(value))
                    throw new Exception($"CubeGridLogic:: set GridClassId: invalid grid class id {value}");

                Utils.Log($"CubeGridLogic::GridClassId setting grid class to {value}", 1);
                _gridClassId = value;
                GridClassHasChanged();
            }
        }

        public GridClass GridClass => ModSessionManager.Config.GetGridClassById(GridClassId);
        public GridModifiers Modifiers => GridClass.Modifiers;
        public GridDamageModifiers DamageModifiers = new GridDamageModifiers();

        public void InitializeLogic(IMyCubeGrid grid)
        {
            Grid = grid;
            if (Grid?.Physics == null) return;
            Utils.Log($"Init Logic: {Grid.EntityId}");

            List<IMyCubeGrid> subs;
            if (Utils.GetMainCubeGrid(Grid, out subs).EntityId != Grid.EntityId) return;

            _gridsPerFactionClassManager = new GridsPerFactionClassManager(ModSessionManager.Config);
            _gridsPerPlayerClassManager = new GridsPerPlayerClassManager(ModSessionManager.Config);
            var config = ModSessionManager.Config;
            if (OwningFaction != null)
            {
                if (!config.IncludeAiFactions && OwningFaction.IsEveryoneNpc()) return;
                if (config.IgnoreFactionTags.Contains(OwningFaction.Tag)) return;
            }
            Blocks.UnionWith(Grid.GetFatBlocks<IMyCubeBlock>());
            if (Blocks == null) return;

            if (Grid.Storage == null) Grid.Storage = new MyModStorageComponent();
            string value;
            if (Grid.Storage.TryGetValue(Constants.GridClassStorageGUID, out value))
            {
                long id;
                var gridClassId = long.TryParse(value, out id) ? id : 0;
                Utils.Log($"[CubeGridLogic] Assigning GridClassId = {gridClassId}");
                _gridClassId = gridClassId;
            }
            else
            {
                _gridClassId = DefaultGridClassConfig.DefaultGridClassDefinition.Id;
                Grid.Storage[Constants.GridClassStorageGUID] = DefaultGridClassConfig.DefaultGridClassDefinition.Id.ToString();
            }

            // If subgrid then blacklist and add blocks to main grid
            if (!AddGridLogic()) return;

            //Init event handlers
            Grid.OnBlockOwnershipChanged += OnBlockOwnershipChanged;
            Grid.OnIsStaticChanged += OnIsStaticChanged;
            Grid.OnBlockAdded += OnBlockAdded;
            Grid.OnBlockRemoved += OnBlockRemoved;
            Grid.OnMarkForClose += GridMarkedForClose;
            MyAPIGateway.Session.Factions.FactionStateChanged += FactionsOnFactionStateChanged;

            List<IMyCubeGrid> subgrids;
            Utils.GetMainCubeGrid(Grid, out subgrids);
            foreach (var subgrid in subgrids)
            {
                subgrid.OnBlockOwnershipChanged += OnBlockOwnershipChanged;
                subgrid.OnIsStaticChanged += OnIsStaticChanged;
                subgrid.OnBlockAdded += OnBlockAdded;
                subgrid.OnBlockRemoved += OnBlockRemoved;

                Blocks.UnionWith(subgrid.GetFatBlocks<IMyCubeBlock>());
            }

            foreach (var blockLimit in GridClass.BlockLimits)
            {
                var limit = blockLimit;
                var relevantBlocks = Blocks.Where(block => limit.BlockTypes
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
            try
            {
                if (this == null) throw new Exception("gridLogic cannot be null");
                if (Grid == null) throw new Exception("gridLogic.Grid cannot be null");
                if (_gridsPerFactionClassManager == null)
                    throw new Exception("gridsPerFactionClassManager cannot be null");
                if (CubeGridLogics == null) throw new Exception("CubeGridLogics cannot be null");

                _gridsPerFactionClassManager.AddCubeGrid(this);
                _gridsPerPlayerClassManager.AddCubeGrid(this);
                CubeGridLogics[Grid.EntityId] = this;

                var concreteGrid = Grid as MyCubeGrid;
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

        public void GridMarkedForClose(IMyEntity ent)
        {
            if (ent.EntityId != Grid.EntityId) return;
            try
            {
                // called when entity is about to be removed for whatever reason (block destroyed, entity deleted, grid despawn because of sync range, etc)
                MyAPIGateway.Session.Factions.FactionStateChanged -= FactionsOnFactionStateChanged;
                foreach (var funcBlock in Blocks.OfType<IMyFunctionalBlock>())
                {
                    funcBlock.EnabledChanged -= FuncBlockOnEnabledChanged;
                }
                RemoveGridLogic(this);
            }
            catch (Exception e)
            {
                Utils.Log(e.ToString());
            }
        }

        private void ApplyModifiers()
        {
            DamageModifiers = GridClass.DamageModifiers;
            foreach (var block in from block in Blocks let terminalBlock = block as IMyTerminalBlock where terminalBlock != null select block)
            {
                CubeGridModifiers.ApplyModifiers(block, Modifiers);
            }
        }

        private long GetMajorityOwner()
        {
            return Grid.BigOwners.FirstOrDefault();
        }

        private IMyFaction GetOwningFaction()
        {
            switch (Grid.BigOwners.Count)
            {
                case 0:
                    return null;
                case 1:
                    return MyAPIGateway.Session.Factions.TryGetPlayerFaction(Grid.BigOwners[0]);
            }

            var ownersPerFaction = new Dictionary<IMyFaction, int>();

            //Find the faction with the most owners
            foreach (var ownerFaction in Grid.BigOwners.Select(owner => MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner)).Where(ownerFaction => ownerFaction != null))
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

        private void GridClassHasChanged()
        {
            Utils.Log($"CubeGridLogic::OnGridClassChanged: new grid class id = {GridClassId}", 2);
            ApplyModifiers();

            _gridsPerFactionClassManager.Reset();
            foreach (var gridLogic in CubeGridLogics) _gridsPerFactionClassManager.AddCubeGrid(gridLogic.Value);
            _gridsPerPlayerClassManager.Reset();
            foreach (var gridLogic in CubeGridLogics) _gridsPerPlayerClassManager.AddCubeGrid(gridLogic.Value);

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

            Grid.Storage[Constants.GridClassStorageGUID] = GridClassId.ToString();
        }

        private void OnBlockAdded(IMySlimBlock obj)
        {
            var concreteGrid = Grid as MyCubeGrid;
            if (concreteGrid?.BlocksCount > GridClass.MaxBlocks)
            {
                Grid.RemoveBlock(obj);
                return;
            }

            var relevantLimits = GetRelevantLimits(obj);
            foreach (var limit in relevantLimits)
            {
                var currentCount = BlocksPerLimit[limit].Count;
                if (currentCount + 1 > limit.MaxCount)
                {
                    Grid.RemoveBlock(obj);
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
            if (obj.FatBlock != null && HasFunctioningBeaconIfNeeded() == false)
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

            var concreteGrid = Grid as MyCubeGrid;
            if (concreteGrid?.BlocksCount < GridClass.MinBlocks)
            {
                GridClassId = 0;
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

        private void FuncBlockOnEnabledChanged(IMyTerminalBlock obj)
        {
            var func = obj as IMyFunctionalBlock;
            if (func == null) return;
            if (HasFunctioningBeaconIfNeeded() == false)
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
                 action != MyFactionStateChange.FactionMemberKick) || Grid.BigOwners[0] != playerId) return;
            if (!_gridsPerFactionClassManager.WillGridBeWithinFactionLimits(this, GridClassId)) GridClassId = 0;
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

        private void RemoveGridLogic(CubeGridLogic gridLogic)
        {
            Grid.OnBlockOwnershipChanged -= OnBlockOwnershipChanged;
            Grid.OnIsStaticChanged -= OnIsStaticChanged;
            Grid.OnBlockAdded -= OnBlockAdded;
            Grid.OnBlockRemoved -= OnBlockRemoved;

            CubeGridLogics.Remove(gridLogic.Grid.EntityId);
        }

        private bool HasFunctioningBeaconIfNeeded()
        {
            return GridClass.ForceBroadCast == false || Blocks.OfType<IMyFunctionalBlock>().Any(block => block is IMyBeacon && block.Enabled);
        }

        private IEnumerable<BlockLimit> GetRelevantLimits(IMySlimBlock block)
        {
            return GridClass.BlockLimits.Where(limit => limit.BlockTypes
                .Any(type => type.TypeId == Utils.GetBlockId(block) && type.SubtypeId == Utils.GetBlockSubtypeId(block)));
        }
    }
}