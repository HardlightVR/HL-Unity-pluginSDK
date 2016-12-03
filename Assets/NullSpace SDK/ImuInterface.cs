/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections.Generic;
using NullSpace.SDK.Enums;

namespace NullSpace.SDK.Tracking
{
	using Quaternion = UnityEngine.Quaternion;

	/// <summary>
	/// If you implement this interface and add your calibration script to the NSManager prefab object, 
	/// the SDK will 
	/// </summary>
	public interface IImuCalibrator
	{
		void ReceiveUpdate(Imu imu, Quaternion rotation);
		Quaternion GetOrientation(Imu imu);
	}
	/// <summary>
	///	Container for a quaternion representing the rotation of an IMU
	/// </summary>
	public class RawIMU
	{
		public Quaternion Orientation;

		public RawIMU(Quaternion q)
		{
			Orientation = q;
		}
	}

	/// <summary>
	/// A stripped down interface which acts as a converter between raw suit orientation data and calibrated data.
	/// This interface does no calibration.
	/// </summary>
	public class ImuInterface : MonoBehaviour, IImuCalibrator
	{

		private IDictionary<Imu, RawIMU> _rawImuQuaternions;
		private IDictionary<Imu, Quaternion> _processedQuaternions;

		public IDictionary<Imu, Quaternion> Orientations
		{
			get { return _processedQuaternions; }
		}

	


		public void Awake()
		{
			_rawImuQuaternions = new Dictionary<Imu, RawIMU>();
			_processedQuaternions = new Dictionary<Imu, Quaternion>();

		}

		public Quaternion GetOrientation(Imu imu)
		{
			if (_processedQuaternions.ContainsKey(imu))
			{
				return _processedQuaternions[imu];
			}
			else
			{
				return Quaternion.identity;
			}
		}
		public void Start()
		{
			NSManager.Instance.UseImuCalibrator(this);

		}
		public void ReceiveUpdate(Imu key, Quaternion q)
		{
			if (_rawImuQuaternions.ContainsKey(key))
			{
				_rawImuQuaternions[key].Orientation = q;
			}
			else
			{
				_rawImuQuaternions[key] = new RawIMU(q);
			}
		}
		public void Update()
		{
			_processedQuaternions[Imu.Chest] = _rawImuQuaternions[Imu.Chest].Orientation;
		}
	}
}