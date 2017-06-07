using UnityEngine;
using System.Collections;
using NullSpace.SDK.Demos;

namespace NullSpace.SDK.Demos
{
	public class RepeatedHaptic : MonoBehaviour
	{
		public bool AllowActivation = true;
		public float ResetTimer;
		public float DefaultResetTime;
		public bool Running = true;
		public HapticHandle MyHaptic;
		public void ActivateHaptic()
		{
			ActivateHaptic(DefaultResetTime);
		}

		public void ActivateHaptic(float AlternateResetTime)
		{
			//Play the haptic

			//Set the reset timer to Proc
		}

		void Update()
		{
			if (Running)
			{
				ResetTimer -= Time.deltaTime;

				if (ResetTimer <= 0 && AllowActivation)
				{
					ActivateHaptic();
				}

				ResetTimer = Mathf.Clamp(ResetTimer, 0, float.MaxValue);
			}
			
		}

		public static RepeatedHaptic AddRepeatedHaptic(GameObject target, HapticHandle handle)
		{
			RepeatedHaptic repeatHaptic = target.AddComponent<RepeatedHaptic>();

			return repeatHaptic;
		}

		public static void RemoveAllRepeatedHaptic(GameObject target, HapticHandle handle)
		{
			RepeatedHaptic repeatHaptic = target.GetComponent<RepeatedHaptic>();
			if (repeatHaptic != null)
			{
				Destroy(repeatHaptic);
			}
		}
	}
}