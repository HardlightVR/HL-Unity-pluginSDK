using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public interface ITracker
	{
		GameObject ObjectToInfluence
		{
			get;
			set;
		}

		//Vive Tracked Position
		VRObjectMimic TrackerMimic
		{
			get;
			set;
		}

		bool MimicEnabled
		{
			get;
			set;
		}

		void Setup(GameObject objectToInfluence, VRObjectMimic Tracker);
	}

	public abstract class AbstractTracker : MonoBehaviour, ITracker
	{
		[Header("Arm Torso Attributes")]
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
		protected GameObject _objectToInfluence;
		public GameObject ObjectToInfluence
		{
			get
			{
				return _objectToInfluence;
			}

			set
			{
				_objectToInfluence = value;
			}
		}

		[SerializeField]
		protected VRObjectMimic _trackerMimic;
		public VRObjectMimic TrackerMimic
		{
			get
			{
				return _trackerMimic;
			}

			set
			{
				_trackerMimic = value;
			}
		}

		public Vector3 Offset = Vector3.down;

		public virtual void Setup(GameObject objectToInfluence, VRObjectMimic Tracker)
		{
			ObjectToInfluence = objectToInfluence;
			TrackerMimic = Tracker;
		}
	}

}