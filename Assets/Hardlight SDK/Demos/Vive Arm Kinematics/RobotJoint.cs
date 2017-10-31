using UnityEngine;

public class RobotJoint : MonoBehaviour
{
	public Vector3 Axis = Vector3.right;
	public Vector3 StartOffset;

	//Not used, commented out in ForwardKinematicArms
	public float MinAngle = -180;
	public float MaxAngle = 180;

	public float AngleInMyAxis;

	public float AngleFromUp;
	public float AngleFromRight;
	public float AngleFromComfort;

	/// <summary>
	/// The intended up direction for this joint.
	/// </summary>
	public Vector3 ComfortUp = Vector3.zero;
	public Vector3 ComfortRight = Vector3.zero;

	public Color myColor = Color.white;
	public MeshRenderer rend;

	//The axis of rotation disc
	public float JointDiscSize = .05f;

	//The general size of a number of the gizmos.
	public float JointGizmoSize = .1f;

	public bool DrawJointGizmos = true;
	public bool IAmComfy = false;

	public bool IsJointComfy()
	{
		return (AngleInMyAxis > MinAngle && AngleInMyAxis < MaxAngle);
	}
	public int GetComfyDirection()
	{
		if (AngleInMyAxis < MaxAngle)
			return 1;
		if (AngleInMyAxis > MinAngle)
			return -1;
		return 1;
	}

	public float GetHowComfy()
	{
		float comfortAssessmentValue = 0;
		if (AngleInMyAxis < MinAngle)
		{
			//-150 - 130
			comfortAssessmentValue= Mathf.Log(MinAngle - AngleInMyAxis);
			return comfortAssessmentValue;
		}
		else if (AngleInMyAxis > MaxAngle)
		{
			comfortAssessmentValue = Mathf.Log(MaxAngle - AngleInMyAxis);
			return comfortAssessmentValue;
		}
		return 0;
	}

	void Awake()
	{
		StartOffset = transform.localPosition;
		if (rend != null)
			rend.material.color = myColor;
	}

	void Update()
	{
		IAmComfy = IsJointComfy();
		if (ComfortUp == Vector3.zero || ComfortRight == Vector3.zero)
		{
			AngleFromRight = 0;
			AngleFromUp = 0;
		}
		else
		{
			AngleFromRight = Vector3.Angle(ComfortRight, transform.right);
			AngleFromUp = Vector3.Angle(ComfortUp, transform.up);
		}

		AngleFromComfort = AngleFromRight + AngleFromUp;
	}

	void OnDrawGizmos()
	{
		if (DrawJointGizmos)
			DrawDisplayGizmos();
	}

	void DrawDisplayGizmos()
	{
		//Start at transform
		//Draw 6 circles on the axis of rotation.

#if UNITY_EDITOR
		Vector3 LocalAxis = transform.rotation * Axis;
		UnityEditor.Handles.color = myColor;
		Gizmos.color = myColor;

		//This draws the axis of rotation for this joint
		UnityEditor.Handles.DrawDottedLine(transform.position - LocalAxis * JointGizmoSize, transform.position + LocalAxis * JointGizmoSize, 5);

		//This draws the circle that the object rotates around.
		UnityEditor.Handles.DrawWireDisc(transform.position, LocalAxis, JointDiscSize);

		//This draws the visual indication of my comfort up.
		#region Draw Up Comfort
		if (ComfortUp != Vector3.zero)
		{
			Gizmos.color = myColor - new Color(0, 0, 0, .25f);
			Vector3 comfortPos = transform.position + ComfortUp * JointGizmoSize + Axis * .005f;
			Vector3 upPos = transform.position + transform.up * JointGizmoSize * 1.25f + Axis * .005f;
			Gizmos.DrawSphere(comfortPos, .01f);
			Gizmos.DrawSphere(upPos, .015f);
			Gizmos.color = Color.blue - new Color(0, 0, 0, .25f);
			UnityEditor.Handles.color = Gizmos.color + new Color(0, 0, 0, .5f);
			Gizmos.DrawLine(comfortPos, comfortPos + Vector3.up * .02f);
			Gizmos.DrawLine(upPos, upPos + Vector3.up * .02f);

			UnityEditor.Handles.DrawLine(comfortPos, upPos);
		}
		#endregion

		//This draws the visual indication of my comfort right.
		#region Draw Right Comfort
		if (ComfortRight != Vector3.zero)
		{
			Gizmos.color = myColor - new Color(0, 0, 0, .25f);
			Vector3 comfortPos = transform.position + ComfortRight * JointGizmoSize + Axis * .005f;
			Vector3 rightPos = transform.position + transform.right * JointGizmoSize * 1.25f + Axis * .005f;
			Gizmos.DrawSphere(comfortPos, .01f);
			Gizmos.DrawSphere(rightPos, .015f);
			Gizmos.color = Color.red - new Color(0, 0, 0, .25f);
			UnityEditor.Handles.color = Gizmos.color + new Color(0, 0, 0, .5f);
			Gizmos.DrawLine(comfortPos, comfortPos + Vector3.right * .02f);
			Gizmos.DrawLine(rightPos, rightPos + Vector3.right * .02f);

			UnityEditor.Handles.DrawLine(comfortPos, rightPos);
		}
		#endregion
#endif
	}

}