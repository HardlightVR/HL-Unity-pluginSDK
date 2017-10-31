using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using Hardlight.SDK;
using System;

namespace Hardlight.SDK
{
	public class BodyMimic : MonoBehaviour
	{
		[Header("Body Hang Origin")]
		public GameObject hmd;

		public enum PositionEvaluationTechnique { PureHmd, PureArms, PureLowerBack, HybridHmdWithArms, HybridLowerBackWithArms, SmartHybrid, None }
		public PositionEvaluationTechnique PositionTechnique = PositionEvaluationTechnique.PureHmd;

		/// <summary>
		/// [Not currently in use]
		/// This is stub code for future tracking improvements that allow the body to tilt with the HMD (left/right, forward/backward)
		/// For more information, look into CalculateHMDTilt()
		/// </summary>
		//[Range(0, .35f)]
		//public float TiltAmtWithHMD = 0.0f;

		/// <summary>
		/// Big surprise, your body is below your head and neck. 
		/// This lets you adjust the body position. You might want to configure this as a game option/calibration step
		/// for players of different body distributions (long neck vs young with shorter neck vs giraffes)
		/// </summary>
		[Header("How far down the body is from HMD")]
		[Range(-2, 2)]
		public float NeckVerticalAnchor = .25f;

		/// <summary>
		/// The body is locked a bit back from where the HMD's position will be. 
		/// This controls how far forward or backward.
		/// </summary>
		[Header("How far fwd or back the body is")]
		[Range(-2, 2)]
		public float NeckFwdAnchor = 0;

		[Header("Weight of the HMD's pose in hybrid modes")]
		[Range(0, 3)]
		public float HmdPoseWeight = 1;

		[Header("Combined arm pose weight in hybrid modes")]
		[Range(0, 3)]
		public float ArmTrackerPoseWeight = 1.25f;

		[Header("Lower back's pose weight in hybrid modes")]
		[Range(0, 3)]
		public float LowerBackTrackerWeight = 1.25f;

		/// <summary>
		/// This lets you configure a different position when this box is checked. Useful to simulate human height when your Vive/Oculus/HMD is on your desk.
		/// </summary>
		[Header("For when your HMD is on your desk")]
		public bool UseDevHeight = false;
		public float devHeightPercentage = -0.15f;

		///// <summary>
		///// The direction the headset currently thinks is forward (regardless of overtilt up or down)
		///// Flattens the fwd vector onto the XZ plane. Then switches to using the Up/Down vector on the XZ plane if looking too far down or too far up.
		///// </summary>
		//public Vector3 assumedForward = Vector3.zero;
		public Vector3 LastUpdatedPosition = Vector3.zero;

		private CalculatedPose HMDPose;
		private CalculatedPose ArmPose;
		private CalculatedPose BackPose;

		/// <summary>
		/// Where the BodyMimic should be (for lerping)
		/// </summary>
		private CalculatedPose _targetPose;
		private CalculatedPose TargetPose
		{
			get
			{
				if (_targetPose == null)
					_targetPose = new CalculatedPose();
				return _targetPose;
			}

			set
			{
				_targetPose = value;
			}
		}

		/// <summary>
		/// The internal control of updateRate
		/// This could be temporarily increased/decreased based on context.
		/// </summary>
		private float updateRate;

		/// <summary>
		/// This controls how rigidly the body follows the HMD's position and orientation
		/// </summary>
		private float TargetUpdateRate = .15f;

		private float UpdateDuration = .75f;
		private float UpdateCounter = .2f;

		public GameObject LeftShoulder;
		public GameObject RightShoulder;
		[SerializeField]
		public AbstractArmMimic LeftArm;
		[SerializeField]
		public AbstractArmMimic RightArm;

		public AbsoluteArmMimic AbsoluteLeftArm;
		public AbsoluteArmMimic AbsoluteRightArm;

		[SerializeField]
		public AbstractTracker LowerBack;

		[SerializeField]
		public BodyVisualPrefabData VisualPrefabs;
		[SerializeField]
		public BodyVisualPrefabData DataModelPrefabs;

		//public AntiqueArmMimic AntiqueLeftArm;
		//public AntiqueArmMimic AntiqueRightArm;

		public GameObject ShoulderBarVisual;

		public bool ShouldCreateVisuals = true;

		/// <summary>
		/// When this distance is exceeded, it will force an update (for teleporting/very fast motion)
		/// </summary>
		[Header("Exceed this val to force update")]
		public float SnapUpdateDist = 1.0f;
		private Vector3 LastRelativePosition;

		[Header("Shoulder Bar Effigy Attributes")]
		bool ShoulderBarDataInitialized = false;
		bool StomachInitialized = false;
		public GameObject ShoulderBarData;
		
		#region Calculated Poses Class & Usage
		[System.Serializable]
		private class CalculatedPose
		{
			private Vector3 _rootPosition;
			private Vector3 _position;
			private Vector3 _forward;
			private Vector3 _up;

			public Vector3 RootPosition
			{
				get
				{
					return _rootPosition;
				}

				set
				{
					_rootPosition = value;
				}
			}
			public Vector3 TorsoPosition
			{
				get
				{
					return _position;
				}

				set
				{
					_position = value;
				}
			}
			public Vector3 Forward
			{
				get
				{
					return _forward;
				}

				set
				{
					_forward = value;
				}
			}
			public Vector3 Up
			{
				get
				{
					return _up;
				}

				set
				{
					_up = value;
				}
			}

			public CalculatedPose(Vector3 position, Vector3 forward, Vector3 up, Vector3 rootPosition)
			{
				TorsoPosition = position;
				RootPosition = rootPosition;
				Forward = forward;
				Up = up;
			}

			public CalculatedPose()
			{
				TorsoPosition = Vector3.zero;
				Forward = Vector3.forward;
				Up = Vector3.up;
			}

			public CalculatedPose(CalculatedPose firstPose, float firstPoseWeight, CalculatedPose secondPose, float secondPoseWeight)
			{
				TorsoPosition = (firstPose.TorsoPosition * firstPoseWeight + secondPose.TorsoPosition * secondPoseWeight) / (firstPoseWeight + secondPoseWeight);
				RootPosition = (firstPose.RootPosition * firstPoseWeight + secondPose.RootPosition * secondPoseWeight) / (firstPoseWeight + secondPoseWeight);
				Forward = (firstPose.Forward * firstPoseWeight + secondPose.Forward * secondPoseWeight) / (firstPoseWeight + secondPoseWeight);
				Up = (firstPose.Up * firstPoseWeight + secondPose.Up * secondPoseWeight) / (firstPoseWeight + secondPoseWeight);
			}

			public void Draw(Color color)
			{
				Debug.DrawLine(TorsoPosition, TorsoPosition + Forward.normalized * .25f, color);
				Debug.DrawLine(TorsoPosition, TorsoPosition + Up.normalized * .5f, color);
				Debug.DrawLine(TorsoPosition, TorsoPosition + Vector3.Cross(Up, Forward).normalized * .35f, color);
			}
		}

		private CalculatedPose EvaluatePositionBySelectedTechnique(float distanceFromGround)
		{
			CalculatedPose result;
			HMDPose = CalculatePoseFromHmd(distanceFromGround);
			ArmPose = CalculatePoseFromArms(distanceFromGround);
			BackPose = CalculatePoseFromLowerBack(distanceFromGround);

			if (PositionTechnique == PositionEvaluationTechnique.PureHmd)
			{
				result = HMDPose;
			}
			else if (PositionTechnique == PositionEvaluationTechnique.PureArms)
			{
				result = ArmPose;
			}
			else if (PositionTechnique == PositionEvaluationTechnique.PureLowerBack)
			{
				result = BackPose;
			}
			else if (PositionTechnique == PositionEvaluationTechnique.HybridHmdWithArms)
			{
				result = CreateHybridPose(HMDPose, HmdPoseWeight, ArmPose, ArmTrackerPoseWeight);
			}
			else if (PositionTechnique == PositionEvaluationTechnique.HybridLowerBackWithArms)
			{
				result = CreateHybridPose(BackPose, LowerBackTrackerWeight, ArmPose, ArmTrackerPoseWeight);
			}
			else if (PositionTechnique == PositionEvaluationTechnique.SmartHybrid)
			{
				result = CalculateTiltTorso();
			}
			else
			{
				result = new CalculatedPose(Vector3.zero, Vector3.forward, Vector3.up, Vector3.zero);
			}
			return result;
		}

		private CalculatedPose CalculatePoseFromHmd(float distanceFromGround)
		{
			Vector3 likelyFwd = CalculateHMDForward();
			Vector3 hmdDown = Vector3.down * distanceFromGround * (UseDevHeight ? devHeightPercentage : NeckVerticalAnchor);
			var targetPosition = likelyFwd * (.25f + NeckFwdAnchor) + GetHMDPosition() + hmdDown;
			return new CalculatedPose(targetPosition, likelyFwd, Vector3.up, hmd.transform.position);
		}

		private Vector3 CalculateHMDForward()
		{
			Vector3 flatRight = hmd.transform.right;
			flatRight.y = 0;

			return Vector3.Cross(flatRight, Vector3.up);
		}

		private Vector3 GetHMDPosition()
		{
			return hmd.transform.position;
		}

		private CalculatedPose CalculatePoseFromArms(float distanceFromGround)
		{
			if (!LeftArm || !RightArm)
			{
				return new CalculatedPose();
			}
			GameObject LeftJoint = (LeftArm as AbsoluteArmMimic).ShoulderJoint;
			GameObject RightJoint = (RightArm as AbsoluteArmMimic).ShoulderJoint;

			if (LeftJoint && RightJoint)
			{
				//Look at both positions.
				Vector3 avgPos = (LeftJoint.transform.position + RightJoint.transform.position) / 2;

				Vector3 avgAbovePos = (LeftJoint.transform.position + (LeftArm as AbsoluteArmMimic).UpperArmData.GetUp() + RightJoint.transform.position + (RightArm as AbsoluteArmMimic).UpperArmData.GetUp() * 3) / 2;
				//Debug.DrawLine(LeftJoint.transform.position, LeftJoint.transform.position + (LeftArm as AbsoluteArmMimic).UpperArmVisual.GetUp() * 3, new Color(.2f, .6f, .9f));
				//Debug.DrawLine(RightJoint.transform.position, RightJoint.transform.position + (RightArm as AbsoluteArmMimic).UpperArmVisual.GetUp() * 3, new Color(.2f, .6f, .9f));

				Debug.DrawLine(LeftJoint.transform.position, avgPos, new Color(.6f, .3f, .9f));
				Debug.DrawLine(RightJoint.transform.position, avgPos, new Color(.9f, .3f, .6f));
				Debug.DrawLine(avgPos, avgAbovePos.normalized * .1f, Color.yellow);


				Vector3 forward = Vector3.Cross(avgAbovePos - avgPos, RightJoint.transform.position - LeftJoint.transform.position);
				//Debug.DrawLine(avgPos, avgPos + forward, Color.red, .5f);

				//Set this as the target position (with the shoulder hang offset)
				var targetPosition = avgPos + forward * (.25f + NeckFwdAnchor);

				return new CalculatedPose(targetPosition, forward, Vector3.up, (LeftJoint.transform.position + RightJoint.transform.position) / 2);
			}

			//Default calculated pose spits out an error.
			return new CalculatedPose();
		}

		private CalculatedPose CalculatePoseFromLowerBack(float distanceFromGround)
		{
			if (LowerBack)
			{
				GameObject lowerBack = (LowerBack as AbsoluteLowerBackTracker).gameObject;

				if (lowerBack)
				{
					//Look at both positions.
					Vector3 avgPos = LowerBack.TrackerMimic.transform.position;
					//Debug.DrawLine(lowerBack.transform.position, avgPos, new Color(.6f, .3f, .9f));
					Vector3 up = LowerBack.TrackerMimic.transform.up;
					Debug.DrawLine(avgPos, avgPos + up * .25f, Color.yellow);

					Vector3 forward = Vector3.Cross(up, LowerBack.TrackerMimic.transform.right);
					Debug.DrawLine(avgPos, avgPos + forward, Color.red);

					//Set this as the target position (with the shoulder hang offset)
					var targetPosition = avgPos + up * (.25f + NeckVerticalAnchor) + forward * (.25f + NeckFwdAnchor);

					return new CalculatedPose(targetPosition, forward, up, LowerBack.TrackerMimic.transform.position);
				}
			}

			//Default calculated pose spits out an error.
			return new CalculatedPose();
		}

		private CalculatedPose CreateHybridPose(CalculatedPose firstPose, float firstWeight, CalculatedPose secondPose, float secondWeight)
		{
			firstPose.Draw(Color.cyan);
			secondPose.Draw(Color.green);

			var mergedPose = new CalculatedPose(firstPose, firstWeight, secondPose, secondWeight);

			mergedPose.Draw(Color.white);

			return mergedPose;
		}

		private CalculatedPose CalculateTiltTorso()
		{
			//Horizontal offset
			Vector3 spineDirection = ArmPose.RootPosition - BackPose.RootPosition;

			Debug.DrawLine(BackPose.RootPosition, BackPose.RootPosition + spineDirection, Color.white);

			//var mergedPose = new CalculatedPose(hmdPos, firstWeight, secondPose, secondWeight);

			//mergedPose.Draw(Color.white);

			return BackPose;
		}
		#endregion

		private void Awake()
		{
			updateRate = TargetUpdateRate;
		}

		void FixedUpdate()
		{
			UpdatePositionAndOrientation();

			UpdateBodyElements();
		}

		#region Update Position And Orientation
		private void UpdatePositionAndOrientation()
		{
			//Start with the body position based on the HMD. 
			//We'll adjust it backward and downward once we solve for this vectors this frame.

			HandleSnapTeleportDistanceCheck();
			HandleUpdateCounterAndRate();

			float distanceFromGround = GetDistanceFromGround();
			LastUpdatedPosition = GetLastUpdatedPosition();

			TargetPose = EvaluatePositionBySelectedTechnique(distanceFromGround);

			AssignBodyMimicPosition();

			//We adjust our transform based on where we ARE & how our pose should orient itself.
			transform.LookAt(transform.position + TargetPose.Forward * 5, TargetPose.Up);
		}

		private void HandleSnapTeleportDistanceCheck()
		{
			//If we teleport or are too far away
			if (Vector3.Distance(TargetPose.TorsoPosition, LastUpdatedPosition) > SnapUpdateDist)
			{
				//Force an update for now
				ImmediateUpdatePosition();
			}
			else
			{
				LastRelativePosition = transform.position - TargetPose.TorsoPosition;
			}
		}

		/// <summary>
		/// Force an update of the BodyMimic (in case of teleports, fast movement)
		/// </summary>
		public void ImmediateUpdatePosition()
		{
			transform.position = TargetPose.TorsoPosition + LastRelativePosition;
		}

		private void HandleUpdateCounterAndRate()
		{
			UpdateCounter += Time.deltaTime * updateRate;
			//This is logic to let us update only some of the time.
			if (UpdateCounter >= UpdateDuration)
			{
				UpdateCounter = 0;
				LastUpdatedPosition = hmd.transform.position;

				//We reset the update rate. The core of this logic was to have certain criteria that used a higher update rate (so we would get closer to the next update quicker)
				updateRate = TargetUpdateRate;
			}
		}

		private float GetDistanceFromGround()
		{
			return hmd.transform.position.y - hmd.transform.parent.transform.position.y;
		}

		private Vector3 GetLastUpdatedPosition()
		{
			return Vector3.Lerp(LastUpdatedPosition, TargetPose.TorsoPosition, .5f);
			// Mathf.Clamp(prog / UpdateDuration, 0, 1));
		}

		private void AssignBodyMimicPosition()
		{
			transform.position = Vector3.Lerp(transform.position, TargetPose.TorsoPosition, updateRate);
		}
		#endregion

		#region Public (Arm/Back) Functions
		//Also Included in our public facing functions is ImmediateUpdatePosition (in Update Position & Orientation region)
		public AbstractTracker BindLowerBackTracker(VRObjectMimic Tracker)
		{
			//Create an Arm Prefab
			var newLowerBackTracker = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("VRMimic/Lower Back Tracker Root")).GetComponent<AbsoluteLowerBackTracker>();

			newLowerBackTracker.Setup(gameObject, Tracker);

			newLowerBackTracker.name = "Absolute Lower Back Tracker";
			//var offset = Quaternion.Inverse(Tracker.transform.rotation) * newLowerBackTracker.transform.rotation;
			var offset = Quaternion.Inverse(newLowerBackTracker.transform.rotation) * Tracker.transform.rotation;
			var before = newLowerBackTracker.transform.rotation;

			LowerBack = newLowerBackTracker;

			return newLowerBackTracker;
		}

		public AbstractArmMimic CreateArm(ArmSide WhichSide, VRObjectMimic Tracker, VRObjectMimic Controller, BodyVisualPrefabData prefabsToUse = null)
		{
			var ExistingArm = AccessArm(WhichSide);

			if (ExistingArm != null)
			{
				//This error will likely get removed later. I use it for now.
				Debug.LogError("Attempted to request an arm when one already existed. Returning existing arm\n", this);
				return ExistingArm;
			}

			//Create the Arm Prefab
			var newArm = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("VRMimic/Absolute Arm Mimic")).GetComponent<AbstractArmMimic>();

			//We can request explicitly different visuals for the arm. (they are saved to the arm here)
			VisualPrefabs = prefabsToUse == null ? VisualPrefabs : prefabsToUse;
			newArm.BodyPartPrefabs = VisualPrefabs;

			//We always want to use the data model prefabs (which have no visual components)
			newArm.NonVisualPrefabs = DataModelPrefabs;

			newArm.name = "Absolute Arm Mimic " + WhichSide.ToString();
			newArm.transform.SetParent(transform);

			//Initialize the arm prefab (handing in the side and connector points)
			newArm.Setup(WhichSide, GetShoulder(WhichSide), Tracker, Controller);

			//Keep track of this as our Left/Right arm
			AttachArmToOurBody(WhichSide, newArm);
			return newArm;
		}

		public void AttachArmToOurBody(ArmSide WhichSide, AbstractArmMimic Arm)
		{
			if (WhichSide == ArmSide.Left)
			{
				LeftArm = Arm;
				//LeftArm.transform.SetParent(LeftShoulder.transform);
				//LeftArm.transform.localPosition = Arm.transform.right * -.5f;
				//LeftArm.MirrorKeyArmElements();
			}
			else
			{
				RightArm = Arm;
				//RightArm.transform.SetParent(RightShoulder.transform);
				//RightArm.transform.localPosition = Arm.transform.right * .5f;
			}
		}

		public GameObject GetShoulder(ArmSide WhichSide)
		{
			if (WhichSide == ArmSide.Left)
			{
				if (LeftShoulder != null)
					return LeftShoulder;
			}
			else
			{
				if (RightShoulder != null)
					return RightShoulder;
			}
			//If this code has reached you, you can add 
			//return null;
			//And comment the exception out. This shouldn't happen, but we know how code & releases work.
			throw new System.Exception("Shoulder Mount Requested [" + WhichSide.ToString() + "] was not added or configured according to the BodyMimic\nThis behavior will attempt an autosetup on the requested arm in the future");
		}

		public AbstractArmMimic AccessArm(ArmSide WhichSide)
		{
			if (WhichSide == ArmSide.Left)
			{
				if (LeftArm != null)
					return LeftArm;
			}
			else
			{
				if (RightArm != null)
					return RightArm;
			}
			//If this code has reached you, you can add 
			return null;
			//And comment the exception out. This shouldn't happen, but we know how code & releases work.
			//throw new System.Exception("Arm Requested [" + WhichSide.ToString() + "] was not added or configured according to the BodyMimic\nThis behavior will attempt an autosetup on the requested arm in the future");
		}

		#region Antique Arm Mimic
		/// <summary>
		/// This is an old technique which used a Kinematic Arm (AntiqueArmMimic)
		/// It is largely obsolete and should be stripped from the public release
		/// </summary>
		/// <param name="WhichSide"></param>
		/// <param name="Tracker"></param>
		/// <param name="Controller"></param>
		/// <returns></returns>
		public AntiqueArmMimic CreateAntiqueArm(ArmSide WhichSide, VRObjectMimic Tracker, VRObjectMimic Controller)
		{
			//Create an Arm Prefab
			AntiqueArmMimic newArm = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Arm Mimic Prefab")).GetComponent<AntiqueArmMimic>();

			newArm.transform.SetParent(transform);

			//Initialize the arm prefab (handing in the side and connector points)
			newArm.Initialize(WhichSide, GetShoulder(WhichSide), Tracker, Controller);

			//Keep track of this as our Left/Right arm?
			Debug.LogError("Did not attach antique arm to body. This function is largely deprecated\n", this);
			//AttachArmToOurBody(WhichSide, newArm);
			return newArm;
		}

		//public void AttachArmToOurBody(ArmSide WhichSide, AntiqueArmMimic Arm)
		//{
		//	if (WhichSide == ArmSide.Left)
		//	{
		//		AntiqueLeftArm = Arm;
		//		AntiqueLeftArm.transform.SetParent(LeftShoulder.transform);
		//		AntiqueLeftArm.transform.localPosition = Arm.transform.right * -.5f;
		//		AntiqueLeftArm.MirrorKeyArmElements();
		//	}
		//	else
		//	{
		//		AntiqueRightArm = Arm;
		//		AntiqueRightArm.transform.SetParent(RightShoulder.transform);
		//		AntiqueRightArm.transform.localPosition = Arm.transform.right * .5f;
		//	}
		//}


		//public AntiqueArmMimic AccessAntiqueArm(ArmSide WhichSide)
		//{
		//	if (WhichSide == ArmSide.Left)
		//	{
		//		if (AntiqueLeftArm != null)
		//			return AntiqueLeftArm;
		//	}
		//	else
		//	{
		//		if (AntiqueRightArm != null)
		//			return AntiqueRightArm;
		//	}
		//	//If this code has reached you, you can add 
		//	//return null;
		//	//And comment the exception out. This shouldn't happen, but we know how code & releases work.
		//	throw new System.Exception("Arm Requested [" + WhichSide.ToString() + "] was not added or configured according to the BodyMimic\nThis behavior will attempt an autosetup on the requested arm in the future");
		//} 
		#endregion
		#endregion

		#region Update Body Elements
		void UpdateBodyElements()
		{
			if (ShoulderBarData == null || !ShoulderBarDataInitialized)
			{
				var left = (LeftArm as AbsoluteArmMimic);
				var right = (RightArm as AbsoluteArmMimic);
				if (left != null && right != null)
				{
					SetupShoulderBar(left, right);
				}
			}
			//Shoulder Bar Visual requires the SB-Data object (and no established visual)
			if (ShouldCreateVisuals && ShoulderBarDataInitialized && ShoulderBarVisual == null)
			{
				CreateShoulderBarVisual(VisualPrefabs.ShoulderConnectorPrefab);
			}

			//If we have data & a visual
			if (ShoulderBarDataInitialized && ShoulderBarVisual != null)
			{
				AssignAbsoluteLowerBackTracker(LowerBack);

				if (LowerBack)
				{
					float shoulderDistance = CalculateShoulderDistance();
					Vector3 CenterPosition = CalculateShoulderCenterPosition(shoulderDistance);

					if (ShouldCreateVisuals)
					{
						var back = (LowerBack as AbsoluteLowerBackTracker);
						back.CreateVisuals(VisualPrefabs.TorsoPrefab);
						ShouldCreateVisuals = false;
					}
				}
			}

			if (CanPositionShoulderBar())
			{
				PositionShoulderBar();
			}
		}

		private void SetupShoulderBar(AbsoluteArmMimic lArm, AbsoluteArmMimic rArm)
		{
			if (lArm == null || rArm == null)
			{
				return;
			}
			ShoulderBarData = GameObject.Instantiate<GameObject>(DataModelPrefabs.ShoulderConnectorPrefab);
			ShoulderBarData.name = "Shoulder Bar Data";
			ShoulderBarData.transform.SetParent(transform);

			AbsoluteLeftArm = lArm;
			AbsoluteRightArm = rArm;
			ShoulderBarDataInitialized = true;
		}

		private void CreateShoulderBarVisual(GameObject shoulderPrefab)
		{
			if (shoulderPrefab != null)
			{
				ShoulderBarVisual = GameObject.Instantiate<GameObject>(shoulderPrefab);
				ShoulderBarVisual.name = shoulderPrefab.name + " [c]";
				ShoulderBarVisual.transform.SetParent(ShoulderBarData.transform);
				ShoulderBarVisual.transform.localScale = Vector3.one;
				ShoulderBarVisual.transform.localPosition = Vector3.zero;
			}
		}

		private void AssignAbsoluteLowerBackTracker(AbstractTracker lowerBack)
		{
			if (LowerBack == null || LowerBack != lowerBack)
			{
				LowerBack = lowerBack;
			}

			var back = (LowerBack as AbsoluteLowerBackTracker);
			back.ShoulderBarData = ShoulderBarData;
			LowerBack.transform.SetParent(transform);
		}

		private Vector3 CalculateShoulderCenterPosition(float distance)
		{
			Vector3 leftToRight = AbsoluteRightArm.ShoulderJoint.transform.position - AbsoluteLeftArm.ShoulderJoint.transform.position;
			return AbsoluteLeftArm.ShoulderJoint.transform.position + leftToRight.normalized * distance / 2;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private float CalculateShoulderDistance()
		{
			return Vector3.Distance(AbsoluteLeftArm.ShoulderJoint.transform.position, AbsoluteRightArm.ShoulderJoint.transform.position);
		}

		private bool CanPositionShoulderBar()
		{
			return ShoulderBarVisual != null && ShoulderBarDataInitialized && AbsoluteLeftArm && AbsoluteRightArm;
		}

		private void PositionShoulderBar()
		{
			Transform shoulder = ShoulderBarData.transform;
			float distance = CalculateShoulderDistance();
			Vector3 CenterPosition = CalculateShoulderCenterPosition(distance);
			shoulder.position = CenterPosition;

			Debug.DrawLine(AbsoluteLeftArm.ShoulderJoint.transform.position, AbsoluteLeftArm.ShoulderJoint.transform.position + CenterPosition.normalized, Color.black);

			Vector3 stretchLocalScale = shoulder.localScale;
			stretchLocalScale.z = distance * 3.2f;
			shoulder.localScale = stretchLocalScale;

			//Debug.DrawLine(shoulder.position, shoulder.position + GetForward(), Color.black);
			//Debug.DrawLine(shoulder.position, shoulder.position + GetUp(), Color.white);
			//Debug.DrawLine(shoulder.position, shoulder.position + GetRight(), Color.grey);
			Vector3 upDir = AverageArmUp();

			shoulder.LookAt(AbsoluteRightArm.ShoulderJoint.transform.position, upDir);
		}

		/// <summary>
		/// Unused
		/// </summary>
		/// <returns></returns>
		private Vector3 CalculateChestEffigyPosition()
		{
			Debug.DrawLine(transform.position, AbsoluteRightArm.ShoulderJoint.transform.position, Color.red);
			Debug.DrawLine(transform.position, AbsoluteLeftArm.ShoulderJoint.transform.position, Color.yellow);
			return (AbsoluteLeftArm.ShoulderJoint.transform.position + AbsoluteRightArm.ShoulderJoint.transform.position) / 2;
		}
		private Vector3 AverageArmUp()
		{
			return (AbsoluteLeftArm.UpperArmData.GetUp() + AbsoluteRightArm.UpperArmData.GetUp()) / 2;
		}
		private Vector3 AverageArmRight()
		{
			return (AbsoluteLeftArm.UpperArmData.GetRight() + AbsoluteRightArm.UpperArmData.GetRight()) / 2;
		}
		private Vector3 AverageArmForward()
		{
			return (AbsoluteLeftArm.UpperArmData.GetForward() + AbsoluteRightArm.UpperArmData.GetForward()) / 2;
		}
		#endregion

		#region Visual Handling
		public void CreateVisuals(BodyVisualPrefabData prefabs = null, VisualDisposer disposer = null)
		{
			if (prefabs == null)
				prefabs = VisualPrefabs;
			if (prefabs == null)
				return;

			DisposeVisuals(disposer);

			//Request each arm make visuals
			//Set ShouldCreateVisuals to true?
			ShouldCreateVisuals = true;
			if (AbsoluteLeftArm)
			{
				AbsoluteLeftArm.SetupArmVisuals(ArmSide.Left, VisualPrefabs);
			}
			if (AbsoluteRightArm)
			{
				AbsoluteRightArm.SetupArmVisuals(ArmSide.Right, VisualPrefabs);
			}
		}

		/// <summary>
		/// Calls disposer.Dispose() at the end of this function.
		/// </summary>
		/// <param name="disposer"></param>
		public void DisposeVisuals(VisualDisposer disposer)
		{
			if (disposer == null)
				new VisualDisposer();
			if (AbsoluteRightArm != null)
				AbsoluteRightArm.DisposeVisuals(disposer);
			if (AbsoluteLeftArm != null)
				AbsoluteLeftArm.DisposeVisuals(disposer);
			DisposeArmBarVisual(disposer);
			(LowerBack as AbsoluteLowerBackTracker).DisposeVisuals(disposer);
			ShoulderBarVisual = null;

			disposer.Dispose();
		}

		/// <summary>
		/// Does not call disposer.Dispose(). You must call it manually after this function.
		/// </summary>
		/// <param name="disposer"></param>
		private void DisposeArmBarVisual(VisualDisposer disposer)
		{
			disposer.RecordVisual(ShoulderBarVisual);
			ShoulderBarVisual = null;
		}
		#endregion

		#region Unused
		//[Header("Floor Evaluation")]
		//public bool UseHeadRaycasting = false;
		//public LayerMask validFloorLayers = ~((1 << 2) | (1 << 9) | (1 << 10) | (1 << 12) | (1 << 13) | (1 << 15));

		///// <summary>
		///// [Unused Stub]
		///// This function is pseudocode for a future feature - calculating body mimic's tilt based on the headset orientation/recent movement
		///// </summary>
		//void CalculateHMDTilt()
		//{
		//Look at the orientation of the HMD.
		//Case 1: Standing Straight
		//		No behavior change
		//
		//
		//
		//Case 2: Look Left/Right (Only Y Axis Rotation)
		//
		//
		//Case 3: Look Up/Down (Only X axis Rotation)
		//		Check for change in local Y position
		//			If Y decreased recently, they might be peering down and leaning over.
		//			TILT Forward around Body's Right vector
		//
		//
		//Case 4: Confused Tilt Left/Right (Only Z axis Rotation)
		//		Check for change in local X vector (their right)
		//			If they moved in their local X space recently, they might be peaking around a corner
		//			TILT L/R around Body's Fwd vector
		//
		//Case 5: [Multiple cases at once]
		//		Due to the complex nature of these steps, it might be better to define each as their own operation that influences the end body orientation, and apply them separately. There will obviously be weird cases from the player doing handstands or cartwheels.
		//
		//}
		#endregion

		void OnDrawGizmos()
		{
			DrawPoseGizmos(HMDPose, Color.yellow);
			DrawPoseGizmos(BackPose, Color.magenta);
			DrawPoseGizmos(ArmPose, Color.cyan);
		}

		void DrawPoseGizmos(CalculatedPose pose, Color col)
		{
			Gizmos.color = col;
			Gizmos.DrawSphere(pose.TorsoPosition, .01f);
			Gizmos.DrawSphere(pose.RootPosition, .035f);
			Gizmos.DrawLine(pose.RootPosition, pose.RootPosition + pose.Up.normalized * .25f);
			Gizmos.DrawLine(pose.RootPosition, pose.RootPosition + pose.Forward.normalized * .1f);
		}

		/// <summary>
		/// This function creates and initializes the Body Mimic
		/// </summary>
		/// <param name="vrCamera">The camera to hide the body from. Calls camera.HideLayer(int)</param>
		/// <param name="hapticObjectLayer">The layer that is removed from the provided camera's culling mask.</param>
		/// <returns>The created body mimic</returns>
		public static BodyMimic Initialize(Camera vrCamera, VRObjectMimic CameraRigMimic, int hapticObjectLayer = HardlightManager.HAPTIC_LAYER)
		{
			GameObject bodyMimicPrefab = Resources.Load<GameObject>("Body Mimic");

			//Instantiate the prefab of the body mimic.
			GameObject newMimic = Instantiate<GameObject>(bodyMimicPrefab);
			newMimic.name = "Body Mimic";

			BodyMimic mimic = null;

			if (newMimic != null)
			{
				if (vrCamera == null)
				{
					Debug.LogWarning("Attempting to initialize body mimic with a null camera.\n\tDefaulting to Camera.main.gameOBject instead. This might not work\n");
				}

				GameObject cameraObject = vrCamera == null ? Camera.main.gameObject : vrCamera.gameObject;

				vrCamera = cameraObject.GetComponent<Camera>();

				//Set the BodyMimic's target to the VRObjectMimic
				mimic = newMimic.GetComponent<BodyMimic>();
				mimic.hmd = vrCamera.GetComponent<WatchedByMimic>().WatchingMimic.gameObject;
				mimic.transform.SetParent(CameraRigMimic.transform);
			}
			if (vrCamera != null)
			{
				vrCamera.HideLayer(hapticObjectLayer);
			}

			return mimic;
		}
	}
}