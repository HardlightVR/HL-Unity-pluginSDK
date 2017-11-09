using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public class ViveArmRepresentation : MonoBehaviour
	{
		public GameObject wristObject;
		public GameObject elbowObject;

		public GameObject ForearmRepresentation;

		public Color GizmoColor = Color.green;
		public float ForearmLength = .15f;

		[Range(0, 1)]
		public float PercentagePlacement = .5f;

		[Header("Runtime Defined")]
		public Vector3 elbowToWrist = Vector3.zero;
		public float potentialForearmDistance;

		//Index 
		void Start()
		{

		}

		void Update()
		{
			CalculateAndAssignForearmPosition();

			ScaleForearmSize();

			HandleForearmOrientation();
		}

		private void CalculateAndAssignForearmPosition()
		{
			elbowToWrist = wristObject.transform.position - elbowObject.transform.position;
			potentialForearmDistance = elbowToWrist.magnitude;
			float percentage = (potentialForearmDistance) * PercentagePlacement;

			transform.position = elbowObject.transform.position + elbowToWrist.normalized * percentage;
		}

		private void ScaleForearmSize()
		{
			Vector3 newScale = ForearmRepresentation.transform.localScale;
			newScale.y = potentialForearmDistance * .4f;
			ForearmRepresentation.transform.localScale = newScale;
		}

		private void HandleForearmOrientation()
		{
			Debug.DrawLine(Vector3.zero, elbowToWrist, Color.black);
			Vector3 cross = Vector3.Cross(wristObject.transform.right, wristObject.transform.up);
			Vector3 dir = -elbowObject.transform.right;
			transform.LookAt(wristObject.transform, dir);
			Debug.DrawLine(elbowObject.transform.position, elbowObject.transform.position + dir * .35f, GizmoColor);

			//transform.LookAt(transform.position + cross * 5);
			Debug.DrawLine(transform.position, transform.position + cross * .35f, GizmoColor);
		}

		void OnDrawGizmos()
		{
			Gizmos.color = GizmoColor;
		}
	}
}