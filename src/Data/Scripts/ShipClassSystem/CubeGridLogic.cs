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
using VRage.Network;
using VRage.ObjectBuilders;
using VRage.Sync;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeGrid), false)]
    public class CubeGridLogic : MyGameLogicComponent, IMyEventProxy
    {
        private static readonly Dictionary<long, CubeGridLogic> CubeGridLogics = new Dictionary<long, CubeGridLogic>();
        private static readonly List<CubeGridLogic> AllCubeGridLogics = new List<CubeGridLogic>();
        private static List<IMyFunctionalBlock> _functionalBlocks = new List<IMyFunctionalBlock>();

        private static readonly GridsPerFactionClassManager GridsPerFactionClassManager =
            new GridsPerFactionClassManager(ModSessionManager.Instance.Config);

        private static readonly GridsPerPlayerClassManager GridsPerPlayerClassManager =
            new GridsPerPlayerClassManager(ModSessionManager.Instance.Config);

        private readonly MySync<long, SyncDirection.FromServer> _gridClassSync = null;

        private IMyCubeGrid _grid;

        public IMyFaction OwningFaction => GetOwningFaction();

        public long MajorityOwningPlayerId => GetMajorityOwner();

        public long GridClassId
        {
            get { return _gridClassSync.Value; }
            set
            {
                if (!Constants.IsServer)
                    throw new Exception("CubeGridLogic:: set GridClassId: Grid class Id can only be set on the server");

                if (!ModSessionManager.IsValidGridClass(value))
                    throw new Exception($"CubeGridLogic:: set GridClassId: invalid grid class id {value}");

                Utils.Log($"CubeGridLogic::GridClassId setting grid class to {value}", 1);

                _gridClassSync.Value = value;
            }
        }

        public GridClass GridClass => ModSessionManager.GetGridClassById(GridClassId);
        public GridModifiers Modifiers => GridClass.Modifiers;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // the base methods are usually empty, except for OnAddedToContainer()'s, which has some sync stuff making it required to be called.
            base.Init(objectBuilder);

            _grid = (IMyCubeGrid)Entity;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            _functionalBlocks = _grid.GetFatBlocks<IMyFunctionalBlock>().ToList();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            //Utils.Log("[CubeGridLogic] FirstUpdate");

            if (_grid?.Physics == null) // ignore projected and other non-physical grids
                return;

            AddGridLogic(this);

            if (Entity.Storage == null) Entity.Storage = new MyModStorageComponent();

            //Init event handlers
            _gridClassSync.ValueChanged += OnGridClassChanged;
            _grid.OnBlockOwnershipChanged += OnBlockOwnershipChanged;
            _grid.OnIsStaticChanged += Grid_OnIsStaticChanged;
            _grid.OnBlockAdded += OnBlockAdded;
            _grid.OnBlockRemoved += OnBlockRemoved;
            _grid.OnGridMerge += OnGridMerge;

            //If server, init persistent storage + apply grid class
            if (Constants.IsServer)
            {
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
            }

            ApplyModifiers();
        }

        public override void MarkForClose()
        {
            base.MarkForClose();

            RemoveGridLogic(this);

            // called when entity is about to be removed for whatever reason (block destroyed, entity deleted, grid despawn because of sync range, etc)
        }

        private void ApplyModifiers()
        {
            Utils.Log($"Applying modifiers {Modifiers}");

            foreach (var block in _grid.GetFatBlocks<IMyTerminalBlock>())
                CubeGridModifiers.ApplyModifiers(block, Modifiers);
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
        private void Grid_OnIsStaticChanged(IMyCubeGrid grid, bool isStatic)
        {
            if (GridClass.StaticOnly && !isStatic) grid.IsStatic = true;
            if (!GridClass.StaticOnly && isStatic) grid.IsStatic = false;
        }

        private void OnGridClassChanged(MySync<long, SyncDirection.FromServer> newGridClassId)
        {
            var withinFactionLimit = GridsPerFactionClassManager.IsGridWithinFactionLimits(this);
            var withinPlayerLimit = GridsPerPlayerClassManager.IsGridWithinPlayerLimits(this);
            Utils.Log($"CubeGridLogic::OnGridClassChanged: new grid class id = {newGridClassId}", 2);
            if (!withinFactionLimit || !withinPlayerLimit)
                GridClassId = 0;

            ApplyModifiers();
            UpdateGridsPerFactionClass();
            _functionalBlocks = _grid.GetFatBlocks<IMyFunctionalBlock>().ToList();
            foreach (var blockLimit in GridClass.BlockLimits)
            {
                var relevantBlocks = _functionalBlocks.Where(block => blockLimit.BlockTypes
                    .Any(t => t.SubtypeId == block.BlockDefinition.SubtypeId &&
                              t.TypeId == block.BlockDefinition.TypeIdString)).ToList();

                for (var i = (int)blockLimit.MaxCount + 1; i < relevantBlocks.Count; i++)
                {
                    relevantBlocks[i].Enabled = false;
                }
            }
        }

        private void OnBlockAdded(IMySlimBlock obj)
        {
            _functionalBlocks.Add(obj.FatBlock as IMyFunctionalBlock);
            // TODO: remove block in question if over block limit or general limit
            var funcBlock = obj.FatBlock as IMyFunctionalBlock;
            if (funcBlock != null)
                funcBlock.EnabledChanged += _ => FuncBlockOnEnabledChanged(funcBlock);

            if (obj.FatBlock != null)
                //Utils.WriteToClient($"Added block TypeId = {Utils.GetBlockId(fatBlock)}, Subtype = {Utils.GetBlockSubtypeId(fatBlock)}");
                CubeGridModifiers.ApplyModifiers(obj.FatBlock, Modifiers);
        }

        private void OnBlockRemoved(IMySlimBlock obj)
        {
            _functionalBlocks.Remove(obj.FatBlock as IMyFunctionalBlock);
            if (obj.FatBlock != null)
                //Utils.WriteToClient($"Added block TypeId = {Utils.GetBlockId(fatBlock)}, Subtype = {Utils.GetBlockSubtypeId(fatBlock)}");
                CubeGridModifiers.ApplyModifiers(obj.FatBlock, Modifiers);
        }

        private void OnBlockOwnershipChanged(IMyCubeGrid obj)
        {
            // TODO: Do damage check
            _functionalBlocks = _grid.GetFatBlocks<IMyFunctionalBlock>().ToList();
            foreach (var blockLimit in GridClass.BlockLimits)
            {
                var relevantBlocks = _functionalBlocks.Where(block => blockLimit.BlockTypes
                    .Any(t => t.SubtypeId == block.BlockDefinition.SubtypeId &&
                              t.TypeId == block.BlockDefinition.TypeIdString)).ToList();
                
                for (var i = (int)blockLimit.MaxCount + 1; i < relevantBlocks.Count; i++)
                {
                    relevantBlocks[i].Enabled = false;
                }
            }
        }

        private void OnGridMerge(IMyCubeGrid grid1, IMyCubeGrid grid2)
        {
            var gridLogicToBeRemoved = CubeGridLogics.FirstOrDefault(l => l.Key == grid2.EntityId);
            CubeGridLogics.Remove(gridLogicToBeRemoved.Key);
            AllCubeGridLogics.RemoveAll(item => item == gridLogicToBeRemoved.Value);
        }

        private void FuncBlockOnEnabledChanged(IMyFunctionalBlock func)
        {
            if(func.Enabled == false) return;
            var subTypeId = func.BlockDefinition.SubtypeId;
            var typeId = func.BlockDefinition.TypeIdString;

            foreach (var blockLimit in GridClass.BlockLimits)
            {
                // Check if type of func can be found in block limit
                if (!blockLimit.BlockTypes.Any(types => types.TypeId == typeId && types.SubtypeId == subTypeId)) return;
                // Get all relevant blocks that match the types from the block limit
                var relevantBlocks = _functionalBlocks.Where(block => blockLimit.BlockTypes
                    .Any(t => t.SubtypeId == block.BlockDefinition.SubtypeId && 
                              t.TypeId == block.BlockDefinition.TypeIdString)).ToList();

                foreach (var blockType in blockLimit.BlockTypes)
                {
                    if (!blockType.IsBlockOfType(func)) continue;
                    if (relevantBlocks.IndexOf(func) > blockLimit.MaxCount) func.Enabled = false;
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
            foreach (var gridLogic in AllCubeGridLogics) GridsPerFactionClassManager.AddCubeGrid(gridLogic);
        }

        private static void AddGridLogic(CubeGridLogic gridLogic)
        {
            try
            {
                if (gridLogic == null) throw new Exception("gridLogic cannot be null");

                if (gridLogic._grid == null) throw new Exception("gridLogic.Grid cannot be null");

                if (AllCubeGridLogics == null) throw new Exception("AllCubeGridLogics cannot be null");

                if (GridsPerFactionClassManager == null)
                    throw new Exception("gridsPerFactionClassManager cannot be null");

                if (CubeGridLogics == null) throw new Exception("CubeGridLogics cannot be null");

                if (!AllCubeGridLogics.Contains(gridLogic)) AllCubeGridLogics.Add(gridLogic);

                // TODO simplify per player and faction checks
                GridsPerFactionClassManager.AddCubeGrid(gridLogic);
                GridsPerPlayerClassManager.AddCubeGrid(gridLogic);
                CubeGridLogics[gridLogic._grid.EntityId] = gridLogic;
            }
            catch (Exception e)
            {
                Utils.Log("CubeGridLogic::AddGridLogic: caught error", 3);
                Utils.LogException(e);
            }
        }

        private static void RemoveGridLogic(CubeGridLogic gridLogic)
        {
            CubeGridLogics.Remove(gridLogic._grid.EntityId);
            AllCubeGridLogics.RemoveAll(item => item == gridLogic);
        }

        public override bool IsSerialized()
        {
            // executed when the entity gets serialized (saved, blueprinted, streamed, etc) and asks all
            //   its components whether to be serialized too or not (calling GetObjectBuilder())
            if (_grid?.Physics == null) return base.IsSerialized();
            if (!Constants.IsServer) return base.IsSerialized();
            try
            {
                Entity.Storage[Constants.GridClassStorageGUID] = GridClassId.ToString();
            }
            catch (Exception e)
            {
                Utils.Log($"Error serialising CubeGridLogic, {e.Message}");
            }

            // you cannot add custom OBs to the game so this should always return the base (which currently is always false).
            return base.IsSerialized();
        }
    }
}