/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using NullSpace.API.Logger;
namespace NullSpace.API
{
    /// <summary>
    /// Synchronizer keeps track of the suit packet stream, re-syncing when necessary
    /// </summary>
	public class Synchronizer
	{
		public enum State
		{
			Synchronized,
			SearchingForSync,
			ConfirmingSync,
			ConfirmingSyncLoss
		}
		private State syncState;
		//Raw ring-buffer which is populated by suit
		private ByteQueue dataStream;
		//Is the packet stream synchronized?
		public bool Synchronized
		{
			get { return this.syncState == State.Synchronized; }
		}

		public State SyncState
		{
			get { return this.syncState; }
		}

		private int packetLength;
		private char packetDelimeter;
        #pragma warning disable
        private byte[] packetFooter;
        #pragma warning restore

        //What is the maximum packets we can read at once?
        private int badSyncCounter;

		private const int BAD_SYNC_LIMIT = 2;
		private PacketDispatcher dispatcher;

		public Synchronizer(ByteQueue dataStream, PacketDispatcher dispatcher) {
			this.dispatcher = dispatcher;
			this.dataStream = dataStream;
			this.packetLength = 16;
			this.packetDelimeter = '$';
			//unused for now
			this.packetFooter = new byte[] { 0x0D, 0x0A };

            this.syncState = State.SearchingForSync;
			this.badSyncCounter = 0;
		}

        /// <summary>
        /// Attempt to read a packet from the raw stream
        /// </summary>
		public void TryReadPacket()
		{

			if (this.dataStream.Length < this.packetLength)
			{
				return;
			}

			switch (syncState)
			{

				case State.SearchingForSync:
					this.searchForSync();
					break;
				case State.ConfirmingSync:
					this.confirmSync();
					break;
				case State.Synchronized:
					this.monitorSync();
					break;
				case State.ConfirmingSyncLoss:
					this.confirmSyncLoss();
					break;
				default:
					break;
			}
		}

		private string byteArrayToString(byte[] arr)
		{
			string formattedString = "";
			foreach (byte b in arr)
			{
				formattedString += b.ToString("X2") + " ";
			}
			return formattedString;
		}

		private void searchForSync() {
			if (this.dataStream.Length < this.packetLength * 2) {
				return;
			}
			byte[] possiblePacket = DequeuePacket();
			if (possiblePacket[0] == this.packetDelimeter &&
                possiblePacket[14] == this.packetFooter[0] && 
                possiblePacket[15] == this.packetFooter[1])
			{
				this.syncState = State.ConfirmingSync;
				return;
			}
         //   SerialAdapter.printByteArray(possiblePacket);
            


            //Attempt to find where the packet begins
            for (int offset = 1; offset < this.packetLength; offset++)
			{
				if (possiblePacket[offset] == this.packetDelimeter)
				{
					int howMuchLeft = this.packetLength - offset;
					//don't care about possiblePacket in the Dequeue call here
					this.dataStream.Dequeue(possiblePacket, 0, howMuchLeft);
					this.syncState = State.ConfirmingSync;
					return;
				}
			}	
			
		}

		private void confirmSync() {
			byte[] possiblePacket = DequeuePacket();
			if (possiblePacket[0] == this.packetDelimeter &&
                possiblePacket[14] == this.packetFooter[0] &&
                possiblePacket[15] == this.packetFooter[1])
			{
				this.syncState = State.Synchronized;
			}
			else
			{
				this.syncState = State.SearchingForSync;
			}
        }

		private void monitorSync() {
			byte[] possiblePacket = DequeuePacket();
			if (possiblePacket[0] != this.packetDelimeter)
			{
				this.badSyncCounter = 1;
				this.syncState = State.ConfirmingSyncLoss;
			}
			else
			{
				this.dispatcher.Dispatch(possiblePacket);
			}

		}

		private void confirmSyncLoss() {
			byte[] possiblePacket = DequeuePacket();
			if (possiblePacket[0] != this.packetDelimeter)
			{
				this.badSyncCounter++;
				if (this.badSyncCounter >= BAD_SYNC_LIMIT)
				{
					this.syncState = State.SearchingForSync;
				}
			}
			else
			{
				this.syncState = State.Synchronized;
			}
		}
		private byte[] DequeuePacket() {
			byte[] possiblePacket = new byte[this.packetLength];
			this.dataStream.Dequeue(possiblePacket, 0, this.packetLength);
			return possiblePacket;
		}
	}
}
