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
	/// A simple class to link up a collider with a certain area on the suit
	/// </summary>
	public class HapticCollider : MonoBehaviour
	{
		[Header("Note: Region ID is set during runtime")]
		public AreaFlag regionID;
		public Collider myCollider;

		void Awake()
		{
			if (myCollider == null)
			{
				myCollider = gameObject.GetComponent<Collider>();
			}
		}

		void Start()
		{
			if (!myCollider.isTrigger)
			{
				Debug.LogWarning("Haptic Collider " + regionID + " is not attached to a trigger volume.\n");
			}
		}

	}
}