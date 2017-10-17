using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class FrameEvaluator : MonoBehaviour
	{
		public Transform AbsoluteObject;
		public Transform IMUObject;
		public Quaternion OffsetA;
		public Quaternion OffsetB;
		public GameObject prefab;
		public GameObject displayA;
		public GameObject displayB;

		void Start()
		{
			displayA = GameObject.Instantiate<GameObject>(prefab);
			displayA.transform.SetParent(transform);
			displayA.transform.position = Vector3.up * 3 + Vector3.right * 1;

			displayB = GameObject.Instantiate<GameObject>(prefab);
			displayB.transform.SetParent(transform);
			displayB.transform.position = Vector3.up * 3 + Vector3.right * 2;
		}

		void Update()
		{
			if (AbsoluteObject && IMUObject)
			{
				OffsetA = Subtract(AbsoluteObject.rotation, IMUObject.rotation);
				OffsetB = Subtract(IMUObject.rotation, AbsoluteObject.rotation);

				displayA.transform.rotation = OffsetA;
				displayB.transform.rotation = OffsetB;
				//				Quaternion newRotation = transform.rotation * otherTransform.rotation
				//				transform.rotation = newRotation * Quaternion.Inverse(otherTransform.rotation)
			}
		}
		private Quaternion Subtract(Quaternion A, Quaternion B)
		{
			return A * Quaternion.Inverse(B);

		}
	}
}