using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK
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

		Dictionary<GameObject, float> Distances;

		[SerializeField]
		public int HapticsLayer = NSManager.HAPTIC_LAYER;
		[SerializeField]
		public bool AddChildObjects = true;
		[SerializeField]
		public bool AddExclusiveTriggerCollider = true;

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

		public void Init()
		{
			SetDefaultAreas();

			SetupParents();

			GenerateSceneReferences();

			CollapseValidAreasForRuntime();
		}

		public void SetupDictionary()
		{
			if (Application.isPlaying)
			{
				Distances = new Dictionary<GameObject, float>();
				for (int i = 0; i < SceneReferences.Count; i++)
				{
					if (SceneReferences[i] != null)
					{
						Distances.Add(SceneReferences[i].Representation, float.MaxValue);
					}
				}
			}
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

		#region Location Functions
		public GameObject GetNearestLocation(Vector3 point, float maxDistance = 5.0f)
		{
			GameObject closest = null;
			float closestDist = 1000;
			//Vector3 objPos = Vector3.one * float.MaxValue;

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

		public GameObject[] GetMultipleNearestLocations(Vector3 point, int maxImpacted = 1, float maxDistance = 5.0f)
		{
			if (Distances == null)
			{
				//Set up the dictionary if we haven't. This is so we don't have to create a dictionary each frame.
				SetupDictionary();
			}

			List<GameObject> hit = new List<GameObject>();

			List<GameObject> closestObjects = new List<GameObject>();

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
				Distances[SceneReferences[i].Representation] = newDist;
			}

			foreach (KeyValuePair<GameObject, float> item in sortedList)
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
					closestObjects.Add(SceneReferences[i].Representation);
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
	}
}