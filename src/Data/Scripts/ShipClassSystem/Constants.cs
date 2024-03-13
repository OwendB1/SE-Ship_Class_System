using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedVsBlueClassSystem
{
    public static class Constants
    {
        public static readonly string ConfigFilename = "RedVsBlueClassSystemConfig.xml";
        public static readonly int MinBlocks = 3;

        public static readonly Guid GridClassStorageGUID = new Guid("02bfbbe4-8168-4a7f-855a-939a0f9d9dd5");
        public static bool IsDedicated => MyAPIGateway.Utilities.IsDedicated;
        public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;
        public static bool IsMultiplayer => MyAPIGateway.Multiplayer.MultiplayerActive;
        public static bool IsClient => !(IsServer && IsDedicated);
    }
}
