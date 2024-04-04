using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class ModSessionManager : MySessionComponentBase
    {
        public static ModSessionManager Instance;
        public static ModConfig Config;

        public Dictionary<long, CubeGridLogic> CubeGridLogics = new Dictionary<long, CubeGridLogic>();
        public readonly Queue<IMyCubeGrid> ToBeInitialized = new Queue<IMyCubeGrid>();

        private int _lastFrameInit;
        private bool _cubeGridsRequested;
        internal static Comms Comms;

        public override void LoadData()
        {
            Comms = new Comms(Settings.COMMS_MESSAGE_ID);
            Config = ModConfig.LoadConfig();
            if (Config == null)
                ModConfig.SaveConfig(DefaultGridClassConfig.DefaultModConfig, Constants.ConfigFilename);

            if (Constants.IsServer)
            {
                MyAPIGateway.Entities.OnEntityAdd += EntityAdded;
                MyAPIGateway.Session.OnSessionReady += HookDamageHandler;
            }
            Instance = this;
        }

        protected override void UnloadData()
        {
            ModConfig.SaveConfig(Config, Constants.ConfigFilename);
            if (Constants.IsServer)
            {
                MyAPIGateway.Entities.OnEntityAdd -= EntityAdded;
                MyAPIGateway.Session.OnSessionReady -= HookDamageHandler;
                ToBeInitialized.Clear();
            }
            CubeGridLogics.Clear();
            GridsPerFactionClassManager.Reset();
            GridsPerPlayerClassManager.Reset();
            //Instance = null;
        }

        private void EntityAdded(IMyEntity ent)
        {
            var grid = ent as IMyCubeGrid;
            if (grid == null) return;
            ToBeInitialized.Enqueue(grid);
            _lastFrameInit = MyAPIGateway.Session.GameplayFrameCounter;
        }

        private void HookDamageHandler()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(99, CubeGridModifiers.GridClassDamageHandler);
        }

        public override void UpdateAfterSimulation()
        {
            CockpitGUI.AddControls();
            var initWaited = MyAPIGateway.Session.GameplayFrameCounter - _lastFrameInit;
            if (ToBeInitialized.Count > 1 && initWaited > 10)
            {
                if (Constants.IsServer)
                {
                    var gridToInitialize = ToBeInitialized.Dequeue();
                    var logic = new CubeGridLogic();
                    logic.Initialize(gridToInitialize);
                }
                else
                {
                    var gridToInitialize = ToBeInitialized.Dequeue();
                    var logic = CubeGridLogics[gridToInitialize.EntityId];
                    logic.Initialize(gridToInitialize);
                }
            }

            if (!Constants.IsClient || _cubeGridsRequested || MyAPIGateway.Session.GameplayFrameCounter < 300) return;
            Comms.RequestCubeGrids();
            _cubeGridsRequested = true;
        }
    }
}