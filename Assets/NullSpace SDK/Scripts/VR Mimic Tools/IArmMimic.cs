using UnityEngine;
using System.Collections;
using System;

namespace NullSpace.SDK
{
	public enum ArmKinematicMode { ControllerOnly, ViveUpperArms, ArmsDisabled }

	public enum ArmSide { Right, Left }

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