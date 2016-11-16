using UnityEngine;
using System.Collections;
using NullSpace.SDK;
using NullSpace.API.Enums;
using NullSpace.API.Tracking;

public class TrackingTest : MonoBehaviour {

	private ImuInterface imus;
	public GameObject TrackedObject;
	// Use this for initialization
	void Start () {
		imus = NSManager.Instance.GetImuInterface();
	}
	
	// Update is called once per frame
	void Update () {
		if (TrackedObject != null)
		{
			TrackedObject.transform.rotation = imus.GetOrientation(Imu.Chest);
		}
	}
}
