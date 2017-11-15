using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	/// <summary>
	/// This class shows off how to use FilterFlags (for if you want to easily filter out certain flags)
	/// </summary>
	public class FilterFlagTest : MonoBehaviour
	{
		[RegionFlag("Base Flag")]
		public AreaFlag baseFlag;

		[SerializeField]
		public FilterFlag RemovedFlags;

		[RegionFlag("Filtered")]
		public AreaFlag FilteredFlag;


		void Update()
		{
			FilteredFlag = RemovedFlags.RemoveInactiveRegions(baseFlag);
		}
	}
}