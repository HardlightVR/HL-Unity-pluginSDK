/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using System.Collections.Generic;
using UnityEngine;
using NullSpace.API.Enums;
using System;
namespace NullSpace.API
{
	public class SuitInfo
	{
		private int _majorVersion;
		private int _minorVersion;

		public int MajorVersion
		{
			get { return _majorVersion; }
		}

		public int MinorVersion
		{
			get { return _minorVersion; }
		}

		public SuitInfo(int major, int minor)
		{
			_majorVersion = major;
			_minorVersion = minor;
		}

		public override string ToString()
		{
			return string.Format("NullSpace Suit Mark {0} Version {1}", _majorVersion, _minorVersion);
		}

	}
	/// <summary>
	/// This consumer is used to gather IMU data from the suit and update a collection of IMUs in real time.
	/// </summary>
	public class SuitInfoConsumer : PacketConsumer
	{

		private Action<SuitInfo> _callback;

        /// <summary>
        /// Create a new ImuConsumer
        /// </summary>
        /// <param name="imuDict">The dictionary in which to store IMU data</param>
		public SuitInfoConsumer(Action<SuitInfo> callback)
		{
			_callback = callback;
		}

        /// <summary>
        /// Consume a packet, extracting the quaternion information from within
        /// </summary>
        /// <param name="packet"></param>
		public void ConsumePacket (byte[] packet)
		{
			Debug.Log("Got info");
			//Imu id = (Imu) packet [11];
			var major = packet[3];
			var minor = packet[4];
			_callback(new SuitInfo(major, minor));

		}

	}
}


