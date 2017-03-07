using UnityEngine;
using System.Collections;

using NullSpace.SDK;
using NullSpace.SDK.FileUtilities;
public class DemoHaptics : MonoBehaviour {

	HapticSequence jolt;
	// Use this for initialization
	void Start () {
		jolt = HapticResources.LoadSequence("Haptics/heartbeat");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(100, 100, 100, 100), "Play Haptic"))
		{
			jolt.Play(AreaFlag.All_Areas);

		}
	}
}
