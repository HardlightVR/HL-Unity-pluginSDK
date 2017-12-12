using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public class FrameEvaluator : MonoBehaviour
	{
		public Transform AbsoluteObject;
		public Transform IMUObject;
		public GameObject prefab;

		public float aboveAmt = .6f;

		[Header("Runtime calculated")]
		public Quaternion OffsetCalibration;

		public FrameOfReferenceDisplay absolute;
		public FrameOfReferenceDisplay IMU;
		public FrameOfReferenceDisplay DisplayA;
		public FrameOfReferenceDisplay DisplayB;
		public FrameOfReferenceDisplay DisplayC;
		public FrameOfReferenceDisplay DisplayD;
		public FrameOfReferenceDisplay DisplayE;

		[System.Serializable]
		public class FrameOfReferenceDisplay
		{
			public Quaternion Offset;
			public Quaternion orientation;
			public GameObject display;
			public Transform displayLocation;
			public Vector3 displayLocOffset;
			public Vector3 pos
			{
				get { return displayLocation.position + displayLocOffset; }
			}
			public FrameOfReferenceDisplay(GameObject displayPrefab, string name, Transform parent, float upComp, Transform objLoc)
			{
				display = GameObject.Instantiate<GameObject>(displayPrefab);
				if (parent != null)
					display.transform.SetParent(parent);
				display.name = name;
				Offset = Quaternion.identity;
				displayLocation = objLoc;
				displayLocOffset = Vector3.up * upComp;
			}

			public void Update(Quaternion setRotation)
			{
				orientation = setRotation;
				display.transform.rotation = Offset * orientation;
				display.transform.position = pos;

				Vector3 upGoal = pos + Vector3.up * .3f;
				Vector3 rightGoal = pos + Vector3.right * .3f;
				Vector3 fwdGoal = pos + Vector3.forward * .3f;

				Vector3 myUp = pos + display.transform.rotation * Vector3.up * .2f;
				Vector3 myRight = pos + display.transform.rotation * Vector3.right * .2f;
				Vector3 myFwd = pos + display.transform.rotation * Vector3.forward * .2f;

				Debug.DrawLine(pos, upGoal, Color.green);
				Debug.DrawLine(pos, rightGoal, Color.red);
				Debug.DrawLine(pos, fwdGoal, Color.blue);

				Debug.DrawLine(myUp, upGoal, Color.green);
				Debug.DrawLine(myRight, rightGoal, Color.red);
				Debug.DrawLine(myFwd, fwdGoal, Color.blue);
			}
		}

		void Start()
		{
			IMU = new FrameOfReferenceDisplay(prefab, "Display IMU", transform, aboveAmt + 0.0f, AbsoluteObject);
			absolute = new FrameOfReferenceDisplay(prefab, "Display Absolute", transform, aboveAmt + 0.5f, AbsoluteObject);
			//DisplayA = new FrameOfReferenceDisplay(prefab, "Display A = (Abs)^-1 * IMU", transform, aboveAmt + 0.7f, AbsoluteObject);
			//DisplayB = new FrameOfReferenceDisplay(prefab, "Display B = (IMU)^-1 * Abs", transform, aboveAmt + 1.05f, AbsoluteObject);
			DisplayB = new FrameOfReferenceDisplay(prefab, "Display B = Euler subtraction", transform, aboveAmt + 1.0f, AbsoluteObject);
			DisplayC = new FrameOfReferenceDisplay(prefab, "Display C = Vector3 Conversion", transform, aboveAmt + 1.5f, AbsoluteObject);
			DisplayD = new FrameOfReferenceDisplay(prefab, "Display D = Calibrated IMU", transform, aboveAmt + 2f, AbsoluteObject);
			DisplayE = new FrameOfReferenceDisplay(prefab, "Display E = Calibrated IMU", transform, aboveAmt + 2.5f, AbsoluteObject);

			#region SteamVR Absolute Frame (not helpful, requires Matrix usage)
			//[StructLayout(LayoutKind.Sequential)]
			//public struct TrackedDevicePose_t
			//{
			//	public HmdMatrix34_t mDeviceToAbsoluteTracking;
			//	public HmdVector3_t vVelocity;
			//	public HmdVector3_t vAngularVelocity;
			//	public ETrackingResult eTrackingResult;
			//	[MarshalAs(UnmanagedType.I1)]
			//	public bool bPoseIsValid;
			//	[MarshalAs(UnmanagedType.I1)]
			//	public bool bDeviceIsConnected;
			//}

			//Valve.VR.TrackedDevicePose_t[] thingies = new Valve.VR.TrackedDevicePose_t[1];
			//steamVR.hmd.GetDeviceToAbsoluteTrackingPose(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, 0, thingies);

			//Matrix4x4 newMat = new Matrix4x4();
			//newMat[00] = thingies[0].mDeviceToAbsoluteTracking.m0;
			//newMat[01] = thingies[0].mDeviceToAbsoluteTracking.m1;
			//newMat[02] = thingies[0].mDeviceToAbsoluteTracking.m2;
			//newMat[03] = thingies[0].mDeviceToAbsoluteTracking.m3;
			//newMat[10] = thingies[0].mDeviceToAbsoluteTracking.m4;
			//newMat[11] = thingies[0].mDeviceToAbsoluteTracking.m5;
			//newMat[12] = thingies[0].mDeviceToAbsoluteTracking.m6;
			//newMat[13] = thingies[0].mDeviceToAbsoluteTracking.m7;
			//newMat[20] = thingies[0].mDeviceToAbsoluteTracking.m8;
			//newMat[21] = thingies[0].mDeviceToAbsoluteTracking.m9;
			//newMat[22] = thingies[0].mDeviceToAbsoluteTracking.m10;
			//newMat[23] = thingies[0].mDeviceToAbsoluteTracking.m11;

			//newMat. 
			#endregion
		}

		public Vector3 eulerAbsolute;
		public Vector3 eulerIMU;
		public Vector3 difference;

		void Update()
		{
			if (AbsoluteObject && IMUObject)
			{
				IMU.Update(IMUObject.rotation);
				absolute.Update(AbsoluteObject.rotation);
				//DisplayA.Update(Subtract(AbsoluteObject.rotation, IMUObject.rotation));
				//DisplayB.Update(Subtract(IMUObject.rotation, AbsoluteObject.rotation));

				eulerAbsolute = AbsoluteObject.rotation.eulerAngles;
				eulerIMU = IMUObject.rotation.eulerAngles;
				difference = eulerAbsolute - eulerIMU;

				DisplayB.Update(Quaternion.Euler(difference) * IMUObject.rotation);

				Vector3 up = Vector3.up * .3f;
				//Vector3 right = Vector3.right * .3f;
				//Vector3 fwd = Vector3.forward * .3f;
				Vector3 imuUp = IMUObject.rotation * up;
				Vector3 absUp = AbsoluteObject.rotation * up;

				//Vector3 imuRight = IMUObject.rotation * right;
				//Vector3 absRight = AbsoluteObject.rotation * right;

				//Vector3 imuFwd = IMUObject.rotation * fwd;
				//Vector3 absFwd = AbsoluteObject.rotation * fwd;

				//Debug.DrawLine(absolute.pos, absolute.pos + absUp, Color.green);
				//Debug.DrawLine(absolute.pos, absolute.pos + absUp, Color.green);
				//Debug.DrawLine(absolute.pos, absolute.pos + absUp, Color.green);

				OffsetCalibration = RotationBetweenVectors(absUp, imuUp);
				DisplayC.Update(OffsetCalibration);
				DisplayD.Update(OffsetCalibration * IMUObject.rotation);
				DisplayE.Update(IMUObject.rotation * OffsetCalibration);
				//DisplayC.Update(IMUObject.rotation * AbsoluteObject.rotation, AbsoluteObject.position + Vector3.up * (aboveAmt + 1.2f));
			}
		}

		private Quaternion Subtract(Quaternion A, Quaternion B)
		{
			//Inverse(A) * B == A * Inverse(B)
			return Quaternion.Inverse(A) * B;
		}

		//Credit: http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-17-quaternions/
		private Quaternion RotationBetweenVectors(Vector3 start, Vector3 dest)
		{
			start.Normalize();
			dest.Normalize();

			float cosTheta = Vector3.Dot(start, dest);
			Vector3 rotationAxis;

			if (cosTheta < -1 + 0.001f)
			{
				// special case when vectors in opposite directions:
				// there is no "ideal" rotation axis
				// So guess one; any will do as long as it's perpendicular to start
				rotationAxis = Vector3.Cross(Vector3.forward, start);
				
				if (rotationAxis.magnitude < 0.01) // bad luck, they were parallel, try again!
					rotationAxis = Vector3.Cross(Vector3.right, start);

				rotationAxis.Normalize();
				return Quaternion.AngleAxis(180, rotationAxis);
			}

			rotationAxis = Vector3.Cross(start, dest);

			float s = Mathf.Sqrt((1 + cosTheta) * 2);
			float invs = 1 / s;

			return new Quaternion(
				s * 0.5f,
				rotationAxis.x * invs,
				rotationAxis.y * invs,
				rotationAxis.z * invs
			);

		}
	}
}