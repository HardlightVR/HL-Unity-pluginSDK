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
						Initialize();
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

		private void Init(bool UseBodyMimic = true, Camera vrCamera = null, int hapticLayer = NSManager.HAPTIC_LAYER)
		{
			if (!initialized)
			{
				if (UseBodyMimic)
				{
					BodyMimic.Initialize(vrCamera, hapticLayer);
					HardlightSuit.Find().SetColliderState();
				}
				initialized = true;
			}
		}

		void Start()
		{
			Init();
		}

		public static void Initialize(Camera vrCamera = null, int hapticLayer = NSManager.HAPTIC_LAYER)
		{
			GameObject singleton = new GameObject();
			instance = singleton.AddComponent<VRMimic>();
			instance.Init();
			singleton.name = "VRMimic [Runtime Singleton]";
		}
	}
}