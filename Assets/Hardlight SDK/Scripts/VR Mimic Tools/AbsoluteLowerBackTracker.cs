using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hardlight.SDK
{
	public class AbsoluteLowerBackTracker : AbstractTracker
	{
		public GameObject ShoulderBarData;
		public GameObject LowerBodyVisual;
		bool ShouldCreateVisuals = true;
		bool StomachInitialized = false;
		public GameObject SingleTorsoEffigy;
		public List<GameObject> TorsoSegments = new List<GameObject>();

		[Range(.1f, .5f)]
		public float TorsoWidth = .3f;
		[Range(.1f, .5f)]
		public float TorsoHeight = .3f;
		[Range(.1f, .5f)]
		public float TorsoDepth = .3f;

		public GameObject UpperBodyAnchor;

		public Vector3 EulerOffset;

		[Header("Segmented Torso Approach")]
		[Range(2, 15)]
		public int SegmentCount = 15;
		public Vector3 ShoulderScale = new Vector3(.4f, .5f, .2f);
		public Vector3 WaistScale = new Vector3(.4f, .5f, .2f);
		public Vector3 TrackerOffset;
		public Vector3 ShoulderOffset;

		void Update()
		{
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

		private void UpdateTorso()
		{
			if ((SingleTorsoEffigy == null || !StomachInitialized) && ShouldCreateVisuals)
			{
				//CreateVisuals();
			}
			if (SingleTorsoEffigy != null && StomachInitialized)
			{
				//Set the position and scale of a Single Torso Effigy
				SingleTorsoEffigy.transform.localScale = new Vector3(TorsoWidth, TorsoHeight, TorsoDepth);
				SingleTorsoEffigy.transform.position = TrackerMimic.transform.position + Offset;

				//Here we add the Euler offset (for if you want it to be oriented differently)
				Quaternion QOffset = Quaternion.identity;
				QOffset.eulerAngles = EulerOffset;
				SingleTorsoEffigy.transform.rotation = TrackerMimic.transform.rotation * QOffset;

				if (TorsoSegments.Count > 0)
				{
					#region Default Variables
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
							float perc = (float)i / ((float)TorsoSegments.Count - 1);
							
							//Handle the scale of the segment
							lerpedScale = Vector3.Lerp(WaistScale, ShoulderScale, perc);
							lerpedScale.y = (lerpedScale.y / TorsoSegments.Count) * 1.2f;
							TorsoSegments[i].transform.localScale = lerpedScale;

							//Handle the position of this segment
							segmentPosition = Vector3.Lerp(
								TrackerMimic.transform.position + TrackerOffset,
								ShoulderBarData.transform.position + ShoulderOffset,
								perc);

							//Each segment orients itself percentage-wise based on the shoulder/back orientation
							TorsoSegments[i].transform.position = segmentPosition + Offset;
							segmentLerpedOrientation = Quaternion.Lerp(TrackerMimic.transform.rotation, target, perc);
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