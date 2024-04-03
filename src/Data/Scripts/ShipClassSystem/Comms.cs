using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    internal class Comms
    {
        private readonly ushort _commsId;
        public Comms(ushort id)
        {
            _commsId = id;
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(_commsId, MessageHandler);
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(_commsId, MessageHandler);
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(_commsId, MessageHandler);
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(_commsId, MessageHandler);
        }

        public void SendChangeGridClassMessage(long entityId, long gridClassId)
        {
            if (!Constants.IsClient) return;
            try
            {
                var messageData = MyAPIGateway.Utilities.SerializeToBinary(new GridMessage
                    { EntityId = entityId, GridClassId = gridClassId });
                var message = MyAPIGateway.Utilities.SerializeToBinary(new Message
                    { Type = MessageType.ChangeGridClass, Data = messageData });
                MyAPIGateway.Multiplayer.SendMessageToServer(_commsId, message);
            }
            catch (Exception e)
            {
                Utils.Log("Comms::SendChangeGridClassMessage error", 3);
                Utils.LogException(e);
            }
        }

        public void SendUpdateCubeGridLogics(Dictionary<long,CubeGridLogic> logics, ulong recipient)
        {
            if (!Constants.IsServer) return;
            try
            {
                var list = logics.Select(l => new GridMessage { EntityId = l.Key, GridClassId = l.Value.GridClassId }).ToList();
                var messageData = MyAPIGateway.Utilities.SerializeToBinary(new UpdateCubeGridLogicsMessage
                    { CubeGrids = list });
                var message = MyAPIGateway.Utilities.SerializeToBinary(new Message
                    { Type = MessageType.UpdateCubeGridLogics, Data = messageData });
                MyAPIGateway.Multiplayer.SendMessageTo(_commsId, message, recipient);
            }
            catch (Exception e)
            {
                Utils.Log("Comms::SendUpdateCubeGridLogics error", 3);
                Utils.LogException(e);
            }
        }

        public void RequestCubeGrids()
        {
            if (!Constants.IsClient) return;
            try
            {
                var messageData = MyAPIGateway.Utilities.SerializeToBinary(MyAPIGateway.Multiplayer.MyId);
                var message = MyAPIGateway.Utilities.SerializeToBinary(new Message
                    { Type = MessageType.RequestCubeGrids, Data = messageData });
                MyAPIGateway.Multiplayer.SendMessageToServer(_commsId, message);
            }
            catch (Exception e)
            {
                Utils.Log("Comms::RequestCubeGrids error", 3);
                Utils.LogException(e);
            }
        }

        private void MessageHandler(ushort handlerId, byte[] data, ulong playerId, bool unknown)
        {
            Utils.Log($"Comms::MessageHandler recieved message length = {data.Length}", 1);

            Message message;

            try
            {
                message = MyAPIGateway.Utilities.SerializeFromBinary<Message>(data);
            }
            catch (Exception e)
            {
                Utils.Log("Comms::MessageHandler: deserialise message error", 3);
                Utils.LogException(e);
                return;
            }

            Utils.Log("Comms::MessageHandler: deserialised message", 1);

            switch (message.Type)
            {
                case MessageType.ChangeGridClass:
                    if (!Constants.IsServer) return;
                    HandleChangeGridClassMessage(message.Data);
                    break;
                case MessageType.UpdateCubeGridLogics:
                    if (!Constants.IsClient || ModSessionManager.Instance.CubeGridLogics.Count > 0) return;
                    HandleUpdateCubeGridLogics(message.Data);
                    break;
                case MessageType.RequestCubeGrids:
                    if (!Constants.IsServer) return;
                    HandleRequestCubeGrids(message.Data);
                    break;
                default:
                    Utils.Log("Comms::MessageHandler: Unknown message type", 2);
                    break;
            }
        }

        private void HandleChangeGridClassMessage(byte[] data)
        {
            GridMessage message;

            try
            {
                message = MyAPIGateway.Utilities.SerializeFromBinary<GridMessage>(data);
            }
            catch (Exception e)
            {
                Utils.Log("Comms::HandleChangeGridClassMessage deserialize message error", 3);
                Utils.LogException(e);
                return;
            }

            var cubeGrid = MyAPIGateway.Entities.GetEntityById(message.EntityId) as IMyCubeGrid;
            var gridLogic = cubeGrid.GetMainGridLogic();
            if (gridLogic == null) return;

            if (ModSessionManager.Config.IsValidGridClassId(message.GridClassId))
            {
                Utils.Log($"Comms::HandleChangeGridClassMessage: Setting grid class id for {message.EntityId} to {message.GridClassId}", 1);
                gridLogic.GridClassId = message.GridClassId;
            }
            else Utils.Log($"Comms::HandleChangeGridClassMessage: Unknown grid class ID {message.GridClassId}", 3);
        }

        private void HandleUpdateCubeGridLogics(byte[] data)
        {
            UpdateCubeGridLogicsMessage message;

            try
            {
                message = MyAPIGateway.Utilities.SerializeFromBinary<UpdateCubeGridLogicsMessage>(data);
            }
            catch (Exception e)
            {
                Utils.Log("Comms::HandleChangeGridClassMessage deserialize message error", 3);
                Utils.LogException(e);
                return;
            }

            if (message.CubeGrids == null) return;
            foreach (var gridMessage in message.CubeGrids)
            {
                ModSessionManager.Instance.CubeGridLogics.Add(gridMessage.EntityId, new CubeGridLogic(gridMessage.GridClassId));
                var grid = MyAPIGateway.Entities.GetEntityById(gridMessage.EntityId) as IMyCubeGrid;
                if (grid == null) continue;
                ModSessionManager.Instance.ToBeInitialized.Enqueue(grid);
            }
        }

        private void HandleRequestCubeGrids(byte[] data)
        {
            ulong recipient;

            try
            {
                recipient = MyAPIGateway.Utilities.SerializeFromBinary<ulong>(data);
            }
            catch (Exception e)
            {
                Utils.Log("Comms::HandleChangeGridClassMessage deserialize message error", 3);
                Utils.LogException(e);
                return;
            }
            SendUpdateCubeGridLogics(ModSessionManager.Instance.CubeGridLogics, recipient);
        }
    }

    internal enum MessageType
    {
        ChangeGridClass,
        UpdateCubeGridLogics,
        RequestCubeGrids
    }

    [ProtoContract]
    internal struct Message
    {
        [ProtoMember(1)] public MessageType Type;
        [ProtoMember(2)] public byte[] Data;
    }

    [ProtoContract]
    internal struct GridMessage
    {
        [ProtoMember(1)] public long EntityId;
        [ProtoMember(2)] public long GridClassId;
    }

    [ProtoContract]
    internal struct UpdateCubeGridLogicsMessage
    {
        [ProtoMember(1)] public List<GridMessage> CubeGrids;
    }
}