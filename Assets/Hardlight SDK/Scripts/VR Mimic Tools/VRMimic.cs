using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Hardlight.SDK
{
	/// <summary>
	/// A singleton style class that manages creating mimic objects that copy the movement/orientation of tracked VR objects (playspace, headset, controllers, tracked objects)
	/// This prevents the need to be tied to SteamVR or Oculus SDKs
	/// </summary>
	public class VRMimic : MonoBehaviour
	{
		[Header("VR Mimic creates children that copy", order = 0)]
		[Space(-10, order = 1)]
		[Header("  the movement of VR Objects", order = 2)]
		[Space(-5, order = 3)]
		[Header("It also creates a smart player body", order = 4)]
		[Space(-10, order = 5)]
		[Header("  for quality positional haptics", order = 6)]

		public bool SkipDebugLogErrors = false;

		private static VRMimic instance;
		public static VRMimic Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<VRMimic>();

					if (FindObjectsOfType<VRMimic>().Length > 1)
					{
						if (!instance.SkipDebugLogErrors)
						{
							Debug.LogError("[VRMimic] There is more than one VRMimic Singleton\n" +
								"There shouldn't be multiple VRMimic objects");
						}
						return instance;
					}

					if (instance == null)
					{
						Debug.LogError("Attemping to reference the Instance of VRMimic tools but Initialize() has not occurred.\n\tCall VRMimic.Initialize() in a Start or Awake function before trying to reference the static Instance.\n");
						//Initialize();
					}
					else
					{
						//Debug.Log("[Singleton] Using instance already created: " +
						//	_instance.gameObject.name + "\n");
					}
				}
				return instance;
			}
			set { instance = value; }
		}

		private bool initialized = false;
		[SerializeField]
		private VRObjectMimic _vrCamera = null;
		public VRObjectMimic VRCamera
		{
			get
			{
				if (!initialized)
				{
					if (!SkipDebugLogErrors)
					{
						Debug.LogError("VRMimic not yet initialized before attempting to get the VRCamera. Will attempt setup assuming Camera.main is the VR Camera.\n");
					}
				}
				return _vrCamera;
			}
		}

		[SerializeField]
		public VRObjectMimic CameraRig;

		public GameObject TrackerMimicFolder;

		#region Tracked Objects
		private List<VRObjectMimic> _trackedObjects;
		public List<VRObjectMimic> TrackedObjects
		{
			get
			{
				if (_trackedObjects == null)
				{
					_trackedObjects = new List<VRObjectMimic>();
				}
				return _trackedObjects;
			}
			set { _trackedObjects = value; }
		}

		private Dictionary<VRObjectMimic.TypeOfMimickedObject, List<VRObjectMimic>> _trackedObjectDict = new Dictionary<VRObjectMimic.TypeOfMimickedObject, List<VRObjectMimic>>();
		public List<VRObjectMimic> GetTrackedObjectsOfType(VRObjectMimic.TypeOfMimickedObject Key)
		{
			if (_trackedObjectDict.ContainsKey(Key))
			{
				if (_trackedObjectDict[Key] == null)
					_trackedObjectDict[Key] = new List<VRObjectMimic>();
				return _trackedObjectDict[Key];
			}
			return new List<VRObjectMimic>();
		}
		public void RecordTrackedObject(VRObjectMimic.TypeOfMimickedObject Key, VRObjectMimic newObject)
		{
			if (!_trackedObjectDict.ContainsKey(Key))
			{
				_trackedObjectDict.Add(Key, new List<VRObjectMimic>());
			}
			if (!_trackedObjectDict[Key].Contains(newObject))
			{
				_trackedObjectDict[Key].Add(newObject);
			}
		}

		public VRObjectMimic GetTrackedObject(int index)
		{
			bool arrayIsNull = TrackedObjects != null;
			if (arrayIsNull && TrackedObjects.Count > index)
			{
				if (TrackedObjects[index] != null)
				{
					return TrackedObjects[index];
				}

			}
			else
			{
				string failureReason = (arrayIsNull ? "there are no tracked objects" : "there are only [" + TrackedObjects.Count + "] tracked objects (excluding controlers)");
				Debug.LogError("Requested Invalid Tracked Object at index [" + index + " when " + failureReason + "\n");
			}
			return null;
		}
		/// <summary>
		/// Sets an object to be tracked. The VRObjectMimic will regularly update based on the tracked object.
		/// Adds a WatchedByMimic component which can be used to get the VRObjectMimic
		/// </summary>
		/// <param name="objectToMimic"></param>
		/// <param name="mimicType"></param>
		public VRObjectMimic AddTrackedObject(GameObject objectToMimic, VRObjectMimic.TypeOfMimickedObject mimicType = VRObjectMimic.TypeOfMimickedObject.TrackedObject, MimickingOptions options = null)
		{
			WatchedByMimic watching = objectToMimic.GetComponent<WatchedByMimic>();
			bool AlreadyTracked = (watching != null);

			//Debug.Log("Adding Tracked Object [" + objectToMimic.name + "] of type [" + mimicType.ToString() + "]\n");
			if (!AlreadyTracked)
			{
				//Make a new mimic object
				GameObject go = new GameObject();
				go.transform.SetParent(CameraRig.transform);
				go.name = "Tracked Object Mimic [" + TrackedObjects.Count + "]";
				VRObjectMimic newTracked = go.AddComponent<VRObjectMimic>();
				newTracked.ObjectToMimic = objectToMimic;
				TrackedObjects.Add(newTracked);
				newTracked.MimickedObjectType = mimicType;
				newTracked.Options = (options == null) ? (new MimickingOptions()) : options;

				RecordTrackedObject(mimicType, newTracked);

				return newTracked;
			}
			if (watching.WatchingMimic == null)
			{
				throw new System.Exception("An unusual exception has occurred. The WatchedByMimic.WatchingMimic is no longer valid.\n\tDid you modify the object watching " + objectToMimic.name + "?");
			}

			return watching.WatchingMimic;

			//throw new System.Exception("Not Implemented Exception - returning already tracked object\n");
		}
		#endregion

		public VRObjectMimic FindMimicOfObject(GameObject watchedObject, uint index = 0)
		{
			var watchers = FindWatchersOfObject(watchedObject);

			if (watchers != null)
			{
				if (watchers.Length > index)
				{
					return watchers[index].WatchingMimic;
				}
			}

			return null;
		}
		public WatchedByMimic[] FindWatchersOfObject(GameObject uncertainObject)
		{
			var watchers = uncertainObject.GetComponents<WatchedByMimic>();

			if (watchers != null)
			{
				if (watchers.Length > 0)
				{
					return watchers;
				}
			}
			return null;
		}

		/// <summary>
		/// A reference to the body mimic for if you need to change runtime parameters.
		/// </summary>
		[SerializeField]
		private BodyMimic _bodyMimic;
		/// <summary>
		/// The BodyMimic (which contains a HardlightSuit/Definition, for haptic functionality)
		/// </summary>
		public BodyMimic ActiveBodyMimic
		{
			get
			{ return _bodyMimic; }

			set
			{ _bodyMimic = value; }
		}

		private void Init(GameObject vrCamera = null, int hapticLayer = HardlightManager.HAPTIC_LAYER)
		{
			//Debug.Log("Attempting initialization " + initialized + "  cam null? " + (vrCamera == null) + "\n");
			if (!initialized)
			{
				//VRObjectMimic.InitializeVRCamera(vrCamera);

				//Set up VRCamera
				VRObjectMimic setupCameraMimic = InitVRCamera(vrCamera);

				//Set up CameraRig
				VRObjectMimic setupRigMimic = InitCameraRig(vrCamera);

				//Set up the Body Mimic
				InitBodyMimic(setupCameraMimic, setupRigMimic, hapticLayer);

				initialized = true;
			}
		}

		private VRObjectMimic InitVRCamera(GameObject vrCamera = null)
		{
			GameObject go = new GameObject();
			go.transform.SetParent(transform);
			go.name = "[" + vrCamera.name + "] Mimic";
			_vrCamera = go.AddComponent<VRObjectMimic>();
			_vrCamera.Init(vrCamera);
			_vrCamera.MimickedObjectType = VRObjectMimic.TypeOfMimickedObject.Camera;
			return _vrCamera;
		}

		private VRObjectMimic InitCameraRig(GameObject vrCamera = null)
		{
			if (vrCamera.transform.parent != null)
			{
				GameObject go = new GameObject();
				go.transform.SetParent(transform);
				go.name = "[Camera Rig Mimic]";

				CameraRig = go.AddComponent<VRObjectMimic>();
				CameraRig.Init(vrCamera.transform.parent.gameObject);
				CameraRig.ObjectToMimic = vrCamera.transform.parent.gameObject;
				CameraRig.MimickedObjectType = VRObjectMimic.TypeOfMimickedObject.CameraRig;
				return CameraRig;
			}
			Debug.LogError("Failed to initialize Camera Rig Mimic.\n\tLikely can't identify the camera rig root for the provided vr camera.\n");
			return null;
		}

		private void InitBodyMimic(VRObjectMimic vrCamera, VRObjectMimic cameraRigMimic, int hapticLayer = HardlightManager.HAPTIC_LAYER)
		{
			Camera playerCameraToHideBodyFrom = vrCamera.ObjectToMimic.GetComponent<Camera>();
			ActiveBodyMimic = BodyMimic.Initialize(playerCameraToHideBodyFrom, cameraRigMimic, hapticLayer);
			HardlightSuit.Find().SetColliderState();
		}

		private void InitTrackerMimicFolder()
		{
			//Create a folder for mimicked objects (trackers)
			GameObject go = new GameObject();
			go.name = "Tracker Mimic Folder";
			TrackerMimicFolder = go;

			Debug.Assert(CameraRig != null, "Asserting Camera Rig is not null. Likely an upstream failure caused by an invalid camera being passed in.", this);

			go.transform.SetParent(CameraRig.transform);
		}

		/// <summary>
		/// A bool check to see if the Instance field is null.
		/// </summary>
		/// <returns></returns>
		public static bool ValidInstance()
		{
			if (instance == null)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Initializes and sets up the VR Mimic tools. Creates a playspace and camera mimic.
		/// Removes the haptic layer from the camera's culling mask (hides objects on that layer)
		/// </summary>
		/// <param name="vrCamera"></param>
		/// <param name="hapticLayer"></param>
		public static void Initialize(GameObject vrCamera, int hapticLayer = HardlightManager.HAPTIC_LAYER)
		{
			if (ValidInstance())
			{
				Debug.LogError("VRMimic is already initialized\nDoes not yet support multiple initializations", VRMimic.Instance);
			}
			else if (vrCamera == null)
			{
				Debug.LogError("VRMimic initialization was requested but a null VR Camera parameter was provided. Aborting\n", VRMimic.Instance);
			}
			else
			{
				//Debug.Log("VR Camera [" + vrCamera.name + "] for initialization\n");
				GameObject singleton = new GameObject();
				instance = singleton.AddComponent<VRMimic>();
				instance.Init(vrCamera, hapticLayer);
				singleton.name = "VRMimic [Runtime Singleton]";
			}
		}
	}
}