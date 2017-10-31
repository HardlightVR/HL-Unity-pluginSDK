using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ArmKinematics : MonoBehaviour
{
	private bool ReportLogs = false;

	[Header("Update Specifics")]
	public bool UpdateRegardlessOfArmMimic = false;
	public bool ApplyForwardKinematics = true;
	public bool ApplyInverseKinematics = true;
	public bool ApplyJointClamping = true;
	public bool UseRotationBase = false;
	public Transform RotationBase;

	public bool DrawComfortDisplaySpheres = false;
	public bool ShouldDrawOrientationLines = false;

	[Header("Kinematic Locations")]
	public RobotJoint[] Joints;
	public float[] OfficialAngles;
	public Vector3[] Offsets;

	[Header("Target Information")]
	public GameObject Target;
	public Vector3 TargetPos
	{
		get
		{
			if (Target == null)
			{
				return Vector3.zero;
			}
			return Target.transform.position;
		}
		set
		{
			if (Target != null)
			{
				Target.transform.position = value;
			}
		}
	}


	public Vector3 ForwardKinematicsResult;
	public Vector3 LastTargetPosition;
	public float DistanceToTarget;
	public float WorldDistanceToTarget;

	[Header("IK Parameters")]
	public float SamplingDistance = 0.25f;
	[Range(1, 3)]
	public int IKSampleRate = 4;
	public float LearningRate = 300.0f;
	public float DistanceThreshold = .1f;

	[Header("Randomization")]
	public bool RandomizeStart = true;
	public bool RandomizeTarget = false;

	public class FwdKineResult
	{
		RobotJoint joint;
		Vector3 point;
		Vector3 up;
		Vector3 right;
		public FwdKineResult(RobotJoint rJoint, Vector3 endPt, Vector3 jointUp, Vector3 jointRight)
		{
			joint = rJoint;
			point = endPt;
			up = jointUp;
			right = jointRight;
		}
	}

	public void Start()
	{
		if (Target == null)
		{
			Target = new GameObject();
			Target.name = "Forward Kinematics Target [Runtime Created]";
		}

		if (RandomizeTarget)
		{
			TargetPos = Joints[0].transform.position + Random.insideUnitSphere;
		}
		if (RandomizeStart)
		{
			if (Joints != null)
			{
				OfficialAngles = new float[Joints.Length - 1];
				for (int i = 0; i < OfficialAngles.Length; i++)
				{
					OfficialAngles[i] = Random.Range(-360.0f, 360.0f);
				}
			}
		}

		Offsets = new Vector3[Joints.Length];

		//string report = "[" + gameObject.name + " - Original Positions]\n";
		//for (int i = 0; i < Joints.Length; i++)
		//{
		//	report += Joints[i].name + "  " + Joints[i].transform.position + "\n";
		//}
		//Debug.Log(report + "\n");
	}

	public void Update()
	{
		if (UpdateRegardlessOfArmMimic && !Application.isPlaying)
		{
			UpdateKinematics();
		}
		GetMaxArmReach();
	}

	// Called in ArmMimic.Update()
	public void UpdateKinematics()
	{
		//Debug.Log("Updating: " + name + " FK: " + ApplyForwardKinematics + " IK: " + ApplyInverseKinematics + "\n", this);
		if (isActiveAndEnabled)
		{
			if (ApplyForwardKinematics)
			{
				//Debug.Log("Updating [" + name + "] forward kinematics\n");
				HandleForwardKinematics();
			}
			if (ApplyInverseKinematics && Application.isPlaying)
			{
				//Debug.Log("Updating [" + name + "] inverse kinematics\n");
				HandleInverseKinematics();
			}
		}

		UpdateWorldDistanceToTarget();
		SimplifyAngleValues();
	}

	public void HandleForwardKinematics()
	{
		ForwardKinematicsResult = ForwardKinematics(GetAngles());
		LastTargetPosition = Joints[Joints.Length - 1].transform.position;
		//	Debug.Log("Position goes from: " + Joints[0].transform.position + " to " + result.ToString() + "\n");
	}

	public void HandleInverseKinematics()
	{
		if (Target != null)
		{
			DistanceToTarget = UpdateWorldDistanceToTarget();

			for (int i = 0; i < IKSampleRate; i++)
			{
				InverseKinematics(TargetPos, GetAngles());
			}
		}
	}

	public Vector3 ForwardKinematics(float[] angles, int depth = int.MaxValue)
	{
		FwdKineResult result;
		int lastIndex = 0;
		//Start with the position of the first joint

		Vector3 prevPoint = Vector3.zero;
		Quaternion rotation = Quaternion.identity;

		string reportAngles = "";
		//For each required joint
		for (int i = 1; i < Joints.Length && i < depth; i++)
		{
			//Make sure we're using the CORRECT current index
			int prevJointIndex = i - 1;
			int curJointIndex = i;

			// Rotates around that joint's axis
			rotation *= Quaternion.AngleAxis(angles[prevJointIndex], Joints[prevJointIndex].Axis);

			//Calculate the point of this joint based on the previous point + the rotation offset start offset.
			Vector3 ThisJointsRotatedOffset = rotation * Joints[curJointIndex].StartOffset;
			Vector3 nextPoint = prevPoint + ThisJointsRotatedOffset;

			#region Offset with our previous joint
			if (Offsets != null && Offsets.Length > curJointIndex)
			{
				reportAngles += "[" + Joints[prevJointIndex].name + "] - assigned " + Joints[prevJointIndex].StartOffset + "  " + Joints[prevJointIndex].transform.localPosition + "\n";
				Offsets[prevJointIndex] = Joints[prevJointIndex].StartOffset;
			}
			#endregion

			#region Rotate with our first joint
			if (RotationBase != null && UseRotationBase)
			{
				Joints[prevJointIndex].transform.rotation = RotationBase.transform.rotation * rotation;
			}
			else
			{
				Joints[prevJointIndex].transform.rotation = rotation;
			}
			#endregion

			Vector3 jointPos = Joints[0].transform.position;
			Vector3 upv3 = (Vector3.up * .35f * i);
			Vector3 right = (Vector3.right * .5f);

			Transform joint = Joints[prevJointIndex].transform;

			if (ShouldDrawOrientationLines)
			{
				DrawOrientationLines(prevJointIndex, jointPos, upv3, right, joint);
			}

			Joints[prevJointIndex].AngleInMyAxis = angles[prevJointIndex];

			prevPoint = nextPoint;
			lastIndex = i;
			//result = new FwdKineResult(Joints[prevJointIndex], nextPoint, 
		}
		//if (Joints[lastIndex] != null)
		//{
		//	result = new FwdKineResult(Joints[lastIndex], Joints[0].transform.position + prevPoint, Vector3.zero, Vector3.zero);
		//}

		if (ReportLogs)
			Debug.Log(reportAngles + "\n");

		return Joints[0].transform.position + prevPoint;
	}

	private void DrawOrientationLines(int prevJointIndex, Vector3 jointPos, Vector3 upv3, Vector3 right, Transform joint)
	{
		Debug.DrawLine(jointPos + upv3 + right, jointPos + (joint.localRotation * Vector3.up) * .4f + upv3 + right, Joints[prevJointIndex].myColor);
		Debug.DrawLine(jointPos + upv3 + right, jointPos + (joint.localRotation * Vector3.right) * .2f + upv3 + right, Joints[prevJointIndex].myColor);
		Debug.DrawLine(jointPos + upv3 + right, jointPos + (joint.localRotation * Vector3.forward) * .2f + upv3 + right, Joints[prevJointIndex].myColor);

		Debug.DrawLine(jointPos + upv3, jointPos + (joint.rotation * Vector3.up) * .4f + upv3, Joints[prevJointIndex].myColor);
		Debug.DrawLine(jointPos + upv3, jointPos + (joint.rotation * Vector3.right) * .2f + upv3, Joints[prevJointIndex].myColor);
		Debug.DrawLine(jointPos + upv3, jointPos + (joint.rotation * Vector3.forward) * .2f + upv3, Joints[prevJointIndex].myColor);
	}

	public void InverseKinematics(Vector3 target, float[] angles)
	{
		if (DistanceFromTarget(target, angles) < DistanceThreshold)
		{
			return;
		}

		//Start at the end of the joints and work backwards (core part of INVERSE Kinematics)
		for (int i = Joints.Length - 1; i >= 0; i--)
		{
			// Gradient descent
			// Update : Solution -= LearningRate * Gradient
			if (i < Joints.Length - 1)
			{
				float gradient = PartialGradient(target, angles, i);
				OfficialAngles[i] -= LearningRate * gradient;

				if (ApplyJointClamping)
				{
					angles[i] = Mathf.Clamp(angles[i], Joints[i].MinAngle, Joints[i].MaxAngle);
				}

				// Early termination
				if (DistanceFromTarget(target, angles) < DistanceThreshold)
					return;
			}
		}
	}

	public float PartialGradient(Vector3 target, float[] angles, int angleIndex)
	{
		//Debug.Log("Checking: " + i + " " + angles.Length + "\n");
		// Saves the angle,
		// it will be restored later
		float angle = angles[angleIndex];

		// Gradient : [F(x+SamplingDistance) - F(x)] / h
		float f_x = DistanceFromTarget(target, angles);

		angles[angleIndex] += SamplingDistance;
		float f_x_plus_d = DistanceFromTarget(target, angles);

		float gradient = (f_x_plus_d - f_x) / SamplingDistance;

		// Restores
		angles[angleIndex] = angle;

		return gradient;
	}

	public float PartialGradientWithComfort(Vector3 target, float[] angles, int angleIndex)
	{
		float sample = SamplingDistance;

		sample = sample * Joints[angleIndex].GetComfyDirection();
		//Debug.Log("Checking: " + i + " " + angles.Length + "\n");
		// Saves the angle,
		// it will be restored later
		float angle = angles[angleIndex];

		// Gradient : [F(x+SamplingDistance) - F(x)] / h
		float f_x = DistanceFromTarget(target, angles);

		angles[angleIndex] += sample;


		float f_x_plus_d = DistanceFromTarget(target, angles);

		float gradient = (f_x_plus_d - f_x) / sample;

		// Restores
		angles[angleIndex] = angle;

		return gradient;
	}

	private void SimplifyAngleValues()
	{
		if (ApplyJointClamping)
		{
			for (int i = 0; i < OfficialAngles.Length; i++)
			{
				float currentAngle = Mathf.FloorToInt(OfficialAngles[i]);

				//This lets us keep the angles between 180 to -180
				if (currentAngle > 180)
				{
					float overflow = currentAngle - 180;
					currentAngle = -180 + overflow;
				}
				else if (currentAngle < -180)
				{
					float overflow = currentAngle + 180;
					currentAngle = 180 - overflow;
				}

				OfficialAngles[i] = currentAngle;
			}
		}
	}

	private float UpdateWorldDistanceToTarget()
	{
		return Vector3.Distance(TargetPos, Joints[Joints.Length - 1].transform.position);
	}

	//To solve inverse kinematics we need to minimise the value returned here.
	public float DistanceFromTarget(Vector3 target, float[] angles, bool primaryCall = false)
	{
		Vector3 point = ForwardKinematics(angles);

		//We will use gradient descent here to work towards that.
		return Vector3.Distance(point, target);
	}

	public float GetMaxArmReach()
	{
		//float maxReach = 0;
		Vector3 sumStartOffset = Vector3.zero;
		for (int i = 0; i < Joints.Length; i++)
		{
			sumStartOffset += Joints[i].StartOffset;
		}
		
		//Debug.Log(sumStartOffset + "\n " + sumStartOffset.magnitude + "\n");
		return sumStartOffset.magnitude;
	}
	public float GetCurrentReach()
	{
		Vector3 sumVectorDiff = Vector3.zero;
		for (int i = 1; i < Joints.Length; i++)
		{
			sumVectorDiff += Joints[i].transform.position - Joints[i - 1].transform.position;
		}
		//Debug.Log(sumVectorDiff + "\n " + sumVectorDiff.magnitude + "\n");
		return sumVectorDiff.magnitude;
	}

	public float[] GetAngles()
	{
		return OfficialAngles.ToArray();
	}

	public Vector3 visualizeOffset = Vector3.forward * .1f;

	void OnDrawGizmos()
	{
		bool ShouldJointDraw = true;
		DrawJoints(ShouldJointDraw);
		TargetDraw(ShouldJointDraw);
		LastTargetSphereCluster();

	}
	private void DrawJoints(bool ShouldJointDraw)
	{
		if (ShouldJointDraw && isActiveAndEnabled)
		{
			Gizmos.color = Color.black;
			Vector3 start = Vector3.zero;
			Vector3 end = Vector3.zero;
			bool atEnd = false;
			if (Joints != null)
			{
				for (int i = 0; i < Joints.Length; i++)
				{
					#region Spin Gizmo
					if (Joints[i] != null)
					{
						Gizmos.color = new Color(Joints[i].myColor.r, Joints[i].myColor.g, Joints[i].myColor.b, .75f);

						atEnd = Joints.Length > i + 1;

						//Debug.Log(Joints.Length + "  " + i + "\n" + atEnd);
						start = Joints[0].transform.position;
						end = !atEnd ? start : Joints[i + 1].transform.position;

#if UNITY_EDITOR
						if (DrawComfortDisplaySpheres)
						{
							Vector3 LocalAxis = transform.rotation * Joints[i].Axis;
							UnityEditor.Handles.color = Gizmos.color - new Color(0, 0, 0, .25f);
							UnityEditor.Handles.DrawDottedLine(start + visualizeOffset, end + visualizeOffset, 3);
							//UnityEditor.Handles.DrawSphere(Joints[i].transform.position, LocalAxis, Joints[i].JointGizmoSize);
							//Gizmos.DrawLine(start + visualizeOffset, end + visualizeOffset);
							bool IsComfy = Joints[i].IsJointComfy();
							Gizmos.color = IsComfy ? Joints[i].myColor : Joints[i].myColor - new Color(0, 0, 0, .5f);
							Gizmos.DrawSphere(start + (visualizeOffset * (i + 1)) / 2, .05f);

							if (!IsComfy)
							{
								Vector3 comfySphere = start + (visualizeOffset * (i + 1)) / 2 + Vector3.up * .5f;
								Gizmos.DrawSphere(comfySphere, .05f);
								Gizmos.DrawLine(comfySphere, comfySphere + Vector3.up * Joints[i].GetHowComfy());
							}
						}
#endif
					}
					#endregion
				}
				//				for (int i = 0; i < Joints.Length; i++)
				//				{
				//					if (Joints[i] != null)
				//					{
				//						Gizmos.color = new Color(Joints[i].myColor.r, Joints[i].myColor.g, Joints[i].myColor.b, 1);

				//						atEnd = Joints.Length > i + 1;
				//						start = ForwardKinematics(GetAngles(), i + 1);
				//						end = !atEnd ? start : ForwardKinematics(GetAngles(), i + 2);

				//#if UNITY_EDITOR
				//						Vector3 LocalAxis = transform.rotation * Joints[i].Axis;
				//						UnityEditor.Handles.color = Gizmos.color;
				//						UnityEditor.Handles.DrawDottedLine(start + visualizeOffset * 2, end + visualizeOffset * 2, 3);
				//						//UnityEditor.Handles.DrawSphere(Joints[i].transform.position, LocalAxis, Joints[i].JointGizmoSize);
				//						//Gizmos.DrawLine(start + visualizeOffset, end + visualizeOffset);
				//						Gizmos.DrawSphere(start + visualizeOffset * 2, .05f);
				//#endif
				//					}
				//				}
			}
		}
	}
	private void TargetDraw(bool ShouldJointDraw)
	{
		if (Target != null)
		{
			Gizmos.color = new Color(.8f, .7f, 0.0f, 1.0f);
			Gizmos.DrawSphere(TargetPos, .025f);
			if (ShouldJointDraw)
				Gizmos.DrawSphere(TargetPos + visualizeOffset, .025f);
		}
	}
	private void LastTargetSphereCluster()
	{
		if (LastTargetPosition != Vector3.zero)
		{
			Gizmos.DrawSphere(LastTargetPosition + Vector3.right * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition - Vector3.right * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition + Vector3.up * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition - Vector3.up * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition + Vector3.forward * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition - Vector3.forward * .03f, .01f);
			Gizmos.DrawSphere(LastTargetPosition, .01f);

			Gizmos.DrawLine(LastTargetPosition, TargetPos);
			Gizmos.color = Color.red;
			//Debug.Log(Joints[Joints.Length - 1].name + "\n");
			Gizmos.DrawLine(TargetPos, Joints[Joints.Length - 1].transform.position);
		}
	}
}