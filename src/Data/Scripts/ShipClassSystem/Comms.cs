using System;
using ProtoBuf;
using Sandbox.ModAPI;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    internal class Comms
    {
        private readonly ushort _commsId;

        public Comms(ushort id)
        {
            _commsId = id;
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(_commsId, MessageHandler);
            if (Constants.IsServer) MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(_commsId, MessageHandler);
        }

        public void SendChangeGridClassMessage(long entityId, long gridClassId)
        {
            try
            {
                var messageData = MyAPIGateway.Utilities.SerializeToBinary(new ChangeGridClassMessage
                    { EntityId = entityId, GridClassId = gridClassId });
                var message = MyAPIGateway.Utilities.SerializeToBinary(new Message
                    { Type = MessageType.ChangeGridClass, Data = messageData });
                Utils.Log($"Comms::SendChangeGridClassMessage sending message to server {entityId}, {gridClassId}", 1);
                MyAPIGateway.Multiplayer.SendMessageToServer(_commsId, message);
            }
            catch (Exception e)
            {
                Utils.Log("Comms::SendChangeGridClassMessage error", 3);
                Utils.LogException(e);
            }
        }

        private void MessageHandler(ushort handlerId, byte[] data, ulong playerId, bool unknown)
        {
            if (!Constants.IsServer)
            {
                Utils.Log("Comms::MessageHandler error, non-server recieved message", 3);
                return;
            }

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
                    HandleChangeGridClassMessage(message.Data);
                    break;
                default:
                    Utils.Log("Comms::MessageHandler: Unknown message type", 2);
                    break;
            }
        }

        private void HandleChangeGridClassMessage(byte[] data)
        {
            ChangeGridClassMessage message;

            try
            {
                message = MyAPIGateway.Utilities.SerializeFromBinary<ChangeGridClassMessage>(data);
            }
            catch (Exception e)
            {
                Utils.Log("Comms::HandleChangeGridClassMessage deserialize message error", 3);
                Utils.LogException(e);
                return;
            }

            var gridLogic = CubeGridLogic.GetCubeGridLogicByEntityId(message.EntityId);

            if (gridLogic != null)
            {
                if (ModSessionManager.IsValidGridClass(message.GridClassId))
                {
                    Utils.Log(
                        $"Comms::HandleChangeGridClassMessage: Setting grid class id for {message.EntityId} to {message.GridClassId}",
                        1);
                    gridLogic.GridClassId = message.GridClassId;
                }
                else
                {
                    Utils.Log($"Comms::HandleChangeGridClassMessage: Unknown grid class ID {message.GridClassId}", 3);
                }
            }
            else
            {
                Utils.Log($"Comms::HandleChangeGridClassMessage: grid missing gridLogic, {message.EntityId}", 3);
            }
        }
    }

    internal enum MessageType
    {
        ChangeGridClass
    }

    [ProtoContract]
    internal struct Message
    {
        [ProtoMember(1)] public MessageType Type;
        [ProtoMember(2)] public byte[] Data;
    }

    [ProtoContract]
    internal struct ChangeGridClassMessage
    {
        [ProtoMember(1)] public long EntityId;
        [ProtoMember(2)] public long GridClassId;
    }
}