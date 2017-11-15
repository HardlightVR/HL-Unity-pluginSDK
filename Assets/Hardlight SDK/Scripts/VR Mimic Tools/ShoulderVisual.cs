using UnityEngine;
using System.Collections;
using System;

//Contents of this namespace are subject to change
namespace Hardlight.SDK.Experimental
{
	/// <summary>
	/// Basically a 2 ball-bearing & shoulder bar visual.
	/// I want to generalize this for further tracking development
	/// </summary>
	public class ShoulderVisual : MonoBehaviour
	{
		public GameObject ShoulderBarRepresentation;

		bool initialized = false;
		public AbsoluteArmMimic LeftArm;
		public AbsoluteArmMimic RightArm;

		public void Setup(AbsoluteArmMimic lArm, AbsoluteArmMimic rArm)
		{
			if (lArm == null || rArm == null || ShoulderBarRepresentation == null)
			{
				gameObject.SetActive(false);
			}
			LeftArm = lArm;
			RightArm = rArm;
			initialized = true;
		}
		void Start()
		{
			if (!initialized)
				gameObject.SetActive(false);
		}

		void Update()
		{
			if (ShoulderBarRepresentation != null)
			{
				Transform shoulder = ShoulderBarRepresentation.transform;
				float distance = GetAnchorDistance();
				Vector3 leftToRight = RightArm.ShoulderJoint.transform.position - LeftArm.ShoulderJoint.transform.position;
				shoulder.position = LeftArm.ShoulderJoint.transform.position + leftToRight.normalized * distance / 2;
				Debug.DrawLine(LeftArm.ShoulderJoint.transform.position, LeftArm.ShoulderJoint.transform.position+ leftToRight.normalized, Color.black);
				Vector3 stretchLocalScale = shoulder.localScale;
				stretchLocalScale.z = distance * 3.2f;
				shoulder.localScale = stretchLocalScale;

				//Debug.DrawLine(shoulder.position, shoulder.position + GetForward(), Color.black);
				//Debug.DrawLine(shoulder.position, shoulder.position + GetUp(), Color.white);
				//Debug.DrawLine(shoulder.position, shoulder.position + GetRight(), Color.grey);
				Vector3 upDir = GetUp();

				shoulder.LookAt(RightArm.ShoulderJoint.transform.position, upDir);
			}
		}

		private float GetAnchorDistance()
		{
			return Vector3.Distance(LeftArm.ShoulderJoint.transform.position, RightArm.ShoulderJoint.transform.position);
		}

		private Vector3 GetCenterAnchorPosition()
		{
			Debug.DrawLine(transform.position, RightArm.ShoulderJoint.transform.position, Color.red);
			Debug.DrawLine(transform.position, LeftArm.ShoulderJoint.transform.position, Color.yellow);
			return (LeftArm.ShoulderJoint.transform.position + RightArm.ShoulderJoint.transform.position) / 2;
		}
		private Vector3 GetUp()
		{
			return (LeftArm.UpperArmData.GetUp() + RightArm.UpperArmData.GetUp()) / 2;
		}
		private Vector3 GetRight()
		{
			return (LeftArm.UpperArmData.GetRight() + RightArm.UpperArmData.GetRight()) / 2;
		}
		private Vector3 GetForward()
		{
			return (LeftArm.UpperArmData.GetForward() + RightArm.UpperArmData.GetForward()) / 2;
		}
	}
}