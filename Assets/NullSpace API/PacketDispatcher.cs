/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using NullSpace.API.Logger;
using UnityEngine;

namespace NullSpace.API
{
    /// <summary>
    /// The PacketDispatcher reads data from the suit, constantly dispatching packets to consumers based on packet type
    /// </summary>
	public class PacketDispatcher
	{
		private Synchronizer synchronizer;
		private int dispatchLimit;
		//We need to quickly find which subscribers want access to a type of packet, so we can dispatch.
		private Dictionary<SuitPacket.PacketType, List<PacketConsumer>> typeToMonitors;

		public int total = 0;
		public string SyncState
		{
			get { return this.synchronizer.SyncState.ToString(); }
		}
		public PacketDispatcher (ByteQueue dataStream)
		{
			this.synchronizer = new Synchronizer(dataStream, this);
			
			this.typeToMonitors = new Dictionary<SuitPacket.PacketType, List<PacketConsumer>> ();
			this.dispatchLimit = 32;
		}

        /// <summary>
        /// Dispatch a single packet to consumers
        /// </summary>
        /// <param name="rawPacket"></param>
		public void Dispatch(byte[] rawPacket)
		{
			total++;
			SuitPacket packet = new SuitPacket(rawPacket);
			SuitPacket.PacketType packetType = packet.Type; 

			if (this.typeToMonitors.ContainsKey(packetType))
			{


                List<PacketConsumer> monitors = this.typeToMonitors[packetType];

                foreach (PacketConsumer monitor in monitors)
				{
					monitor.ConsumePacket(packet.RawPacket);
				}
			}
			else
			{
				Log.Message("No monitors for packet type {0}, packet is {1}", packetType, packet);

			}
		}

        /// <summary>
        /// Dispatch as many packets as are available, up to a limit
        /// </summary>
		public void DispatchAvailable ()
		{

			int packetsAttempted = 0;
			while (packetsAttempted < this.dispatchLimit)
			{
				this.synchronizer.TryReadPacket();
				packetsAttempted++;
			}
		}

        /// <summary>
        /// Register a PacketConsumer with the dispatcher
        /// </summary>
        /// <param name="monitor">The PacketConsumer to register</param>
        /// <param name="packetType">The PacketType that should be dispatched</param>
		public void AddSubscriber (PacketConsumer monitor, SuitPacket.PacketType packetType)
		{
			//Go ahead and add the subscriber if we already have a list for it
			if (typeToMonitors.ContainsKey(packetType)) {
				typeToMonitors[packetType].Add(monitor);
			} else {
				//Or we need to create the list first.
				typeToMonitors[packetType] = new List<PacketConsumer>();
				typeToMonitors[packetType].Add(monitor);
			}
		}

	
        /// <summary>
        /// Returns true if the packet stream is synchronized
        /// </summary>
		public bool Synchronized {
			get {
				return this.synchronizer.Synchronized;
			}
		}


	}
}


