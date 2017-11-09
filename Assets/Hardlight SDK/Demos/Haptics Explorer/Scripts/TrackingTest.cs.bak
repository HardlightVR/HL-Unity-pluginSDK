/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using Hardlight.SDK;
using Hardlight.SDK.Tracking;

namespace Hardlight.SDK.Demos
{
	public class TrackingTest : MonoBehaviour
	{
		private IImuCalibrator imus;
		public GameObject TrackedObject;
		public GameObject ParentObject;
		public Imu MyIMU = Imu.Chest;
		public bool DisableObject = true;
		public bool ShowOnGUI = false;

		void Start()
		{
			imus = HardlightManager.Instance.GetImuCalibrator();
			HardlightManager.Instance.SetImuCalibrator(GetComponent<DefaultImuCalibrator>());

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
			if (TrackedObject != null)
			{
				var tracking = HardlightManager.Instance.PollTracking();
				TrackedObject.transform.rotation = imus.GetOrientation(MyIMU);
			}
		}
	}
}