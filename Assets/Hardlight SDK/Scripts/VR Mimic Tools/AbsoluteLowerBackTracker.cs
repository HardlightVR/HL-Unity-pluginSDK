using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Contents of this namespace are subject to change
namespace Hardlight.SDK.Experimental
{
	/// <summary>
	/// This is a tracker that is used to define the torso.
	/// The torso created with this script is similar to a soft-body
	/// Soft Body Torso does things like allow your waist to face a different direction from your shoulders.
	/// There is not yet a good way to distribute regional haptic colliders among the body.
	/// This class is likely VERY volatile
	/// </summary>
	public class AbsoluteLowerBackTracker : AbstractTracker
	{
		public GameObject ShoulderBarData;
		public GameObject LowerBodyVisual;
		bool ShouldCreateVisuals = true;
		bool StomachInitialized = false;
		public bool UseTrackerPosition = true;
		public GameObject SingleTorsoEffigy;
		public List<GameObject> TorsoSegments = new List<GameObject>();

		[Header("Body Mimic Pose")]
		[SerializeField]
		internal BodyMimic.CalculatedPose CurrentIntendedPose;

		[Header("Anchor")]
		public GameObject UpperBodyAnchor;

		public Vector3 SegmentEulerOffset;
		public Vector3 TargetObjectOffset;
		public Quaternion targetOffsetQuat;

		[Header("Segmented Torso Approach")]
		[Range(2, 15)]
		public int SegmentCount = 15;
		public Vector3 UpperTorsoScale
		{
			get { return VRMimic.Instance.ActiveBodyMimic.BodyDimensions.UpperTorsoDimensions; }
		}
		public Vector3 LowerTorsoScale
		{
			get { return VRMimic.Instance.ActiveBodyMimic.BodyDimensions.LowerTorsoDimensions; }
		}
		public float TorsoHeight
		{
			get { return VRMimic.Instance.ActiveBodyMimic.BodyDimensions.TorsoHeight; }
		}
		public Vector3 TrackerOffset;
		public Vector3 ShoulderOffset;
		public bool DrawDebug = false;

		void Update()
		{
			targetOffsetQuat = Quaternion.identity;
			targetOffsetQuat.eulerAngles = TargetObjectOffset;

			transform.localPosition = Vector3.zero;
			transform.rotation = Quaternion.identity;
			if (TrackerMimic && ShoulderBarData)
			{
				UpdateTorso();
			}
		}

		public void CreateVisuals(GameObject TorsoPrefab)
		{
			if (TorsoPrefab == null)
			{
				Debug.LogError("Torso Prefab is null. It is needed to set up " + name + "'s visuals\n", this);
				return;
			}
			if (ShouldCreateVisuals)
			{
				SetupStomach(TorsoPrefab);
			}
			else
			{
				Debug.Log("Shouldn't create visuals\n", this);
			}
		}

		private Vector3 GetShoulderPosition()
		{
			if (UseTrackerPosition)
				return TrackerMimic.transform.position + ShoulderOffset;
			else
				return ShoulderBarData.transform.position + ShoulderOffset;
		}
		private Vector3 GetTorsoPosition()
		{
			if (UseTrackerPosition)
				return TrackerMimic.transform.position + Offset;
			else
			{
				Quaternion QOffset = Quaternion.identity;
				//QOffset.eulerAngles = Vector3.zero;

				Vector3 torsoUp = QOffset * ShoulderBarData.transform.right;
				if (DrawDebug)
				{
					Debug.DrawLine(ShoulderBarData.transform.position, ShoulderBarData.transform.position + torsoUp, Color.magenta);
				}

				return ShoulderBarData.transform.position + TrackerOffset + torsoUp.normalized * TorsoHeight;
			}
		}
		private Quaternion GetTorsoRotation()
		{
			Quaternion QOffset = Quaternion.identity;
			QOffset.eulerAngles = SegmentEulerOffset;
			if (UseTrackerPosition)
				return TrackerMimic.transform.rotation * QOffset;
			else
			{
				//Vector3 up = QOffset * ShoulderBarData.transform.up;
				return ShoulderBarData.transform.rotation * QOffset;
			}
		}

		private void UpdateTorso()
		{
			if ((SingleTorsoEffigy == null || !StomachInitialized) && ShouldCreateVisuals)
			{
				//CreateVisuals();
			}
			if (SingleTorsoEffigy != null && StomachInitialized)
			{
				//Set the position and scale of a Single Torso Effigy
				SingleTorsoEffigy.transform.localScale = UpperTorsoScale;
				SingleTorsoEffigy.transform.position = GetShoulderPosition();

				//Here we add the Euler offset (for if you want it to be oriented differently)

				SingleTorsoEffigy.transform.rotation = GetTorsoRotation();

				if (TorsoSegments.Count > 0)
				{
					#region Default Variables
					Quaternion QOffset = Quaternion.identity;
					QOffset.eulerAngles = SegmentEulerOffset;

					var target = ShoulderBarData.transform.rotation * QOffset;
					Vector3 segmentPosition = Vector3.zero;
					Vector3 lerpedScale = Vector3.zero;
					Quaternion segmentLerpedOrientation = Quaternion.identity;
					#endregion

					for (int i = 0; i < TorsoSegments.Count; i++)
					{
						if (TorsoSegments[i] != null)
						{
							//Find the vertical percent that this segment represents
							float perc = i / ((float)TorsoSegments.Count - 1);

							//Handle the scale of the segment
							lerpedScale = Vector3.Lerp(LowerTorsoScale, UpperTorsoScale, perc);
							lerpedScale.z = (lerpedScale.z / TorsoSegments.Count) * 1.2f;
							TorsoSegments[i].transform.localScale = lerpedScale;

							//Handle the position of this segment
							segmentPosition = Vector3.Lerp(
								GetTorsoPosition(),
								GetShoulderPosition(),
								perc);

							//Each segment orients itself percentage-wise based on the shoulder/back orientation
							TorsoSegments[i].transform.position = segmentPosition + Offset;
							segmentLerpedOrientation = Quaternion.Lerp(GetTorsoRotation(), target, perc);
							TorsoSegments[i].transform.rotation = segmentLerpedOrientation;
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates a single volume that represents the player's torso
		/// </summary>
		public void SetupStomach(GameObject TorsoPrefab)
		{
			SingleTorsoEffigy = GameObject.Instantiate<GameObject>(TorsoPrefab);
			SingleTorsoEffigy.name = "Torso Effigy";
			SingleTorsoEffigy.transform.localPosition = Offset;
			SingleTorsoEffigy.transform.SetParent(transform);

			StomachInitialized = true;

			//DISABLING THIS WHILE TORSO SEGMENTS ARE DEVELOPED
			SingleTorsoEffigy.SetActive(false);

			SetupTorsoSegments(SegmentCount, TorsoPrefab);
		}

		/// <summary>
		/// Creates N segments from the shoulders down to the location of the back tracker.
		/// </summary>
		/// <param name="segments"></param>
		public void SetupTorsoSegments(int segments, GameObject TorsoPrefab)
		{
			for (int i = 0; i < segments; i++)
			{
				var newSegment = GameObject.Instantiate<GameObject>(TorsoPrefab);
				newSegment.name = "Torso Segment [" + i + "]";
				newSegment.transform.localPosition = Offset;
				newSegment.transform.SetParent(transform);
				TorsoSegments.Add(newSegment);
			}
		}

		/// <summary>
		/// Does not call disposer.Dispose(). You must call it manually after this function populates it.
		/// </summary>
		/// <param name="disposer"></param>
		public void DisposeVisuals(VisualDisposer disposer)
		{
			disposer.RecordVisual(SingleTorsoEffigy);
			SingleTorsoEffigy = null;

			for (int i = TorsoSegments.Count - 1; i >= 0; i--)
			{
				disposer.RecordVisual(TorsoSegments[i].gameObject);
			}
			TorsoSegments.Clear();
		}
	}
}