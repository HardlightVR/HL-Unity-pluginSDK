using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Tracking
{
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

		void Update()
		{

		}
	}
}