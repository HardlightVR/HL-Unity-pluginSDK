using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hardlight.SDK.Tracking
{
	/// <summary>
	/// The core class for the VR Mimic tools. It is used to mimic cameras, controllers, camera rigs and tracked objects.
	/// It also provides the ability for a positional, scalar and euler rotation offset of the mimicked object 
	/// </summary>
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

		//All three of these variables might be replaced with at transformation matrix, allowing for better mimic usage.
		//This would be used for controlling a giant entity that towers above you, copying your movements
		public Vector3 ScaleMultiplier;
		//This would be used for controlling an entity N units away from you, copying your movements.
		public Vector3 PositionOffset;
		//This is a temporarily feature, it will be replaced with better rotation changes.
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
			string icon = (MimickedObjectType == TypeOfMimickedObject.Camera ? "VRObjectMimic HMD Icon" : MimickedObjectType == TypeOfMimickedObject.TrackedObject ? "VRObjectMimic Tracker Icon" : "VRObjectMimic Controller Icon");
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
