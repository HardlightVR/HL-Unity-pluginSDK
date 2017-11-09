using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// This tests to see that DirectMode is switching back to Retained Mode (standard use case for file based effects and code based effects)
	/// This class is being kept for its ability to regression test the auto-mode switching
	/// </summary>
	public class ModeSwitchTest : MonoBehaviour
	{
		public HapticCurve curve;
		void Start()
		{
			seq = HapticSequence.LoadFromAsset("Haptics/double_click");
			//By using a double click we can distinctly hear the difference.

			//Play it on load to make sure it loaded.
			seq.Play(AreaFlag.All_Areas);
		}

		HapticSequence seq;
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				StartCoroutine(DirectToRetainSwitchTest());
			}
		}

		/// <summary>
		/// This should sound like the curve and then a double click after N seconds
		/// </summary>
		/// <returns>This is a coroutine.</returns>
		IEnumerator DirectToRetainSwitchTest(float interruptAtTime = 1.5f)
		{
			//Start the curve
			curve.Play();

			yield return new WaitForSeconds(interruptAtTime);
			
			//Stop the curve
			curve.Stop();

			//Immediately play the file haptic. This should trigger a switch to Retained Mode.
			seq.Play(AreaFlag.All_Areas);
		}
	}
}