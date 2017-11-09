using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public class UpperArmMimic : MonoBehaviour
	{
		public GameObject UpperArmBody;
		public GameObject UpperArmVisual;
		public GameObject Elbow;
		private Transform child;
		public Vector3 TrackerEulerOffset = new Vector3(0,0,0);
		public Vector3 rotatedOffset = new Vector3(0, 0f, -.05f);
		public Vector3 globalOffset = new Vector3(0, 0f, -.05f);
		public HardlightCollider UpperArmCollider;

		[Header("Adjust to control arm tracker orientation")]
		public float zRotation = 0;

		void Update()
		{
			PositionAndRotateVisual();
		}

		public void PositionAndRotateVisual()
		{
			if (child)
			{
				Quaternion euler = Quaternion.identity;
				euler.eulerAngles = TrackerEulerOffset;

				Quaternion rot = transform.rotation;
				var newOffset = euler * rotatedOffset;
				
				//Incorporate our ability to offset based on euler angles.
				child.transform.localPosition = newOffset + globalOffset;
			}
		}

		public void AssignSide(ArmSide side)
		{
			if (UpperArmCollider != null)
			{
				if (side == ArmSide.Left)
				{
					if (UpperArmCollider.regionID.ContainsArea(AreaFlag.Upper_Arm_Right))
					{
						UpperArmCollider.MyLocation.Where = UpperArmCollider.MyLocation.Where.AddArea(AreaFlag.Upper_Arm_Left);
						UpperArmCollider.MyLocation.Where = UpperArmCollider.MyLocation.Where.RemoveArea(AreaFlag.Upper_Arm_Right);
					}
				}
				else if (side == ArmSide.Right)
				{
					if (UpperArmCollider.regionID.ContainsArea(AreaFlag.Upper_Arm_Left))
					{
						UpperArmCollider.MyLocation.Where = UpperArmCollider.MyLocation.Where.AddArea(AreaFlag.Upper_Arm_Right);
						UpperArmCollider.MyLocation.Where = UpperArmCollider.MyLocation.Where.RemoveArea(AreaFlag.Upper_Arm_Left);
					}
				}
			}
		}
		public void Mirror()
		{
			EnsureChildValidity();
			if (child)
				child.transform.Rotate(new Vector3(0, 0, 180));
		}

		public Vector3 GetUp()
		{
			EnsureChildValidity();
			if (child)
				return -child.transform.up;

			return Vector3.up;
		}

		private void EnsureChildValidity()
		{
			if (!child)
				child = transform.GetChild(0);
		}

		public Vector3 GetRight()
		{
			EnsureChildValidity();
			if (child)
				return -child.transform.right;
			return Vector3.right;
		}
		public Vector3 GetForward()
		{
			EnsureChildValidity();
			if (child)
				return -child.transform.forward;
			return Vector3.forward;
		}

		//void OnDrawGizmos()
		//{
		//	Gizmos.color = Color.green;
		//	Gizmos.DrawLine(transform.position, transform.position + transform.up * .1f);
		//	Gizmos.color = Color.red;
		//	Gizmos.DrawLine(transform.position, transform.position + transform.right * .1f);
		//	Gizmos.color = Color.blue;
		//	Gizmos.DrawLine(transform.position, transform.position + transform.forward * .1f);
		//}
	}
}