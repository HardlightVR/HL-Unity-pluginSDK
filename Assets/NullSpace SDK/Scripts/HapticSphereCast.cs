using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NullSpace.SDK.Demos
{
	[ExecuteInEditMode]
	public class HapticSphereCast : MonoBehaviour
	{
		public bool SphereCastActive = true;
		public GameObject SpherecastStartObject;

		public Vector3 startLocation;
		public Vector3 localDirection = Vector3.up;
		private Vector3 worldDirection = Vector3.zero;

		//[Header("Note: Scale only works in Z Axis currently")]
		private bool GrowRangeWithScale = false;

		[Header("Sphere Cast Attributes")]
		[Range(0.05f, 500)]
		private float sphereCastRange = 1;
		[Range(0.01f, .5f)]
		private float sphereCastRadius = .1f;

		private HapticSequence mySequence = new HapticSequence();
		[Header("Haptic Information")]
		public string sequenceFileName = "Haptics/pain_short";
		private float sequenceStrength = 1.0f;

		private Vector2 RangeOfDelayBetweenReplays = new Vector2(.2f, .5f);
		private float timeSinceLastPlay = 0.0f;

		[Header("Gizmo Drawing Attribute")]
		public bool ShowGizmos = true;
		[Range(0, 30)]
		public int sphereCount = 10;

		private List<HapticHandle> handleList = new List<HapticHandle>();

		private bool Ready = false;
		private float scaledRange = 1;
		HardlightSuit suit;

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
			mySequence.LoadFromAsset(sequenceFileName);
			handleList = new List<HapticHandle>();

			if (SpherecastStartObject == null)
			{
				Debug.LogError("[Haptic Spherecast] - [" + name + "] has a null object for where it should begin.\n", this);
			}
		}

		void Update()
		{
			if (SpherecastStartObject != null)
			{
				worldDirection = transform.rotation * localDirection;
				startLocation = SpherecastStartObject.transform.position;

				var scaledDir = SphereCastRange * (Vector3.Dot(worldDirection, transform.lossyScale));
				//Debug.Log(Vector3.Dot(worldDirection, transform.lossyScale) + "\n" + worldDirection + "  " + transform.lossyScale + "\n",this);

				scaledRange = GrowRangeWithScale ? SphereCastRange * transform.lossyScale.z : SphereCastRange;

				if (Application.isPlaying && SphereCastActive)
				{
					var Where = suit.GetAreasFromSphereCast(startLocation, worldDirection, SphereCastRadius, scaledRange);

					var singles = Where.ToArray();

					//for (int i = 0; i < singles.Length; i++)
					//{
					//}
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
						var handle = mySequence.CreateHandle(Where, SequenceStrength);
						handleList.Add(handle);
						handle.Play();
						timeSinceLastPlay = Random.Range(RangeOfDelayBetweenReplays.x, RangeOfDelayBetweenReplays.y);
						Ready = false;
					}
					if (Where == AreaFlag.None)
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