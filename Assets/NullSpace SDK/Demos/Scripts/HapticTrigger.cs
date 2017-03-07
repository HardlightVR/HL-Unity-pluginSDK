/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;

namespace NullSpace.SDK.Demos
{
	/// <summary>
	/// Scene-specific script to trigger haptic effects
	/// </summary>
	public class HapticTrigger : MonoBehaviour
	{

		void Awake()
		{
		}

		void Start()
		{
			NSManager.Instance.DisableTracking();
		}
	}
	
		
}