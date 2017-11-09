using UnityEngine;
using Hardlight.SDK;

public class DemoHaptics : MonoBehaviour {

	HapticSequence heartbeat;

	void Start () {
		heartbeat = HapticSequence.LoadFromAsset("Haptics/heartbeat");
	}
	


	private void OnGUI()
	{
		if (GUI.Button(new Rect(100, 100, 100, 100), "Play Heartbeat"))
		{
			heartbeat.Play(AreaFlag.Chest_Left);
		}
	}
}
