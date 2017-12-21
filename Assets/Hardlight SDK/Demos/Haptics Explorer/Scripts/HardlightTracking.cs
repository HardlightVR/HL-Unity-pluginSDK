/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using Hardlight.SDK;
using Hardlight.SDK.Experimental;
using System;

namespace Hardlight.SDK.Experimental
{
	public class HardlightTracking : MonoBehaviour
	{
		public GameObject TrackedRepresentation;
		public GameObject ParentObject;
		public Imu whichIMU = Imu.Chest;
		public bool DisableObject = true;
		public bool ShowOnGUI = false;
		public bool VisibleIdentity = false;
		public bool AutoEnableTracking = false;
		public Quaternion rawIncQuat;
		public Quaternion lastQuat;

		[Range(0, 1)]
		public float PercentOfNewData = .95f;

		[Header("Chirality Reversal Control")]
		private bool reverseX = false;
		private bool reverseY = false;
		private bool reverseZ = false;
		private bool reverseW = true;

		[Header("Use Offset")]
		public Vector3 preChiralOffset = Vector3.zero;

		public Vector3 Offset = new Vector3(-270, 0, 90);

		private float BaseZOffsetAmount = 90;
		[Header("Hardlight SDK\\Resources\\Saved Tracking Calibration")]
		[SerializeField]
		private SavedTrackingCalibration _trackingCalibration;
		public SavedTrackingCalibration Calibration
		{
			get
			{
				if (_trackingCalibration == null)
				{
					_trackingCalibration = Resources.Load<SavedTrackingCalibration>("Saved Tracking Calibration");
				}
				if (_trackingCalibration == null)
					_trackingCalibration = ScriptableObject.CreateInstance<SavedTrackingCalibration>();
				return _trackingCalibration;
			}

			set
			{
				if (_trackingCalibration == null)
				{
					_trackingCalibration = Resources.Load<SavedTrackingCalibration>("Saved Tracking Calibration");
				}
				if (_trackingCalibration == null)
					_trackingCalibration = ScriptableObject.CreateInstance<SavedTrackingCalibration>();
				_trackingCalibration = value;
			}
		}
		public float AdditionalZOffsetAmount
		{
			get
			{
				return Calibration.Heading;
			}

			set
			{
				Calibration.Heading = value;
			}
		}

		void Start()
		{
			BaseZOffsetAmount = Offset.z;
			
			if (ParentObject != null)
			{
				ParentObject.SetActive(!DisableObject);
			}

			if (AutoEnableTracking)
				EnableTracking();
		}

		public void EnableTracking()
		{
			if (ParentObject != null)
			{
				ParentObject.SetActive(true);
			}
			HardlightManager.Instance.EnableTracking();
		}

		public void DisableTracking()
		{
			if (ParentObject != null)
			{
				ParentObject.SetActive(false);
			}
			HardlightManager.Instance.DisableTracking();
		}

		void Update()
		{
			if (TrackedRepresentation != null)
			{
				Offset.z = BaseZOffsetAmount + AdditionalZOffsetAmount;

				var tracking = HardlightManager.Instance.PollTracking();
				Quaternion assign = Quaternion.identity;

				if (whichIMU == Imu.Chest)
				{
					assign = tracking.Chest;
				}
				else if (whichIMU == Imu.Left_Upper_Arm)
				{
					assign = tracking.LeftUpperArm;
				}
				else if (whichIMU == Imu.Right_Upper_Arm)
				{
					assign = tracking.RightUpperArm;
				}
				VisibleIdentity = assign != Quaternion.identity;

				assign.y = -assign.y;

				rawIncQuat = assign;
				lastQuat = assign;

				Vector3 up = assign * Vector3.up * .1f;
				Vector3 fwd = assign * Vector3.forward * .1f;
				Vector3 rght = assign * Vector3.right * .1f;
				Vector3 pos = TrackedRepresentation.transform.position + Vector3.up * .1f;
				//Debug.DrawLine(pos, pos + up, Color.green);
				//Debug.DrawLine(pos, pos + fwd, Color.blue);
				//Debug.DrawLine(pos, pos + rght, Color.red);

				float angle = 0;
				Vector3 axis = Vector3.zero;

				string infoDetails = name + " Diag\n";

				//assign.ToAngleAxis(out angle, out axis);
				//diagnosis += "Axis: " + axis + "   -    " + angle + "   -    " + assign + "\n";
				//testQuat = Quaternion.AngleAxis(angleMult * angle, axis);
				//Debug.DrawLine(pos - axis * .1f, pos + axis * .1f, Color.magenta);

				//Quaternion testFinal = ReverseChirality(assign * testQuat);
				//diagnosis += "Test Approach: " + testFinal + "   -    " + "\n\n";

				#region PreChiralOffset Quaternion Creation
				Quaternion preChiralOffsetQuat = Quaternion.identity;
				preChiralOffsetQuat = Quaternion.AngleAxis(preChiralOffset.z, Vector3.forward) * preChiralOffsetQuat;
				preChiralOffsetQuat = Quaternion.AngleAxis(preChiralOffset.y, Vector3.up) * preChiralOffsetQuat;
				preChiralOffsetQuat = Quaternion.AngleAxis(preChiralOffset.x, Vector3.right) * preChiralOffsetQuat;
				#endregion

				preChiralOffsetQuat.ToAngleAxis(out angle, out axis);
				infoDetails += "preChirQuad Axis: " + axis + "   -    " + angle + "   -    " + preChiralOffsetQuat + "\n";

				//=============================================
				assign = ReverseChirality(assign * preChiralOffsetQuat);
				//=============================================

				assign.ToAngleAxis(out angle, out axis);
				infoDetails += "RevChir(Assign*PreChiralQuat): " + axis + "   -    " + angle + "   -    " + assign + "\n\n";

				#region PostChiralOffset Quaternion Creation
				Quaternion postChiralOffsetQuat = Quaternion.identity;
				postChiralOffsetQuat = Quaternion.AngleAxis(Offset.z, Vector3.forward) * postChiralOffsetQuat;
				postChiralOffsetQuat = Quaternion.AngleAxis(Offset.y, Vector3.up) * postChiralOffsetQuat;
				postChiralOffsetQuat = Quaternion.AngleAxis(Offset.x, Vector3.right) * postChiralOffsetQuat;
				postChiralOffsetQuat = ReverseChiralityXZ(postChiralOffsetQuat);
				#endregion

				postChiralOffsetQuat.ToAngleAxis(out angle, out axis);
				infoDetails += "Offset Axis: " + axis + "  -  " + angle + "   -    " + postChiralOffsetQuat + "\n\n";
				//Debug.DrawLine(pos - axis * .1f, pos + axis * .1f, Color.black);

				//=============================================
				var FinalizedQuat = postChiralOffsetQuat * assign;
				//=============================================

				FinalizedQuat.ToAngleAxis(out angle, out axis);
				infoDetails += "Final Axis: " + axis + "   -    " + angle + "   -    " + FinalizedQuat + "\n\n";

				SetRepresentationOrientation(FinalizedQuat);

				//Debug.Log(infoDetails + "\n");
			}
		}

		private void SetRepresentationOrientation(Quaternion target)
		{
			TrackedRepresentation.transform.rotation = Quaternion.Lerp(TrackedRepresentation.transform.rotation, target, PercentOfNewData);
		}

		private void HeadsetAutoCalibration(Quaternion first)
		{
			Vector3 pos = TrackedRepresentation.transform.position + Vector3.up * 2.1f;
			Vector3 hmdDrawPos = TrackedRepresentation.transform.position + Vector3.up * 2.1f + Vector3.right * .3f;

			//Raw Quat
			Vector3 up = first * Vector3.up * .1f;
			Vector3 fwd = first * Vector3.forward * .1f;
			Vector3 rght = first * Vector3.right * .1f;


			Vector3 hmdUp = VRMimic.Instance.VRCamera.transform.up.normalized * .1f;
			Vector3 hmdFwd = VRMimic.Instance.VRCamera.transform.forward.normalized * .1f;
			Vector3 hmdRght = VRMimic.Instance.VRCamera.transform.right.normalized * .1f;

			//Debug.DrawLine(pos, pos + up, Color.green);
			//Debug.DrawLine(pos, pos + fwd, Color.blue);
			//Debug.DrawLine(pos, pos + rght, Color.red);

			//Debug.DrawLine(hmdDrawPos, hmdDrawPos + hmdUp, Color.green);
			//Debug.DrawLine(hmdDrawPos, hmdDrawPos + hmdFwd, Color.blue);
			//Debug.DrawLine(hmdDrawPos, hmdDrawPos + hmdRght, Color.red);
		}

		private void DrawTrackingUpdate(Quaternion assign, Vector3 north, Vector3 imuUp)
		{
			#region TrackingUpdate North/Up vectors
			Vector3 up = Vector3.up;
			//Debug.DrawLine(up * 3, up * 3 + north.normalized, Color.red);
			//Debug.DrawLine(up * 3, up * 3 + assign * north.normalized, Color.magenta);
			//Debug.DrawLine(up * 3, up * 3 + Quaternion.Inverse(assign) * north.normalized, Color.black);

			//Debug.DrawLine(up * 5, up * 5 + imuUp.normalized, Color.green);
			//Debug.DrawLine(up * 5, up * 5 + assign * imuUp.normalized, Color.cyan);
			//Debug.DrawLine(up * 5, up * 5 + Quaternion.Inverse(assign) * imuUp.normalized, Color.white);

			Vector3 chestRight = Vector3.Cross(imuUp.normalized, north.normalized);
			//Debug.DrawLine(up * 4, up * 4 + chestRight.normalized, Color.blue);
			//Debug.DrawLine(up * 4, up * 4 + assign * chestRight.normalized, Color.yellow);
			//Debug.DrawLine(up * 4, up * 4 + Quaternion.Inverse(assign) * chestRight.normalized, Color.grey);

			//Debug.Log("Chest North: " + north.normalized + "  Red \t\tMagenta: " + assign * north.normalized +
			//	"\n\t\tBlack: " + (Quaternion.Inverse(assign) * north.normalized).normalized);
			//Debug.Log("Chest Up:    " + imuUp.normalized + "  Green \t\tCyan:    " + assign * imuUp.normalized +
			//	"\n\t\tWhite: " + (Quaternion.Inverse(assign) * imuUp.normalized).normalized);
			#endregion
		}

		private Quaternion ReverseChiralityXZ(Quaternion quat)
		{
			var q = quat;
			q.x = -q.x;
			q.z = -q.z;
			return q;
		}

		private UnityEngine.Quaternion ReverseChirality(UnityEngine.Quaternion quat)
		{
			if (reverseX)
				quat.x = -quat.x;
			if (reverseY)
				quat.y = -quat.y;
			if (reverseZ)
				quat.z = -quat.z;
			if (reverseW)
				quat.w = -quat.w;
			return quat;
		}
	}
}