/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/


using UnityEngine;
using System.Collections;
using NullSpace.SDK;
using NullSpace.SDK.Enums;
using NullSpace.SDK.Tracking;

public class TrackingTest : MonoBehaviour {

	private IImuCalibrator imus;
	public GameObject TrackedObject;
	// Use this for initialization
	void Start () {
		imus = NSManager.Instance.GetImuCalibrator();
	}
	
	// Update is called once per frame
	void Update () {
		if (TrackedObject != null)
		{
			TrackedObject.transform.rotation = imus.GetOrientation(Imu.Chest);
		}
	}
}
