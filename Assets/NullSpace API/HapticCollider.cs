/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using UnityEngine;
using NullSpace.API.Enums;
using NullSpace.API.Logger;

namespace NullSpace.SDK
{
	/// <summary>
	/// A simple class to keep track of the location ID of a collider
	/// </summary>
	public class HapticCollider : MonoBehaviour
	{
		[Header("Note: Region ID is set during runtime")]
		public Location regionID;

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
				Log.Warning("Haptic Collider " + regionID + " is not attached to a trigger volume.\n");
			}
		}

	}
}