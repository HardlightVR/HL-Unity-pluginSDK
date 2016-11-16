/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/











using System.Collections.Generic;
using UnityEngine;
using NullSpace.API.Enums;

namespace NullSpace.API.Tracking
{
    /// <summary>
    /// This consumer is used to gather IMU data from the suit and update a collection of IMUs in real time.
    /// </summary>
	public class ImuConsumer : PacketConsumer
	{
		private Dictionary<Imu, IMU> imuDict;

        /// <summary>
        /// Create a new ImuConsumer
        /// </summary>
        /// <param name="imuDict">The dictionary in which to store IMU data</param>
		public ImuConsumer (Dictionary<Imu, IMU> imuDict)
		{
			this.imuDict = imuDict;
		}

        /// <summary>
        /// Consume a packet, extracting the quaternion information from within
        /// </summary>
        /// <param name="packet"></param>
		public void ConsumePacket (byte[] packet)
		{
			Imu id = (Imu) packet [11];
			if (imuDict.ContainsKey (id)) {
            //    Debug.Log("Packet Qyaternion: ");
              //  SerialAdapter.printByteArray(packet);
				Quaternion result = this.ParseQuaternion (packet);
				imuDict [id].Orientation = result;
			}
		}

		private Quaternion ParseQuaternion (byte[] rec)
		{
			float[] q = new float[4];
			q [0] = ((rec [2] << 8) | rec [3]) / 16384.0f;
			q [1] = ((rec [4] << 8) | rec [5]) / 16384.0f;
			q [2] = ((rec [6] << 8) | rec [7]) / 16384.0f;
			q [3] = ((rec [8] << 8) | rec [9]) / 16384.0f;
			for (int i = 0; i < 4; i++) {
				if (q [i] >= 2) {
					q [i] = -4 + q [i];
				}
			}
			return new Quaternion (q [3], q [0], q [1], q [2]);
		}
	}
}


