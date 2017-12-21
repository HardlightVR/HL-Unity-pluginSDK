using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Experimental
{
	[CreateAssetMenu(menuName = "Hardlight/VR Saved Tracking Heading")]
	public class SavedTrackingCalibration : ScriptableObject
	{
		[Header("Head Offset")]
		[SerializeField]
		[Range(-0, 360)]
		private float heading = 0;

		public float Heading
		{
			get
			{
				return heading;
			}

			set
			{
				heading = value;
			}
		}
	}
}