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
            Config = ModConfig.LoadOrGetDefaultConfig(Constants.ConfigFilename);

            if (Constants.IsServer)
                //Save whatever config you're using
                ModConfig.SaveConfig(Config, Constants.ConfigFilename);
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(99, CubeGridModifiers.GridClassDamageHandler);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();

            BeaconGUI.AddControls(ModContext);
        }

        public static GridClass GetGridClassById(long GridClassId)
        {
            return Instance.Config.GetGridClassById(GridClassId);
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