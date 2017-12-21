using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Experimental
{
	public class RequestArmMimic : RequestTracker
	{
		public GameObject Controller;
		public ArmSide MySide = ArmSide.Left;
		public bool UseNewArmCreation = false;

		protected override VRObjectMimic RequestTrackerMimic()
		{
			if (Controller != null && Tracker != null)
			{
				//VRObjectMimic controllerMimic = VRMimic.Instance.FindMimicOfObject(Controller);
				VRObjectMimic controllerMimic = VRMimic.Instance.AddTrackedObject(Controller, controllerMimicType);

				if (controllerMimic == null)
				{
					//Setup controller tracker
				}
				else
				{
					controllerMimic.Init(Controller);
				}

				VRObjectMimic mimic = VRMimic.Instance.AddTrackedObject(Tracker);
				if (mimic == null)
				{

				}
				else
				{
					mimic.Init(Tracker);
				}

				var newArm = VRMimic.Instance.ActiveBodyMimic.CreateArm(MySide, mimic, controllerMimic, prefabsToUse);
				Debug.Log("Created new IArm.\n\t[Click to select it]", newArm);

				return mimic;
			}
			return null;
		}
	}
}