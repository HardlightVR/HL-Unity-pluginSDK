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

		private bool initialized = false;

		public void Init(GameObject NewMimicTarget)
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

				transform.position = ObjectToMimic.transform.position + PositionOffset;
				transform.rotation = ObjectToMimic.transform.rotation;
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
			transform.rotation = ObjectToMimic.transform.rotation;
			transform.localScale = ObjectToMimic.transform.localScale + ScaleMultiplier;
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
