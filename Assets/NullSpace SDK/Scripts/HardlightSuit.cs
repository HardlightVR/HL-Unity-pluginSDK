using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK
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

		#region In-Editor Collider Coloring Variables
#if UNITY_EDITOR
		/// <summary>
		/// Colors the pads in the editor for easier debugging to see when areas are hit.
		/// </summary>
		public bool ColorRendererInEditor = true;
		/// <summary>
		/// This variable is used to store the original box color so we can correctly revert.
		/// </summary>
		private Color defaultBoxColor = default(Color);
#endif 
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
		public SuitDefinition _definition;
		[SerializeField]
		public SuitDefinition Definition
		{
			set { _definition = value; }
			get
			{
				if (_definition == null)
				{
					_definition = ScriptableObject.CreateInstance<SuitDefinition>();
					_definition.Init();

					//Call the transplant function.
				}
				return _definition;
			}
		}

		#region Transplanted Lists & Fields (from SuitDefinition)
		//This is a bunch of transplanted SuitDefinition class content to leverage prefab serialization.
		//See the summary comment on SuitDefinition in HardlightSuit.cs to understand why
		[SerializeField]
		public string SuitName = "Player Body";
		[SerializeField]
		public GameObject SuitRoot;

		[SerializeField]
		public List<AreaFlag> DefinedAreas;

		//The Game Objects to fill the fields (which will get hardlight collider references)
		[SerializeField]
		public List<GameObject> ZoneHolders;

		//the objects added. Will get a nice button list to quick get to each of them.
		[SerializeField]
		public List<HardlightCollider> SceneReferences;

		[SerializeField]
		public int HapticsLayer = NSManager.HAPTIC_LAYER;
		[SerializeField]
		public bool AddChildObjects = true;
		[SerializeField]
		public bool AddExclusiveTriggerCollider = true;
		private bool initialized = false;

		public void CheckListValidity()
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
		#endregion

		/// <summary>
		/// This is a function that collapses the valid areas for runtime.
		/// Prevents hitting a null reference when we try to find a specific area.
		/// </summary>
		public void CollapseValidAreasForRuntime()
		{
			for (int i = SceneReferences.Count - 1; i > -1; i--)
			{
				bool validDefined = (DefinedAreas == null);
				bool zonesDefined = (ZoneHolders == null);
				bool refsDefined = (SceneReferences == null);
				if (validDefined || zonesDefined || refsDefined)
				{
					Debug.LogError("Pruning malfunction\n");
				}

				if (SceneReferences[i] == null)
				{
					SceneReferences.RemoveAt(i);
					ZoneHolders.RemoveAt(i);
					DefinedAreas.RemoveAt(i);
				}
			}
		}

		public void Init()
		{
			//If we AREN'T initialized
			if (!initialized)
			{
				//Get rid of empty fields
				CollapseValidAreasForRuntime();

				//Populate our ScriptableObject definition with the serialized lists.
				Definition.DefinedAreas = DefinedAreas.ToList();
				Definition.ZoneHolders = ZoneHolders.ToList();
				Definition.SceneReferences = SceneReferences.ToList();
				Definition.AddChildObjects = AddChildObjects;
				Definition.HapticsLayer = HapticsLayer;
				Definition.AddExclusiveTriggerCollider = AddExclusiveTriggerCollider;
				Definition.SetupDictionary();
				initialized = true;
			}
		}

		private void Start()
		{
			Init();
		}

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
		/// An easy way to find the current HardlightSuit in the scene (this becomes trickier if you have multiple suits in play at once IE a networking situation)
		/// It will initialized the VRMimic if it is not yet initialized.
		/// </summary>
		/// <returns></returns>
		public static HardlightSuit Find()
		{
			HardlightSuit body = FindObjectOfType<HardlightSuit>();
			if (body != null)
			{
				body.Init();
				return body;
			}
			VRMimic.Initialize();

			body = FindObjectOfType<HardlightSuit>();
			if (body != null)
			{
				body.Init();
				return body;
			}
			return null;
		}

		/// <summary>
		/// [Helper function] Looks for a haptic file - "Resources/Haptics/" + file
		/// Identical to making a new HapticSequence and calling it's LoadFromAsset("Haptics/" + file) function
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public HapticSequence GetSequence(string file)
		{
			HapticSequence seq = new HapticSequence();
			seq.LoadFromAsset("Haptics/" + file);
			return seq;
		}

		/// <summary>
		/// Plays the file on the nearest SINGLE HapticLocation
		/// Note: A HapticLocation can have an AreaFlag with multiple pads selected.
		/// </summary>
		/// <param name="point">A point in world space to compare</param>
		/// <param name="file">Looks for a haptic file - "Resources/Haptics/" + file</param>
		/// <param name="maxDistance">The max distance the point can be from any HapticLocations</param>
		public void Hit(Vector3 point, string file, float maxDistance = 5.0f)
		{
			AreaFlag loc = FindNearestFlag(point, maxDistance);
			if (loc != AreaFlag.None)
			{
				HapticSequence seq = GetSequence(file);
				seq.CreateHandle(loc).Play();
			}
			else
			{
				Debug.Log("None\n");
			}
		}

		/// <summary>
		/// Calls ImpulseGenerator.BeginEmanatingEffect with the given sequence and depth.
		/// </summary>
		/// <param name="loc">The location to start the emanation.</param>
		/// <param name="seq">The sequence to use.</param>
		/// <param name="depth">How many steps you want the emanation to take.</param>
		/// <param name="duration">How long the entire impulse should take</param>
		public void EmanatingHit(AreaFlag loc, HapticSequence seq, float duration = .75f, int depth = 2)
		{
			ImpulseGenerator.BeginEmanatingEffect(loc, depth).WithDuration(duration).Play(seq);
		}

		/// <summary>
		/// Calls ImpulseGenerator.BeginTraversingImpulse with the given sequence.
		/// </summary>
		/// <param name="startLocation">The origin of the traversing impulse.</param>
		/// <param name="endLocation">The destination of the traversing impulse.</param>
		/// <param name="seq">The sequence to use.</param>
		/// <param name="duration">How long the entire impulse should take</param>
		public void TraversingHit(AreaFlag startLocation, AreaFlag endLocation, HapticSequence seq, float duration = .75f)
		{
			ImpulseGenerator.BeginTraversingImpulse(startLocation, endLocation).WithDuration(duration).Play(seq);
		}

		/// <summary>
		/// Begins an emanation at the nearest flag from the point.
		/// Has support for repetitions
		/// </summary>
		/// <param name="point">A point near the player's body</param>
		/// <param name="seq">The HapticSequence to play on each pad visited.</param>
		/// <param name="impulseDuration">How long the entire impulse takes to visit each step of the depth</param>
		/// <param name="depth">The depth of the emanating impulse</param>
		/// <param name="repeats">Support for repeated impulse</param>
		/// <param name="delayBetweenRepeats">Do we delay between the impulse plays (delay of 0 will play all at once, having no effect)</param>
		/// <param name="maxDistance">Will not return locations further than the max distance.</param>
		public void HitImpulse(Vector3 point, HapticSequence seq, float impulseDuration = .2f, int depth = 2, int repeats = 0, float delayBetweenRepeats = .15f, float maxDistance = 5.0f)
		{
			AreaFlag loc = FindNearestFlag(point, maxDistance);
			if (loc != AreaFlag.None)
			{
				ImpulseGenerator.Impulse imp = ImpulseGenerator.BeginEmanatingEffect(loc, depth).WithEffect(seq).WithDuration(impulseDuration);
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
			AreaFlag loc = FindNearestFlag(point, maxDistance);
			if (loc != AreaFlag.None)
			{
				ImpulseGenerator.BeginEmanatingEffect(loc, depth).WithEffect(Effect.Pulse, effectDuration, strength).WithDuration(impulseDuration).Play();
			}
			else
			{
				Debug.LogWarning("Invalid Hit at " + point + "\n");
			}
		}

		/// <summary>
		/// Finds the nearest HapticLocation.MyLocation on the PlayerTorso to the provided point
		/// </summary>
		/// <param name="point">The world space to compare to the PlayerTorso body.</param>
		/// <param name="maxDistance">Disregard body parts less than the given distance</param>
		/// <returns>Defaults to AreaFlag.None if no areas are within range.</returns>
		public AreaFlag FindNearestFlag(Vector3 point, float maxDistance = 5.0f)
		{
			//Maybe get a list of nearby regions?
			GameObject closest = Definition.GetNearestLocation(point, maxDistance);

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
		public AreaFlag FindAllFlagsWithinRange(Vector3 point, float maxDistance, bool DisplayInEditor = false)
		{
			AreaFlag result = AreaFlag.None;
			GameObject[] closest = Definition.GetMultipleNearestLocations(point, 16, maxDistance);
			for (int i = 0; i < closest.Length; i++)
			{
				HapticLocation loc = closest[i].GetComponent<HapticLocation>();
				if (loc != null)
				{
					if (DisplayInEditor)
					{
						ColorHapticLocationInEditor(loc, Color.cyan);
					}

					//Debug.Log("Adding: " + loc.name + "\n");
					result = result.AddFlag(loc.Where);
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
			GameObject[] closest = Definition.GetMultipleNearestLocations(point, 1, maxDistance);

			//Debug.Log("Find Nearby: " + closest.Length + "\n");
			for (int i = 0; i < closest.Length; i++)
			{
				HapticLocation loc = closest[i].GetComponent<HapticLocation>();
				//Debug.DrawLine(source, loc.transform.position, Color.green, 15.0f);
				if (closest[i] != null && loc != null)
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
			GameObject[] closest = Definition.GetMultipleNearestLocations(point, 16, maxDistance);

			//Debug.Log("Find Nearby: " + closest.Length + "\n");
			for (int i = 0; i < closest.Length; i++)
			{
				HapticLocation loc = closest[i].GetComponent<HapticLocation>();
				Debug.DrawLine(point, loc.transform.position, Color.green, 15.0f);
				if (closest[i] != null && loc != null)
				{
					RaycastHit hit;
					float dist = Vector3.Distance(point, loc.transform.position);
					if (Physics.Raycast(point, loc.transform.position - point, out hit, dist, hitLayers))
					{
						Debug.Log("Hit: " + hit.collider.name + "\n" + hit.collider.gameObject.layer + "\n");
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
		/// Gets a random HapticLocation on the configured PlayerTorso.
		/// </summary>
		/// <returns>A valid HapticLocation on the body (defaults to null if none are configured or if it is configured incorrectly.</returns>
		public HapticLocation FindRandomLocation(bool DisplayInEditor = false)
		{
			HapticLocation loc = Definition.GetRandomLocationObject().GetComponent<HapticLocation>();
			if (loc != null)
			{
				if (DisplayInEditor)
				{
					Debug.Log("HIT\n");
					ColorHapticLocationInEditor(loc, Color.blue);
				}

				return loc;
			}
			else
				Debug.LogError("Failed to complete PlayerBody.FindRandomLocation(). The returned object did not have a HapticLocation component\n");
			return null;
		}

		/// <summary>
		/// This function is a no-op outside of the editor.
		/// Preprocessor defines keep it from impacting your game's performance.
		/// </summary>
		/// <param name="location">The HapticLocation to color (gets the MeshRenderer)</param>
		/// <param name="color">Defaults to red - the color to use. Will return to the default color of all haptic locations afterward.</param>
		public void ColorHapticLocationInEditor(HapticLocation location, Color color = default(Color))
		{
#if UNITY_EDITOR
			if (color == default(Color))
			{
				color = Color.red;
			}
			MeshRenderer rend = location.gameObject.GetComponent<MeshRenderer>();
			if (rend != null)
			{
				StartCoroutine(ColorHapticLocationCoroutine(rend, color));
			}
#endif
		}

		/// <summary>
		/// [Unused] intended as an Editor window function.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="suit"></param>
		public void AddSceneReference(int index, HardlightCollider suit)
		{
			if (SceneReferences == null)
			{
				SceneReferences = new List<HardlightCollider>();
			}

			//if (SceneReferences.Count < index)
			//{
			SceneReferences.Add(suit);
			//}

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
			rend.material.color = Color.red;
			yield return new WaitForSeconds(duration);
			rend.material.color = defaultBoxColor;
		}
#endif

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
	}
}