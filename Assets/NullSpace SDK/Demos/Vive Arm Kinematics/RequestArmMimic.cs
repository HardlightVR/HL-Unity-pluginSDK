using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class RequestArmMimic : MonoBehaviour
	{
		public VRObjectMimic.TypeOfMimickedObject controllerMimicType = VRObjectMimic.TypeOfMimickedObject.ControllerA;
		public GameObject Controller;
		public GameObject Tracker;
		public Vector3 TrackerOffset = new Vector3();
		public ArmSide MySide = ArmSide.Left;
		public BodyVisualPrefabData prefabsToUse;
		public bool UseNewArmCreation = false;

		void Start()
		{
			RequestArm();
		}

		private void RequestArm()
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
					mimic.Init(Tracker, TrackerOffset);
				}

				if (UseNewArmCreation)
				{
					var newArm = VRMimic.Instance.ActiveBodyMimic.CreateArm(MySide, mimic, controllerMimic, prefabsToUse);
					Debug.Log("Created new IArm.\n\t[Click to select it]", newArm);
				}
				else
				{
					var newArm = VRMimic.Instance.ActiveBodyMimic.CreateAntiqueArm(MySide, mimic, controllerMimic);
					Debug.Log("Created new arm.\n\t[Click to select it]", newArm);
				}
			}
		}

	}
}