using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
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
        private static readonly Queue<CubeGridLogic> ToBeCheckedOnServerQueue = new Queue<CubeGridLogic>();

        private static readonly GridsPerFactionClassManager GridsPerFactionClassManager =
            new GridsPerFactionClassManager(ModSessionManager.Instance.Config);

        private static readonly GridsPerPlayerClassManager GridsPerPlayerClassManager =
            new GridsPerPlayerClassManager(ModSessionManager.Instance.Config);

        private readonly MySync<GridCheckResults, SyncDirection.FromServer> _gridCheckResultsSync = null;

        private readonly MySync<long, SyncDirection.FromServer> _gridClassSync = null;

        private DetailedGridClassCheckResult _detailedGridClassCheckResult;

        private bool _isClientGridClassCheckDirty = true;

        private bool _isServerGridClassDirty;

        private IMyFaction _owningFaction;

        private IMyCubeGrid _grid;

        private bool _isGridOwnerDirty = true;

        public bool IsServerGridClassDirty
        {
            get { return _isServerGridClassDirty; }

            protected set
            {
                if (value == _isServerGridClassDirty) return;
                if (value) ToBeCheckedOnServerQueue.Enqueue(this);

                _isServerGridClassDirty = value;
            }
        }

        public IMyFaction OwningFaction
        {
            get
            {
                if (!_isGridOwnerDirty) return _owningFaction;
                _owningFaction = GetOwningFaction();
                _isGridOwnerDirty = false;

                return _owningFaction;
            }
        }

        public DetailedGridClassCheckResult DetailedGridClassCheckResult
        {
            get
            {
                if (!_isClientGridClassCheckDirty) return _detailedGridClassCheckResult;
                _detailedGridClassCheckResult = GridClass?.CheckGrid(_grid);
                _isClientGridClassCheckDirty = false;

                return _detailedGridClassCheckResult;
            }
        }

        public long GridClassId
        {
            get { return _gridClassSync.Value; }
            set
            {
                if (!Constants.IsServer)
                    throw new Exception("CubeGridLgic:: set GridClassId: Grid class Id can only be set on the server");

                if (!ModSessionManager.IsValidGridClass(value))
                    throw new Exception($"CubeGridLgic:: set GridClassId: invalid grid class id {value}");

                Utils.Log($"CubeGridLogic::GridClassId setting grid class to {value}", 1);

                _gridClassSync.Value = value;
            }
        }

        public GridCheckResults GridCheckResults => _gridCheckResultsSync.Value;
        public GridClass GridClass => ModSessionManager.GetGridClassById(GridClassId);
        public bool GridMeetsGridClassRestrictions => GridCheckResults.CheckPassedForGridClass(GridClass);

        public GridModifiers Modifiers => GridMeetsGridClassRestrictions
            ? GridClass.Modifiers
            : ModSessionManager.GetGridClassById(0).Modifiers;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // the base methods are usually empty, except for OnAddedToContainer()'s, which has some sync stuff making it required to be called.
            base.Init(objectBuilder);

            _grid = (IMyCubeGrid)Entity;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
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
            _gridCheckResultsSync.ValueChanged += GridCheckResultsSync_ValueChanged;

            _grid.OnBlockOwnershipChanged += OnBlockOwnershipChanged;
            _grid.OnIsStaticChanged += Grid_OnIsStaticChanged;

            if (Constants.IsClient)
            {
                _grid.OnBlockAdded += ClientOnBlockChanged;
                _grid.OnBlockRemoved += ClientOnBlockChanged;
            }

            //If server, init persistant storage + apply grid class
            if (Constants.IsServer)
            {
                IsServerGridClassDirty = true;

                _grid.OnBlockAdded += ServerOnBlockAdded;
                _grid.OnBlockRemoved += ServerOnBlockRemoved;


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
                            $"[CubeGridLogic] Error parsing serialised GridClassId: {Entity.Storage[Constants.GridClassStorageGUID]}, EntityId = {_grid.EntityId}";

                        Utils.Log(msg, 1);
                        Utils.Log(e.Message, 1);
                    }

                    //TODO validate gridClassId
                    Utils.Log($"[CubeGridLogic] Assigning GridClassId = {gridClassId}");
                    _gridClassSync.Value = gridClassId;
                }
            }

            ApplyModifiers();

            // NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            // NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            // NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }


        public override void MarkForClose()
        {
            base.MarkForClose();

            RemoveGridLogic(this);

            // called when entity is about to be removed for whatever reason (block destroyed, entity deleted, grid despawn because of sync range, etc)
        }

        // less commonly used methods:

        public override bool IsSerialized()
        {
            // executed when the entity gets serialized (saved, blueprinted, streamed, etc) and asks all
            //   its components whether to be serialized too or not (calling GetObjectBuilder())
            if (_grid?.Physics == null) return base.IsSerialized();
            if (!Constants.IsServer) return base.IsSerialized();
            try
            {
                // serialise state here
                Entity.Storage[Constants.GridClassStorageGUID] = GridClassId.ToString();
            }
            catch (Exception e)
            {
                Utils.Log($"Error serialising CubeGridLogic, {e.Message}");
            }

            // you cannot add custom OBs to the game so this should always return the base (which currently is always false).
            return base.IsSerialized();
        }

        /*public override void UpdatingStopped()
        {
            base.UpdatingStopped();

            // only called when game is paused.
        }*/

        public void CheckGridLimits()
        {
            if (!Constants.IsServer) return;
            IsServerGridClassDirty = false;

            var grid = _grid as MyCubeGrid;
            var gridClass = GridClass;

            if (gridClass == null)
            {
                Utils.Log("Missing grid class");
                return;
            }

            if (grid == null)
            {
                Utils.Log("Missing grid grid");
                return;
            }

            if (_gridCheckResultsSync == null)
            {
                Utils.Log("Missing grid grid check results sync");
                return;
            }

            var checkResult = gridClass.CheckGrid(grid);
            _gridCheckResultsSync.Value =
                GridCheckResults.FromDetailedGridClassCheckResult(checkResult, gridClass.Id);
        }

        private void ApplyModifiers()
        {
            Utils.Log($"Applying modifiers {Modifiers}");

            foreach (var block in _grid.GetFatBlocks<IMyTerminalBlock>())
                CubeGridModifiers.ApplyModifiers(block, Modifiers);
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
        private void ClientOnBlockChanged(IMySlimBlock obj)
        {
            _isClientGridClassCheckDirty = true;
        }

        private void Grid_OnIsStaticChanged(IMyCubeGrid arg1, bool arg2)
        {
            //TODO recheck of grid class
            if (Constants.IsServer) IsServerGridClassDirty = true;

            if (Constants.IsClient) _isClientGridClassCheckDirty = true;
        }

        private void OnGridClassChanged(MySync<long, SyncDirection.FromServer> newGridClassId)
        {
            Utils.Log($"CubeGridLogic::OnGridClassChanged: new grid class id = {newGridClassId}", 2);

            ApplyModifiers();

            if (Constants.IsServer) IsServerGridClassDirty = true;

            if (Constants.IsClient) _isClientGridClassCheckDirty = true;

            /*if (MyAPIGateway.Session.OnlineMode != VRage.Game.MyOnlineModeEnum.OFFLINE && MyAPIGateway.Session.IsServer)
                MyAPIGateway.Utilities.SendMessage($"Synced server value on server: {obj.Value}");
            else
                MyAPIGateway.Utilities.ShowMessage("Test", $"Synced server value on client: {obj.Value}");*/
        }

        private void GridCheckResultsSync_ValueChanged(MySync<GridCheckResults, SyncDirection.FromServer> obj)
        {
            //Utils.WriteToClient($"GridCheck results = {obj.Value}");

            ApplyModifiers();
        }

        private void ServerOnBlockAdded(IMySlimBlock obj)
        {
            var fatBlock = obj.FatBlock;

            if (fatBlock != null)
                //Utils.WriteToClient($"Added block TypeId = {Utils.GetBlockId(fatBlock)}, Subtype = {Utils.GetBlockSubtypeId(fatBlock)}");
                CubeGridModifiers.ApplyModifiers(fatBlock, Modifiers);

            IsServerGridClassDirty = true;
            _isGridOwnerDirty = true;
        }

        private void ServerOnBlockRemoved(IMySlimBlock obj)
        {
            IsServerGridClassDirty = true;
            _isGridOwnerDirty = true;
        }

        private void OnBlockOwnershipChanged(IMyCubeGrid obj)
        {
            _isGridOwnerDirty = true;
        }

        public static List<CubeGridLogic> GetGridsToBeChecked(int max)
        {
            if (!Constants.IsServer) throw new Exception("This method should only be called on the server");

            var output = new List<CubeGridLogic>();

            while (ToBeCheckedOnServerQueue.Count > 0 && output.Count < max)
            {
                var grid = ToBeCheckedOnServerQueue.Dequeue();
                grid.IsServerGridClassDirty = false;

                if (!grid.MarkedForClose) output.Add(grid);
            }

            return output;
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
    }

    [ProtoContract]
    public struct GridCheckResults
    {
        [ProtoMember(1)] public bool MaxBlocks;
        [ProtoMember(2)] public bool MinBlocks;
        [ProtoMember(3)] public bool MaxPCU;
        [ProtoMember(4)] public bool MaxMass;
        [ProtoMember(5)] public ulong BlockLimits;
        [ProtoMember(6)] public long GridClassId;
        [ProtoMember(7)] public bool ValidGridType;

        public bool CheckPassedForGridClass(GridClass gridClass)
        {
            if (gridClass == null) return false;

            if (gridClass.Id == 0) return true; //default/unknown grid class always passes

            if (gridClass.Id !=
                GridClassId) return false; //this GridCheckResult is for a different grid class, so always fails

            if (!MaxBlocks || !MaxPCU || !MaxMass) return false;

            return BlockLimits == 0;
        }

        public override string ToString()
        {
            return
                $"[GridCheckResults GridClassId={GridClassId} MaxBlocks={MaxBlocks} MaxPCU={MaxPCU} MaxMass={MaxMass} BlockLimits={BlockLimits} ]";
        }

        public static GridCheckResults FromDetailedGridClassCheckResult(DetailedGridClassCheckResult result,
            long gridClassId)
        {
            ulong blockLimits = 0;

            // ReSharper disable once InvertIf
            if (result.BlockLimits != null)
                for (var i = 0; i < result.BlockLimits.Length; i++)
                    if (!result.BlockLimits[i].Passed)
                        blockLimits += 1UL << i;

            return new GridCheckResults
            {
                MaxBlocks = result.MaxBlocks.Passed,
                MinBlocks = result.MinBlocks.Passed,
                MaxPCU = result.MaxPCU.Passed,
                MaxMass = result.MaxMass.Passed,
                BlockLimits = blockLimits,
                GridClassId = gridClassId,
                ValidGridType = result.ValidGridType
            };
        }
    }

    //MainGrid.GridGeneralDamageModifier.ValidateAndSet(0.75f);
}