using UnityEngine;
using System.Collections;

//Contents of this namespace are subject to change
namespace Hardlight.SDK.Experimental
{
	/// <summary>
	/// Used for comparing an IMU and an Absolute tracker like a Vive Puck
	/// </summary>
	public class SuitAbsoluteComparer : MonoBehaviour
	{
		public RequestTracker ArmSetup;
		public TrackingTest suitObjSetup;
		public FrameEvaluator representation;
		public float aboveAmt = .6f;

		public void Start()
		{
			var go = new GameObject("Frame Evaluator - " + ArmSetup.controllerMimicType.ToString());
			representation = go.AddComponent<FrameEvaluator>();
			representation.prefab = Resources.Load("Rotation Visualizer") as GameObject;

			representation.IMUObject = suitObjSetup.TrackedRepresentation.transform;
			representation.AbsoluteObject = ArmSetup.RequestedMimic.transform;
			representation.aboveAmt = aboveAmt;
		}
	}
}