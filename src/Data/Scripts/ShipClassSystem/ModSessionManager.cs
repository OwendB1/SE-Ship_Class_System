using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Network;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class ModSessionManager : MySessionComponentBase, IMyEventProxy
    {
        private Comms _comms;

        public ModConfig Config;

        public static ModSessionManager Instance { get; private set; }

        internal static Comms Comms => Instance._comms;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            Instance = this;

            Utils.Log("Init");

            _comms = new Comms(Settings.COMMS_MESSAGE_ID);
            Config = ModConfig.LoadConfig();
            ModConfig.SaveConfig(Config, Constants.ConfigFilename);
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(99, CubeGridModifiers.GridClassDamageHandler);
        }

        public override void UpdateAfterSimulation()
        {
            if (!Constants.IsServer || CubeGridLogic.ToBeInitialized.Count < 1) return;
            var gridToInitialize = CubeGridLogic.ToBeInitialized.Dequeue();
            gridToInitialize.InitializeLogic();
        }

        public static string[] GetIgnoredFactionTags()
        {
            return Instance.Config.IgnoreFactionTags;
        }

        public static GridClass GetGridClassById(long gridClassId)
        {
            return Instance.Config.GetGridClassById(gridClassId);
        }

        public static GridClass[] GetAllGridClasses()
        {
            return Instance.Config.GridClasses ?? Array.Empty<GridClass>();
        }

        internal static bool IsValidGridClass(long gridClassId)
        {
            return Instance.Config.IsValidGridClassId(gridClassId);
        }
    }
}