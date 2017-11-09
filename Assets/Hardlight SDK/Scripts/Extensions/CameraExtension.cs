using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public static class CameraExtension
	{
		public static void HideLayer(this Camera camera, int layerToHide)
		{
			if (layerToHide <= 31 && layerToHide >= 0)
			{
				if (camera.cullingMask == 0)
				{
					Debug.LogError("Camera [" + camera.name + "] has nothing in it's culling mask. CameraExtension.HideLayer will do nothing here.\n\tReporting as an error because you likely want something on that culling mask?\n");
				}
				else
				{
					//Define the layermask without the given layer. (usually the Haptics layer)
					//This will not work if our base flag set is Nothing (but that shouldn't happen)
					camera.cullingMask = camera.cullingMask - (1 << layerToHide);
				}
			}
			else
			{
				if (camera == null)
				{
					Debug.LogError("Invalid Layer [" + layerToHide + "] provided along with a [NULL] camera for hiding" + "\n");
				}
				else
				{
					Debug.LogError("Invalid Layer [" + layerToHide + "] provided to camera [" + camera.name + "]for hiding" + "\n");
				}
			}
		}
	}
}