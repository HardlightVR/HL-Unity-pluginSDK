using UnityEngine;
using NullSpace.SDK;

public class DemoHaptics : MonoBehaviour {

	HapticSequence heartbeat = new HapticSequence();

	void Start () {
		heartbeat.LoadFromAsset("Haptics/heartbeat");
	}
	


	private void OnGUI()
	{
		if (GUI.Button(new Rect(100, 100, 100, 100), "Play Heartbeat"))
		{
			heartbeat.Play(AreaFlag.Chest_Left);
		}
	}
}
