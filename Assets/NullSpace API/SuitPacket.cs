/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;

namespace NullSpace.API
{
	/// <summary>
	/// Represents a packet produced by the suit.
	/// </summary>
	public class SuitPacket
	{

		public enum PacketType
		{
			/// <summary>
			/// Represents a packet containing IMU sensor quaternion data
			/// </summary>
			ImuData = 0x33,
			/// <summary>
			/// Represents a packet containing the status of an IMU sensor
			/// </summary>
			ImuStatus,
			/// <summary>
			/// Represents a packet containing health information for a motor
			/// </summary>
			DrvStatus = 0x15,
			/// <summary>
			/// Represents a ping update from the suit
			/// </summary>
			Ping = 0x02,
            FifoOverflow = 0x34,
			Undefined
		}

		private byte[] rawPacket;
		private PacketType packetType;
		public PacketType Type
		{
			get
			{
				return this.packetType;
			}
		}

		public byte[] RawPacket
		{
			get
			{
				return rawPacket;
			}
		}

		/// <summary>
		/// Construct a SuitPacket from the bytes making up the raw packet
		/// </summary>
		/// <param name="rawPacket"></param>
		public SuitPacket(byte[] rawPacket)
		{
			this.rawPacket = rawPacket;
			if (Enum.IsDefined(typeof(PacketType), (int) this.rawPacket[2]))
			{
				this.packetType = (PacketType) this.rawPacket[2];
			}
			else
			{
				this.packetType = PacketType.Undefined;
			}
		}


		/// <summary>
		/// Return a hex formatted string representing the packet
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string formattedString = "";
			foreach (byte b in rawPacket)
			{
				formattedString += b.ToString("X2") + " ";
			}
			return formattedString;
		}



	}

}

