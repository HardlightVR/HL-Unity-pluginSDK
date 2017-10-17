using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public class FilterFlagTest : MonoBehaviour
	{
		[RegionFlag("")]
		public AreaFlag baseFlag;

		[SerializeField]
		public FilterFlag RemovedFlags;

		[RegionFlag("")]
		public AreaFlag FilteredFlag;


		void Update()
		{
			FilteredFlag = RemovedFlags.RemoveInactiveRegions(baseFlag);
		}
	}
}