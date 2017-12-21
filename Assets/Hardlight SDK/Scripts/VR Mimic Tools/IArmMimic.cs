using UnityEngine;
using System.Collections;
using System;

//Contents of this namespace are subject to change
namespace Hardlight.SDK.Experimental
{
	/// <summary>
	/// This enum will grow to encompass the IMU arm modes as our internal development & tests move forward.
	/// </summary>
	public enum ArmKinematicMode { ControllerOnly, ViveUpperArms, ArmsDisabled, ImuUpperArms }

	/// <summary>
	/// Unfortunately, we aren't leaving the door open to support more than two arms.
	/// There is some really cool research on the concept of phantom limbs (which are controlled by input from both arms).
	/// </summary>
	public enum ArmSide { Right, Left }

	/// <summary>
	/// A basic approach to how to handle the arm mimic behaviors
	/// </summary>
	public interface IArmMimic
	{
		//Shoulder Mount position
		GameObject ShoulderMount
		{
			get;
			set;
		}

		//Vive Tracked Position
		VRObjectMimic TrackerMount
		{
			get;
			set;
		}

		//Linked controller position
		VRObjectMimic ControllerConnection
		{
			get;
			set;
		}

		bool ControllerInRange
		{
			get;
			set;
		}

		ArmSide WhichArm
		{
			get;
			set;
		}

		ArmKinematicMode ArmMode
		{
			get;
		}

		bool MimicEnabled
		{
			get;
			set;
		}

		HardlightCollider ForearmCollider
		{
			get;
			set;
		}
		HardlightCollider UpperArmCollider
		{
			get;
			set;
		}

		void Setup(ArmSide WhichSide, GameObject ShoulderMountConnector, VRObjectMimic Tracker, VRObjectMimic Controller);
	}

	/// <summary>
	/// Following the Interface-Abstract-UseCase approach allows for defaulted behavior while still achieving interface required implementations.
	/// </summary>
	public abstract class AbstractArmMimic : MonoBehaviour, IArmMimic
	{
		public virtual ArmKinematicMode ArmMode
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		[SerializeField]
		[Header("Arm Mimic Attributes")]
		protected ArmSide _whichArm = ArmSide.Right;
		public ArmSide WhichArm
		{
			get
			{
				return _whichArm;
			}

			set
			{
				_whichArm = value;
			}
		}

		[SerializeField]
		protected bool _controllerInRange = true;
		public bool ControllerInRange
		{
			get
			{
				return _controllerInRange;
			}

			set
			{
				_controllerInRange = value;
			}
		}

		[SerializeField]
		protected bool _mimicEnabled = true;
		public bool MimicEnabled
		{
			get
			{
				return _mimicEnabled;
			}

			set
			{
				_mimicEnabled = value;
			}
		}

		[SerializeField]
		protected GameObject _shoulderMount;
		public GameObject ShoulderMount
		{
			get
			{
				return _shoulderMount;
			}

			set
			{
				_shoulderMount = value;
			}
		}

		[SerializeField]
		protected VRObjectMimic _trackerMount;
		public VRObjectMimic TrackerMount
		{
			get
			{
				return _trackerMount;
			}

			set
			{
				_trackerMount = value;
			}
		}

		[SerializeField]
		protected VRObjectMimic _controllerConnection;
		public VRObjectMimic ControllerConnection
		{
			get
			{
				return _controllerConnection;
			}

			set
			{
				_controllerConnection = value;
			}
		}

		[SerializeField]
		[Header("Haptic Colliders")]
		protected HardlightCollider _forearmCollider;
		public HardlightCollider ForearmCollider
		{
			get
			{
				return _forearmCollider;
			}

			set
			{
				_forearmCollider = value;
			}
		}

		[SerializeField]
		protected HardlightCollider _upperArmCollider;
		public HardlightCollider UpperArmCollider
		{
			get
			{
				return _upperArmCollider;
			}

			set
			{
				_upperArmCollider = value;
			}
		}


		[SerializeField]
		protected BodyVisualPrefabData _bodyPartPrefabs;
		public BodyVisualPrefabData BodyPartPrefabs
		{
			get
			{
				if (_bodyPartPrefabs == null)
					Debug.LogError("[" + name + "] Arm Mimic's visual data is not assigned.\n", this);
				return _bodyPartPrefabs;
			}

			set
			{
				_bodyPartPrefabs = value;
			}
		}
		[SerializeField]
		protected BodyVisualPrefabData _nonVisualPrefabs;
		public BodyVisualPrefabData NonVisualPrefabs
		{
			get
			{
				if (_nonVisualPrefabs == null)
					Debug.LogError("[" + name + "] Arm Mimic's data model prefabs are not assigned.\n", this);
				return _nonVisualPrefabs;
			}

			set
			{
				_nonVisualPrefabs = value;
			}
		}

		public virtual void Setup(ArmSide WhichSide, GameObject ShoulderMountConnector, VRObjectMimic Tracker, VRObjectMimic Controller)
		{
			throw new NotImplementedException();
		}
	}
}