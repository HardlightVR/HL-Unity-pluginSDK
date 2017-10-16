using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	public class ScaleWithSpherecast : MonoBehaviour
	{
		[Header("Note: only adapts to radius & range")]
		[Header("You must configure center/aim manually")]
		public HapticSphereCast sphereCast;

		void Awake()
		{
			EnsureValidSpherecast();
		}

		void Update()
		{
			EnsureValidSpherecast();

			transform.localScale = new Vector3(sphereCast.SphereCastRadius * 2, sphereCast.SphereCastRange / 2, sphereCast.SphereCastRadius * 2);
		}

		private void EnsureValidSpherecast()
		{
			if (sphereCast == null)
			{
				if (transform.parent != null)
				{
					sphereCast = transform.parent.GetComponent<HapticSphereCast>();
				}
			}
			if (sphereCast == null && isActiveAndEnabled)
			{
				enabled = false;
				Debug.LogError("[" + name + "] unable to find it's observed HapticSphereCast.\n\tDisabling the scaling script.", this);
			}
		}
	}
}