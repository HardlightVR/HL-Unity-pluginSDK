/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections.Generic;


namespace Hardlight.SDK.Tracking
{
	using System;
	using Quaternion = UnityEngine.Quaternion;

	public enum Imu { Chest, Left_Forearm, Left_Upper_Arm, Right_Forearm, Right_Upper_Arm };

	/// <summary>
	/// If you implement this interface and add your calibration script to the HardlightManager prefab object, 
	/// the SDK will 
	/// </summary>
	public interface IImuCalibrator
	{
		void ReceiveUpdate(TrackingUpdate update);
		Quaternion GetOrientation(Imu imu);
	}

	public class MockImuCalibrator : IImuCalibrator
	{
		public Quaternion GetOrientation(Imu imu)
		{
			return Quaternion.identity;
		}

		public void ReceiveUpdate(TrackingUpdate t)
		{
			//do nothing
		}
	}

	public class MockRotatingImuCalibrator : IImuCalibrator
	{
		private Quaternion orientation;
		private Quaternion target;
		public Quaternion GetOrientation(Imu imu)
		{
			orientation = Quaternion.RotateTowards(orientation, target, 2.5f * Time.deltaTime);
			if (Quaternion.Angle(orientation, target) < 2)
			{
				target = UnityEngine.Random.rotation;
			}

			return orientation;
		}

		public void ReceiveUpdate(TrackingUpdate t)
		{
			//do nothing
		}
	}

	public class CalibratorWrapper : IImuCalibrator
	{
		private IImuCalibrator _calibrator;
		public void SetCalibrator(IImuCalibrator c)
		{
			_calibrator = c;
		}
		public CalibratorWrapper(IImuCalibrator c)
		{
			_calibrator = c;
		}
		public Quaternion GetOrientation(Imu imu)
		{
			return _calibrator.GetOrientation(imu);
		}

		public void ReceiveUpdate(TrackingUpdate update)
		{
			_calibrator.ReceiveUpdate(update);
		}
	}
	/// <summary>
	///	Container for a quaternion representing the rotation of an IMU
	/// </summary>
	public class ImuOrientation
	{
		private Quaternion _orientation;
		public Quaternion Orientation
		{
			get
			{
				return _orientation;
			}

			set
			{
				_orientation = value;
			}
		}

		public ImuOrientation(Quaternion q)
		{
			Orientation = q;
		}
	}

	
}