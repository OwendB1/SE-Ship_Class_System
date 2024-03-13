using Sandbox.Game.Entities;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Network;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class ModSessionManager : MySessionComponentBase, IMyEventProxy
    {
        internal Comms _Comms;


        public ModConfig Config;


        private MyEntity lastControlledEntity;
        public static ModSessionManager Instance { get; private set; }

        internal static Comms Comms => Instance._Comms;

        public override void Init(MyObjectBuilder_SessionComponent SessionComponent)
        {
            base.Init(SessionComponent);

            Instance = this;

            Utils.Log("Init");

            _Comms = new Comms(Settings.COMMS_MESSAGE_ID);
            Config = ModConfig.LoadOrGetDefaultConfig(Constants.ConfigFilename);

            if (Constants.IsServer)
                //Save whatever config you're using
                ModConfig.SaveConfig(Config, Constants.ConfigFilename);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();

            BeaconGUI.AddControls(ModContext);

            if (Constants.IsServer)
            {
                var gridsToCheck = CubeGridLogic.GetGridsToBeChecked(Settings.MAX_GRID_PROCESSED_PER_TICK);

                foreach (var gridLogic in gridsToCheck) gridLogic.CheckGridLimits();
            }

            if (Constants.IsClient)
            {
                // Existing code for controlled entities and predictions
                var controlledEntity = Utils.GetControlledGrid();
                var cockpitEntity = Utils.GetControlledCockpit(controlledEntity);

                if (controlledEntity != null && !controlledEntity.Equals(lastControlledEntity))
                {
                    lastControlledEntity = controlledEntity;
                    var controlled = controlledEntity as MyCubeGrid;

                    if (controlled != null)
                    {
                        var cubeGridLogic = controlled.GetGridLogic();

                        if (cubeGridLogic != null && !cubeGridLogic.GridMeetsGridClassRestrictions)
                        {
                            var gridClass = cubeGridLogic.GridClass;

                            if (gridClass != null)
                                Utils.ShowNotification(
                                    $"Class \"{gridClass.Name}\" not valid for grid \"{controlled.DisplayName}\"");
                            else
                                Utils.ShowNotification($"Unknown class assigned to grid \"{controlled.DisplayName}\"");
                        }
                        else if (cubeGridLogic == null)
                        {
                            Utils.Log($"Grid missing CubeGridLogic: \"{controlled.DisplayName}\"", 1);
                        }
                    }
                }
                else if (controlledEntity == null)
                {
                    lastControlledEntity = null;
                }
            }
        }

        public static GridClass GetGridClassById(long GridClassId)
        {
            return Instance.Config.GetGridClassById(GridClassId);
        }

        public static GridClass[] GetAllGridClasses()
        {
            return Instance.Config.GridClasses ?? new GridClass[0];
        }

        internal static bool IsValidGridClass(long gridClassId)
        {
            return Instance.Config.IsValidGridClassId(gridClassId);
        }
    }
}