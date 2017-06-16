using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using NullSpace.SDK;

namespace NullSpace.SDK
{
	public class BodyMimic : MonoBehaviour
	{
		[Header("Body Hang Origin")]
		public GameObject hmd;

		/// <summary>
		/// [Not currently in use]
		/// This is stub code for future tracking improvements that allow the body to tilt with the HMD (left/right, forward/backward)
		/// For more information, look into CalculateHMDTilt()
		/// </summary>
		//[Range(0, .35f)]
		//public float TiltAmtWithHMD = 0.0f;

		/// <summary>
		/// Big surprise, your body is below your head and neck. 
		/// This lets you adjust the body position. You might want to configure this as a game option/calibration step
		/// for players of different body distributions (long neck vs young with shorter neck vs giraffes)
		/// </summary>
		[Header("How far down the body is from HMD")]
		[Range(-2, 2)]
		public float NeckVerticalAnchor = .25f;

		/// <summary>
		/// The body is locked a bit back from where the HMD's position will be. 
		/// This controls how far forward or backward.
		/// </summary>
		[Header("How far fwd or back the body is")]
		[Range(-2, 2)]
		public float NeckFwdAnchor = 0;

		/// <summary>
		/// This lets you configure a different position when this box is checked. Useful to simulate human height when your Vive/Oculus/HMD is on your desk.
		/// </summary>
		[Header("For when your HMD is on your desk")]
		public bool UseDevHeight = false;
		public float devHeightPercentage = -0.15f;

		/// <summary>
		/// The direction the headset currently thinks is forward (regardless of overtilt up or down)
		/// Flattens the fwd vector onto the XZ plane. Then switches to using the Up/Down vector on the XZ plane if looking too far down or too far up.
		/// </summary>
		public Vector3 assumedForward = Vector3.zero;
		public Vector3 LastUpdatedPosition = Vector3.zero;

		/// <summary>
		/// The internal control of updateRate
		/// This could be temporarily increased/decreased based on context.
		/// </summary>
		private float updateRate;

		/// <summary>
		/// This controls how rigidly the body follows the HMD's position and orientation
		/// </summary>
		private float TargetUpdateRate = .15f;

		private float UpdateDuration = .75f;
		private float UpdateCounter = .2f;
		/// <summary>
		/// Where the BodyMimic should be (for lerping)
		/// </summary>
		Vector3 targetPosition;

		/// <summary>
		/// When this distance is exceeded, it will force an update (for teleporting/very fast motion)
		/// </summary>
		[Header("Exceed this val to force update")]
		public float SnapUpdateDist = 1.0f;
		private Vector3 LastRelativePosition;

		//[Header("Floor Evaluation")]
		//public bool UseHeadRaycasting = false;
		//public LayerMask validFloorLayers = ~((1 << 2) | (1 << 9) | (1 << 10) | (1 << 12) | (1 << 13) | (1 << 15));

		private void Awake()
		{
			updateRate = TargetUpdateRate;
		}

		void FixedUpdate()
		{
			Vector3 hmdNoUp = hmd.transform.forward;
			hmdNoUp.y = 0;
			Vector3 hmdUpNoY = hmd.transform.up;
			hmdUpNoY.y = 0;

			//If we teleport or are too far away
			if (Vector3.Distance(hmd.transform.position, LastUpdatedPosition) > SnapUpdateDist)
			{
				//Force an update for now
				ImmediateUpdate();
			}
			else
			{
				LastRelativePosition = transform.position - hmd.transform.position;
			}

			//We want to use the HMD's Up to find which way we should actually look to solve the overtilting problem
			float mirrorAngleAmt = Vector3.Angle(hmd.transform.forward, Vector3.up);

			//Check if we need to do a mirror operation
			if (mirrorAngleAmt < 5 || mirrorAngleAmt > 175)
			{
				hmdNoUp = -hmdNoUp;
			}

			UpdateCounter += Time.deltaTime * updateRate;

			//This is logic to let us update only some of the time.
			if (UpdateCounter >= UpdateDuration)
			{
				UpdateCounter = 0;
				LastUpdatedPosition = hmd.transform.position;

				//We reset the update rate. The core of this logic was to have certain criteria that used a higher update rate (so we would get closer to the next update quicker)
				updateRate = TargetUpdateRate;
			}

			//float prog = UpdateDuration - UpdateCounter;
			LastUpdatedPosition = Vector3.Lerp(LastUpdatedPosition, hmd.transform.position, .5f);// Mathf.Clamp(prog / UpdateDuration, 0, 1));

			Vector3 flatRight = hmd.transform.right;
			flatRight.y = 0;

			Vector3 rep = Vector3.Cross(flatRight, Vector3.up);

			assumedForward = rep.normalized;

			Debug.DrawLine(hmd.transform.position + Vector3.up * 5.5f, hmd.transform.position + rep + Vector3.up * 5.5f, Color.grey, .08f);

			float dist = hmd.transform.position.y - hmd.transform.parent.transform.position.y;
			//Debug.Log(hit.collider.gameObject.name + "\n is " + dist + " away " + dist * beltHeightPercentage + "  " + hit.collider.gameObject.layer);
			Vector3 hmdDown = Vector3.down * dist * (UseDevHeight ? devHeightPercentage : NeckVerticalAnchor);
			targetPosition = assumedForward * (.25f + NeckFwdAnchor) + hmd.transform.position + hmdDown;

			transform.position = Vector3.Lerp(transform.position, targetPosition, updateRate);

			//Create the transform based on our position and where we should face.
			transform.LookAt(transform.position + assumedForward * 5, Vector3.up);
		}

		/// <summary>
		/// Force an update of the BodyMimic (in case of teleports, fast movement)
		/// </summary>
		public void ImmediateUpdate()
		{
			transform.position = hmd.transform.position + LastRelativePosition;
		}

		/// <summary>
		/// [Unused Stub]
		/// This function is pseudocode for a future feature - calculating body mimic's tilt based on the headset orientation/recent movement
		/// </summary>
		void CalculateHMDTilt()
		{
			//Look at the orientation of the HMD.
			//Case 1: Standing Straight
			//		No behavior change
			//
			//
			//
			//Case 2: Look Left/Right (Only Y Axis Rotation)
			//
			//
			//Case 3: Look Up/Down (Only X axis Rotation)
			//		Check for change in local Y position
			//			If Y decreased recently, they might be peering down and leaning over.
			//			TILT Forward around Body's Right vector
			//
			//
			//Case 4: Confused Tilt Left/Right (Only Z axis Rotation)
			//		Check for change in local X vector (their right)
			//			If they moved in their local X space recently, they might be peaking around a corner
			//			TILT L/R around Body's Fwd vector
			//
			//Case 5: [Multiple cases at once]
			//		Due to the complex nature of these steps, it might be better to define each as their own operation that influences the end body orientation, and apply them separately. There will obviously be weird cases from the player doing handstands or cartwheels.
			//
		}

		/// <summary>
		/// This function creates and initializes the Body Mimic
		/// </summary>
		/// <param name="vrCamera">The camera to hide the body from. Calls camera.HideLayer(int)</param>
		/// <param name="hapticObjectLayer">The layer that is removed from the provided camera's culling mask.</param>
		/// <returns>The created body mimic</returns>
		public static BodyMimic Initialize(Camera vrCamera = null, int hapticObjectLayer = NSManager.HAPTIC_LAYER)
		{
			GameObject go = Resources.Load<GameObject>("Body Mimic");

			//Instantiate the prefab of the body mimic.
			GameObject newMimic = Instantiate<GameObject>(go);
			newMimic.name = "Body Mimic";

			BodyMimic mimic = null;

			if (newMimic != null)
			{
				GameObject cameraObject = vrCamera == null ? Camera.main.gameObject : vrCamera.gameObject;
				MimickedObjects objs = VRObjectMimic.Get(cameraObject);
				vrCamera = cameraObject.GetComponent<Camera>();

				//Set the BodyMimic's target to the VRObjectMimic
				mimic = newMimic.GetComponent<BodyMimic>();
				mimic.hmd = objs.VRCamera.gameObject;
				mimic.transform.SetParent(VRObjectMimic.Get(cameraObject).Root.transform);
			}
			if (vrCamera != null)
			{
				vrCamera.HideLayer(hapticObjectLayer);
			}

			return mimic;
		}
	}
}