using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Experimental
{
	public class SuitAbsoluteComparer : MonoBehaviour
	{
		public RequestTracker ArmSetup;
		public HardlightTracking suitObjSetup;
		public FrameEvaluator representation;
		public float aboveAmt = .6f;

		public void Start()
		{
			var go = new GameObject(ArmSetup.controllerMimicType.ToString() + " - Frame Evaluator");
			representation = go.AddComponent<FrameEvaluator>();
			representation.prefab = Resources.Load("Rotation Visualizer") as GameObject;
			Debug.Log("Suit Absolute Comparer\n\t[Click to Select]", go);

			representation.IMUObject = suitObjSetup.TrackedRepresentation.transform;
			representation.AbsoluteObject = ArmSetup.RequestedMimic.transform;
			representation.aboveAmt = aboveAmt;
		}

		void Update()
		{

		}
	}
}