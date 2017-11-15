using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	/// <summary>
	/// A class for filtering out certain flags. Uses AreaFlagExtensions AddFlag/RemoveArea.
	/// This is mostly a convenience class.
	/// </summary>
	[System.Serializable]
	public class FilterFlag
	{
		[RegionFlag("Inactive Regions")]
		public AreaFlag InactiveRegions = AreaFlag.None;

		public void DisableArea(AreaFlag AreaToFilterOut)
		{
			InactiveRegions.AddFlag(AreaToFilterOut);
		}
		public void EnableArea(AreaFlag AreaToAllow)
		{
			InactiveRegions.RemoveArea(AreaToAllow);
		}

		public AreaFlag RemoveInactiveRegions(AreaFlag baseFlag)
		{
			return baseFlag.RemoveArea(InactiveRegions);
		}
	}
}