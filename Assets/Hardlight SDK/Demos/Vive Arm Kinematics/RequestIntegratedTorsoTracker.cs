using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Experimental
{
	public class RequestIntegratedTorsoTracker : RequestTracker
	{
		public Vector3 TrackerOffset = new Vector3();
		public MimickingOptions ImuTrackerOptions;

		[Header("Runtime Evaluated")]
		[SerializeField]
		private bool TrackerIsValid = false;
		[SerializeField]
		private HardlightTracking imuTracker;

		protected override VRObjectMimic RequestTrackerMimic()
		{
			if (Tracker != null)
			{
				if (!TrackerIsValid && imuTracker == null)
				{
					imuTracker = Tracker.GetComponent<HardlightTracking>();
					if (imuTracker)
					{
						TrackerIsValid = true;
					}
					else
					{
						Debug.LogError("Attempted to get an IMU tracker script [" + typeof(HardlightTracking).ToString() + "] from " + Tracker + " but it did not have the necessary script.\n");
					}
				}
			}
			else
			{
				Debug.LogError("Attempted to request an integrated tracker mimic from a null object. Returning null object\n");
				return null;
			}

			if (imuTracker != null && TrackerIsValid)
			{
				//Debug.Log("Assigning tracked representation: " + imuTracker.TrackedRepresentation + "\n", imuTracker.TrackedRepresentation);
				VRObjectMimic mimic = VRMimic.Instance.AddTrackedObject(imuTracker.TrackedRepresentation, VRObjectMimic.TypeOfMimickedObject.TrackedObject, ImuTrackerOptions);
				if (mimic == null)
				{

				}
				else
				{
					mimic.Init(imuTracker.TrackedRepresentation);
				}

				var newArm = VRMimic.Instance.ActiveBodyMimic.BindLowerBackTracker(mimic);
				Debug.Log("Created new ITorsoTracker.\n\t[Click to select it]", newArm);
				return mimic;
			}
			return null;
		}
	}
}