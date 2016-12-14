/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/


using UnityEngine;
using NullSpace.SDK;
using NullSpace.SDK.Tracking;

public class TrackingTest : MonoBehaviour
{

	private IImuCalibrator imus;
	public GameObject TrackedObject;


	void Start()
	{

		imus = NSManager.Instance.GetImuCalibrator();
		NSManager.Instance.SetImuCalibrator(GetComponent<DefaultImuCalibrator>());

	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(25, 25, 120, 80), "Enable Tracking"))
		{
			NSManager.Instance.EnableTracking();
		}
		if (GUI.Button(new Rect(25, 110, 120, 80), "Disable Tracking"))
		{
			NSManager.Instance.DisableTracking();
		}
	}
	void Update()
	{
		if (TrackedObject != null)
		{
			TrackedObject.transform.rotation = imus.GetOrientation(Imu.Chest);
		}
	}
}
