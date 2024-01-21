//Sytems
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
//Sandboxs
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI;
using Sandbox.Definitions;
//Vrage
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using VRage.Library.Collections;
using VRage.Game.ModAPI.Network;
using VRage.Sync;

namespace Blues_Ship_Matrix
{
	public class BlueSync<T>
	{
		public T Value {get;private set;}
		private readonly ushort modID = 5015;
		public BlueSync()
		{
			MyAPIGateway.Multiplayer.RegisterMessageHandler(modID, MessageHandler);
			MyLog.Default.WriteLine("BlueSync: Registerd Handler");
		}
		
		private void MessageHandler(byte[] data)
		{
			MyLog.Default.WriteLine("BlueSync: Recived Message");
			try
			{
				var message = MyAPIGateway.Utilities.SerializeFromBinary<T>(data);
				MyLog.Default.WriteLine("BlueSync: Serialized From Binary");
				this.Value=message;
				MyLog.Default.WriteLine("BlueSync: Set Value");
			}
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"BlueSync: Error - {e.Message}");
            }
		}
		public void ValidateAndSet(T NewValue)
		{
			if(!MyAPIGateway.Multiplayer.MultiplayerActive){Value=NewValue;MyLog.Default.WriteLine("BlueSync: Multiplayer Offline Message Not Sent");return;}
			try
			{
				byte[] messageData;
				messageData=MyAPIGateway.Utilities.SerializeToBinary(NewValue);
				MyLog.Default.WriteLine("BlueSync: Serialized To Binary");
				if(MyAPIGateway.Utilities.IsDedicated){MyAPIGateway.Multiplayer.SendMessageToServer(modID, messageData);}
				if(MyAPIGateway.Multiplayer.IsServer){MyAPIGateway.Multiplayer.SendMessageToOthers(modID, messageData);}
				MyLog.Default.WriteLine("BlueSync: Sent Message");
			}
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"BlueSync: Error - {e.Message}");
            }
		}
	}
}