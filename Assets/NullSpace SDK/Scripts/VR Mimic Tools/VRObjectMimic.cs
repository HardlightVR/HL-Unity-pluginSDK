using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class VRObjectMimic : MonoBehaviour
	{
		private static MimickedObjects _holder;
		public static MimickedObjects Get(GameObject coreCamera = null)
		{
			if (_holder == null)
			{
				_holder = Initialize(coreCamera);
			}
			return _holder;
		}

		public GameObject ObjectToMimic;
		/// <summary>
		/// Future feature - track if the headset is idle/active. Same with the controllers.
		/// This allows for default behavior.
		/// </summary>
		public enum DetectionState { Active, Idle }
		public enum MimickedObject { Camera, ControllerA, ControllerB }
		public MimickedObject MimickedObjectType;

		public Vector3 ScaleMultiplier;
		public Vector3 PositionOffset;

		private bool initialized = false;


		void Init(GameObject NewMimicTarget)
		{
			if (!initialized)
			{
				ObjectToMimic = NewMimicTarget;

				transform.position = ObjectToMimic.transform.position + PositionOffset;
				transform.rotation = ObjectToMimic.transform.rotation;
				transform.localScale = ObjectToMimic.transform.localScale + ScaleMultiplier;
				initialized = true;
			}
		}

		void Start()
		{
			Init(null);
		}

		void Update()
		{
			transform.position = ObjectToMimic.transform.position + PositionOffset;
			transform.rotation = ObjectToMimic.transform.rotation;
			transform.localScale = ObjectToMimic.transform.localScale + ScaleMultiplier;
		}

		public static MimickedObjects Initialize(GameObject CameraToMimic = null)
		{
			if (CameraToMimic == null)
			{
				//We assume the main camera is the VR Camera.
				CameraToMimic = FindCameraObject();
			}

			MimickedObjects mimickingObjects = new MimickedObjects();

			VRMimic parent = VRMimic.Instance;
			parent.name = "VR Mimic Objects";
			mimickingObjects.Root = parent.gameObject;

			GameObject go = new GameObject();
			go.transform.SetParent(parent.transform);
			go.name = "Camera Mimic";
			mimickingObjects.VRCamera = go.AddComponent<VRObjectMimic>();
			mimickingObjects.VRCamera.Init(CameraToMimic);
			mimickingObjects.VRCamera.MimickedObjectType = MimickedObject.Camera;

			//mimickingObjects.ControllerA = controllers.First();
			//mimickingObjects.ControllerB = controllers.Last();

			_holder = mimickingObjects;

			return Get(CameraToMimic);
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

	public class MimickedObjects
	{
		public GameObject Root;
		public VRObjectMimic VRCamera;
		//public VRObjectMimic ControllerA;
		//public VRObjectMimic ControllerB;
	}
}
