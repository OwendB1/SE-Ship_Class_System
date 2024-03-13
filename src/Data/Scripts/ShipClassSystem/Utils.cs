using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace RedVsBlueClassSystem
{
    public static class Utils
    {
        /*public static void ClientDebug(string msg)
        {
            if (Constants.IsClient && Settings.Debug)
            {
                MyAPIGateway.Utilities.ShowMessage("[[BSCS]]: ", msg);
            }
        }*/

        public static void ShowNotification(string msg, int disappearTime = 10000, string font = MyFontEnum.Red)
        {
            if (MyAPIGateway.Session?.Player != null)
                MyAPIGateway.Utilities.ShowNotification(msg, disappearTime, font);
        }

        public static void WriteToClient(string msg)
        {
            if (Constants.IsClient)
            {
                MyAPIGateway.Utilities.ShowMessage("[RvBCS]: ", msg);
            }
        }

        public static void Log(string msg, int logPriority = 0)
        {
            if (logPriority >= Settings.LOG_LEVEL)
            {
                MyLog.Default.WriteLine($"[RvBCS]: {msg}");
            }

            if(logPriority >= Settings.CLIENT_OUTPUT_LOG_LEVEL)
            {
                MyAPIGateway.Utilities.ShowMessage($"[RvBCS={logPriority}]: ", msg);
            }
        }

        public static void LogException(Exception e)
        {
            Log($"Exception message = {e.Message}, Stack trace:\n{e.StackTrace}", 3);
        }

        public static void SaveConfig<T>(string variableId, string filename, T data)
        {
            string saveText = MyAPIGateway.Utilities.SerializeToXML(data);

            MyAPIGateway.Utilities.SetVariable(variableId, saveText);

            Log($"Saving config file to: {filename}", 0);

            using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(string)))
            {
                file.Write(saveText);
            }
        }

        public static string GetBlockId(IMyCubeBlock block)
        {
            return Convert.ToString(block.BlockDefinition.TypeId).Replace("MyObjectBuilder_", "");
        }

        public static string GetBlockSubtypeId(IMyCubeBlock block)
        {
            return Convert.ToString(block.BlockDefinition.SubtypeId);
        }

        public static CubeGridLogic GetGridLogic(this IMyCubeGrid grid)
        {
            return grid.GameLogic?.GetAs<CubeGridLogic>();
        }

        public static CubeGridLogic GetGridLogic(this MyCubeGrid grid)
        {
            return grid.GameLogic?.GetAs<CubeGridLogic>();
        }

        public static CubeGridLogic GetGridLogic(this IMyCubeBlock block)
        {
            return block.CubeGrid.GameLogic?.GetAs<CubeGridLogic>();
        }

        public static CubeGridLogic GetGridLogic(this IMyTerminalBlock block)
        {
            return block.CubeGrid.GameLogic?.GetAs<CubeGridLogic>();
        }

        public static MyEntity GetControlledGrid()
        {
            try
            {
                if (MyAPIGateway.Session == null || MyAPIGateway.Session.Player == null)
                {
                    return null;
                }

                var controlledEntity = MyAPIGateway.Session.Player.Controller?.ControlledEntity?.Entity;
                if (controlledEntity == null)
                {
                    return null;
                }

                if (controlledEntity is IMyCockpit || controlledEntity is IMyRemoteControl)
                {
                    return (controlledEntity as IMyCubeBlock).CubeGrid as MyEntity;
                }
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Error in GetControlledGrid: {e}");
            }

            return null;
        }

        public static MyEntity GetControlledCockpit(MyEntity controlledGrid)
        {
            if (controlledGrid == null)
                return null;

            var grid = controlledGrid as MyCubeGrid;
            if (grid == null)
                return null;

            foreach (var block in grid.GetFatBlocks())
            {
                var cockpit = block as MyCockpit; // Convert the block to MyCockpit
                if (cockpit != null)
                {
                    if (cockpit.WorldMatrix != null)  // Add null check here
                        return cockpit;
                }
            }
            return null;
        }

        public static T[] ConcatArrays<T>(params T[][] p)
        {
            var position = 0;
            var outputArray = new T[p.Sum(a => a.Length)];
            foreach (var curr in p)
            {
                Array.Copy(curr, 0, outputArray, position, curr.Length);
                position += curr.Length;
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

    public static class VectorUtils
    {
        public static Vector2 Round(this Vector2 vector)
        {
            return new Vector2((float)Math.Round(vector.X), (float)Math.Round(vector.Y));
        }

        public static Vector2 Floor(this Vector2 vector)
        {
            return new Vector2((float)Math.Floor(vector.X), (float)Math.Round(vector.Y));
        }

        public static Vector2 Ceiling(this Vector2 vector)
        {
            return new Vector2((float)Math.Ceiling(vector.X), (float)Math.Round(vector.Y));
        }
    }
}
