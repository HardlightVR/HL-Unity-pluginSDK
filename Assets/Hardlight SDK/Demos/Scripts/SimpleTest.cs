using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	public class SimpleTest : MonoBehaviour
	{
		HapticSequence seq;
		HapticSequence pat;
		HapticSequence exp;

		void Start()
		{
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.G))
			{
				seq = HapticSequence.LoadFromAsset("Haptics/pulse");
			}
			if (Input.GetKeyDown(KeyCode.H))
			{
				seq.Play(AreaFlag.All_Areas);
			}
		}
	}
}