using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK
{
	/// <summary>
	/// Creates observing objects that mimic the movement and orientation of their mimic'd objects.
	/// Useful to avoid nested prefabs & for creating an intermediate step to do movement processing.
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
		/// <summary>
		/// The public reference for getting access to the VRMimic.
		/// </summary>
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
			set
			{
				if (value != null)
					instance = value;
			}
		}
		//public Dictionary<VRObjectMimic.TypeOfMimickedObject, int> Mimics;

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
		public VRObjectMimic AddTrackedObject(GameObject objectToMimic, VRObjectMimic.TypeOfMimickedObject mimicType = VRObjectMimic.TypeOfMimickedObject.TrackedObject)
		{
			bool AlreadyTracked = false;

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

				return newTracked;
			}

			throw new System.Exception("Not Implemented Exception - returning already tracked object\n");
			//return null;
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
		public BodyMimic ActiveBodyMimic
		{
			get
			{
				return _bodyMimic;
			}

			set
			{
				_bodyMimic = value;
			}
		}

		private void Init(GameObject vrCamera = null, int hapticLayer = NSManager.HAPTIC_LAYER)
		{
			//Debug.Log("Attempting initialization " + initialized + "  cam null? " + (vrCamera == null) + "\n");
			if (!initialized)
			{
				//Suit disconnect events are not consistent enough yet.
				//NSManager.Instance.SuitConnected += OnSuitDisconnect;

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

			//This is future feature for letting developers check 'head proximity collisions'
			go = new GameObject();
			var collider = go.AddComponent<SphereCollider>();
			collider.isTrigger = true;
			collider.radius = .35f;
			var rb = go.AddComponent<Rigidbody>();
			rb.constraints = RigidbodyConstraints.FreezeAll;
			go.name = "Player Head Collider";
			go.transform.SetParent(_vrCamera.transform);

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

		private void InitBodyMimic(VRObjectMimic vrCamera, VRObjectMimic cameraRigMimic, int hapticLayer = NSManager.HAPTIC_LAYER)
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
		/// This feature isn't completely finished yet. The lower level disconnect events sometimes occur even when a disconnect did not occur.
		/// </summary>
		private void OnSuitDisconnect(object sender, SuitConnectionArgs e)
		{
			CreateSuitDisconnectNotification();
		}

		private void CreateSuitDisconnectNotification()
		{
			var prefab = Resources.Load("Disconnect Notification");
			if (prefab != null)
			{
				GameObject go = GameObject.Instantiate(prefab) as GameObject;
				go.GetComponent<FollowGazeCenter>().CameraToFollow = VRCamera.ObjectToMimic.GetComponent<Camera>();
				go.name = "Disconnect Notification";
			}
		}

		public static bool ValidInstance()
		{
			if (instance == null)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Initializes and sets up 
		/// </summary>
		/// <param name="vrCamera"></param>
		/// <param name="hapticLayer"></param>
		public static void Initialize(GameObject vrCamera, int hapticLayer = NSManager.HAPTIC_LAYER)
		{
			if (ValidInstance())
			{
				Debug.LogError("VRMimic is already initialized\nDoes not yet support multiple initializations");
			}
			else if (vrCamera == null)
			{
				Debug.LogError("VRMimic was requested initialization with a null VR Camera\n");
			}
			else
			{
				//Debug.Log("VR Camera [" + vrCamera.name + "] for initialization\n");
				GameObject singleton = new GameObject();
				instance = singleton.AddComponent<VRMimic>();
				instance.Init(vrCamera, hapticLayer);
				singleton.name = "VRMimic [Runtime Singleton]";

				if (vrCamera != null && vrCamera.transform.parent != null && vrCamera.transform.parent.parent != null)
				{
					singleton.transform.SetParent(vrCamera.transform.parent.parent);
				}

			}
		}
	}
}