using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	/// <summary>
	/// A simple component representing Area information.
	/// It is good to use this instead of giving a script an AreaFlag value (another script might not know who has which AreaFlags.
	/// If a gameObject has a HapticLocation, you can just call .GetComponent<HapticLocation>() to get the proper location
	/// </summary>
	public class HapticLocation : MonoBehaviour
	{
		/// <summary>
		/// An empty, single or complex AreaFlag.
		/// Empty Ex - AreaFlag.None
		/// Single Ex - AreaFlag.Lower_Ab_Left
		/// Complex Ex1 - AreaFlag.Chest_Right|AreaFlag.Shoulder_Left
		/// Complex Ex2 - AreaFlag.Left_All
		/// </summary>
		[RegionFlag]
		public AreaFlag Where;
	}
}