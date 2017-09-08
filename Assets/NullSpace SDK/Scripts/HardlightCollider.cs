/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections.Generic;

namespace NullSpace.SDK
{
	/// <summary>
	/// A simple class to link up a collider with a certain area on the suit
	/// </summary>
	[RequireComponent(typeof(HapticLocation))]
	public class HardlightCollider : MonoBehaviour
	{
		//You can enable [EnumFlag] over [RegionFlag] if you're having problems with the more customized inspector.
		//Additionally, I provided EnumFlag for easy reference of a similar implementation
		//[EnumFlag]

		[Header("Note: Attach this to your player's body.", order = 0)]
		[Space(-10, order = 1)]
		[Header("You can select multiple areas for a single collider.", order = 2)]
		[Space(10, order = 3)]

		[Header("For this specific pad's collisions:", order = 4)]
		public Collider myCollider;
		[Header("If collider is null, performs a GetComponent", order = 5)]
		public bool TryFindCollider = false;

		[Header("Size of this location (for searching)", order = 5)]
		public float LocationSize = .025f;

		public bool LocationActive
		{
			get { return MyLocation.LocationActive; }
			set { MyLocation.LocationActive = value; }
		}

		public bool AutoCreateAdditionalPointsFromBounds = false;

		public AreaFlag regionID
		{
			get { return MyLocation.Where; }
			set { MyLocation.Where = value; }
		}

		public List<Vector3> AdditionalLocalPoints = new List<Vector3>();

		private HapticLocation _myLocation;
		public HapticLocation MyLocation
		{
			get
			{
				CheckMyLocation();
				return _myLocation;
			}
		}

		public Vector3 ObjectPosition
		{
			get { return MyLocation.transform.position; }
		}
		public GameObject Representation
		{
			get
			{
				return MyLocation.gameObject;
			}
		}
		void Awake()
		{
			//In case it isn't assigned or created by the BodySetup tool.
			if (TryFindCollider && myCollider == null)
			{
				//Find our collider
				myCollider = gameObject.GetComponent<Collider>();
			}
		}

		void Start()
		{
			CheckMyLocation();
			//If we tried to find the collider AND it was null.
			if (TryFindCollider && myCollider == null)
			{
				Debug.LogError("HardlightCollider does not have a collider set - Name [" + name + "] with regionID [" + regionID + "].\n");
			}
			else
			{
				if (AutoCreateAdditionalPointsFromBounds)
				{
					ProcessBounds();
				}

				//If we have a collider AND it isn't a trigger
				if (myCollider != null && !myCollider.isTrigger)
				{
					//Throw a warning, cause you don't want that.
					Debug.LogWarning("Haptic Collider " + regionID + " is not attached to a trigger volume.\n");
				}
			}

			//We don't want haptics on different layers. It isn't an error, but a good practice thing.
			if (gameObject.layer != NSManager.HAPTIC_LAYER)
			{
				Debug.LogWarning("You should aim to keep haptic content to Layer [" + NSManager.HAPTIC_LAYER + "].\n");
			}

		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.End))
			{
				ProcessBounds();
			}
		}

		void CheckMyLocation()
		{
			if (_myLocation == null)
			{
				HapticLocation loc = GetComponent<HapticLocation>();

				if (loc == null)
				{
					Debug.LogError("Haptic Location of " + name + " is null despite being a required component.\n\tYou should NEVER hit this. There is only one case I know of which involves recompiling after adding the RequiresComponent attribute.");
				}
				Debug.Assert(loc != null);
				_myLocation = loc;
			}
		}

		private void ProcessBounds()
		{
			//Collider must be enabled for bound extents to be non-zero.
			bool colliderWasEnabled = myCollider.enabled;
			myCollider.enabled = true;
			var src = myCollider.bounds.extents;
			var v3 = src;

			//We divide by 2 to put the extents between the center and the corner.
			AdditionalLocalPoints.Add(v3 / 2);
			AdditionalLocalPoints.Add(-v3 / 2);

			v3 = new Vector3(-src.x, src.y, src.z);
			AdditionalLocalPoints.Add(v3 / 2);
			AdditionalLocalPoints.Add(-v3 / 2);

			v3 = new Vector3(src.x, -src.y, src.z);
			AdditionalLocalPoints.Add(v3 / 2);
			AdditionalLocalPoints.Add(-v3 / 2);

			v3 = new Vector3(src.x, src.y, -src.z);
			AdditionalLocalPoints.Add(v3 / 2);
			AdditionalLocalPoints.Add(-v3 / 2);

			//Restore the collider's enabled state.
			myCollider.enabled = colliderWasEnabled;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.black - new Color(0, 0, 0, .2f);
			Gizmos.DrawSphere(transform.position, .01f);
			Gizmos.color = Color.cyan - new Color(0, 0, 0, .5f);
			//Gizmos.DrawSphere(transform.position, LocationSize);

			for (int i = 0; i < AdditionalLocalPoints.Count; i++)
			{
				//for (int k = 0; k < AdditionalLocalPoints.Count; k++)
				//{
				//	if (i != k)
				//	{
				//		Gizmos.DrawLine(transform.position + transform.rotation * AdditionalLocalPoints[i], transform.position + transform.rotation * AdditionalLocalPoints[k]);
				//	}
				//}
				//Gizmos.DrawSphere(transform.position + transform.rotation * AdditionalLocalPoints[i], .005f);
				//The Location Size for additional local points is cut in half.
				Gizmos.DrawSphere(transform.position + transform.rotation * AdditionalLocalPoints[i], LocationSize);
			}
		}
	}
}