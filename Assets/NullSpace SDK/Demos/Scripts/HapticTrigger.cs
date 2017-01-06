/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/


using UnityEngine;

namespace NullSpace.SDK
{

	/// <summary>
	/// Scene-specific script to trigger haptic effects
	/// </summary>
	public class HapticTrigger : MonoBehaviour
	{

		Sequence bump;

		void Start()
		{
			bump = new Sequence("ns.bump");
		}

		void Update()
		{

		}

		void OnTriggerEnter(Collider collider)
		{
			bump.CreateHandle(collider.GetComponent<HapticCollider>().regionID).Play();
		}
	}
}