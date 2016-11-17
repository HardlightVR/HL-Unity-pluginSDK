/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections.Generic;
using NullSpace.API.Enums;
using NullSpace.SDK;
namespace NullSpace.API.Tracking
{
    /// <summary>
    /// This interface provides a simple way of accessing data from the suit's inertial motion tracking units. 
    /// </summary>
	public class ImuInterface
	{
		private Dictionary<Imu, IMU> imuDict = new Dictionary<Imu, IMU>();
		private ImuConsumer consumer;
        /// <summary>
        /// The consumer associated with the ImuInterface, which is responsible for fetching data from the sensors. 
        /// </summary>
		public ImuConsumer ImuConsumer { get { return consumer; } }

        /// <summary>
        /// Construct a new ImuInterface, with default IMU sensors including Left Arm, Right Arm, and Chest. 
        /// </summary>
		public ImuInterface()
		{
			this.imuDict = new Dictionary<Imu, IMU>();

			//Make sure the consumer has something to put its data into
			consumer = new ImuConsumer(imuDict);
		}

   

        /// <summary>
        /// Fetch a list of IMUs tracked by this interface
        /// </summary>
        /// <returns>List of IMUs</returns>
		public List<IMU> GetImuList()
		{
			return new List<IMU>(this.imuDict.Values);
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

	
	}
}
