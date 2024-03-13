using Sandbox.Common.ObjectBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Sync;
using VRage.Game.ModAPI.Network;
using VRage.Network;
using Sandbox.ModAPI;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using ProtoBuf;

namespace RedVsBlueClassSystem
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeGrid), false)]
    public class CubeGridLogic : MyGameLogicComponent, IMyEventProxy
    {
        private static Dictionary<long, CubeGridLogic> CubeGridLogics = new Dictionary<long, CubeGridLogic>();
        private static List<CubeGridLogic> AllCubeGridLogics = new List<CubeGridLogic>();
        private static Queue<CubeGridLogic> ToBeCheckedOnServerQueue = new Queue<CubeGridLogic>();
        private static GridsPerFactionClassManager gridsPerFactionClassManager = new GridsPerFactionClassManager(ModSessionManager.Instance.Config);

        private IMyCubeGrid Grid;

        private MySync<long, SyncDirection.FromServer> GridClassSync = null;
        private MySync<GridCheckResults, SyncDirection.FromServer> GridCheckResultsSync = null;

        private bool _IsServerGridClassDirty = false;
        public bool IsServerGridClassDirty {
            get { return _IsServerGridClassDirty; }

            protected set {
                if (value != _IsServerGridClassDirty) {
                    if (value)
                    {
                        ToBeCheckedOnServerQueue.Enqueue(this);
                    }

                    _IsServerGridClassDirty = value;
                }
            } }

        private bool IsGridOwnerDirty = true;

        private IMyFaction _OwningFaction = null;
        public IMyFaction OwningFaction { get {
                if (IsGridOwnerDirty) {
                    _OwningFaction = GetOwningFaction();
                    IsGridOwnerDirty = false;
                }

                return _OwningFaction;
            } }

        private bool _isClientGridClassCheckDirty = true;

        private DetailedGridClassCheckResult _detailedGridClassCheckResult;
        public DetailedGridClassCheckResult DetailedGridClassCheckResult { get {
                if (_isClientGridClassCheckDirty) {
                    _detailedGridClassCheckResult = GridClass?.CheckGrid(Grid);
                    _isClientGridClassCheckDirty = false;
                }

                return _detailedGridClassCheckResult;
            } }

        public long GridClassId { get { return GridClassSync.Value; } set {
                if (!Constants.IsServer)
                {
                    throw new Exception("CubeGridLgic:: set GridClassId: Grid class Id can only be set on the server");
                }

                if (!ModSessionManager.IsValidGridClass(value))
                {
                    throw new Exception($"CubeGridLgic:: set GridClassId: invalid grid class id {value}");
                }

                Utils.Log($"CubeGridLogic::GridClassId setting grid class to {value}", 1);

                GridClassSync.Value = value;
            }
        }
        public GridCheckResults GridCheckResults { get { return GridCheckResultsSync.Value; } }
        public GridClass GridClass { get { return ModSessionManager.GetGridClassById(GridClassId); } }
        public bool GridMeetsGridClassRestrictions { get { return GridCheckResults.CheckPassedForGridClass(GridClass); } }
        public GridModifiers Modifiers { get
            {
                return GridMeetsGridClassRestrictions ? GridClass.Modifiers : ModSessionManager.GetGridClassById(0).Modifiers;
            } }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // the base methods are usually empty, except for OnAddedToContainer()'s, which has some sync stuff making it required to be called.
            base.Init(objectBuilder);

            Grid = (IMyCubeGrid)Entity;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            //Utils.Log("[CubeGridLogic] FirstUpdate");

            if (Grid?.Physics == null) // ignore projected and other non-physical grids
            {
                return;
            }

            AddGridLogic(this);

            if (Entity.Storage == null)
            {
                Entity.Storage = new MyModStorageComponent();
            }

            //Init event handlers
            GridClassSync.ValueChanged += OnGridClassChanged;
            GridCheckResultsSync.ValueChanged += GridCheckResultsSync_ValueChanged;

            Grid.OnBlockOwnershipChanged += OnBlockOwnershipChanged;
            Grid.OnIsStaticChanged += Grid_OnIsStaticChanged;

            if (Constants.IsClient) {
                Grid.OnBlockAdded += ClientOnBlockChanged;
                Grid.OnBlockRemoved += ClientOnBlockChanged;
            }

            //If server, init persistant storage + apply grid class
            if (Constants.IsServer)
            {
                IsServerGridClassDirty = true;

                Grid.OnBlockAdded += ServerOnBlockAdded;
                Grid.OnBlockRemoved += ServerOnBlockRemoved;


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
                        string msg = $"[CubeGridLogic] Error parsing serialised GridClassId: {Entity.Storage[Constants.GridClassStorageGUID]}, EntityId = {Grid.EntityId}";

                        Utils.Log(msg, 1);
                        Utils.Log(e.Message, 1);
                    }

                    //TODO validate gridClassId
                    Utils.Log($"[CubeGridLogic] Assigning GridClassId = {gridClassId}");
                    GridClassSync.Value = gridClassId;
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
            if (Grid?.Physics != null)
            {
                if (Constants.IsServer)
                {
                    try
                    {
                        // serialise state here
                        Entity.Storage[Constants.GridClassStorageGUID] = GridClassId.ToString();
                    }
                    catch (Exception e)
                    {
                        Utils.Log($"Error serialising CubeGridLogic, {e.Message}");
                    }

                }
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
            if (Constants.IsServer)
            {
                IsServerGridClassDirty = false;

                var grid = Grid as MyCubeGrid;
                var gridClass = GridClass;

                if (gridClass == null) {
                    Utils.Log("Missing grid class");
                    return;
                }

                if (grid == null)
                {
                    Utils.Log("Missing grid grid");
                    return;
                }

                if (GridCheckResultsSync == null)
                {
                    Utils.Log("Missing grid grid check results sync");
                    return;
                }

                var checkResult = gridClass.CheckGrid(grid);
                GridCheckResultsSync.Value = GridCheckResults.FromDetailedGridClassCheckResult(checkResult, gridClass.Id);
            }
        }

        private void ApplyModifiers()
        {
            Utils.Log($"Applying modifiers {Modifiers}");

            foreach (var block in Grid.GetFatBlocks<IMyTerminalBlock>())
            {
                CubeGridModifiers.ApplyModifiers(block, Modifiers);
            }
        }

        private IMyFaction GetOwningFaction()
        {
            if (Grid.BigOwners.Count == 0)
            {
                return null;
            }

            if (Grid.BigOwners.Count == 1)
            {
                return MyAPIGateway.Session.Factions.TryGetPlayerFaction(Grid.BigOwners[0]);
            }

            var ownersPerFaction = new Dictionary<IMyFaction, int>();

            //Find the faction with the most owners
            foreach (var owner in Grid.BigOwners)
            {
                var OwnerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(Grid.BigOwners[0]);
                
                if (OwnerFaction != null)
                {
                    if (!ownersPerFaction.ContainsKey(OwnerFaction))
                    {
                        ownersPerFaction[OwnerFaction] = 1;
                    }
                    else
                    {
                        ownersPerFaction[OwnerFaction]++;
                    }
                }
            }

            if (ownersPerFaction.Count == 0)
            {
                return null;
            }

            //new select the faction with the most owners
            return ownersPerFaction.MaxBy(kvp => kvp.Value).Key;
        }

        //Event handlers
        private void ClientOnBlockChanged(IMySlimBlock obj)
        {
            _isClientGridClassCheckDirty = true;
        }

        private void Grid_OnIsStaticChanged(IMyCubeGrid arg1, bool arg2)
        {
            //TODO
            if (Constants.IsServer)
            {
                IsServerGridClassDirty = true;//need to trigger a recheck of grid class
            }

            if (Constants.IsClient)
            {
                _isClientGridClassCheckDirty = true;
            }
        }

        private void OnGridClassChanged(MySync<long, SyncDirection.FromServer> newGridClassId)
        {
            Utils.Log($"CubeGridLogic::OnGridClassChanged: new grid class id = {newGridClassId}", 2);

            ApplyModifiers();

            if (Constants.IsServer)
            {
                IsServerGridClassDirty = true;
            }

            if (Constants.IsClient)
            {
                _isClientGridClassCheckDirty = true;
            }

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
            IMyCubeBlock fatBlock = obj.FatBlock;

            if (fatBlock != null)
            {
                //Utils.WriteToClient($"Added block TypeId = {Utils.GetBlockId(fatBlock)}, Subtype = {Utils.GetBlockSubtypeId(fatBlock)}");

                CubeGridModifiers.ApplyModifiers(fatBlock, Modifiers);
            }

            IsServerGridClassDirty = true;
            IsGridOwnerDirty = true;
        }

        private void ServerOnBlockRemoved(IMySlimBlock obj)
        {
            IsServerGridClassDirty = true;
            IsGridOwnerDirty = true;
        }

        private void OnBlockOwnershipChanged(IMyCubeGrid obj)
        {
            IsGridOwnerDirty = true;
        }

        public static List<CubeGridLogic> GetGridsToBeChecked(int max)
        {
            if (!Constants.IsServer)
            {
                throw new Exception("This method should only be called on the server");
            }

            var output = new List<CubeGridLogic>();

            while (ToBeCheckedOnServerQueue.Count > 0 && output.Count < max)
            {
                var grid = ToBeCheckedOnServerQueue.Dequeue();
                grid.IsServerGridClassDirty = false;

                if (!grid.MarkedForClose)
                {
                    output.Add(grid);
                }

            }

            return output;
        }

        public static CubeGridLogic GetCubeGridLogicByEntityId(long entityId)
        {
            if (CubeGridLogics.ContainsKey(entityId))
            {
                return CubeGridLogics[entityId];
            }

            return null;
        }

        public static void UpdateGridsPerFactionClass()
        {
            gridsPerFactionClassManager.Reset();

            foreach(var gridLogic in AllCubeGridLogics)
            {
                gridsPerFactionClassManager.AddCubeGrid(gridLogic);
            }
        }

        private static void AddGridLogic(CubeGridLogic gridLogic)
        {
            try
            {
                if (gridLogic == null) {
                    throw new Exception("gridLogic cannot be null");
                }

                if(gridLogic.Grid == null)
                {
                    throw new Exception("gridLogic.Grid cannot be null");
                }

                if(AllCubeGridLogics == null)
                {
                    throw new Exception("AllCubeGridLogics cannot be null");
                }

                if (gridsPerFactionClassManager == null)
                {
                    throw new Exception("gridsPerFactionClassManager cannot be null");
                }

                if (CubeGridLogics == null)
                {
                    throw new Exception("CubeGridLogics cannot be null");
                }
                
                if (!AllCubeGridLogics.Contains(gridLogic))
                {
                    AllCubeGridLogics.Add(gridLogic);
                }
                
                gridsPerFactionClassManager.AddCubeGrid(gridLogic);
                CubeGridLogics[gridLogic.Grid.EntityId] = gridLogic;
            }
            catch (Exception e) {
                Utils.Log($"CubeGridLogic::AddGridLogic: caught error", 3);
                Utils.LogException(e);
            }
        }

        private static void RemoveGridLogic(CubeGridLogic gridLogic)
        {
            CubeGridLogics.Remove(gridLogic.Grid.EntityId);
            AllCubeGridLogics.RemoveAll((item) => item == gridLogic);
        }
    }

    [ProtoContract]
    public struct GridCheckResults
    {
        [ProtoMember(1)]
        public bool MaxBlocks;
        [ProtoMember(2)]
        public bool MinBlocks;
        [ProtoMember(3)]
        public bool MaxPCU;
        [ProtoMember(4)]
        public bool MaxMass;
        [ProtoMember(5)]
        public ulong BlockLimits;
        [ProtoMember(6)]
        public long GridClassId;
        [ProtoMember(7)]
        public bool ValidGridType;

        public bool CheckPassedForGridClass(GridClass gridClass) {
            if(gridClass == null)
            {
                return false;
            }

            if(gridClass.Id == 0)
            {
                return true;//default/unknown grid class always passes
            }

            if(gridClass.Id != GridClassId)
            {
                return false;//this GridCheckResult is for a different grid class, so always fails
            }

            if(!MaxBlocks || !MaxPCU || !MaxMass)
            {
                return false;
            }

            if(BlockLimits != 0)
            {
                return false;
            }
            
            return true;
        }

        public override string ToString()
        {
            return $"[GridCheckResults GridClassId={GridClassId} MaxBlocks={MaxBlocks} MaxPCU={MaxPCU} MaxMass={MaxMass} BlockLimits={BlockLimits} ]";
        }

        public static GridCheckResults FromDetailedGridClassCheckResult(DetailedGridClassCheckResult result, long gridClassId)
        {
            ulong BlockLimits = 0;

            if(result.BlockLimits != null)
            {
                for(int i = 0; i < result.BlockLimits.Length; i++)
                {
                    if(!result.BlockLimits[i].Passed)
                    {
                        BlockLimits += 1UL << i;
                    }
                }
            }

            return new GridCheckResults() { 
                MaxBlocks = result.MaxBlocks.Passed, 
                MinBlocks = result.MinBlocks.Passed, 
                MaxPCU = result.MaxPCU.Passed, 
                MaxMass = result.MaxMass.Passed, 
                BlockLimits = BlockLimits, 
                GridClassId = gridClassId,
                ValidGridType = result.ValidGridType
            };
        }
    }

    //MainGrid.GridGeneralDamageModifier.ValidateAndSet(0.75f);
}
