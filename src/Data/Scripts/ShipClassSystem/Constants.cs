using System;
using Sandbox.ModAPI;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public static class Constants
    {
        public static readonly string ConfigFilename = "ShipClassConfig.xml";
        public static readonly int MinBlocks = 3;

        public static readonly Guid GridClassStorageGUID = new Guid("a8807ad4-524d-441a-a89a-0671fbfb1dd3");
        public static bool IsDedicated => MyAPIGateway.Utilities.IsDedicated;
        public static bool IsServer => MyAPIGateway.Multiplayer.IsServer;
        public static bool IsMultiplayer => MyAPIGateway.Multiplayer.MultiplayerActive;
        public static bool IsClient => !(IsServer && IsDedicated);
    }
}