using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	/// <summary>
	/// This component is attached to objects that are registered with the VRMimic systems.
	/// Each VRObjectMimic has a WatchedByMimic. Both of these components know of the other.
	/// Useful for avoid duplicate copycats.
	/// </summary>
	public class WatchedByMimic : MonoBehaviour
	{
		[SerializeField]
		private VRObjectMimic _watchingMimic;
		public VRObjectMimic WatchingMimic
		{
			get
			{
				if (_watchingMimic == null)
				{
					Debug.LogError("WatchedByMimic's watching mimic was not assigned correctly\n\tPerhaps it was not added and assigned correctly?");
				}
				return _watchingMimic;
			}
			set
			{
				if (value == null)
				{
					Debug.LogError("Invalidly assigned the Watching Mimic of WatchedByMimic\n");
				}
				_watchingMimic = value;
			}
		}
	}
}