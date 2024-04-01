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
        public readonly Dictionary<long, CubeGridLogic> CubeGridLogics = new Dictionary<long, CubeGridLogic>();

        public static ModConfig Config;

        private readonly Queue<IMyCubeGrid> _toBeInitialized = new Queue<IMyCubeGrid>();
        private int _lastFrameInit;
        internal static Comms Comms;

        public override void LoadData()
        {
            Comms = new Comms(Settings.COMMS_MESSAGE_ID);
            Config = ModConfig.LoadConfig();
            ModConfig.SaveConfig(Config, Constants.ConfigFilename);

            MyAPIGateway.Entities.OnEntityAdd += EntityAdded;
            MyAPIGateway.Session.OnSessionReady += HookDamageHandler;
            Instance = this;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Entities.OnEntityAdd -= EntityAdded;
            MyAPIGateway.Session.OnSessionReady -= HookDamageHandler;
            _toBeInitialized.Clear();
            CubeGridLogics.Clear();
            Instance = null;
        }

        private void EntityAdded(IMyEntity ent)
        {
            var grid = ent as IMyCubeGrid;
            if (grid == null) return;
            _toBeInitialized.Enqueue(grid);
            _lastFrameInit = MyAPIGateway.Session.GameplayFrameCounter;
        }

        private void HookDamageHandler()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(99, CubeGridModifiers.GridClassDamageHandler);
        }

        public override void UpdateAfterSimulation()
        {
            CockpitGUI.AddControls();
            var framesWaited = MyAPIGateway.Session.GameplayFrameCounter - _lastFrameInit;
            if (_toBeInitialized.Count < 1 || framesWaited < 10) return;
            var gridToInitialize = _toBeInitialized.Dequeue();
            var logic = new CubeGridLogic();
            logic.InitializeLogic(gridToInitialize);
        }
    }
}