/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using NullSpace.SDK;
using NullSpace.SDK.Tracking;

namespace NullSpace.SDK.Demos
{
	public class TrackingTest : MonoBehaviour
	{
		private IImuCalibrator imus;
		public GameObject TrackedObject;
		public GameObject ParentObject;
		public bool DisableObject = true;
		public bool ShowOnGUI = false;

		void Start()
		{
			imus = NSManager.Instance.GetImuCalibrator();
			NSManager.Instance.SetImuCalibrator(GetComponent<DefaultImuCalibrator>());

			if (ParentObject != null)
			{
				ParentObject.SetActive(!DisableObject);
			}
		}

		void OnGUI()
		{
		
		}

		public void EnableTracking()
		{
			if (ParentObject != null)
			{
				ParentObject.SetActive(true);
			}
			NSManager.Instance.EnableTracking();
		}

		public void DisableTracking()
		{
			if (ParentObject != null)
			{
				ParentObject.SetActive(false);
			}
			NSManager.Instance.DisableTracking();
		}

		void Update()
		{
			if (TrackedObject != null)
			{
				TrackedObject.transform.rotation = imus.GetOrientation(Imu.Chest);
			}
		}
	}
}