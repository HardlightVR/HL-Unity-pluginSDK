using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Hardlight.SDK
{
	/// <summary>
	/// This is a UnityComponent that contains a single SuitDefinition.
	/// A SuitDefinition is comprirsed of a list of Suit Node holders and the corresponding AreaFlags that each node represents.
	/// </summary>
	public class HardlightSuit : MonoBehaviour
	{
		/// <summary>
		/// This technique uses a larger single body model to detect collisions, and then turns that location data into a position.
		/// It is preferred to using regional collisions (which is often subject to Tunneling)
		/// Tunneling may still occur, but it will be more accurate and will catch projectiles that would otherwise be missed.
		/// For the best solution, use a predictive raycast solution or look into Unity Rigidbody.collisionDetectionMode
		/// </summary>
		//	Link: https://docs.unity3d.com/ScriptReference/Rigidbody-collisionDetectionMode.html
		public bool AllowSingleVolumeCollisions = false;
		/// <summary>
		/// This technique uses each individual HapticLocation as a trigger collider. This is less efficient and less accurate.
		/// I would highly recommend using the SingleVolume technique.
		/// </summary>
		public bool AllowRegionalCollisions = false;

		#region Collider Coloring Variables
		/// <summary>
		/// Colors the pads in the editor for easier debugging to see when areas are hit.
		/// </summary>
		public bool ColorRenderersOutsideEditor = false;
		/// <summary>
		/// This variable is used to store the original box color so we can correctly revert.
		/// </summary>
		private Color defaultBoxColor = default(Color);
		#endregion

		private Collider _singleVolumeCollider;
		public Collider SingleVolumeCollider
		{
			get
			{
				if (_singleVolumeCollider == null)
				{
					_singleVolumeCollider = GetComponent<Collider>();
				}
				return _singleVolumeCollider;
			}
		}

		/// <summary>
		/// This field is not used quite as nicely as we'd like.
		/// Unity's serialization of ScriptableObjects doesn't work nicely with prefabs.
		/// When you Apply Changes to the BodyMimic (or other HardlightSuit.cs prefabs) it won't serialize and save the SerializedObject fields
		/// This means we needed to transplant and populate the needed lists.
		/// This is a short term inefficiency that is intended to be fixed later.
		/// </summary>
		[SerializeField]
		[Header("For Defined Areas, Zone Holders and Scene Refs")]
		public SuitDefinition _definition;
		public SuitDefinition Definition
		{
			set { _definition = value; }
			get
			{
				if (_definition == null)
				{
					_definition = ScriptableObject.CreateInstance<SuitDefinition>();
					_definition.Init();
				}
				return _definition;
			}
		}

		/// <summary>
		/// A value for filtering out specific regions.
		/// Ex: Your player's health is a heartbeat haptic effect, therefore you never want to play anything else on that pad.
		/// You would add the Chest_Left to the FilterFlag.
		/// </summary>
		[SerializeField]
		public FilterFlag DisabledRegions
		{
			get { return Definition.DisabledRegions; }
			set { Definition.DisabledRegions = value; }
		}

		#region Transplanted Lists & Fields (from SuitDefinition)
		//This is a bunch of transplanted SuitDefinition class content to leverage prefab serialization.
		//See the summary comment on SuitDefinition in HardlightSuit.cs to understand why
		[SerializeField]
		public string SuitName = "Player Body";
		[SerializeField]
		public GameObject SuitRoot;

		[SerializeField]
		[Header("Do not refer to these fields at runtime")]
		public List<AreaFlag> DefinedAreas;

		//The Game Objects to fill the fields (which will get hardlight collider references)
		[SerializeField]
		public List<GameObject> ZoneHolders;

		//the objects added. Will get a nice button list to quick get to each of them.
		[SerializeField]
		public List<HardlightCollider> SceneReferences;

		[SerializeField]
		[Space(12)]
		public int HapticsLayer = HardlightManager.HAPTIC_LAYER;
		[SerializeField]
		public bool AddChildObjects = true;
		[SerializeField]
		public bool AddExclusiveTriggerCollider = true;
		private bool initialized = false;

#if UNITY_EDITOR
		public void _EditorOnlyCheckListValidity()
		{
			//Ensure the lists are all valid
			if (DefinedAreas == null || DefinedAreas.Count == 0)
			{
				DefinedAreas = Definition.DefinedAreas.ToList();
			}
			if (ZoneHolders == null || ZoneHolders.Count == 0)
			{
				ZoneHolders = Definition.ZoneHolders.ToList();
			}
			if (SceneReferences == null || SceneReferences.Count == 0)
			{
				SceneReferences = Definition.SceneReferences.ToList();
			}
		}
#endif
		#endregion

		#region Start, Init and other Setup	
		private void Start()
		{
			Init();
		}

		public void Init()
		{
			//If we AREN'T initialized
			if (!initialized)
			{
				if (!AreListsValid())
				{
					Debug.LogError("Attempting to initialize HardlightSuit [" + name + "] but one or more of my lists are null.\n\tThis is likely a problem with the asset or prefab itself.\n");
				}

				//Get rid of empty fields
				CollapseValidAreasForRuntime();

				//Populate our ScriptableObject definition with the serialized lists.
				Definition.DefinedAreas = DefinedAreas.ToList();
				Definition.ZoneHolders = ZoneHolders.ToList();
				Definition.SceneReferences = SceneReferences.ToList();

				Definition.AddChildObjects = AddChildObjects;
				Definition.HapticsLayer = HapticsLayer;
				Definition.AddExclusiveTriggerCollider = AddExclusiveTriggerCollider;

				DefinedAreas.Clear();
				ZoneHolders.Clear();
				SceneReferences.Clear();
			
				Definition.SetupDistanceDictionary();
				initialized = true;
			}
		}

		private bool AreListsValid()
		{
			bool validDefined = (DefinedAreas == null);
			bool zonesDefined = (ZoneHolders == null);
			bool refsDefined = (SceneReferences == null);
			if (validDefined || zonesDefined || refsDefined)
			{
				//Debug.LogError("Pruning malfunction\n");
				return false;
			}
			return true;
		}

		private void CollapseValidAreasForRuntime()
		{
			List<int> indicesOfInvalidReferences = FindIndicesOfInvalidReferences();

			AddInvalidRegionsToFilter(indicesOfInvalidReferences);
			RemoveInvalidReferences(indicesOfInvalidReferences);
		}

		private List<int> FindIndicesOfInvalidReferences()
		{
			List<int> indicesOfInvalidReferences = new List<int>();

			for (int i = 0; i < SceneReferences.Count; i++)
			{
				if (SceneReferences[i] == null)
				{
					indicesOfInvalidReferences.Add(i);
				}
			}
			return indicesOfInvalidReferences;
		}

		private void AddInvalidRegionsToFilter(List<int> indicesOfInvalidReferences)
		{
			//Step through the list normally.
			for (int i = 0; i < indicesOfInvalidReferences.Count; i++)
			{
				if (DefinedAreas.Count > i)
				{
					DisabledRegions.DisableArea(DefinedAreas[i]);
				}
			}
		}

		/// <summary>
		/// This is a function that collapses the valid areas for runtime.
		/// Prevents hitting a null reference when we try to find a specific area.
		/// </summary>
		private void RemoveInvalidReferences(List<int> indicesOfInvalidReferences)
		{
			//WARNING:
			//Don't traverse list in front->back order, deletion will change the indices

			//Start at the back of the invalid list (last indices) -> delete those first
			for (int i = indicesOfInvalidReferences.Count - 1; i > -1; i--)
			{
				int indexOfInvalidElement = indicesOfInvalidReferences[i];
				SceneReferences.RemoveAt(indexOfInvalidElement);
				ZoneHolders.RemoveAt(indexOfInvalidElement);
				DefinedAreas.RemoveAt(indexOfInvalidElement);
			}
		}
		#endregion

		/// <summary>
		/// The HardlightSuit supports two forms of collisions:
		/// [Recommended] Single Volume - The body is used to detect collisions, then Unity distances are calculated to figure out which HapticLocations were hit
		/// Regional Collision - Each HapticLocation has it's own collider and manages it's own collisions. Check HardLightSuit's comments for more details.
		/// </summary>
		/// <param name="singleVolumeCollisions"></param>
		/// <param name="regionalCollisions"></param>
		public void SetColliderState(bool singleVolumeCollisions = true, bool regionalCollisions = false)
		{
			AllowSingleVolumeCollisions = singleVolumeCollisions;
			AllowRegionalCollisions = regionalCollisions;

			SingleVolumeCollider.enabled = AllowSingleVolumeCollisions;
			SingleVolumeCollider.isTrigger = true;

			SetAllHardlightColliderStates(AllowRegionalCollisions);
		}

		private void SetAllHardlightColliderStates(bool targetState)
		{
			for (int i = 0; i < SceneReferences.Count; i++)
			{
				if (SceneReferences[i] != null)
				{
					SceneReferences[i].myCollider.isTrigger = true;
					SceneReferences[i].myCollider.enabled = targetState;
				}
			}
		}

		/// <summary>
		/// This function replaces existing elements so we can change the suit definition more easily at runtime (say the player's arm is added or destroyed)
		/// </summary>
		/// <param name="SingleFlagToModify"></param>
		/// <param name="SingleHolder"></param>
		/// <param name="newCollider"></param>
		public bool ModifyValidRegions(AreaFlag SingleFlagToModify, GameObject SingleHolder, HardlightCollider newCollider)
		{
			if (!SingleFlagToModify.IsSingleArea())
			{
				Debug.LogError("Attempted to modify the valid regions of the Hardlight Suit by providing a complex AreaFlag.\n\tThis function does not yet support complex area flags. Call it individually for each flag if you need to do this.");

				return false;
			}

			if (SingleHolder == null || newCollider == null || SingleFlagToModify == AreaFlag.None)
			{
				Debug.LogError("Attempted to modify the valid regions of the Hardlight Suit to provide invalid elements (either the collider or the holder) or to provide an AreaFlag of None (" + SingleFlagToModify.ToString() + ")");
				return false;
			}

			return ReplaceValidRegions(SingleFlagToModify, SingleHolder, newCollider);
		}

		/// <summary>
		/// The core actions of ModifyValidRegions (which does the validity checking this function wants)
		/// </summary>
		/// <param name="SingleFlagToModify"></param>
		/// <param name="SingleHolder"></param>
		/// <param name="newCollider"></param>
		private bool ReplaceValidRegions(AreaFlag SingleFlagToModify, GameObject SingleHolder, HardlightCollider newCollider)
		{
			bool Succeeded = false;
			bool ReplacedExistingElement = false;

			var indexOfFlag = -1;
			GameObject oldHolder = null;
			//HardlightCollider oldCollider = null;
			//If we have the area flag already
			if (Definition.DefinedAreas.Contains(SingleFlagToModify))
			{
				ReplacedExistingElement = true;

				//Find which index it is (index is the access key to all three lists)
				indexOfFlag = Definition.DefinedAreas.IndexOf(SingleFlagToModify);

				//Store the old holder and old collider
				oldHolder = Definition.ZoneHolders[indexOfFlag];
				//oldCollider = Definition.SceneReferences[indexOfFlag];
				if (oldHolder != null && oldHolder != SingleHolder)
				{
					oldHolder.SetActive(false);
				}

				//No longer have this location as disabled
				DisabledRegions.EnableArea(SingleFlagToModify);

				//Replace the old elements
				Definition.ZoneHolders[indexOfFlag] = SingleHolder;
				Definition.SceneReferences[indexOfFlag] = newCollider;
				Succeeded = true;
			}
			//Flag does not yet exist in the dictionary (it was empty so it was deleted)
			else
			{
				//Store the index of the new element we're adding.
				indexOfFlag = DefinedAreas.Count;
				Definition.DefinedAreas.Add(SingleFlagToModify);

				//Add the new elements
				Definition.ZoneHolders.Add(SingleHolder);
				Definition.SceneReferences.Add(newCollider);

				//This location is now enabled.
				DisabledRegions.EnableArea(SingleFlagToModify);

				Succeeded = true;
			}

			if (ReplacedExistingElement)
			{
				//Do something about the old elements?
				//What if we want to revert?
			}

			return Succeeded;
		}

		/// <summary>
		/// A static self reference for the HardlightSuit.
		/// Removes the need to use a GetComponent every time a user wants to have a reference to the suit.
		/// </summary>
		private static HardlightSuit _suit;
		/// <summary>
		/// A static self reference for the HardlightSuit.
		/// Removes the need to use a GetComponent every time a user wants to have a reference to the suit.
		/// </summary>
		public static HardlightSuit Suit
		{
			get
			{
				if (_suit == null)
				{
					_suit = FindObjectOfType<HardlightSuit>();
					if (_suit != null)
					{
						_suit.Init();
						return _suit;
					}
				}

				if (VRMimic.ValidInstance() && _suit == null)
				{
					Debug.LogError("Attempted to get a reference to HardlightSuit.Suit before calling VRMimic.Initialize()\nMust run VRMimic Initialize first - so you can configure hiding settings.");
				}
				return _suit;
			}
		}

		/// <summary>
		/// An easy way to find the current HardlightSuit in the scene. 
		/// (Only calls a Unity Find() function if the suit is null)
		/// Will be extended once multiple suits are in the scene at once (networked play)
		/// </summary>
		public static HardlightSuit Find()
		{
			return Suit;
		}

		/// <summary>
		/// [Helper function] Looks for a haptic sequence File - "Resources/Haptics/" + filename
		/// Identical to making a new HapticSequence and calling it's LoadFromAsset("Haptics/" + filename) function
		/// </summary>
		/// <param name="sequenceFile"></param>
		/// <returns></returns>
		public HapticSequence GetSequence(string sequenceFile)
		{
			HapticSequence seq = HapticSequence.LoadFromAsset("Haptics/" + sequenceFile);
			return seq;
		}

		///// <summary>
		///// This function has unintended consequences if you use multiple areaflags on the same area (such as Chest_Both)
		///// </summary>
		///// <param name="enabled"></param>
		///// <param name="flag"></param>
		//public void SetHapticLocationActivity(bool enabled, AreaFlag flag)
		//{
		//	throw new System.Exception("Incomplete\n");

		//	if (DefinedAreas.Contains(flag))
		//	{
		//		var indexOfArea = DefinedAreas.IndexOf(flag);
		//		if (indexOfArea < 0)
		//		{
		//			Debug.LogError("Deactivate Haptic Location failed. It did not contain the requested flag " + flag.ToString() + "\n");
		//			return;
		//		}
		//		SceneReferences[indexOfArea].LocationActive = enabled;
		//		AreaFlag locationsArea = SceneReferences[indexOfArea].MyLocation.Where;
		//		if (enabled)
		//		{
		//			DisabledRegions.DisableArea(locationsArea);
		//		}
		//		else
		//		{
		//			DisabledRegions.DisableArea(locationsArea);
		//		}
		//	}
		//}

		#region Simple Hit (Nearest and Nearby)
		/// <summary>
		/// Plays the sequence file on the single nearest HapticLocation
		/// Note: A HapticLocation can have an AreaFlag with multiple pads selected.
		/// </summary>
		/// <param name="point">A point in world space to compare</param>
		/// <param name="sequenceFile">Looks for a haptic sequence - "Resources/Haptics/" + filename</param>
		/// <param name="maxDistance">The max distance the point can be from any HapticLocations</param>
		public AreaFlag HitNearest(Vector3 point, string sequenceFile, float maxDistance = 5.0f)
		{
			return HitNearest(point, GetSequence(sequenceFile), maxDistance);
		}

		/// <summary>
		/// Plays the sequence on the single nearest HapticLocation
		/// Note: A HapticLocation can have an AreaFlag with multiple pads selected.
		/// </summary>
		/// <param name="point">A point in world space to compare</param>
		/// <param name="sequence">The sequence to play on the nearest location</param>
		/// <param name="maxDistance">The max distance the point can be from any HapticLocations</param>
		public AreaFlag HitNearest(Vector3 point, HapticSequence sequence, float maxDistance = 5.0f)
		{
			AreaFlag Where = GetNearestArea(point, maxDistance);
			if (Where != AreaFlag.None)
			{
				sequence.CreateHandle(Where).Play();
			}
			else
			{
				Debug.Log("Projectile hit the HardlightSuit but found no objects hit. Perhaps the maxDistance (" + maxDistance + ") is too small?\n\tOr the suit/prefab wasn't configured correctly.\n");
			}
			return Where;
		}

		/// <summary>
		/// Plays the file on all HapticLocations within a certain radius of the provided point.
		/// </summary>
		/// <param name="point">A point in world space to compare</param>
		/// <param name="sequenceFile">Looks for a haptic sequence - "Resources/Haptics/" + filename</param>
		/// <param name="impactRadius">The body is about .6 wide, .72 tall and .25 deep</param>
		public AreaFlag HitNearby(Vector3 point, string sequenceFile, float impactRadius = .35f)
		{
			return HitNearby(point, GetSequence(sequenceFile), impactRadius);
		}

		/// <summary>
		/// Plays the sequence on all HapticLocations within a certain radius of the provided point.
		/// </summary>
		/// <param name="point">A point in world space to compare</param>
		/// <param name="sequence">The sequence to play on the nearby locations</param>
		/// <param name="impactRadius">The body is about .6 wide, .72 tall and .25 deep</param>
		public AreaFlag HitNearby(Vector3 point, HapticSequence sequence, float impactRadius = .35f)
		{
			AreaFlag Where = GetAreasWithinRange(point, impactRadius, true);

			if (Where != AreaFlag.None)
			{
				sequence.CreateHandle(Where).Play();
			}
			else
			{
				Debug.Log("Projectile hit the HardlightSuit but found no objects hit. Perhaps the impactRadius (" + impactRadius + ") is too small?\n\tOr the suit/prefab wasn't configured correctly.\n");
			}

			return Where;
		}
		#endregion

		#region Impulses
		/// <summary>
		/// Calls ImpulseGenerator.BeginEmanatingEffect with the given sequence and depth.
		/// </summary>
		/// <param name="origin">The location to start the emanation.</param>
		/// <param name="sequence">The sequence to use.</param>
		/// <param name="depth">How many steps you want the emanation to take.</param>
		/// <param name="impulseDuration">How long the entire impulse should take</param>
		public void EmanatingHit(AreaFlag origin, HapticSequence sequence, float impulseDuration = .75f, int depth = 2)
		{
			ImpulseGenerator.BeginEmanatingEffect(origin, depth).WithDuration(impulseDuration).Play(sequence);
		}

		/// <summary>
		/// Calls ImpulseGenerator.BeginTraversingImpulse with the given sequence.
		/// </summary>
		/// <param name="startLocation">The origin of the traversing impulse.</param>
		/// <param name="endLocation">The destination of the traversing impulse.</param>
		/// <param name="sequence">The sequence to use.</param>
		/// <param name="impulseDuration">How long the entire impulse should take</param>
		public void TraversingHit(AreaFlag startLocation, AreaFlag endLocation, HapticSequence sequence, float impulseDuration = .75f)
		{
			ImpulseGenerator.BeginTraversingImpulse(startLocation, endLocation).WithDuration(impulseDuration).Play(sequence);
		}

		/// <summary>
		/// Begins an emanation at the nearest flag from the point.
		/// Has support for repetitions
		/// </summary>
		/// <param name="point">A point near the player's body</param>
		/// <param name="sequence">The HapticSequence to play on each pad visited.</param>
		/// <param name="impulseDuration">How long the entire impulse takes to visit each step of the depth</param>
		/// <param name="depth">The depth of the emanating impulse</param>
		/// <param name="repeats">Support for repeated impulse</param>
		/// <param name="delayBetweenRepeats">Do we delay between the impulse plays (delay of 0 will play all at once, having no effect)</param>
		/// <param name="maxDistance">Will not return locations further than the max distance.</param>
		public void HitImpulse(Vector3 point, HapticSequence sequence, float impulseDuration = .2f, int depth = 2, int repeats = 0, float delayBetweenRepeats = .15f, float maxDistance = 5.0f)
		{
			AreaFlag loc = GetNearestArea(point, maxDistance);
			if (loc != AreaFlag.None)
			{
				ImpulseGenerator.Impulse imp = ImpulseGenerator.BeginEmanatingEffect(loc, depth).WithEffect(sequence).WithDuration(impulseDuration);
				if (repeats > 0)
				{
					StartCoroutine(RepeatedEmanations(imp, delayBetweenRepeats, repeats));
				}
				else
				{
					imp.Play();
				}
			}
			else
			{
				Debug.LogWarning("Invalid Hit at " + point + "\n");
			}
		}

		/// <summary>
		/// Begins an emanation at the nearest flag from the point.
		/// Assigns a strength of 1.0f to the effect. (no attenuation support yet)
		/// </summary>
		/// <param name="point">A point near the player's body</param>
		/// <param name="eff">The effect to use in the emanation.</param>
		/// <param name="effectDuration">How long the impulse's effect is</param>
		/// <param name="impulseDuration">How long the entire impulse takes to visit each step of the depth</param>
		/// <param name="strength">How strong the individual effect is (no support for attenuation yet)</param>
		/// <param name="depth">The depth of the emanating impulse</param>
		/// <param name="maxDistance">Will not return locations further than the max distance.</param>
		public void HitImpulse(Vector3 point, Effect eff = Effect.Pulse, float effectDuration = .2f, float impulseDuration = .5f, float strength = 1.0f, int depth = 1, float maxDistance = 5.0f)
		{
			AreaFlag loc = GetNearestArea(point, maxDistance);
			if (loc != AreaFlag.None)
			{
				ImpulseGenerator.BeginEmanatingEffect(loc, depth).WithEffect(Effect.Pulse, effectDuration, strength).WithDuration(impulseDuration).Play();
			}
			else
			{
				Debug.LogWarning("Invalid Hit at " + point + "\n");
			}
		}
		#endregion

		#region GetColliders
		/// <summary>
		/// Gets an array of colliders that had points within the haptic spherecast.
		/// </summary>
		/// <param name="source">The point of origination in world space</param>
		/// <param name="direction">The direction of the spherecast in world space(magnitude does not matter)</param>
		/// <param name="sphereCastRadius">The radius of the spherecast (</param>
		/// <param name="sphereCastLength">The spherecast length</param>
		/// <param name="displayInEditor">Displays a debug coloring on the returned hardlight collider objects</param>
		public HardlightCollider[] GetCollidersFromSphereCast(Vector3 source, Vector3 direction, float sphereCastRadius = .25f, float sphereCastLength = 100, bool displayInEditor = false)
		{
			var closest = Definition.FindCollidersWithinSphereCast(source, direction, sphereCastRadius, sphereCastLength);

			if (displayInEditor)
			{
				ColorColliders(closest, Color.blue);
			}

			return closest;
		}

		/// <summary>
		/// Gets an array of colliders that had points within range of the point+distance.
		/// </summary>
		/// <param name="point">The center point in world space for the 'within range check'</param>
		/// <param name="maxDistance">The max distance in any direction from point</param>
		/// <param name="displayInEditor">Displays a debug coloring on the returned hardlight collider objects</param>
		public HardlightCollider[] GetCollidersWithinRange(Vector3 point, float maxDistance, bool displayInEditor = false)
		{
			var closest = Definition.GetMultipleNearestLocations(point, 16, maxDistance);

			if (displayInEditor)
			{
				ColorColliders(closest, Color.red);
			}

			return closest;
		}
		#endregion

		#region Haptic Spherecasting
		/// <summary>
		/// Returns a complex area flag of all haptic locations within a 3D cylindrical haptic vector
		/// </summary>
		/// <param name="source">The start point in world space</param>
		/// <param name="direction">Vector direction from starting point</param>
		/// <param name="sphereCastRadius"></param>
		/// <param name="sphereCastLength"></param>
		/// <returns></returns>
		public AreaFlag GetAreasFromSphereCast(Vector3 source, Vector3 direction, float sphereCastRadius = .25f, float sphereCastLength = 100)
		{
			//Get the array of colliders
			var hit = Definition.FindCollidersWithinSphereCast(source, direction, sphereCastRadius, sphereCastLength);

			//Get the areaflags out of those colliders.
			return FindAreaFlagFromHardlightColliders(hit);
		}

		private AreaFlag FindAreaFlagFromHardlightColliders(HardlightCollider[] hit)
		{
			AreaFlag hitAreas = AreaFlag.None;

			for (int i = 0; i < hit.Length; i++)
			{
				hitAreas = hitAreas.AddArea(hit[i].regionID);
				ColorHapticLocationInEditor(hit[i].MyLocation, new Color(0.0f, .7f, 0.0f, 1), 0.1f);
			}

			return hitAreas;
		}
		#endregion

		#region Finding HapticLocation and Flags
		/// <summary>
		/// Finds the nearest HapticLocation.Where on the HardlightSuit to the provided point
		/// </summary>
		/// <param name="point">The world space to compare to the PlayerTorso body.</param>
		/// <param name="maxDistance">Disregard body parts less than the given distance</param>
		/// <returns>Defaults to AreaFlag.None if no areas are within range.</returns>
		public AreaFlag GetNearestArea(Vector3 point, float maxDistance = 5.0f, bool UseExpensiveNearLocation = false)
		{
			//Maybe get a list of nearby regions?
			//GameObject closest = Definition.GetNearestLocation(point, maxDistance);
			GameObject closest = UseExpensiveNearLocation ? Definition.GetNearestLocationAdvanced(point, maxDistance) : Definition.GetNearestLocation(point, maxDistance);

			//Debug.Log("closest: " + closest.name + "\n");
			if (closest != null && closest.GetComponent<HapticLocation>() != null)
			{
				HapticLocation loc = closest.GetComponent<HapticLocation>();
				ColorHapticLocationInEditor(loc, Color.cyan);
				return loc.Where;
			}
			Debug.LogError("Could not find the closest pad. Returning an empty location\n" + closest.name);
			return AreaFlag.None;
		}

		/// <summary>
		/// This function finds all HapticLocations within maxDistance of the provided worldspace Point.
		/// </summary>
		/// <param name="point">A worldspace point to compare</param>
		/// <param name="maxDistance">The max distance to look for HapticLocations from this suit's definition.</param>
		/// <returns>AreaFlag with the flagged areas within range. Tip: Use value.AreaCount() or value.IsSingleArea()</returns>
		public AreaFlag GetAreasWithinRange(Vector3 point, float maxDistance, bool DisplayInEditor = false)
		{
			AreaFlag result = AreaFlag.None;
			var closest = GetCollidersWithinRange(point, maxDistance, DisplayInEditor);
			for (int i = 0; i < closest.Length; i++)
			{
				if (closest[i].MyLocation != null)
				{
					if (DisplayInEditor)
					{
						ColorHapticLocationInEditor(closest[i].MyLocation, Color.cyan);
					}

					//Debug.Log("Adding: " + loc.name + "\n");
					result = result.AddFlag(closest[i].MyLocation.Where);
				}
			}
			//Debug.Log("Result of find all flags: " + result.AreaCount() + "\n");
			return result;
		}

		/// <summary>
		/// Finds a Haptic Location near the source point.
		/// </summary>
		/// <param name="point">A point near the player's body</param>
		/// <returns>Returns null if no location is within the max distance</returns>
		public HapticLocation FindNearbyLocation(Vector3 point, float maxDistance = 5.0f)
		{
			//Maybe get a list of nearby regions?
			HardlightCollider[] closest = Definition.GetMultipleNearestLocations(point, 1, maxDistance);

			//Debug.Log("Find Nearby: " + closest.Length + "\n");
			for (int i = 0; i < closest.Length; i++)
			{
				HapticLocation loc = closest[i].MyLocation;
				//Debug.DrawLine(source, loc.transform.position, Color.green, 15.0f);
				if (closest[i] != null && loc != null && loc.LocationActive)
				{
					Debug.DrawLine(point, loc.transform.position, Color.red, 15.0f);

					ColorHapticLocationInEditor(loc, Color.red);
					return loc;
				}
			}
			return null;
		}

		/// <summary>
		/// Finds a Haptic Location near the source point. Only returns a HapticLocation that we have valid line of sight to the object.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="requireLineOfSight"></param>
		/// <param name="hitLayers"></param>
		/// <returns>A single HapticLocation that is close to the point and within line of sight of it. Defaults to null if nothing within MaxDistance is within range.</returns>
		public HapticLocation FindNearbyLocation(Vector3 point, bool requireLineOfSight, LayerMask hitLayers, float maxDistance = 5.0f)
		{
			HardlightCollider[] closest = Definition.GetMultipleNearestLocations(point, 16, maxDistance);

			//Debug.Log("Find Nearby: " + closest.Length + "\n");
			for (int i = 0; i < closest.Length; i++)
			{
				HapticLocation loc = closest[i].MyLocation;
				Debug.DrawLine(point, loc.transform.position, Color.green, 15.0f);
				if (closest[i] != null && loc != null && loc.LocationActive)
				{
					RaycastHit hit;
					float dist = Vector3.Distance(point, loc.transform.position);
					if (Physics.Raycast(point, loc.transform.position - point, out hit, dist, hitLayers))
					{
						//Debug.Log("Hit: " + hit.collider.name + "\n" + hit.collider.gameObject.layer + "\n");
						Debug.DrawLine(point, hit.point, Color.red, 15.0f);
					}
					else
					{
						Debug.DrawLine(point, loc.transform.position, Color.green, 15.0f);

						ColorHapticLocationInEditor(loc, Color.green);
						return loc;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets a random HapticLocation on the configured HardlightSuit.
		/// NOTE: Will remove DisabledRegions
		/// </summary>
		/// <param name="OnlyAreasWithinSet">The AreaFlags you want to randomly select from.</param>
		/// <param name="DisplayInEditor"></param>
		/// <returns>A valid HapticLocation on the body (defaults to null if none are configured or if it is configured incorrectly.</returns>
		public HapticLocation FindRandomLocation(AreaFlag OnlyAreasWithinSet = AreaFlag.All_Areas, bool RemoveDisabledRegions = false, bool DisplayInEditor = false)
		{
			if (RemoveDisabledRegions)
			{
				OnlyAreasWithinSet = OnlyAreasWithinSet.RemoveArea(DisabledRegions.InactiveRegions);
			}

			HapticLocation loc = Definition.GetRandomLocationWithinSet(OnlyAreasWithinSet).GetComponent<HapticLocation>();
			if (loc != null)
			{
				if (!loc.LocationActive)
				{
					Debug.LogError("FindRandomLocation and GetRandomLocationWithinSet have returned a HapticLocation that is marked as inactive.\n\tThis is an expected bug but should be fixed in the future - try using DeactiveHapticLocation to turn regions off.\n");
				}
				if (DisplayInEditor)
				{
					ColorHapticLocationInEditor(loc, Color.blue);
				}

				return loc;
			}
			else
				Debug.LogError("Failed to complete PlayerBody.FindRandomLocation(). The returned object did not have a HapticLocation component\n");
			return null;
		}
		#endregion

		#region Debug Coloring
		/// <summary>
		/// An easy way to request coloring for multiple colliders at once.
		/// Preprocessor #if UNITY_EDITOR (does nothing in a built game)
		/// </summary>
		/// <param name="colliders">The colliders to color</param>
		/// <param name="color">If you pass in default(Color) it will default to red.</param>
		/// <param name="duration">How long to color the colliders (repeated calls will work but wont look good)</param>
		private void ColorColliders(HardlightCollider[] colliders, Color color = default(Color), float duration = .5f)
		{
			bool inEditor = false;

#if UNITY_EDITOR
			inEditor = true;
#endif

			if (ColorRenderersOutsideEditor || inEditor)
			{
				for (int i = 0; i < colliders.Length; i++)
				{
					if (colliders[i].MyLocation != null)
					{
						ColorHapticLocationInEditor(colliders[i].MyLocation, color);
					}
				}
			}
		}

		/// <summary>
		/// This function is a no-op outside of the editor.
		/// Preprocessor defines keep it from impacting your game's performance.
		/// </summary>
		/// <param name="location">The HapticLocation to color (gets the MeshRenderer)</param>
		/// <param name="color">Defaults to red - the color to use. Will return to the default color of all haptic locations afterward.</param>
		public void ColorHapticLocationInEditor(HapticLocation location, Color color = default(Color), float duration = .5f)
		{
			if (color == default(Color))
			{
				color = Color.red;
			}
			MeshRenderer rend = location.gameObject.GetComponent<MeshRenderer>();
			if (rend != null)
			{
				if (defaultBoxColor == default(Color))
				{
					defaultBoxColor = rend.material.color;
				}

				TemporaryRendererColoring.CreateTemporaryColoring(rend, defaultBoxColor, color, duration);

#if UNITY_EDITOR
				//StartCoroutine(ColorHapticLocationCoroutine(rend, color, duration));
#endif

			}
			else
			{
				Debug.LogError("Renderer is null\n");
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// An editor exclusive function which colors the colliders (not visible during play mode)
		/// Called by ColorHapticLocationInEditor
		/// </summary>
		/// <param name="rend"></param>
		/// <param name="col"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		private IEnumerator ColorHapticLocationCoroutine(MeshRenderer rend, Color col, float duration = .5f)
		{
			if (defaultBoxColor == default(Color))
			{
				defaultBoxColor = rend.material.color;
			}
			rend.material.color = col;
			if (duration > 0)
			{
				yield return new WaitForSeconds(duration);
			}
			yield return null;
			rend.material.color = defaultBoxColor;
		}
#endif
		#endregion

		#region Repeating and Delayed Impulses
		/// <summary>
		/// A coroutine for repeating an emanation on a delay X times.
		/// </summary>
		/// <param name="impulse"></param>
		/// <param name="delay"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public IEnumerator RepeatedEmanations(ImpulseGenerator.Impulse impulse, float delay, int count)
		{
			impulse.Play();
			for (int i = 0; i < count - 1; i++)
			{
				yield return new WaitForSeconds(delay);
				impulse.Play();
			}
		}

		/// <summary>
		/// A coroutine for playing an impulse AFTER a float delay.
		/// </summary>
		/// <param name="impulse"></param>
		/// <param name="delay"></param>
		/// <returns></returns>
		IEnumerator DelayEmanation(ImpulseGenerator.Impulse impulse, float delay)
		{
			yield return new WaitForSeconds(delay);
			impulse.Play();
		}
		#endregion
	}
}