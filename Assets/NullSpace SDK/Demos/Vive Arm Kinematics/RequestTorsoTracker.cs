using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class RequestTorsoTracker : MonoBehaviour
	{
		public GameObject Tracker;

		void Start()
		{
			if (Tracker != null)
			{
				VRObjectMimic mimic = VRMimic.Instance.AddTrackedObject(Tracker);
				if (mimic == null)
				{

				}
				else
				{
					mimic.Init(Tracker);
				}

				var newArm = VRMimic.Instance.ActiveBodyMimic.BindLowerBackTracker(mimic);
				Debug.Log("Created new ITorsoTracker.\n\t[Click to select it]", newArm);
			}
		}
	}
}