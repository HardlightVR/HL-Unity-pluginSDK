using UnityEngine;
using NullSpace.API.Logger;

namespace NullSpace.SDK
{
	/// <summary>
	/// A simple class to keep track of the location ID of a collider
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
				Log.Warning("Haptic Collider " + regionID + " is not attached to a trigger volume.\n");
			}
		}

	}
}