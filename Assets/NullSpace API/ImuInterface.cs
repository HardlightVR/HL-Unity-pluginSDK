/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections.Generic;
using NullSpace.API.Enums;

namespace NullSpace.API.Tracking
{
    /// <summary>
    /// This interface provides a simple way of accessing data from the suit's inertial motion tracking units. 
    /// </summary>
	public class ImuInterface
	{
		private List<IMU> imuList;
		private Dictionary<Imu, IMU> imuDict = new Dictionary<Imu, IMU>();
		private ImuConsumer consumer;
        /// <summary>
        /// The consumer associated with the ImuInterface, which is responsible for fetching data from the sensors. 
        /// </summary>
		public ImuConsumer ImuConsumer { get { return consumer; } }
		private int currentImu = 0;

        /// <summary>
        /// Construct a new ImuInterface, with default IMU sensors including Left Arm, Right Arm, and Chest. 
        /// </summary>
		public ImuInterface()
		{
			this.imuDict = new Dictionary<Imu, IMU>();
			this.imuList = new List<IMU>();


            this.AddImu(new IMU(Imu.Left_Forearm, "Left Arm"));
            this.AddImu(new IMU(Imu.Right_Forearm, "Right Arm"));
            this.AddImu(new IMU(Imu.Chest, "Chest"));

			//Make sure the consumer has something to put its data into
			consumer = new ImuConsumer(imuDict);
		}

        /// <summary>
        /// Unstable
        /// </summary>
        /// <param name="adapter"></param>
		public void RequestImuData(ICommunicationAdapter adapter)
		{
			byte[] writeMess = new byte[8] { 0x24, 0x02, 0x33, 0x08, (byte)imuList[currentImu].Id, 0x00, 0x00, 0x0A };
			adapter.Write(writeMess);
			currentImu = (currentImu + 1) % imuList.Count;
		}

        /// <summary>
        /// Fetch a list of IMUs tracked by this interface
        /// </summary>
        /// <returns>List of IMUs</returns>
		public List<IMU> GetImuList()
		{
			return this.imuList;
		}

        /// <summary>
        /// Unstable
        /// </summary>
        /// <param name="imu"></param>
        /// <param name="offset"></param>
		public void SetOffset(Imu imu, Quaternion offset)
		{
			if (imuDict.ContainsKey(imu))
			{
				imuDict[imu].Offset = offset;
			}
		}

        /// <summary>
        /// Retrieve the orientation of the requested IMU
        /// </summary>
        /// <param name="which">The IMU id</param>
        /// <returns></returns>
		public Quaternion GetOrientation(Imu which)
		{

			if (imuDict.ContainsKey(which))
			{
				Quaternion thisOrientation = imuDict[which].Orientation;
              //  Debug.Log(thisOrientation);
				return Quaternion.Inverse(thisOrientation) * imuDict[which].Offset;
			}
			else
			{
				Debug.Log("Could not find IMU with key " + which.ToString() + ", returning identity quaternion");
				return Quaternion.identity;
			}
		}

		private void AddImu(IMU imu)
		{
			//This does not check if stuff is already in array or hashtable
			imuList.Add(imu);
			imuDict.Add(imu.Id, imu);
		}
	}
}
