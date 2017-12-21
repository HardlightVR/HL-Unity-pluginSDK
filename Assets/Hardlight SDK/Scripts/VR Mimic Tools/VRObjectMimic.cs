using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hardlight.SDK
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
		private bool initialized = false;

		[SerializeField]
		private MimickingOptions _mimicOptions;
		public MimickingOptions Options
		{
			get
			{
				if (_mimicOptions == null)
					_mimicOptions = new MimickingOptions();
				return _mimicOptions;
			}

			set
			{
				if (_mimicOptions == null && value == null)
					_mimicOptions = new MimickingOptions();
				_mimicOptions = value;
			}
		}

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

				if (Options.MimicPosition)
					transform.position = ObjectToMimic.transform.position + Options.PositionOffset;
				if (Options.MimicRotation)
					transform.rotation = ObjectToMimic.transform.rotation * CalculateOffsetQuaternion();
				if (Options.MimicScale)
					transform.localScale = ObjectToMimic.transform.localScale + Options.ScaleMultiplier;
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
			if (Options.MimicPosition)
				transform.position = ObjectToMimic.transform.position + Options.PositionOffset;
			if (Options.MimicRotation)
				transform.rotation = ObjectToMimic.transform.rotation * CalculateOffsetQuaternion();
			if (Options.MimicScale)
				transform.localScale = ObjectToMimic.transform.localScale + Options.ScaleMultiplier;
		}

		private Quaternion CalculateOffsetQuaternion()
		{
			Quaternion offset = Quaternion.identity;
			offset.eulerAngles = Options.EulerOffset;
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
