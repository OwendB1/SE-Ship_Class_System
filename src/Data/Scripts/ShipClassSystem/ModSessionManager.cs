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
        internal static Comms Comms;

        public override void LoadData()
        {
            Comms = new Comms(Settings.COMMS_MESSAGE_ID);
            Config = ModConfig.LoadConfig();
            if (Config == null)
                ModConfig.SaveConfig(DefaultGridClassConfig.DefaultModConfig, Constants.ConfigFilename);
            MyAPIGateway.Entities.OnEntityAdd += EntityAdded;
            MyAPIGateway.Session.OnSessionReady += HookDamageHandler;
            Instance = this;
        }

        protected override void UnloadData()
        {
            ModConfig.SaveConfig(Config, Constants.ConfigFilename);
            MyAPIGateway.Entities.OnEntityAdd -= EntityAdded;
            MyAPIGateway.Session.OnSessionReady -= HookDamageHandler;
            //foreach (var logic in CubeGridLogics)
            //{
            //    logic.Value.GridMarkedForClose();
            //}
            ToBeInitialized.Clear();
            CubeGridLogics.Clear();
            GridsPerFactionClassManager.Reset();
            GridsPerPlayerClassManager.Reset();
            Comms.Discard();
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
            var initWaited = MyAPIGateway.Session.GameplayFrameCounter - _lastFrameInit;
            if (ToBeInitialized.Count <= 1 || initWaited <= 10) return;
            if (Constants.IsClient && MyAPIGateway.Session.ControlledObject == null) return;
            var gridToInitialize = ToBeInitialized.Dequeue();
            var logic = new CubeGridLogic();
            logic.Initialize(gridToInitialize);
        }
    }
}