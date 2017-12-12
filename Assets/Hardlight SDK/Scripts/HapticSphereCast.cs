using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// A 3D line of haptics with a defined radius.
	/// </summary>
	[ExecuteInEditMode] //We execute in edit mode so if you adjust the rotation/radius in edit mode, it will update the object. Haptics wont happen outside of play mode
	public class HapticSphereCast : MonoBehaviour
	{
		/// <summary>
		/// Whether or not the collisions are detected and haptics are played
		/// </summary>
		public bool SphereCastActive = true;
		/// <summary>
		/// The object the spherecast begins at.
		/// </summary>
		public GameObject SpherecastStartObject;

		public Vector3 startLocation;
		public Vector3 localDirection = Vector3.up;
		private Vector3 worldDirection = Vector3.zero;

		[Header("Note: Scale only works in Z Axis currently")]
		private bool GrowRangeWithScale = false;

		[Header("Sphere Cast Attributes")]
		[SerializeField]
		[Range(0.05f, 500)]
		private float sphereCastRange = 1;
		[SerializeField]
		[Range(0.01f, .5f)]
		private float sphereCastRadius = .1f;


		/// <summary>
		/// 
		/// </summary>
		[Header("Haptic Information")]
		[SerializeField]
		private HapticSequence mySequence;
		public HapticSequence MySequence
		{
			get
			{
				if (mySequence == null)
				{
					mySequence = HapticSequence.CreateNew("Empty Sequence");
				}
				return mySequence;
			}

			set
			{ mySequence = value; }
		}
		[SerializeField]
		private float sequenceStrength = 1.0f;

		/// <summary>
		/// Should haptic effects stop playing when the spherecast hits nothing.
		/// </summary>
		public bool ClearHapticsOnExit = true;

		/// <summary>
		/// Provide two identical values to create a smoother texture.
		/// Provide spread values to create odd textures.
		/// </summary>
		[SerializeField]
		private Vector2 RangeOfDelayBetweenReplays = new Vector2(.2f, .5f);
		private float timeSinceLastPlay = 0.0f;

		[Header("Gizmo Drawing Attribute")]
		public bool ShowGizmos = true;
		[Range(0, 30)]
		public int sphereCount = 10;

		private List<HapticHandle> handleList = new List<HapticHandle>();

		[SerializeField]
		private bool Ready = false;
		private float scaledRange = 1;
		HardlightSuit suit;

		/// <summary>
		/// The size of the sphere cast (a rapier would have a low value)
		/// </summary>
		public float SphereCastRadius
		{
			get
			{
				return sphereCastRadius;
			}

			set
			{
				sphereCastRadius = value;
			}
		}
		/// <summary>
		/// How long the line is (a laser would want a high value)
		/// </summary>
		public float SphereCastRange
		{
			get
			{
				return sphereCastRange;
			}

			set
			{
				sphereCastRange = value;
			}
		}

		/// <summary>
		/// A control for adjusting the strength of the sequence.
		/// </summary>
		public float SequenceStrength
		{
			get
			{
				return sequenceStrength;
			}

			set
			{
				sequenceStrength = value;
			}
		}
		/// <summary>
		/// Defines one of the values of RangeOfDelayBetweenReplays
		/// </summary>
		public float MinRangeBetweenPlays
		{
			get
			{
				return RangeOfDelayBetweenReplays.x;
			}

			set
			{
				RangeOfDelayBetweenReplays.x = value;
			}
		}
		/// <summary>
		/// Defines one of the values of RangeOfDelayBetweenReplays
		/// </summary>
		public float MaxRangeBetweenPlays
		{
			get
			{
				return RangeOfDelayBetweenReplays.y;
			}

			set
			{
				RangeOfDelayBetweenReplays.y = value;
			}
		}

		void Start()
		{
			suit = HardlightSuit.Find();
			if (Application.isPlaying)
			{
				handleList = new List<HapticHandle>();

				if (SpherecastStartObject == null)
				{
					Debug.LogError("[Haptic Spherecast] - [" + name + "] has a null object for where it should begin.\n", this);
				}
			}
		}

		void Update()
		{
			if (SpherecastStartObject != null)
			{
				worldDirection = transform.rotation * localDirection;
				startLocation = SpherecastStartObject.transform.position;

				//var scaledDir = SphereCastRange * (Vector3.Dot(worldDirection, transform.lossyScale));
				//Debug.Log(Vector3.Dot(worldDirection, transform.lossyScale) + "\n" + worldDirection + "  " + transform.lossyScale + "\n",this);

				scaledRange = GrowRangeWithScale ? SphereCastRange * transform.lossyScale.z : SphereCastRange;

				if (Application.isPlaying && SphereCastActive)
				{
					ApplySphereCast();
				}
			}
		}

		private void ApplySphereCast()
		{
			//Get the area flags for where this spherecast is hitting.
			var Where = suit.GetAreasFromSphereCast(startLocation, worldDirection, SphereCastRadius, scaledRange);

			//This handles the time delay between replays.
			if (!Ready)
			{
				timeSinceLastPlay -= Time.deltaTime;
				if (timeSinceLastPlay <= 0)
				{
					Ready = true;
				}
			}
			if (Where != AreaFlag.None && Ready)
			{
				var handle = MySequence.CreateHandle(Where, SequenceStrength);
				handleList.Add(handle);
				handle.Play();

				//Set our timing for later.
				timeSinceLastPlay = Random.Range(RangeOfDelayBetweenReplays.x, RangeOfDelayBetweenReplays.y);
				Ready = false;
			}
			//Clear out haptics on exit.
			if (ClearHapticsOnExit && Where == AreaFlag.None)
			{
				for (int i = 0; i < handleList.Count; i++)
				{
					if (handleList[i] != null)
					{
						handleList[i].Stop();
					}
					handleList.Clear();
				}
			}
		}

		void OnDrawGizmos()
		{
			if (ShowGizmos)
			{
				Gizmos.color = Color.white;
				Vector3 target = startLocation + worldDirection * scaledRange;
				Gizmos.DrawLine(startLocation, target);

				float total = sphereCount;
				for (int i = 0; i < total; i++)
				{
					if (i == 0)
					{
						Gizmos.color = Color.white - new Color(.75f, .75f, 0, .25f);
					}
					else if (i == total - 1)
					{
						Gizmos.color = Color.white - new Color(0, .75f, .75f, .15f);
					}
					else
					{
						Gizmos.color = Color.white - new Color(.5f, 0, .5f, .85f);
					}

					Gizmos.DrawSphere(Vector3.Lerp(startLocation, target, i / total), SphereCastRadius);
				}
			}
		}
	}
}