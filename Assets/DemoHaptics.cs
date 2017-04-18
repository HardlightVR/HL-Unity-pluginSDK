using UnityEngine;
using NullSpace.SDK;

public class DemoHaptics : MonoBehaviour {

	HapticSequence jolt = new HapticSequence();

	void Start () {
		jolt.LoadFromAsset("Haptics/heartbeat");
	}
	


	private void OnGUI()
	{
		if (GUI.Button(new Rect(100, 100, 100, 100), "Play Haptic"))
		{
			jolt.Play(AreaFlag.All_Areas);
		}
	}
}
