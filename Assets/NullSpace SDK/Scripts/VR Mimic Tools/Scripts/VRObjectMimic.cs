using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK
{
	public class VRObjectMimic : MonoBehaviour
	{
		public GameObject ObjectToMimic;
		/// <summary>
		/// Future feature - track if the headset is idle/active. Same with the controllers.
		/// This allows for default behavior.
		/// </summary>
		public enum DetectionState { Active, Idle }
		public enum TypeOfMimickedObject { Camera, ControllerA, ControllerB, CameraRig, TrackedObject }
		public TypeOfMimickedObject MimickedObjectType;

		public Vector3 ScaleMultiplier;
		public Vector3 PositionOffset;
		public Vector3 EulerOffset;

		private bool initialized = false;

		public void Init(GameObject NewMimicTarget, Vector3 rotationOffset = default(Vector3))
		{
			if (!initialized)
			{
				if (ObjectToMimic == null || NewMimicTarget != null)
				{
					ObjectToMimic = NewMimicTarget;
				}

				if (NewMimicTarget == null)
				{
					Debug.Log(name + " - New mimic target is null\n");
				}

				EulerOffset = rotationOffset;

				transform.position = ObjectToMimic.transform.position + PositionOffset;
				transform.rotation = ObjectToMimic.transform.rotation * CalculateOffsetQuaternion();
				transform.localScale = ObjectToMimic.transform.localScale + ScaleMultiplier;
				initialized = true;

				WatchedByMimic watching = NewMimicTarget.GetComponent<WatchedByMimic>();

				if (watching != null)
				{
					Debug.LogError("Multiple mimics are watching [" + NewMimicTarget.name + "]\n\tAdded additional watcher, but you might fail to get the correct watcher.");
				}
				watching = NewMimicTarget.AddComponent<WatchedByMimic>();
				watching.WatchingMimic = this;
			}
		}

		void Start()
		{
		}

		void Update()
		{
			transform.position = ObjectToMimic.transform.position + PositionOffset;
			transform.rotation =  ObjectToMimic.transform.rotation * CalculateOffsetQuaternion();
			transform.localScale = ObjectToMimic.transform.localScale + ScaleMultiplier;
		}

		private Quaternion CalculateOffsetQuaternion()
		{
			Quaternion offset = Quaternion.identity;
			offset.eulerAngles = EulerOffset;
			return offset;
		}

		void OnDrawGizmos()
		{
			string icon = (MimickedObjectType == TypeOfMimickedObject.Camera ? "hmd-icon" : MimickedObjectType == TypeOfMimickedObject.TrackedObject ? "tracker-icon" : "controller-icon");
			Gizmos.DrawIcon(transform.position, icon, true);
		}

		public static GameObject FindCameraObject()
		{
			GameObject cameraObject = Camera.main.gameObject;

			//Find the headset and each of the controllers
			//		This solution is the most efficient, but couples us to steam vr.
			//		cameraObject = FindObjectOfType<SteamVR_Camera>().gameObject;

			return cameraObject;
		}
	}
}
