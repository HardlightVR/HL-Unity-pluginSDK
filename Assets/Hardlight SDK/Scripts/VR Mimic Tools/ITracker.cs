using UnityEngine;
using System.Collections;

//Contents of this namespace are subject to change
namespace Hardlight.SDK.Experimental
{
	/// <summary>
	/// This interface is for representing a tracker (either built into the suit or an absolute tracker like Vive pucks/standard)
	/// It is not locked in stone quite yet (hence Experimental namespace)
	/// It is being released to give an idea for the direction/solution we're building towards.
	/// We'd love to hear your feedback: devs@hardlightvr.com if you feel like pawing through the code
	/// </summary>
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

	/// <summary>
	/// Using an Interface-Abstract-UseCase approach allows for defaulted behavior while maintaining required components.
	/// Continuation of the goals for ITracker (see that summary)
	/// </summary>
	public abstract class AbstractTracker : MonoBehaviour, ITracker
	{
		[Header("Abstract Tracker Attributes")]
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