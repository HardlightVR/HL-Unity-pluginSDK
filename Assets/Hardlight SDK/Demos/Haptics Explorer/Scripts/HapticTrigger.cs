/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// Scene-specific script to trigger haptic effects
	/// </summary>
	public class HapticTrigger : MonoBehaviour
	{
		public SuitDemo suitDemo;

		void OnTriggerEnter(Collider collider)
		{
			//If it is on the haptics layer
			if (collider.gameObject.layer == HardlightManager.HAPTIC_LAYER)
			{
				HardlightCollider hit = collider.GetComponent<HardlightCollider>();
				if (hit != null)
				{
					var handle = LibraryManager.Inst.LastSequence.CreateHandle(hit.regionID);
					handle.Play();
					if (suitDemo)
					{
						suitDemo.DisplayHandleHaptic(hit, handle);
					}
					else
					{
						Debug.LogError("The Suit Demo of HapticTrigger [" + name + "] is not assigned\n", this);
					}
				}
			}
		}
	}
}