﻿using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace ShipClassSystem
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class ModSessionManager : MySessionComponentBase
    {
        public static ModSessionManager Instance;
        public static ModConfig Config;

        public Dictionary<long, CubeGridLogic> CubeGridLogics = new Dictionary<long, CubeGridLogic>();
        public readonly Queue<IMyEntity> ToBeInitialized = new Queue<IMyEntity>();

        private int _lastFrameInit;
        internal static Comms Comms;

        public override void LoadData()
        {
            Comms = new Comms(Settings.COMMS_MESSAGE_ID);
            if (Constants.IsServer)
            {
                Config = ModConfig.LoadConfig();
                if (Config == null)
                {
                    Config = DefaultGridClassConfig.DefaultModConfig;
                    ModConfig.SaveConfig(Config, Constants.ConfigFilename);
                }
            } else Comms.RequestConfig();
            MyAPIGateway.Entities.OnEntityAdd += EntityAdded;
            MyAPIGateway.Entities.OnEntityRemove += EntityRemoved;
            MyAPIGateway.Session.OnSessionReady += HookDamageHandler;
            Instance = this;
        }

        protected override void UnloadData()
        {
            if (Constants.IsServer)
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
            if (grid == null && MyAPIGateway.Session.GameplayFrameCounter < 100) return;
            ToBeInitialized.Enqueue(ent);
        }

        private void EntityRemoved(IMyEntity ent)
        {
            var grid = ent as IMyCubeGrid;
            if (grid == null) return;
            if (CubeGridLogics.ContainsKey(grid.EntityId))
                CubeGridLogics[grid.EntityId].RemoveGridLogic();
        }

        private void HookDamageHandler()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(99, CubeGridModifiers.GridClassDamageHandler);
        }

        public override void UpdateAfterSimulation()
        {
            if (Config == null) return;
            var initWaited = MyAPIGateway.Session.GameplayFrameCounter - _lastFrameInit;
            if (ToBeInitialized.Count <= 1 || initWaited < 60) return;
            if (Constants.IsClient && MyAPIGateway.Session.ControlledObject == null) return;
            var target = ToBeInitialized.Dequeue();
            var logic = new CubeGridLogic();
            logic.Initialize(target);
        }
    }
}