using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Hardlight.SDK
{
	public class AbsoluteArmMimic : AbstractArmMimic
	{
		public override ArmKinematicMode ArmMode
		{
			get
			{
				return ArmKinematicMode.ViveUpperArms;
			}
		}
		//Unity doesn't let us serialize a property or put this before a property... (dumbnitude)
		[Header("Arm Type:\t[Vive Puck Upper Arms]", order = 0)]
		[Space(12, order = 1)]
		public bool ValidDataModelArms = false;
		public GameObject elbowObject;
		public GameObject ForearmRepresentation;
		public GameObject WristObject;
		public GameObject ShoulderJoint;

		public GameObject WristObjectVisual;
		public GameObject ShoulderJointVisual;

		public Color GizmoColor = Color.green;
		public float ForearmLength = .5f;

		[Range(0, 1)]
		public float PercentagePlacement = .5f;
		[Range(0, 4)]
		public float ArmScale = .5f;

		[Header("Modified each Update")]
		public Vector3 elbowToWrist = Vector3.zero;
		public float potentialForearmDistance;

		//private bool _enableEditing = false;
		//public bool EnableEditing
		//{
		//	get { return _enableEditing; }
		//	set
		//	{
		//		//Turn on/off all the VRTK editing objects?
		//		_enableEditing = value;
		//	}
		//}

		public UpperArmMimic UpperArmData;
		public ForearmMimic ForearmData;

		public Vector3 shoulderOffsetAmount = new Vector3(0, -.2f, 0);
		public Vector3 ControllerOffsetAmount = new Vector3(0, 0, -.122f);

		public override void Setup(ArmSide WhichSide, GameObject ShoulderMountConnector, VRObjectMimic Tracker, VRObjectMimic Controller)
		{
			WhichArm = WhichSide;

			ShoulderMount = ShoulderMountConnector;

			//	Assign the upper arm tracker
			TrackerMount = Tracker;

			//	Assign the lower arm prefab attributes
			ControllerConnection = Controller;
			if (!ValidDataModelArms)
			{
				SetupArmWithNoVisuals(WhichSide, NonVisualPrefabs);
			}
			if (ValidDataModelArms)
			{
				SetupArmVisuals(WhichSide, BodyPartPrefabs);
			}
			SetArmColliderAreaFlags();
		}

		#region Arm Prefab Setup
		private void SetupArmWithNoVisuals(ArmSide WhichSide, BodyVisualPrefabData NonVisualPrefabs)
		{
			SetupUpperArm(WhichSide, NonVisualPrefabs.UpperArmPrefab);
			SetupForearm(WhichSide, NonVisualPrefabs.ForearmPrefab);
			SetupWristJoint(ControllerConnection, NonVisualPrefabs.JointPrefab);
			SetupShoulderJoint(NonVisualPrefabs.JointPrefab);
			ValidDataModelArms = true;

		}
		public void SetupArmVisuals(ArmSide WhichSide, BodyVisualPrefabData visualPrefabs)
		{
			AttachUpperArmVisual(WhichSide, visualPrefabs.UpperArmPrefab);
			AttachForearmVisual(WhichSide, visualPrefabs.ForearmPrefab);
			AttachWristJoint(ControllerConnection, visualPrefabs.JointPrefab);
			AttachShoulderJoint(visualPrefabs.JointPrefab);
		}

		private void SetupUpperArm(ArmSide WhichSide, GameObject UpperArmPrefab)
		{
			//Create the upper arm prefab.
			UpperArmData = GameObject.Instantiate<GameObject>(UpperArmPrefab).GetComponent<UpperArmMimic>();
			UpperArmData.AssignSide(WhichSide);
			UpperArmData.transform.SetParent(transform);
			UpperArmCollider = UpperArmData.UpperArmCollider;
			if (WhichSide == ArmSide.Right)
			{
				UpperArmData.Mirror();
			}
			elbowObject = UpperArmData.Elbow;
		}
		private void SetupForearm(ArmSide WhichSide, GameObject ForearmPrefab)
		{
			//Create the lower arm prefab.
			ForearmData = GameObject.Instantiate<GameObject>(ForearmPrefab).GetComponent<ForearmMimic>();
			ForearmData.AssignSide(WhichSide);
			ForearmData.transform.SetParent(transform);
			ForearmCollider = ForearmData.ForearmCollider;
			ForearmData.transform.localPosition = Vector3.zero;
			ForearmRepresentation = ForearmData.ForearmBody;
		}
		private void SetupWristJoint(VRObjectMimic Controller, GameObject JointPrefab)
		{
			WristObject = GameObject.Instantiate<GameObject>(JointPrefab);
			WristObject.transform.SetParent(Controller.transform);
			WristObject.transform.localPosition = ControllerOffsetAmount;
		}
		private void SetupShoulderJoint(GameObject JointPrefab)
		{
			ShoulderJoint = Instantiate(JointPrefab);
			ShoulderJoint.transform.SetParent(UpperArmData.UpperArmBody.transform);
			ShoulderJoint.transform.localPosition = shoulderOffsetAmount;
			ShoulderJoint.transform.localScale = Vector3.one * .13f;
		}

		private void AttachUpperArmVisual(ArmSide WhichSide, GameObject UpperArmPrefab)
		{
			//Create the upper arm prefab.
			UpperArmData.UpperArmVisual = GameObject.Instantiate<GameObject>(UpperArmPrefab);
			UpperArmData.UpperArmVisual.name = UpperArmPrefab.name + " [c]";
			UpperArmData.UpperArmVisual.transform.SetParent(UpperArmData.UpperArmBody.transform);
			UpperArmData.UpperArmVisual.transform.localPosition = Vector3.zero;
			UpperArmData.UpperArmVisual.transform.localRotation = Quaternion.identity;

		}
		private void AttachForearmVisual(ArmSide WhichSide, GameObject ForearmPrefab)
		{
			//Create the forearm prefab.
			ForearmData.ForearmVisual = GameObject.Instantiate<GameObject>(ForearmPrefab);
			ForearmData.ForearmVisual.name = ForearmPrefab.name + " [c]";
			ForearmData.ForearmVisual.transform.SetParent(ForearmData.ForearmBody.transform);
			ForearmData.ForearmVisual.transform.localPosition = Vector3.zero;
			ForearmData.ForearmVisual.transform.localRotation = Quaternion.identity;
			ForearmData.ForearmVisual.transform.localScale = Vector3.one;
		}
		private void AttachWristJoint(VRObjectMimic Controller, GameObject JointPrefab)
		{
			//Create and position the wrist object (it gets childed to the controller)
			WristObjectVisual = GameObject.Instantiate<GameObject>(JointPrefab);
			WristObjectVisual.transform.SetParent(WristObject.transform);
			WristObjectVisual.transform.localPosition = Vector3.zero;
		}
		private void AttachShoulderJoint(GameObject JointPrefab)
		{
			//Create and position a joint object (same as wirst) for the shoulder
			ShoulderJointVisual = GameObject.Instantiate<GameObject>(JointPrefab);
			ShoulderJointVisual.transform.SetParent(ShoulderJoint.transform);
			ShoulderJointVisual.transform.localPosition = Vector3.zero;
			ShoulderJointVisual.transform.localScale = Vector3.one;
			ForearmData.ForearmVisual.transform.localRotation = Quaternion.identity;
		}
		#endregion

		private void SetArmColliderAreaFlags()
		{
			//Set our colliders to use the correct side.
			ForearmCollider.regionID = WhichArm == ArmSide.Left ? AreaFlag.Forearm_Left : AreaFlag.Forearm_Right;
			UpperArmCollider.regionID = WhichArm == ArmSide.Left ? AreaFlag.Upper_Arm_Left : AreaFlag.Upper_Arm_Right;

			//Add the arms to the suit themselves
			bool result = HardlightSuit.Find().ModifyValidRegions(ForearmCollider.regionID, ForearmCollider.gameObject, ForearmCollider);
			if (!result)
				Debug.LogError("Unable to modify HardlightSuit's valid regions\n");
			result = HardlightSuit.Find().ModifyValidRegions(UpperArmCollider.regionID, UpperArmCollider.gameObject, UpperArmCollider);
			if (!result)
				Debug.LogError("Unable to modify HardlightSuit's valid regions\n");
		}

		private void UnsetArmColliderAreaFlags()
		{
			Debug.LogWarning("Arm HardlightColliders are not being disabled even though there are no visual arms.\n\tUncertain if they should be removed\n", this);
			//throw new NotImplementedException("Currently, the arm colliders are never turned off after they are added.\n");
		}

		void Update()
		{
			HandleObjectOffsets();

			PositionUpperArm();

			if (ControllerConnection != null && elbowObject != null && WristObject != null)
			{
				if (ForearmData != null && UpperArmData != null)
				{
					CalculateAndAssignForearmPosition();

					ScaleForearmSize();

					HandleForearmOrientation();
				}
			}
		}

		private void HandleObjectOffsets()
		{
			if (WristObject)
			{	WristObject.transform.localPosition = ControllerOffsetAmount; }
			if (ShoulderJoint)
			{ ShoulderJoint.transform.localPosition = shoulderOffsetAmount; }
		}

		private void PositionUpperArm()
		{
			if (UpperArmData != null && TrackerMount != null)
			{
				UpperArmData.transform.position = TrackerMount.transform.position;
				UpperArmData.transform.rotation = TrackerMount.transform.rotation;
			}
		}

		private void CalculateAndAssignForearmPosition()
		{
			elbowToWrist = WristObject.transform.position - elbowObject.transform.position;
			potentialForearmDistance = elbowToWrist.magnitude;
			float percentage = (potentialForearmDistance) * PercentagePlacement;

			ForearmData.transform.position = elbowObject.transform.position + elbowToWrist.normalized * percentage;
		}

		private void ScaleForearmSize()
		{
			Vector3 newScale = ForearmRepresentation.transform.localScale;
			newScale.y = potentialForearmDistance * ArmScale;
			ForearmRepresentation.transform.localScale = newScale;
		}

		private void HandleForearmOrientation()
		{
			//Debug.DrawLine(Vector3.zero, elbowToWrist, Color.black);
			Vector3 cross = Vector3.Cross(WristObject.transform.right, ControllerConnection.transform.up);
			Vector3 dir = elbowObject.transform.forward;
			ForearmData.transform.LookAt(WristObject.transform, dir);
		}

		/// <summary>
		/// Does not call disposer.Dispose(). You must call it manually after this function adds visuals to the disposer.
		/// </summary>
		/// <param name="disposer"></param>
		public VisualDisposer DisposeVisuals(VisualDisposer disposer)
		{
			disposer.RecordVisual(UpperArmData.UpperArmVisual);
			disposer.RecordVisual(ForearmData.ForearmVisual);
			disposer.RecordVisual(WristObjectVisual);
			disposer.RecordVisual(ShoulderJointVisual);

			UnsetArmColliderAreaFlags();

			UpperArmData.UpperArmVisual = null;
			ForearmData.ForearmVisual = null;
			if (WristObjectVisual)
			{
				WristObjectVisual.transform.SetParent(null);
				WristObjectVisual = null;
			}
			if (ShoulderJointVisual)
			{
				ShoulderJointVisual.transform.SetParent(null);
				ShoulderJointVisual = null;
			}
			//Remove the hardlight colliders

			return disposer;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = GizmoColor;
			if (UpperArmData && UpperArmData.Elbow != null)
			{
				Gizmos.DrawLine(UpperArmData.Elbow.transform.position, WristObject.transform.position);
			}
		}
	}
}