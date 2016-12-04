using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NullSpace.SDK.Enums;
using UnityEngine;
namespace NullSpace.SDK.Tracking {
	using Quaternion = UnityEngine.Quaternion;

	/// <summary>
	/// A stripped down interface which acts as a converter between raw suit orientation data and calibrated data.
	/// Note: This particular interface is for demonstration purposes and does no calibration, only providing access to raw data.
	/// </summary>
	public class DefaultImuCalibrator : MonoBehaviour, IImuCalibrator
	{
		/// <summary>
		/// We will store the incoming data in a dictionary. We wrap the quaternions in a structure which could be
		/// augmented with information and/or delegates specific to calibrating particular IMUs
		/// </summary>
		private IDictionary<Imu, ImuOrientation> _rawQuaternions;

		/// <summary>
		/// Calibrated data will be stored in a dictionary
		/// </summary>
		private IDictionary<Imu, Quaternion> _processedQuaternions;


		public void Awake()
		{
			_rawQuaternions = new Dictionary<Imu, ImuOrientation>();
			_processedQuaternions = new Dictionary<Imu, Quaternion>();

			foreach (Imu imu in Enum.GetValues(typeof(Imu))) {
				_processedQuaternions[imu] = new Quaternion();
				_rawQuaternions[imu] = new ImuOrientation(Quaternion.identity);
			}

		}

		public Quaternion GetOrientation(Imu imu)
		{
			return _processedQuaternions[imu];	
		}
		
		public void ReceiveUpdate(TrackingUpdate update)
		{
			_rawQuaternions[Imu.Chest].Orientation = update.Chest;
			_rawQuaternions[Imu.Left_Forearm].Orientation = update.LeftForearm;
			_rawQuaternions[Imu.Left_Upper_Arm].Orientation = update.LeftUpperArm;
			_rawQuaternions[Imu.Right_Forearm].Orientation = update.RightForearm;
			_rawQuaternions[Imu.Right_Upper_Arm].Orientation = update.RightUpperArm;
		}

		/// <summary>
		/// Every frame, do something with the data. In this case we simply copy from the raw data into the 
		/// processed data.
		/// </summary>
		public void Update()
		{
			_processedQuaternions[Imu.Chest] = _rawQuaternions[Imu.Chest].Orientation;
		}
	}
}
