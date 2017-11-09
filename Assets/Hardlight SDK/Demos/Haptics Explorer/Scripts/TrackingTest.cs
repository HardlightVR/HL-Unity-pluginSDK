/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using Hardlight.SDK;
using Hardlight.SDK.Tracking;

namespace Hardlight.SDK.Tracking
{
	public class TrackingTest : MonoBehaviour
	{
		private IImuCalibrator imus;
		public GameObject TrackedRepresentation;
		public GameObject ParentObject;
		public Imu whichIMU = Imu.Chest;
		public bool DisableObject = true;
		public bool ShowOnGUI = false;
		public bool VisibleIdentity = false;
		public bool AutoEnableTracking = false;

		void Start()
		{
			imus = GetComponent<DefaultImuCalibrator>();
			HardlightManager.Instance.SetImuCalibrator(GetComponent<DefaultImuCalibrator>());

			if (ParentObject != null)
			{
				ParentObject.SetActive(!DisableObject);
			}

			if (AutoEnableTracking)
				EnableTracking();
		}

		public void EnableTracking()
		{
			if (ParentObject != null)
			{
				ParentObject.SetActive(true);
			}
			HardlightManager.Instance.EnableTracking();
		}

		public void DisableTracking()
		{
			if (ParentObject != null)
			{
				ParentObject.SetActive(false);
			}
			HardlightManager.Instance.DisableTracking();
		}

		void Update()
		{
			if (TrackedRepresentation != null)
			{
				var tracking = HardlightManager.Instance.PollTracking();
				//Debug.Log(tracking.Chest + "\n" + imus.GetType().ToString());
				Quaternion assign = Quaternion.identity;

				if (whichIMU == Imu.Chest)
				{
					assign = tracking.Chest;
				}
				else if (whichIMU == Imu.Left_Upper_Arm)
				{
					assign = tracking.LeftUpperArm;
				}
				else if (whichIMU == Imu.Right_Upper_Arm)
				{
					assign = tracking.RightUpperArm;
				}
				VisibleIdentity = assign != Quaternion.identity;

				TrackedRepresentation.transform.rotation = assign;// imus.GetOrientation(MyIMU);
			}
		}
	}
}