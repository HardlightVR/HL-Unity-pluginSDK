using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Experimental
{
	public abstract class RequestTracker : MonoBehaviour
	{
		public VRObjectMimic.TypeOfMimickedObject controllerMimicType = VRObjectMimic.TypeOfMimickedObject.ControllerA;
		public GameObject Tracker;
		private VRObjectMimic requestedMimic;
		public BodyVisualPrefabData prefabsToUse;

		private bool initialized = false;

		public VRObjectMimic RequestedMimic
		{
			get
			{
				Init();
				return requestedMimic;
			}
		}

		void Init()
		{
			if (!initialized)
			{
				//Debug.Log("Initializing Request Tracker " + name + "\n");
				if (Tracker != null)
				{
					requestedMimic = RequestTrackerMimic();
					initialized = true;
				}
				else
				{
					enabled = false;
					Debug.LogError("Unable to initialize request tracker (Tracker is null). Disabling this component\n", this);
				}
			}

		}

		void Start()
		{
			Init();
		}

		protected abstract VRObjectMimic RequestTrackerMimic();
	}
}