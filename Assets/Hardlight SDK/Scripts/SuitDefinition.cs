using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Hardlight.SDK
{
	public class SuitDefinition : ScriptableObject
	{
		[SerializeField]
		public string SuitName = "Player Body";
		[SerializeField]
		public GameObject SuitRoot;
		public bool HasRoot
		{
			get { return SuitRoot != null; }
		}

		[SerializeField]
		public List<AreaFlag> DefinedAreas;

		//The Game Objects to fill the fields (which will get hardlight collider references)
		[SerializeField]
		public List<GameObject> ZoneHolders;

		//the objects added. Will get a nice button list to quick get to each of them.
		[SerializeField]
		public List<HardlightCollider> SceneReferences;

		Dictionary<HardlightCollider, float> Distances = new Dictionary<HardlightCollider, float>();

		[SerializeField]
		public int HapticsLayer = HardlightManager.HAPTIC_LAYER;
		[SerializeField]
		public bool AddChildObjects = true;
		[SerializeField]
		public bool AddExclusiveTriggerCollider = true;

		/// <summary>
		/// A value for filtering out specific regions.
		/// Ex: Your player's health is a heartbeat haptic effect, therefore you never want to play anything else on that pad.
		/// You would add the Chest_Left to the FilterFlag.
		/// </summary>
		[SerializeField]
		[Header("Disabled Regions")]
		public FilterFlag _disabledRegions;
		public FilterFlag DisabledRegions
		{
			get
			{
				if (_disabledRegions == null)
				{
					//Make a new one.
					_disabledRegions = new FilterFlag();
				}
				return _disabledRegions;
			}
			set { _disabledRegions = value; }
		}

		#region Setup and Initialization
		public void SetupDistanceDictionary()
		{
			if (Application.isPlaying)
			{
				for (int i = 0; i < SceneReferences.Count; i++)
				{
					if (SceneReferences[i] != null)
					{
						Distances.Add(SceneReferences[i], float.MaxValue);
					}
				}
			}
		}

		public void Init()
		{
			SetDefaultAreas();

			SetupParents();

			GenerateSceneReferences();

			CollapseValidAreasForRuntime();
		}

		public void CollapseValidAreasForRuntime()
		{
			if (Application.isPlaying)
			{
				for (int i = SceneReferences.Count - 1; i > -1; i--)
				{
					if (SceneReferences[i] == null)
					{
						SceneReferences.RemoveAt(i);
						ZoneHolders.RemoveAt(i);
						DefinedAreas.RemoveAt(i);
					}
				}
			}
		}

		public void SetDefaultAreas()
		{
			if (DefinedAreas == null || DefinedAreas.Count == 0)
			{
				DefinedAreas = new List<AreaFlag>();

				DefinedAreas.Add(AreaFlag.Forearm_Left);
				DefinedAreas.Add(AreaFlag.Upper_Arm_Left);

				DefinedAreas.Add(AreaFlag.Shoulder_Left);
				DefinedAreas.Add(AreaFlag.Back_Left);
				DefinedAreas.Add(AreaFlag.Chest_Left);

				DefinedAreas.Add(AreaFlag.Upper_Ab_Left);
				DefinedAreas.Add(AreaFlag.Mid_Ab_Left);
				DefinedAreas.Add(AreaFlag.Lower_Ab_Left);

				DefinedAreas.Add(AreaFlag.Forearm_Right);
				DefinedAreas.Add(AreaFlag.Upper_Arm_Right);

				DefinedAreas.Add(AreaFlag.Shoulder_Right);
				DefinedAreas.Add(AreaFlag.Back_Right);
				DefinedAreas.Add(AreaFlag.Chest_Right);

				DefinedAreas.Add(AreaFlag.Upper_Ab_Right);
				DefinedAreas.Add(AreaFlag.Mid_Ab_Right);
				DefinedAreas.Add(AreaFlag.Lower_Ab_Right);
			}
		}

		public void SetupParents()
		{
			if (ZoneHolders == null || ZoneHolders.Count == 0)
			{
				//Debug.Log("Resetting Suit Holders\n");
				ZoneHolders = new List<GameObject>();

				for (int i = 0; i < DefinedAreas.Count; i++)
				{
					ZoneHolders.Add(null);
				}
			}
		}

		public void GenerateSceneReferences()
		{
			if (SceneReferences == null || SceneReferences.Count == 0)
			{
				//Debug.Log("Resetting Filled Areas\n");
				SceneReferences = new List<HardlightCollider>();

				for (int i = 0; i < DefinedAreas.Count; i++)
				{
					SceneReferences.Add(null);
				}
			}
		}

		public int CountValidZoneHolders()
		{
			int count = 0;
			for (int i = 0; i < ZoneHolders.Count; i++)
			{
				if (ZoneHolders[i] != null)
				{
					count++;
				}
			}

			return count;
		}
		#endregion

		#region Location Functions
		/// <summary>
		/// 
		/// </summary>
		/// <param name="start">World space start point for spherecast</param>
		/// <param name="direction">A unit vector</param>
		/// <param name="sphereCastRadius"></param>
		/// <param name="sphereCastLength"></param>
		public HardlightCollider[] FindCollidersWithinSphereCast(Vector3 start, Vector3 direction, float sphereCastRadius, float sphereCastLength = 100)
		{
			List<HardlightCollider> inRangeOfLine = new List<HardlightCollider>();

			Vector3 A, B;
			A = start;
			B = start + direction.normalized * sphereCastLength;

			for (int i = 0; i < SceneReferences.Count; i++)
			{
				//This is the actual spherecast math operation (sphere to points collision)
				bool hit = IsHardlightColliderIsInRangeOfLine(SceneReferences[i], sphereCastRadius, A, B);
				if (hit)
				{
					inRangeOfLine.Add(SceneReferences[i]);
				}
			}
			return inRangeOfLine.ToArray();
		}

		//This is a 'Distance Point is from Line' calculation. Ref: http://www.r-5.org/files/books/computers/algo-list/realtime-3d/Christer_Ericson-Real-Time_Collision_Detection-EN.pdf (Pg 128/129)
		//Short idea is: Have Line A->B, Want distance Point C is from AB. Get vector from A to B. Use dot products to project point C onto the line. This point will be D (closest point on line AB to point C). Then you find the distance (or sqrMagnitude for cheaper calculation cost) of C->D
		//We specifically care if any relevant points to the collider are within range of the line AB.
		/// <summary>
		/// Checks if a collider (or its additional points) are within range of the line AB
		/// </summary>
		/// <param name="collider">The data object for the suit item</param>
		/// <param name="range">Radius of line AB</param>
		/// <param name="A">Start for line AB</param>
		/// <param name="B">End for line AB</param>
		private bool IsHardlightColliderIsInRangeOfLine(HardlightCollider collider, float range, Vector3 A, Vector3 B)
		{
			Vector3 checkedPosition;
			Vector3 ClosestPoint = Vector3.one * 10000;
			checkedPosition = collider.transform.position;
			Vector3 AB = B - A;
			float t = Vector3.Dot(checkedPosition - A, AB) / Vector3.Dot(AB, AB);
			ClosestPoint = A + t * AB;

			bool betweenPoints = false, hit = false;

			float SpherecastSizeAndLocationSize = collider.DefaultLocationSize + range;
			bool withinRadiusOfLine = CheckIfObjectWithinSquaredDistanceOfPoint(checkedPosition, ClosestPoint, SpherecastSizeAndLocationSize);
			if (t > 0 && t < 1f)
			{
				betweenPoints = true;
			}

			if (withinRadiusOfLine && betweenPoints)
			{
				hit = true;
			}

			//Hardlight Colliders feature additional local points which allows for cheaper calculation
			for (int i = 0; i < collider.BubbleCollisionPoints.Count; i++)
			{
				var current = collider.BubbleCollisionPoints[i];
				if (!hit)
				{
					checkedPosition = current.transform.position;
					withinRadiusOfLine = CheckIfObjectWithinSquaredDistanceOfPoint(checkedPosition, ClosestPoint, SpherecastSizeAndLocationSize);

					betweenPoints = false;
					if (t > 0 && t < 1f)
					{
						betweenPoints = true;
					}

					if (withinRadiusOfLine && betweenPoints)
					{
						hit = true;
					}
					if (hit)
					{
						Debug.DrawLine(checkedPosition, ClosestPoint, Color.green);
					}
					else
					{
						Debug.DrawLine(ClosestPoint, ClosestPoint - (ClosestPoint - checkedPosition).normalized * range, Color.red);
						Debug.DrawLine(checkedPosition, checkedPosition + (ClosestPoint - checkedPosition).normalized, Color.blue);
					}
				}
			}
#if UNITY_EDITOR
			if (hit)
			{
				Debug.DrawLine(checkedPosition, ClosestPoint, Color.green);
			}
#endif

			return hit;
		}

		/// <summary>
		/// Basically Vector3.Distance that avoids the square root (which is more expensive than squaring the distance you're checking again)
		/// </summary>
		/// <param name="CheckedObjectPosition"></param>
		/// <param name="ComparisonPoint"></param>
		/// <param name="squaredDistance">The </param>
		/// <returns></returns>
		private static bool CheckIfObjectWithinSquaredDistanceOfPoint(Vector3 CheckedObjectPosition, Vector3 ComparisonPoint, float squaredDistance)
		{
			Vector3 difference = ComparisonPoint - CheckedObjectPosition;
			if (difference.sqrMagnitude < squaredDistance * squaredDistance)
			{
				return true;
			}
			return false;
		}

		public GameObject GetNearestLocation(Vector3 point, float maxDistance = 5.0f)
		{
			GameObject closest = null;
			float closestDist = 1000;

			//Look through all the objects. Check which is closest.
			for (int i = 0; i < SceneReferences.Count; i++)
			{
				//Disallow locations we have marked as inactive.
				if (SceneReferences[i].LocationActive)
				{
					//objPos = SceneReferences[i] != null ? SceneReferences[i].ObjectPosition : Vector3.one * float.MaxValue;
					float newDist = Vector3.Distance(point, SceneReferences[i].ObjectPosition);

					if (newDist < closestDist && newDist < maxDistance)
					{
						closest = SceneReferences[i].Representation;
						closestDist = newDist;
					}
				}
			}
			//Debug.Log("Closest: " + closest.name + "\n");

			return closest;
		}

		public GameObject GetNearestLocationAdvanced(Vector3 point, float maxDistance = 5.0f)
		{
			GameObject closest = null;
			float closestDist = 1000;
			Vector3 diff = Vector3.zero;

			//Look through all the objects. Check which is closest.
			for (int i = 0; i < SceneReferences.Count; i++)
			{
				//Disallow locations we have marked as inactive.
				if (SceneReferences[i].LocationActive)
				{
					for (int k = 0; k < SceneReferences[i].BubbleCollisionPoints.Count; k++)
					{
						diff = point - SceneReferences[i].BubbleCollisionPoints[i].transform.position;
						float newDist2 = diff.sqrMagnitude;
						if (newDist2 < closestDist && newDist2 < maxDistance * maxDistance)
						{
							closest = SceneReferences[i].Representation;
							closestDist = newDist2;

						}
					}
				}
			}
			//Debug.Log("Closest: " + closest.name + "\n");

			return closest;
		}

		public HardlightCollider[] GetMultipleNearestLocations(Vector3 point, int maxImpacted = 1, float maxDistance = 5.0f)
		{
			if (Distances == null)
			{
				//Set up the dictionary if we haven't. This is so we don't have to create a dictionary each frame.
				SetupDistanceDictionary();
			}

			List<HardlightCollider> hit = new List<HardlightCollider>();

			List<HardlightCollider> closestObjects = new List<HardlightCollider>();

			//This could possibly be more efficient. Linq is easy.
			var sortedList = from pair in Distances
							 orderby pair.Value ascending
							 select pair;

			float closestDist = 1000;

			//For all the regions
			for (int i = 0; i < SceneReferences.Count; i++)
			{
				//Calculate the V3 distance to the point.
				float newDist = Vector3.Distance(point, SceneReferences[i].ObjectPosition);
				Distances[SceneReferences[i]] = newDist;
			}

			foreach (KeyValuePair<HardlightCollider, float> item in sortedList)
			{
				bool wantMore = hit.Count < maxImpacted;
				bool withinDistance = item.Value < maxDistance;

				//Don't add more elements than requested
				if (wantMore && withinDistance)
				{
					//Don't add the same element more than once. I think we could eschew this more expensive check.
					if (!hit.Contains(item.Key))
					{
						hit.Add(item.Key);
					}
				}
			}

			//Look through all the objects. Find the closest N values closest.
			for (int i = 0; i < SceneReferences.Count; i++)
			{
				float newDist = Vector3.Distance(point, SceneReferences[i].ObjectPosition);

				bool isCloser = newDist < closestDist;
				bool islocationActive = SceneReferences[i].LocationActive;
				if (isCloser && islocationActive)
				{
					closestObjects.Add(SceneReferences[i]);
				}
			}

			//Debug.Log(hit.Count + "\n");
			return hit.ToArray();
		}

		/// <summary>
		/// Gets a random GameObject from the suit (which has a HapticLocation) that is within the set of potential areas.
		/// You can request 'AreaFlag.Right_All' to get any random right area.
		/// </summary>
		/// <param name="setOfPotentialAreas">Please </param>
		/// <returns>A game object within the random set. Can be null if you ask for AreaFlag.None or for spots that aren't on your configured body.</returns>
		public GameObject GetRandomLocationWithinSet(AreaFlag setOfPotentialAreas)
		{
			//TODO: Refactor this
			//Give me a random area flag within the set of area flag.
			//Apply filter to that set.
			//Then get the index of that area.


			var limitedAreas = DefinedAreas.Where(x => setOfPotentialAreas.HasFlag(x));
			int index = Random.Range(0, limitedAreas.Count());

			if (limitedAreas.Count() > 0)
			{
				int sceneReferenceIndex = DefinedAreas.IndexOf(limitedAreas.ElementAt(index));

				//Debug.Log("Rand Loc: " + index + " of " + limitedAreas.Count() + "  " + limitedAreas.ElementAt(index).ToString() + "  " + DefinedAreas.IndexOf(limitedAreas.ElementAt(index)) + "\n" + SceneReferences[sceneReferenceIndex].name);
				//This is not possible.
				//if (index > regions.Count)
				//{
				//}
				//else
				if (SceneReferences[sceneReferenceIndex] == null)
				{
					Debug.LogError("Attempted to get Random Location inside HardlightSuit's SuitDefinition.\n\tNo locations should be null. Check to make sure the fields in the Body Mimic prefab were assigned.");
				}
				else
				{
					return SceneReferences[sceneReferenceIndex].Representation;
				}
			}
			return null;
		}
		#endregion

		#region Visibility control (if you aren't using CameraExtension.HideLayer)
		public void SetAllVisibility(bool revealed)
		{
			if (SceneReferences != null)
			{
				return;
			}

			for (int i = 0; i < SceneReferences.Count; i++)
			{
				SetVisiblity(revealed, SceneReferences[i].Representation);
			}
		}
		private void SetVisiblity(bool revealed, GameObject region)
		{
			if (region != null)
			{
				MeshRenderer rend = region.GetComponent<MeshRenderer>();
				if (rend != null)
				{
					rend.enabled = revealed;
				}
			}
		}
		#endregion

		#region Editor Panes
		public int _CountValidZoneHoldersInEditor()
		{
			int count = 0;
			for (int i = 0; i < ZoneHolders.Count; i++)
			{
				if (ZoneHolders[i] != null)
				{
					count++;
				}
			}

			return count;
		}
		#endregion
	}
}