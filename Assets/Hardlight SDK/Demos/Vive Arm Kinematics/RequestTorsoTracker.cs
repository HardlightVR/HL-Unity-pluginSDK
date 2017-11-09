using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public class RequestTorsoTracker : RequestTracker
	{
		protected override VRObjectMimic RequestTrackerMimic()
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
				return mimic;
			}
			return null;
		}
	}
}