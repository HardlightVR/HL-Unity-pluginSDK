using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
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
		public VRObjectMimic VRCamera
		{
			get
			{
				if (!initialized)
				{
					if (!SkipDebugLogErrors)
					{
						Debug.LogError("VRMimic not yet initialized before attempting to get the VRCamera. Will attempt setup assuming Camera.main is the VR Camera.");
					}
				}
				return VRObjectMimic.Get().VRCamera;
			}
		}

		/// <summary>
		/// A reference to the body mimic for if you need to change runtime parameters.
		/// </summary>
		public BodyMimic body;

		private void Init(GameObject vrCamera = null, int hapticLayer = NSManager.HAPTIC_LAYER)
		{
			if (!initialized)
			{
				VRObjectMimic.Initialize(vrCamera);
				InitBodyMimic(hapticLayer);
				initialized = true;
			}
		}

		private void InitBodyMimic(int hapticLayer = NSManager.HAPTIC_LAYER)
		{
			Camera playerCameraToHideBodyFrom = VRObjectMimic.Get().VRCamera.ObjectToMimic.GetComponent<Camera>();
			body = BodyMimic.Initialize(playerCameraToHideBodyFrom, hapticLayer);
			HardlightSuit.Find().SetColliderState();

		}

		void Start()
		{
			Init();
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
		public static void Initialize(GameObject vrCamera = null, int hapticLayer = NSManager.HAPTIC_LAYER)
		{
			if (ValidInstance())
			{
				Debug.LogError("VRMimic is already initialized\nDoes not yet support multiple initializations");
			}
			else
			{
				GameObject singleton = new GameObject();
				instance = singleton.AddComponent<VRMimic>();
				instance.Init(vrCamera, hapticLayer);
				singleton.name = "VRMimic [Runtime Singleton]";
			}
		}
	}
}