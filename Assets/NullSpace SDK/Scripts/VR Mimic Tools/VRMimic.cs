using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class VRMimic : MonoBehaviour
	{
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
						Debug.LogError("[VRMimic] There is more than one VRMimic Singleton\n" +
							"There shouldn't be multiple VRMimic objects");
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
					Debug.LogError("VRMimic not yet initialized before attempting to get the VRCamera. Will attempt setup assuming Camera.main is the VR Camera.");
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