using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public static class Utils
    {
        public static void ShowNotification(string msg, int disappearTime = 10000, string font = MyFontEnum.Red)
        {
            if (!Constants.IsServer)
                MyAPIGateway.Utilities.ShowNotification(msg, disappearTime, font);
        }

        public static void WriteToClient(string msg)
        {
            if (Constants.IsClient) MyAPIGateway.Utilities.ShowMessage("[Ship Classes]: ", msg);
        }

        public static void Log(string msg, int logPriority = 0)
        {
            if (logPriority >= Settings.LOG_LEVEL) MyLog.Default.WriteLine($"[Ship Classes]: {msg}");

            if (logPriority >= Settings.CLIENT_OUTPUT_LOG_LEVEL)
                MyAPIGateway.Utilities.ShowMessage($"[Ship Classes={logPriority}]: ", msg);
        }

        public static void LogException(Exception e)
        {
            Log($"Exception message = {e.Message}, Stack trace:\n{e.StackTrace}", 3);
        }

        public static string GetBlockTypeId(IMyCubeBlock block)
        {
            return Convert.ToString(block.BlockDefinition.TypeId).Replace("MyObjectBuilder_", "");
        }

        public static string GetBlockTypeId(IMySlimBlock block)
        {
            return Convert.ToString(block.GetObjectBuilder().TypeId).Replace("MyObjectBuilder_", "");
        }

        public static string GetBlockSubtypeId(IMyCubeBlock block)
        {
            return Convert.ToString(block.BlockDefinition.SubtypeId);
        }

        public static string GetBlockSubtypeId(IMySlimBlock block)
        {
            return Convert.ToString(block.GetObjectBuilder().SubtypeId);
        }

        public static CubeGridLogic GetMainGridLogic(this IMyCubeGrid grid)
        {
            List<IMyCubeGrid> subgrids;
            var main = GetMainCubeGrid(grid, out subgrids);

            CubeGridLogic logic;
            ModSessionManager.Instance.CubeGridLogics.TryGetValue(main.EntityId, out logic);
            return logic;
        }

        public static CubeGridLogic GetMainGridLogic(this IMyTerminalBlock block)
        {
            List<IMyCubeGrid> subgrids;
            var main = GetMainCubeGrid(block.CubeGrid, out subgrids);

            CubeGridLogic logic;
            ModSessionManager.Instance.CubeGridLogics.TryGetValue(main.EntityId, out logic);
            return logic;
        }

        public static IMyCubeGrid GetMainCubeGrid(IMyCubeGrid grid, out List<IMyCubeGrid> subgrids)
        {
            var group = grid.GetGridGroup(GridLinkTypeEnum.Mechanical);
            var grids = new List<IMyCubeGrid>();

            group?.GetGrids(grids);
            grids = grids.Where(g => g?.Physics != null).ToList();

            var biggestGrid = grids.OfType<MyCubeGrid>().MaxBy(concrete => concrete.BlocksCount);
            subgrids = grids.Where(g => g.EntityId != biggestGrid.EntityId).ToList();
            return biggestGrid;
        }

        public static T[] ConcatArrays<T>(params T[][] p)
        {
            var position = 0;
            var outputArray = new T[p.Sum(a => a.Length)];
            foreach (var current in p)
            {
                Array.Copy(current, 0, outputArray, position, current.Length);
                position += current.Length;
            }

            return outputArray;
        }
    }

    public static class TextUtils
    {
        public static readonly float CharWidth = 20;
        public static readonly float BaseLineHeight = 30f;

        public static float GetLineHeight(float scale = 1f)
        {
            return BaseLineHeight * scale;
        }

        public static float GetTextWidth(string text, float scale = 1f)
        {
            //It might be more complex than this..?
            return text.Length * CharWidth * scale;
        }

        public static float GetTextHeight(string text, float scale = 1f)
        {
            return NumLines(text) * GetLineHeight(scale);
        }

        public static int NumLines(string text)
        {
            var charDiff = text.Length - text.Replace("\n", string.Empty).Length;

            return charDiff + 1;
        }
    }
}